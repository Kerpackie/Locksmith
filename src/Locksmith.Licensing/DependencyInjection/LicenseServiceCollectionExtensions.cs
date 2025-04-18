using Locksmith.Core.Security;
using Locksmith.Licensing.Config;
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

        // Configure options
        services.Configure(builder.ValidationOptions ?? (_ => { }));

        // Provide LicenseValidationOptions directly if needed
        services.AddSingleton(sp => sp.GetRequiredService<IOptions<LicenseValidationOptions>>().Value);

        // Required: Secret provider
        if (builder.SecretProvider == null)
            throw new InvalidOperationException("A secret provider must be configured.");
        services.AddSingleton<ISecretProvider>(builder.SecretProvider);

        // Optional: Revocation
        if (builder.RevocationProvider != null)
            services.AddSingleton(builder.RevocationProvider);

        // Validator (or default)
        services.AddSingleton<ILicenseValidator>(sp =>
            builder.Validator ?? new DefaultLicenseValidator(
                sp.GetRequiredService<LicenseValidationOptions>()
            ));

        services.AddTransient<LicenseKeyService>();

        return services;
    }
}