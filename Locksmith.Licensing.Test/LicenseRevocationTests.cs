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

    private LicenseInfo CreateLicense(out Guid licenseId)
    {
        var license = new LicenseInfo
        {
            Name = "Revoked User",
            ProductId = "revoked-product",
            ExpirationDate = DateTime.UtcNow.AddDays(5)
        };

        licenseId = license.LicenseId;
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
        Assert.Equal("License has been revoked.", result.Error);
    }

    [Fact]
    public void Validate_Should_Succeed_When_License_Is_Not_Revoked()
    {
        var license = CreateLicense(out var licenseId);
        var service = CreateService(new[] { Guid.NewGuid() }); // unrelated revoked ID

        var key = service.Generate(license);
        var result = service.Validate(key);

        Assert.True(result.IsValid);
        Assert.Equal(licenseId, result.LicenseInfo.LicenseId);
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