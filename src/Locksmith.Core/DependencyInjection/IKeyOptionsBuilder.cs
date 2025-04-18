using Locksmith.Core.Config;
using Locksmith.Core.Models;
using Locksmith.Core.Revocation;
using Locksmith.Core.Security;
using Microsoft.Extensions.DependencyInjection;

namespace Locksmith.Core.DependencyInjection;

public interface IKeyOptionsBuilder<TOptions>
    where TOptions : KeyServiceOptions
{
    IServiceCollection Services { get; }

    IKeyOptionsBuilder<TOptions> UseSecretProvider(ISecretProvider provider);

    IKeyOptionsBuilder<TOptions> ConfigureValidationOptions(Action<TOptions> configure);

    IKeyOptionsBuilder<TOptions> EnforceLimitValidation(bool enabled = true);

    IKeyOptionsBuilder<TOptions> UseRevocationProvider<TDescriptor>(IKeyRevocationProvider<TDescriptor> provider)
        where TDescriptor : KeyDescriptor;

}