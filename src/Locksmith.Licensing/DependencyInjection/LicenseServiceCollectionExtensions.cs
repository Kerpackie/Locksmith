using Locksmith.Core.Revocation;
using Locksmith.Core.Security;
using Locksmith.Core.Validation;
using Locksmith.Licensing.Config;
using Locksmith.Licensing.Models;
using Locksmith.Licensing.Services;
using Locksmith.Licensing.Validation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Locksmith.Licensing.DependencyInjection;

public static class LicensingServiceCollectionExtensions
{
    public static IServiceCollection AddLicensing(this IServiceCollection services, Action<LicenseOptionsBuilder> configure)
    {
        var builder = new LicenseOptionsBuilder(services);
        configure(builder);

        // Configure LicenseValidationOptions
        services.Configure(builder.ValidationOptions ?? (_ => { }));

        // Required: Secret Provider
        if (builder.SecretProvider == null)
            throw new InvalidOperationException("A secret provider must be configured using UseSecretProvider(...)");
        services.AddSingleton<ISecretProvider>(builder.SecretProvider);

        // Optional: Revocation Provider
        if (builder.RevocationProvider != null)
            services.AddSingleton<IKeyRevocationProvider<LicenseDescriptor>>(builder.RevocationProvider);

        // Validator
        services.AddSingleton<ILicenseValidator>(sp =>
        {
            var options = sp.GetRequiredService<IOptions<LicenseValidationOptions>>().Value;
            var revocation = sp.GetService<IKeyRevocationProvider<LicenseDescriptor>>();
            return new DefaultLicenseValidator(options, revocation);
        });

        // License key service
        services.AddTransient<LicenseKeyService>();

        return services;
    }
}