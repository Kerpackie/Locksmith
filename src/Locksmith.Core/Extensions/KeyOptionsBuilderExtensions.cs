using Locksmith.Core.Config;
using Locksmith.Core.DependencyInjection;
using Locksmith.Core.Security;

namespace Locksmith.Core.Extensions;

/// <summary>
/// Provides extension methods for configuring <see cref="IKeyOptionsBuilder{TOptions}"/>.
/// </summary>
public static class KeyOptionsBuilderExtensions
{
    /// <summary>
    /// Configures the builder to use an in-memory test secret provider.
    /// </summary>
    /// <typeparam name="TOptions">The type of the key validation options.</typeparam>
    /// <param name="builder">The options builder.</param>
    /// <param name="secret">The test secret to use. Defaults to "test-secret".</param>
    /// <returns>The same builder instance for fluent configuration.</returns>
    public static IKeyOptionsBuilder<TOptions> UseTestSecret<TOptions>(
        this IKeyOptionsBuilder<TOptions> builder,
        string secret = "test-secret")
        where TOptions : KeyServiceOptions
    {
        return builder.UseSecretProvider(new InMemoryRotatingSecretProvider(secret));
    }
}