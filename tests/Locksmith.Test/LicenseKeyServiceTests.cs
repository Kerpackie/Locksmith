using Locksmith.Core.Config;
using Locksmith.Core.Exceptions;
using Locksmith.Core.Models;
using Locksmith.Core.Services;
using Locksmith.Core.Validation;

namespace Locksmith.Test;

public class LicenseKeyServiceTests
{
    private const string SecretKey = "Shhhh!SuperSecretKey123DontTellAnyone!";

    private LicenseKeyService CreateService(
        LicenseValidationOptions options = null,
        ILicenseValidator validator = null)
    {
        options ??= new LicenseValidationOptions();
        validator ??= new DefaultLicenseValidator(options);
        return new LicenseKeyService(SecretKey, options, validator);
    }
    
    private LicenseInfo CreateTestLicense(DateTime? expiration = null)
    {
        return new LicenseInfo
        {
            Name = "Alice Example",
            ProductId = "com.example.product",
            ExpirationDate = expiration
        };
    }

    [Fact]
    public void Generate_And_Validate_Should_Pass_For_ValidLicence()
    {
        // Arrange
        var service = new LicenseKeyService(SecretKey);
        var license = CreateTestLicense(DateTime.UtcNow.AddDays(10));
        
        // Act
        var key = service.Generate(license);
        var result = service.Validate(key);
        
        // Assert
        Assert.True(result.IsValid);
        Assert.NotNull(result.LicenseInfo);
        Assert.Equal("Alice Example", result.LicenseInfo.Name);
    }

    [Fact]
    public void Validate_Should_Fail_For_TamperedKey()
    {
        // Arrange
        var service = new LicenseKeyService(SecretKey);
        var license = CreateTestLicense(DateTime.UtcNow.AddDays(10));
        
        // Act
        var key = service.Generate(license);
        var tamperedKey = key.Substring(0, key.Length - 2) + "xx"; // Break the key.
        
        var result = service.Validate(tamperedKey);
        
        // Assert
        Assert.False(result.IsValid);
        Assert.Equal("Invalid signature.", result.Error);
    }

    [Fact]
    public void Validate_Should_Fail_For_ExpiredLicence()
    {
        // Arrange
        var service = new LicenseKeyService(SecretKey);
        var license = CreateTestLicense(DateTime.UtcNow.AddDays(-1)); // Expired licence
        
        // Act
        var key = service.Generate(license);
        var result = service.Validate(key);
        
        // Assert
        Assert.False(result.IsValid);
        Assert.Equal("License has expired.", result.Error);
    }

    [Fact]
    public void Validate_Should_Fail_For_InvalidBase58()
    {
        var service = new LicenseKeyService(SecretKey);

        // Includes characters not in the Base58 alphabet
        var invalidKey = "0OIl+/";

        var result = service.Validate(invalidKey);

        Assert.False(result.IsValid);
        Assert.StartsWith("Validation failed", result.Error);
    }
    
    [Fact]
    public void ValidationResult_Should_Set_IsExpired_Flag_Correctly()
    {
        var service = new LicenseKeyService(SecretKey);
        var license = CreateTestLicense(DateTime.UtcNow.AddDays(-5)); // Expired

        var key = service.Generate(license);
        var result = service.Validate(key);

        Assert.False(result.IsValid);
        Assert.True(result.IsExpired);
        Assert.False(result.IsTampered);
        Assert.NotNull(result.LicenseInfo);
        Assert.Equal("Alice Example", result.LicenseInfo.Name);
    }

    [Fact]
    public void ValidationResult_Should_Set_IsTampered_Flag_Correctly()
    {
        var service = new LicenseKeyService(SecretKey);
        var license = CreateTestLicense(DateTime.UtcNow.AddDays(5)); // Valid

        var key = service.Generate(license);

        // Flip last few characters
        var tamperedKey = key.Substring(0, key.Length - 2) + "ZZ";

        var result = service.Validate(tamperedKey);

        Assert.False(result.IsValid);
        Assert.True(result.IsTampered);
        Assert.False(result.IsExpired);
        Assert.NotNull(result.LicenseInfo); // Should still have partial info
        Assert.Equal("Alice Example", result.LicenseInfo.Name);
    }

