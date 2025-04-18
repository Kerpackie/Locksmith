using Locksmith.Licensing.Models;
using Locksmith.Licensing.Revocation;
using Locksmith.Licensing.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Locksmith.Licensing.Test;

public class LicenseRevocationTests : TestBase
{
    private LicenseKeyService CreateService(IEnumerable<Guid> revokedIds)
    {
        var service = BuildServiceProvider(
            overrideSecretProvider: CreateFakeSecretProvider(),
            configureOptions: options =>
            {
                options.ValidateLicenseFields = false;
                options.ThrowOnValidationError = false;
            },
            overrideRevocationProvider: new ListRevocationProvider(revokedIds)
        );

        return service.GetRequiredService<LicenseKeyService>();
    }

    private LicenseDescriptor CreateLicense(out Guid licenseId)
    {
        var license = new LicenseDescriptor
        {
            Name = "Revoked User",
            ProductId = "revoked-product",
            Expiration = DateTime.UtcNow.AddDays(5)
        };

        licenseId = license.KeyId;
        return license;
    }

    [Fact]
    public void Validate_Should_Fail_When_License_Is_Revoked()
    {
        var license = CreateLicense(out var licenseId);
        var service = CreateService(new[] { licenseId });

        var key = service.Generate(license);
        var result = service.Validate(key);

        Assert.False(result.IsValid);
        Assert.Equal("Key has been revoked.", result.Error);
    }

    [Fact]
    public void Validate_Should_Succeed_When_License_Is_Not_Revoked()
    {
        var license = CreateLicense(out var licenseId);
        var service = CreateService(new[] { Guid.NewGuid() }); // unrelated revoked ID

        var key = service.Generate(license);
        var result = service.Validate(key);

        Assert.True(result.IsValid);
        Assert.Equal(licenseId, result.Key.KeyId);
    }

    [Fact]
    public void Validate_Should_Succeed_When_RevocationProvider_Is_Not_Used()
    {
        var license = CreateLicense(out var licenseId);

        var provider = BuildServiceProvider(
            overrideSecretProvider: CreateFakeSecretProvider(),
            configureOptions: opts => opts.ValidateLicenseFields = false);

        var service = provider.GetRequiredService<LicenseKeyService>();

        var key = service.Generate(license);
        var result = service.Validate(key);

        Assert.True(result.IsValid);
    }
}