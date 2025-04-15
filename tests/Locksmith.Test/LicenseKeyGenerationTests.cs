using Locksmith.Core.Exceptions;
using Locksmith.Core.Models;
using Locksmith.Core.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Locksmith.Test;


public class LicenseKeyGenerationTests : TestBase
{
    [Fact]
    public void Generate_Should_Throw_When_Validation_Fails_And_ThrowOnValidationError_Is_True()
    {
        var service = BuildServiceProvider(configureOptions: o => o.ThrowOnValidationError = true)
            .GetRequiredService<LicenseKeyService>();

        var invalidLicense = new LicenseInfo { Name = "", ProductId = "com.example.product", ExpirationDate = DateTime.UtcNow.AddDays(10) };

        Assert.Throws<LicenseValidationException>(() => service.Generate(invalidLicense));
    }

    [Fact]
    public void TryGenerate_Should_Succeed_For_ValidLicense()
    {
        var service = BuildServiceProvider().GetRequiredService<LicenseKeyService>();
        var license = CreateTestLicense(DateTime.UtcNow.AddDays(5));

        var result = service.TryGenerate(license);

        Assert.True(result.Success);
        Assert.NotNull(result.LicenseKey);
        Assert.Null(result.Error);
    }

    [Fact]
    public void TryGenerate_Should_Return_Error_For_InvalidLicenseData()
    {
        var fakeValidator = CreateFakeValidator("Name is required");
        var service = BuildServiceProvider(overrideValidator: fakeValidator).GetRequiredService<LicenseKeyService>();

        var invalidLicense = new LicenseInfo { Name = "", ProductId = "com.example.product", ExpirationDate = DateTime.UtcNow.AddDays(10) };
        var result = service.TryGenerate(invalidLicense);

        Assert.False(result.Success);
        Assert.Null(result.LicenseKey);
        Assert.Equal("Name is required", result.Error);
    }

    [Fact]
    public void TryGenerate_Should_Catch_ValidationException_When_ThrowOnValidationError_Is_True()
    {
        var fakeValidator = CreateFakeValidator("field check failed");
        var service = BuildServiceProvider(overrideValidator: fakeValidator).GetRequiredService<LicenseKeyService>();

        var license = CreateTestLicense(DateTime.UtcNow.AddDays(10));
        var result = service.TryGenerate(license);

        Assert.False(result.Success);
        Assert.Equal("field check failed", result.Error);
    }
}