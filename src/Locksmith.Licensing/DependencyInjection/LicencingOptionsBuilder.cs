using Locksmith.Core.DependencyInjection;
using Locksmith.Core.Models;
using Locksmith.Core.Revocation;
using Locksmith.Core.Security;
using Locksmith.Licensing.Config;
using Locksmith.Licensing.Models;
using Locksmith.Licensing.Revocation;
using Locksmith.Licensing.Validation;
using Microsoft.Extensions.DependencyInjection;

namespace Locksmith.Licensing.DependencyInjection;

public class LicenseOptionsBuilder : IKeyOptionsBuilder<LicenseValidationOptions>
{
    public IServiceCollection Services { get; }

    internal Action<LicenseValidationOptions>? ValidationOptions { get; private set; }
    internal ISecretProvider? SecretProvider { get; private set; }
    internal IKeyRevocationProvider<LicenseDescriptor>? RevocationProvider { get; private set; }

    public LicenseOptionsBuilder(IServiceCollection services)
    {
        Services = services;
    }

    public IKeyOptionsBuilder<LicenseValidationOptions> UseSecretProvider(ISecretProvider provider)
    {
        SecretProvider = provider;
        return this;
    }

    public IKeyOptionsBuilder<LicenseValidationOptions> ConfigureValidationOptions(Action<LicenseValidationOptions> configure)
    {
        ValidationOptions = configure;
        return this;
    }

    public IKeyOptionsBuilder<LicenseValidationOptions> EnforceLimitValidation(bool enabled = true)
    {
        return ConfigureValidationOptions(opt => opt.EnforceLimitValidation = enabled);
    }

    public IKeyOptionsBuilder<LicenseValidationOptions> UseRevocationProvider<TDescriptor>(IKeyRevocationProvider<TDescriptor> provider)
        where TDescriptor : KeyDescriptor
    {
        if (provider is IKeyRevocationProvider<LicenseDescriptor> licenseRevocationProvider)
        {
            RevocationProvider = licenseRevocationProvider;
        }
        return this;
    }
    
    public LicenseOptionsBuilder RequireScopes(params string[] scopes)
    {
        ConfigureValidationOptions(opt =>
        {
            opt.EnforceScopes = true;
            opt.RequiredScopes = scopes.ToList();
        });
        return this;
    }

}

