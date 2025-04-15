using Locksmith.Core.Config;
using Locksmith.Core.Security;
using Microsoft.Extensions.DependencyInjection;

namespace Locksmith.Core.DependencyInjection;

public class LocksmithOptionsBuilder
{
    public IServiceCollection Services { get; }

    internal Action<LicenseValidationOptions>? ValidationOptions { get; private set; }
    internal ISecretProvider? SecretProvider { get; private set; }

    public LocksmithOptionsBuilder(IServiceCollection services)
    {
        Services = services;
    }

    public LocksmithOptionsBuilder UseSecretProvider(ISecretProvider provider)
    {
        SecretProvider = provider;
        return this;
    }

    public LocksmithOptionsBuilder ConfigureValidationOptions(Action<LicenseValidationOptions> configure)
    {
        ValidationOptions = configure;
        return this;
    }
}