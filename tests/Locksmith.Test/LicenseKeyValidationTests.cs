using Locksmith.Core.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Locksmith.Test;

public class LicenseKeyValidationTests : TestBase
{
    [Fact]
    public void Validate_Should_Fail_For_TamperedKey()
    {
        var service = BuildServiceProvider().GetRequiredService<LicenseKeyService>();
        var key = service.Generate(CreateTestLicense(DateTime.UtcNow.AddDays(10)));
        var tamperedKey = key.Substring(0, key.Length - 2) + "xx";

        var result = service.Validate(tamperedKey);

        Assert.False(result.IsValid);
        Assert.Equal("Invalid signature.", result.Error);
    }

    [Fact]
    public void Validate_Should_Fail_For_InvalidBase58()
    {
        var service = BuildServiceProvider().GetRequiredService<LicenseKeyService>();
        var result = service.Validate("!!bad_key!!");

        Assert.False(result.IsValid);
        Assert.True(result.IsMalformed);
    }

    [Fact]
    public void Validate_Should_Fail_For_ExpiredLicence()
    {
        var service = BuildServiceProvider(configureOptions: o => o.ValidateLicenseFields = false)
            .GetRequiredService<LicenseKeyService>();

        var key = service.Generate(CreateTestLicense(DateTime.UtcNow.AddDays(-1)));
        var result = service.Validate(key);

        Assert.False(result.IsValid);
        Assert.True(result.IsExpired);
        Assert.Equal("License has expired.", result.Error);
    }

    [Fact]
    public void ValidationResult_Should_Set_IsExpired_Flag_Correctly()
    {
        var service = BuildServiceProvider(configureOptions: o => o.ValidateLicenseFields = false)
            .GetRequiredService<LicenseKeyService>();

        var key = service.Generate(CreateTestLicense(DateTime.UtcNow.AddDays(-5)));
        var result = service.Validate(key);

        Assert.False(result.IsValid);
        Assert.True(result.IsExpired);
    }

    [Fact]
    public void ValidationResult_Should_Set_IsTampered_Flag_Correctly()
    {
        var service = BuildServiceProvider().GetRequiredService<LicenseKeyService>();
        var key = service.Generate(CreateTestLicense(DateTime.UtcNow.AddDays(5)));
        var tamperedKey = key.Substring(0, key.Length - 2) + "ZZ";

        var result = service.Validate(tamperedKey);

        Assert.False(result.IsValid);
        Assert.True(result.IsTampered);
    }

    [Fact]
    public void ValidationResult_Should_Set_IsMalformed_Flag_When_Decode_Fails()
    {
        var service = BuildServiceProvider().GetRequiredService<LicenseKeyService>();
        var result = service.Validate("$$$notvalidbase58###");

        Assert.False(result.IsValid);
        Assert.True(result.IsMalformed);
        Assert.Null(result.LicenseInfo);
    }
    
    [Fact]
    public void TryGenerate_Should_Return_Error_If_Validator_Fails()
    {
        var fakeValidator = CreateFakeValidator("forced failure");

        var service = BuildServiceProvider(overrideValidator: fakeValidator)
            .GetRequiredService<LicenseKeyService>();

        var license = CreateTestLicense();
        var result = service.TryGenerate(license);

        Assert.False(result.Success);
        Assert.Equal("forced failure", result.Error);
    }
    
    [Fact]
    public void Generate_Should_Invoke_Validator()
    {
        var fakeValidator = CreateFakeValidator();
        var service = BuildServiceProvider(overrideValidator: fakeValidator)
            .GetRequiredService<LicenseKeyService>();

        var license = CreateTestLicense();
        var key = service.Generate(license);

        Assert.Single(fakeValidator.ValidatedLicenses);
        Assert.Equal("Alice Example", fakeValidator.ValidatedLicenses[0].Name);
    }
}