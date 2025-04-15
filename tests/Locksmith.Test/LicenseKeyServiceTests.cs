using Locksmith.Core.Models;
using Locksmith.Core.Services;

namespace Locksmith.Test;

public class LicenseKeyServiceTests
{
    private const string SecretKey = "Shhhh!SuperSecretKey123DontTellAnyone!";

    private LicenseInfo CreateTestLicence(DateTime? expiration = null)
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
        var license = CreateTestLicence(DateTime.UtcNow.AddDays(10));
        
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
        var license = CreateTestLicence(DateTime.UtcNow.AddDays(10));
        
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
        var license = CreateTestLicence(DateTime.UtcNow.AddDays(-1)); // Expired licence
        
        // Act
        var key = service.Generate(license);
        var result = service.Validate(key);
        
        // Assert
        Assert.False(result.IsValid);
        Assert.Equal("License has expired.", result.Error);
    }

    [Fact]
    public void Validate_Should_Fail_For_InvalidBase64()
    {
        // Arrange
        var service = new LicenseKeyService(SecretKey);
        
        // Act
        var result = service.Validate("ThisIsn'tBase64?!");
        
        // Assert
        Assert.False(result.IsValid);
        Assert.Equal("Validation failed: The input is not a valid Base-64 string as it contains a non-base 64 character, more than two padding characters, or an illegal character among the padding characters.", result.Error);
    }
    
}