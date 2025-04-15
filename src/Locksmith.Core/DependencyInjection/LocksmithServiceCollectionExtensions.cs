using Locksmith.Core.Config;
using Locksmith.Core.Machine;
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
        // Create a new LocksmithOptionsBuilder instance to configure Locksmith options.
        var builder = new LocksmithOptionsBuilder(services);
        configure(builder);

        // Configure license validation options if provided.
        services.Configure(builder.ValidationOptions ?? (_ => { }));

        // Add the secret provider to the service collection, using a default implementation if none is provided.
        services.AddSingleton<ISecretProvider>(builder.SecretProvider ??
                                               new InMemoryRotatingSecretProvider("default-secret"));

        // Add the license validator to the service collection.
        services.AddSingleton<ILicenseValidator>(sp =>
        {
            var opts = sp.GetRequiredService<IOptions<LicenseValidationOptions>>().Value;
            var fingerprint = sp.GetRequiredService<IMachineFingerprintProvider>();
            return new DefaultLicenseValidator(opts, fingerprint);
        });

        // Add the LicenseKeyService to the service collection as a transient service.
        services.AddTransient<LicenseKeyService>();

        return services;
    }
}