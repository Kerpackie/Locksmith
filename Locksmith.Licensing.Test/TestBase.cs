using System.Text;
using Locksmith.Core.Models;
using Locksmith.Core.Revocation;
using Locksmith.Core.Security;
using Locksmith.Core.Validation;
using Locksmith.Licensing.Config;
using Locksmith.Licensing.Revocation;
using Locksmith.Licensing.Validation;
using Microsoft.Extensions.DependencyInjection;
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
        ILicenseValidator? overrideValidator = null,
        ILicenseRevocationProvider? overrideRevocationProvider = null)
    {
        var services = new ServiceCollection();

        if (configureOptions != null)
            services.Configure(configureOptions);
        else
            services.Configure<LicenseValidationOptions>(_ => { });

        services.AddSingleton(sp =>
            sp.GetRequiredService<IOptions<LicenseValidationOptions>>().Value);

        if (overrideSecretProvider != null)
        {
            services.AddSingleton(overrideSecretProvider);
        }
        else
        {
            services.AddSingleton<ISecretProvider>(
                new DefaultSecretProvider(currentSecret, additionalSecrets));
        }

        if (overrideValidator != null)
        {
            services.AddSingleton(overrideValidator);
        }
        else
        {
            services.AddSingleton<ILicenseValidator>(sp =>
            {
                var opts = sp.GetRequiredService<IOptions<LicenseValidationOptions>>().Value;
                return new DefaultLicenseValidator(opts);
            });
        }

        if (overrideRevocationProvider != null)
        {
            services.AddSingleton(overrideRevocationProvider);
        }

        services.AddTransient<LicenseKeyService>();
        return services.BuildServiceProvider();
    }

    protected LicenseInfo CreateTestLicense(DateTime? expiration = null)
    {
        return new LicenseInfo
        {
            Name = "Alice Example",
            ProductId = "com.example.product",
            ExpirationDate = expiration
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