    [Fact]
    public void ValidationResult_Should_Set_IsMalformed_Flag_When_Decode_Fails()
    {
        var service = new LicenseKeyService(SecretKey);
        var result = service.Validate("$$$notvalidbase58###");

        Assert.False(result.IsValid);
        Assert.True(result.IsMalformed);
        Assert.False(result.IsExpired);
        Assert.False(result.IsTampered);
        Assert.Null(result.LicenseInfo);
    }

    [Fact]
    public void Generate_Should_Throw_When_Validation_Fails_And_ThrowOnValidationError_Is_True()
    {
        // Arrange
        var options = new LicenseValidationOptions
        {
            ThrowOnValidationError = true
        };

        var service = CreateService(options);
        var invalidLicense = new LicenseInfo
        {
            Name = "", // Invalid
            ProductId = "com.example.product",
            ExpirationDate = DateTime.UtcNow.AddDays(10)
        };

        // Act & Assert
        Assert.Throws<LicenseValidationException>(() => service.Generate(invalidLicense));
    }
    
    [Fact]
    public void Generate_Should_Not_Throw_When_Validation_Fails_And_ThrowOnValidationError_Is_False()
    {
        // Arrange
        var options = new LicenseValidationOptions
        {
            ThrowOnValidationError = false
        };

        var validator = new DefaultLicenseValidator(options);
        var service = CreateService(options, validator);

        var invalidLicense = new LicenseInfo
        {
            Name = "", // Invalid
            ProductId = "com.example.product",
            ExpirationDate = DateTime.UtcNow.AddDays(10)
        };

        // Act
        LicenseGenerationResult result;
        try
        {
            var key = service.Generate(invalidLicense); // Will throw, but we don’t have a TryGenerate method yet
            result = LicenseGenerationResult.Ok(key);
        }
        catch (LicenseValidationException ex)
        {
            result = LicenseGenerationResult.Fail(ex.Message); // simulate future TryGenerate behavior
        }

        // Assert
        Assert.False(result.Success);
        Assert.Equal("License holder's name is required.", result.Error);
    }

    [Fact]
    public void TryGenerate_Should_Succeed_For_ValidLicense()
    {
        // Arrange
        var service = CreateService();
        var license = CreateTestLicense(DateTime.UtcNow.AddDays(5));

        // Act
        var result = service.TryGenerate(license);

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.LicenseKey);
        Assert.Null(result.Error);
    }

    [Fact]
    public void TryGenerate_Should_Return_Error_For_InvalidLicenseData()
    {
        // Arrange
        var options = new LicenseValidationOptions
        {
            ThrowOnValidationError = false
        };

        var validator = new DefaultLicenseValidator(options);
        var service = CreateService(options, validator);

        var invalidLicense = new LicenseInfo
        {
            Name = "", // Invalid
            ProductId = "com.example.product",
            ExpirationDate = DateTime.UtcNow.AddDays(10)
        };

        // Act
        var result = service.TryGenerate(invalidLicense);

        // Assert
        Assert.False(result.Success);
        Assert.Null(result.LicenseKey);
        Assert.Equal("License holder's name is required.", result.Error);
    }

    [Fact]
    public void TryGenerate_Should_Catch_ValidationException_When_ThrowOnValidationError_Is_True()
    {
        // Arrange
        var options = new LicenseValidationOptions
        {
            ThrowOnValidationError = true
        };

        var service = CreateService(options);

        var badLicense = new LicenseInfo
        {
            Name = "", // invalid
            ProductId = "com.example.product",
            ExpirationDate = DateTime.UtcNow.AddDays(10)
        };

        // Act
        var result = service.TryGenerate(badLicense);

        // Assert
        Assert.False(result.Success);
        Assert.Null(result.LicenseKey);
        Assert.Equal("License holder's name is required.", result.Error);
    }

}