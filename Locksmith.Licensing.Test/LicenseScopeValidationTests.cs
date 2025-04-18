using Locksmith.Licensing.Config;
using Locksmith.Licensing.Models;
using Locksmith.Licensing.Services;
using Locksmith.Licensing.Validation;
using Microsoft.Extensions.DependencyInjection;

namespace Locksmith.Licensing.Test;

public class LicenseScopeValidationTests : TestBase
{
    private LicenseKeyService CreateService(IEnumerable<string> requiredScopes, bool enforceScopes = true)
    {
        return BuildServiceProvider(
            configureOptions: opts =>
            {
                opts.EnforceScopes = enforceScopes;
                opts.RequiredScopes = requiredScopes.ToList();
            })
            .GetRequiredService<LicenseKeyService>();
    }

    private LicenseDescriptor CreateLicense(IEnumerable<string>? scopes = null)
    {
        return new LicenseDescriptor
        {
            Name = "Scoped User",
            ProductId = "scoped-product",
            Expiration = DateTime.UtcNow.AddDays(10),
            Scopes = scopes?.ToList()
        };
    }

    [Fact]
    public void License_With_All_Required_Scopes_Should_Pass()
    {
        var service = CreateService(new[] { "feature:export", "tier:pro" });
        var license = CreateLicense(new[] { "feature:export", "tier:pro", "env:prod" });
        
        var result = service.TryGenerate(license);
        Assert.True(result.Success);

        var validated = service.Validate(result.EncodedKey);
        Assert.True(validated.IsValid);
    }

    [Fact]
    public void License_Missing_Required_Scope_Should_Fail()
    {
        var genService = BuildServiceProvider(
            overrideValidator: new DefaultLicenseValidator(new LicenseValidationOptions()),
            configureOptions: opts =>
            {
                opts.EnforceScopes = true;
                opts.RequiredScopes = new[] { "feature:export", "tier:pro" }.ToList();
                opts.ThrowOnValidationError = false;
                opts.ValidateLicenseFields = false; 
            }).GetRequiredService<LicenseKeyService>();
        
        var service = CreateService(new []{ "feature:export", "tier:pro" });
        var license = CreateLicense(new[] { "feature:export" });

        var result = genService.TryGenerate(license);
        Assert.True(result.Success);
        
        var validated = service.Validate(result.EncodedKey);
        Assert.False(validated.IsValid);
        Assert.Equal("Required license scopes not present.", validated.Error);
    }
    
    [Fact]
    public void Scope_Validation_Should_Be_Skipped_When_Disabled()
    {
        var service = CreateService(new[] { "feature:export" }, enforceScopes: false);
        var license = CreateLicense(new[] { "something:else" });

        var result = service.TryGenerate(license);
        Assert.True(result.Success);

        var validated = service.Validate(result.EncodedKey!);
        Assert.True(validated.IsValid);
    }

    [Fact]
    public void License_With_No_Scopes_Should_Fail_If_Required()
    {
        var genService = BuildServiceProvider(
            configureOptions: opts =>
            {
                opts.EnforceScopes = true;
                opts.RequiredScopes = new[] { "feature:export", "tier:pro" }.ToList();
                opts.ThrowOnValidationError = false;
                opts.ValidateLicenseFields = false; 
            }).GetRequiredService<LicenseKeyService>();
        
        var service = CreateService(new[] { "feature:premium" });
        var license = CreateLicense(null);

        var result = genService.TryGenerate(license);
        Assert.True(result.Success);

        var validated = service.Validate(result.EncodedKey!);
        Assert.False(validated.IsValid);
        Assert.Equal("Required license scopes not present.", validated.Error);
    }

    [Fact]
    public void License_With_No_Required_Scopes_Should_Always_Pass()
    {
        var service = CreateService(new string[0]);
        var license = CreateLicense(new[] { "any:scope" });

        var result = service.TryGenerate(license);
        Assert.True(result.Success);

        var validated = service.Validate(result.EncodedKey!);
        Assert.True(validated.IsValid);
    }
    
}