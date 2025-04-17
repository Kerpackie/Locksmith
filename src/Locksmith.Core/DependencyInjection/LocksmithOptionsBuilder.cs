using Locksmith.Core.Config;
using Locksmith.Core.Security;
using Microsoft.Extensions.DependencyInjection;

namespace Locksmith.Core.DependencyInjection;

/// <summary>
/// Provides a builder for configuring Locksmith options, such as secret providers
/// and license validation options, during dependency injection setup.
/// </summary>
public class LocksmithOptionsBuilder
{
    /// <summary>
    /// Gets the service collection to which Locksmith services are added.
    /// </summary>
    public IServiceCollection Services { get; }

    /// <summary>
    /// Gets or sets the action used to configure license validation options.
    /// This is an internal property and is not directly accessible outside the class.
    /// </summary>
    internal Action<LicenseValidationOptions>? ValidationOptions { get; private set; }

    /// <summary>
    /// Gets or sets the secret provider used for license key operations.
    /// This is an internal property and is not directly accessible outside the class.
    /// </summary>
    internal ISecretProvider? SecretProvider { get; private set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="LocksmithOptionsBuilder"/> class
    /// with the specified service collection.
    /// </summary>
    /// <param name="services">The service collection to which Locksmith services will be added.</param>
    public LocksmithOptionsBuilder(IServiceCollection services)
    {
        Services = services;
    }

    /// <summary>
    /// Configures the secret provider to be used for license key operations.
    /// </summary>
    /// <param name="provider">The secret provider implementation to use.</param>
    /// <returns>The current <see cref="LocksmithOptionsBuilder"/> instance for method chaining.</returns>
    public LocksmithOptionsBuilder UseSecretProvider(ISecretProvider provider)
    {
        SecretProvider = provider;
        return this;
    }

    /// <summary>
    /// Configures the license validation options using the specified configuration action.
    /// </summary>
    /// <param name="configure">An action to configure the <see cref="LicenseValidationOptions"/>.</param>
    /// <returns>The current <see cref="LocksmithOptionsBuilder"/> instance for method chaining.</returns>
    public LocksmithOptionsBuilder ConfigureValidationOptions(Action<LicenseValidationOptions> configure)
    {
        ValidationOptions = configure;
        return this;
    }
    
    /// <summary>
    /// Configures the required license scopes for validation and enforces their presence.
    /// </summary>
    /// <param name="scopes">An array of required license scopes.</param>
    /// <returns>The current <see cref="LocksmithOptionsBuilder"/> instance for method chaining.</returns>
    public LocksmithOptionsBuilder RequireScopes(params string[] scopes)
    {
        ConfigureValidationOptions(opt =>
        {
            opt.EnforceScopes = true;
            opt.RequiredScopes = scopes.ToList();
        });

        return this;
    }
    
    /// <summary>
    /// Configures whether license limit validation should be enforced.
    /// </summary>
    /// <param name="enabled">
    /// A boolean value indicating whether to enable or disable limit validation.
    /// Defaults to <c>true</c>.
    /// </param>
    /// <returns>
    /// The current <see cref="LocksmithOptionsBuilder"/> instance for method chaining.
    /// </returns>
    public LocksmithOptionsBuilder EnforceLimitValidation(bool enabled = true)
    {
        return ConfigureValidationOptions(opts =>
        {
            opts.EnforceLimitValidation = enabled;
        });
    }
}