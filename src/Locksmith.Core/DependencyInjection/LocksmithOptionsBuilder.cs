using Locksmith.Core.Config;
using Locksmith.Core.Revocation;
using Locksmith.Core.Security;
using Locksmith.Core.Services;
using Locksmith.Core.Validation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

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
    /// </summary>
    internal Action<LicenseValidationOptions>? ValidationOptions { get; private set; }

    /// <summary>
    /// Gets or sets the secret provider used for license security.
    /// </summary>
    internal ISecretProvider? SecretProvider { get; private set; }

    /// <summary>
    /// Gets or sets the license validator used to validate licenses.
    /// </summary>
    internal ILicenseValidator? Validator { get; private set; }

    /// <summary>
    /// Gets or sets the license revocation provider used to handle license revocation.
    /// </summary>
    internal ILicenseRevocationProvider? RevocationProvider { get; private set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="LocksmithOptionsBuilder"/> class.
    /// </summary>
    /// <param name="services">The service collection to which Locksmith services will be added.</param>
    public LocksmithOptionsBuilder(IServiceCollection services)
    {
        Services = services;
    }

    /// <summary>
    /// Configures the secret provider to be used for license security.
    /// </summary>
    /// <param name="provider">The secret provider to use.</param>
    /// <returns>The current <see cref="LocksmithOptionsBuilder"/> instance.</returns>
    public LocksmithOptionsBuilder UseSecretProvider(ISecretProvider provider)
    {
        SecretProvider = provider;
        return this;
    }

    /// <summary>
    /// Configures the license validator to be used for license validation.
    /// </summary>
    /// <param name="validator">The license validator to use.</param>
    /// <returns>The current <see cref="LocksmithOptionsBuilder"/> instance.</returns>
    public LocksmithOptionsBuilder UseLicenseValidator(ILicenseValidator validator)
    {
        Validator = validator;
        return this;
    }

    /// <summary>
    /// Configures the license revocation provider to be used for handling license revocation.
    /// </summary>
    /// <param name="provider">The license revocation provider to use.</param>
    /// <returns>The current <see cref="LocksmithOptionsBuilder"/> instance.</returns>
    public LocksmithOptionsBuilder UseRevocationProvider(ILicenseRevocationProvider provider)
    {
        RevocationProvider = provider;
        return this;
    }

    /// <summary>
    /// Configures the license validation options.
    /// </summary>
    /// <param name="configure">An action to configure the <see cref="LicenseValidationOptions"/>.</param>
    /// <returns>The current <see cref="LocksmithOptionsBuilder"/> instance.</returns>
    public LocksmithOptionsBuilder ConfigureValidationOptions(Action<LicenseValidationOptions> configure)
    {
        ValidationOptions = configure;
        return this;
    }

    /// <summary>
    /// Configures the required scopes for license validation.
    /// </summary>
    /// <param name="scopes">The scopes to require.</param>
    /// <returns>The current <see cref="LocksmithOptionsBuilder"/> instance.</returns>
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
    /// Configures whether limit validation should be enforced.
    /// </summary>
    /// <param name="enabled">A value indicating whether limit validation should be enforced. Defaults to <c>true</c>.</param>
    /// <returns>The current <see cref="LocksmithOptionsBuilder"/> instance.</returns>
    public LocksmithOptionsBuilder EnforceLimitValidation(bool enabled = true)
    {
        return ConfigureValidationOptions(opts =>
        {
            opts.EnforceLimitValidation = enabled;
        });
    }
}