namespace Locksmith.Licensing.Test;

public class LicenseTypeValidationTests : TestBase
{
    private LicenseKeyService CreateService(bool enforceTypeRules = true)
    {
        var provider = BuildServiceProvider(
            configureOptions: opts => opts.EnforceLicenseTypeRules = enforceTypeRules);

        return provider.GetRequiredService<LicenseKeyService>();
    }

    private LicenseInfo CreateLicense(LicenseType type, DateTime? expiration = null)
    {
        return new LicenseInfo()
        {
            Name = "Type Test",
            ProductId = "type-product",
            Type = type,
            ExpirationDate = expiration
        };
    }

    [Fact]
    public void Trail_License_Must_Have_Expiration()
    {
        var service = CreateService();
        var license = CreateLicense(LicenseType.Trial);
        
        var result = service.TryGenerate(license);
        
        Assert.False(result.Success);
        Assert.Equal("Trial licenses must have an expiration date.", result.Error);
    }

    [Fact]
    public void Subscription_License_Must_Have_Expiration()
    {
        var service = CreateService();
        var license = CreateLicense(LicenseType.Subscription);

        var result = service.TryGenerate(license);
        
        Assert.False(result.Success);
        Assert.Equal("Subscription licenses must have an expiration date.", result.Error);
    }
    
    [Fact]
    public void Full_License_Can_Be_Without_Expiration()
    {
        var service = CreateService();
        var license = CreateLicense(LicenseType.Full);

        var result = service.TryGenerate(license);
        
        Assert.True(result.Success);
    }
    
    [Fact]
    public void OEM_License_Can_Be_Without_Expiration()
    {
        var service = CreateService();
        var license = CreateLicense(LicenseType.OEM);

        var result = service.TryGenerate(license);
        
        Assert.True(result.Success);
    }
    
    [Fact]
    public void Enterprise_License_Can_Be_Without_Expiration()
    {
        var service = CreateService();
        var license = CreateLicense(LicenseType.Enterprise);

        var result = service.TryGenerate(license);
        
        Assert.True(result.Success);
    }
    
    [Fact]
    public void Academic_License_Can_Be_Without_Expiration()
    {
        var service = CreateService();
        var license = CreateLicense(LicenseType.Academic);

        var result = service.TryGenerate(license);
        
        Assert.True(result.Success);
    }
    
    [Fact]
    public void Academic_License_With_Expiration_Should_Pass()
    {
        var service = CreateService();
        var license = CreateLicense(LicenseType.Academic, DateTime.UtcNow.AddDays(30));

        var result = service.TryGenerate(license);

        Assert.True(result.Success);
    }
    
    [Fact]
    public void Type_Rules_Should_Be_Skipped_If_Disabled()
    {
        var service = CreateService(enforceTypeRules: false);
        var license = CreateLicense(LicenseType.Trial); // no expiration, but validation disabled

        var result = service.TryGenerate(license);

        Assert.True(result.Success);
    }
}