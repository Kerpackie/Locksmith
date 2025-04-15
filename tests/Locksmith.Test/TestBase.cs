using System;
using System.Collections.Generic;
using System.Text;
using Locksmith.Core.Config;
using Locksmith.Core.Machine;
using Locksmith.Core.Models;
using Locksmith.Core.Security;
using Locksmith.Core.Services;
using Locksmith.Core.Validation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Locksmith.Test;

public abstract class TestBase
{
    protected const string DefaultSecret = "Shhhh!SuperSecretKey123DontTellAnyone!";

    protected ServiceProvider BuildServiceProvider(
        string currentSecret = DefaultSecret,
        IEnumerable<string>? additionalSecrets = null,
        Action<LicenseValidationOptions>? configureOptions = null,
        ISecretProvider? overrideSecretProvider = null,
        ILicenseValidator? overrideValidator = null)
    {
        var services = new ServiceCollection();

        if (configureOptions != null)
            services.Configure(configureOptions);
        else
            services.Configure<LicenseValidationOptions>(_ => { });

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
        
        if (!services.Any(sd => sd.ServiceType == typeof(IMachineFingerprintProvider)))
        {
            services.AddSingleton<IMachineFingerprintProvider>(new DefaultMachineFingerprintProvider());
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
