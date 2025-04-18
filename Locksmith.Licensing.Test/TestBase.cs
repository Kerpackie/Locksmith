using System.Text;
using Locksmith.Core.Models;
using Locksmith.Core.Revocation;
using Locksmith.Core.Security;
using Locksmith.Core.Validation;
using Locksmith.Licensing.Config;
using Locksmith.Licensing.DependencyInjection;
using Locksmith.Licensing.Models;
using Locksmith.Licensing.Revocation;
using Locksmith.Licensing.Services;
using Locksmith.Licensing.Validation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace Locksmith.Licensing.Test;

public abstract class TestBase
{
    protected const string DefaultSecret = "Shhhh!SuperSecretKey123DontTellAnyone!";

    protected ServiceProvider BuildServiceProvider(
    string currentSecret = DefaultSecret,
    IEnumerable<string>? additionalSecrets = null,
    Action<LicenseValidationOptions>? configureOptions = null,
    ISecretProvider? overrideSecretProvider = null,
    IKeyValidator<LicenseDescriptor>? overrideValidator = null,
    ILicenseRevocationProvider? overrideRevocationProvider = null)
{
    var services = new ServiceCollection();

    // configure options
    if (configureOptions != null)
        services.Configure(configureOptions);
    else
        services.Configure<LicenseValidationOptions>(_ => { });

    services.AddSingleton(sp =>
        sp.GetRequiredService<IOptions<LicenseValidationOptions>>().Value);

    // secret provider
    if (overrideSecretProvider != null)
    {
        services.AddSingleton(overrideSecretProvider);
    }
    else
    {
        services.AddSingleton<ISecretProvider>(
            new DefaultSecretProvider(currentSecret, additionalSecrets));
    }

    // validator: register under both ILicenseValidator and IKeyValidator<LicenseDescriptor>
    if (overrideValidator != null)
    {
        services.AddSingleton<IKeyValidator<LicenseDescriptor>>(overrideValidator);
        services.AddSingleton<ILicenseValidator>(sp =>
            (ILicenseValidator)sp.GetRequiredService<IKeyValidator<LicenseDescriptor>>());
    }
    else
    {
        services.AddSingleton<ILicenseValidator>(sp =>
            new DefaultLicenseValidator(
                sp.GetRequiredService<IOptions<LicenseValidationOptions>>().Value
            )
        );
        services.AddSingleton<IKeyValidator<LicenseDescriptor>>(sp =>
            sp.GetRequiredService<ILicenseValidator>());
    }

    // revocation provider: register under both ILicenseRevocationProvider and IKeyRevocationProvider<LicenseDescriptor>
    if (overrideRevocationProvider != null)
    {
        services.AddSingleton<ILicenseRevocationProvider>(overrideRevocationProvider);
        services.AddSingleton<IKeyRevocationProvider<LicenseDescriptor>>(sp =>
            sp.GetRequiredService<ILicenseRevocationProvider>());
    }

    // finally, the service itself
    services.AddTransient<LicenseKeyService>();

    return services.BuildServiceProvider();
}



    protected LicenseDescriptor CreateTestLicense(DateTime? expiration = null)
    {
        return new LicenseDescriptor
        {
            Name = "Alice Example",
            ProductId = "com.example.product",
            Expiration = expiration
        };
    }

    protected FakeSecretProvider CreateFakeSecretProvider(string current = "fake-secret", params string[]? additional)
    {
        var provider = new FakeSecretProvider
        {
            CurrentSecret = Encoding.UTF8.GetBytes(current)
        };

        if (additional != null)
        {
            provider.ValidationSecrets.Clear();
            provider.ValidationSecrets.AddRange(additional.Select(s => Encoding.UTF8.GetBytes(s)));
            provider.ValidationSecrets.Add(provider.CurrentSecret); // Include current
        }

        return provider;
    }

    protected FakeLicenseValidator CreateFakeValidator(string? failWithMessage = null)
    {
        return new FakeLicenseValidator { ForcedErrorMessage = failWithMessage };
    }
}
