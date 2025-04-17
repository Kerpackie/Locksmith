using Locksmith.Core.Config;
using Locksmith.Core.Revocation;
using Locksmith.Core.Security;
using Locksmith.Core.Services;
using Locksmith.Core.Validation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Locksmith.Core.DependencyInjection;

/// <summary>
/// Provides extension methods for configuring Locksmith services in an <see cref="IServiceCollection"/>.
/// </summary>
public static class LocksmithServiceCollectionExtensions
{
    /// <summary>
    /// Adds Locksmith services to the specified <see cref="IServiceCollection"/>.
    /// </summary>
    /// <param name="services">The service collection to which Locksmith services will be added.</param>
    /// <param name="configure">An action to configure the <see cref="LocksmithOptionsBuilder"/>.</param>
    /// <returns>The updated <see cref="IServiceCollection"/> with Locksmith services added.</returns>
    /// <example>
    /// Example usage:
    /// <code>
    /// services.AddLocksmith(locksmith =>
    /// {
    ///     locksmith
    ///         .UseSecretProvider(new InMemoryRotatingSecretProvider("my-current", new[] { "old-1", "old-2" }))
    ///         .ConfigureValidationOptions(options =>
    ///         {
    ///             options.ThrowOnValidationError = true;
    ///             options.ClockSkew = TimeSpan.FromMinutes(10);
    ///         });
    /// });
    /// </code>
    /// </example>
    public static IServiceCollection AddLocksmith(
        this IServiceCollection services,
        Action<LocksmithOptionsBuilder> configure)
    {
        var builder = new LocksmithOptionsBuilder(services);
        configure(builder);

        services.Configure(builder.ValidationOptions ?? (_ => { }));

        if (builder.SecretProvider == null)
            throw new InvalidOperationException("A secret provider must be configured. Use `UseSecretProvider(...)`.");
        services.AddSingleton<ISecretProvider>(builder.SecretProvider);

        services.AddSingleton<ILicenseValidator>(sp =>
        {
            var opts = sp.GetRequiredService<IOptions<LicenseValidationOptions>>().Value;
            return new DefaultLicenseValidator(opts);
        });

        if (builder.RevocationProvider != null)
            services.AddSingleton(builder.RevocationProvider);

        services.AddTransient<LicenseKeyService>(sp =>
        {
            var secretProvider = sp.GetRequiredService<ISecretProvider>();
            var options = sp.GetRequiredService<IOptions<LicenseValidationOptions>>().Value;
            var validator = sp.GetRequiredService<ILicenseValidator>();
            var revocationProvider = sp.GetService<ILicenseRevocationProvider>();

            return new LicenseKeyService(secretProvider, options, validator, revocationProvider);
        });

        return services;
    }
}