using Locksmith.Core.DependencyInjection;
using Locksmith.Core.Security;

namespace Locksmith.Core.Extensions;

/// <summary>
/// Provides extension methods for configuring the <see cref="LocksmithOptionsBuilder"/>.
/// </summary>
public static class LocksmithOptionsBuilderExtensions
{
    /// <summary>
    /// Configures the <see cref="LocksmithOptionsBuilder"/> to use a test secret for license security.
    /// </summary>
    /// <param name="builder">The <see cref="LocksmithOptionsBuilder"/> to configure.</param>
    /// <param name="secret">The test secret to use. Defaults to "test-secret".</param>
    /// <returns>The configured <see cref="LocksmithOptionsBuilder"/> instance.</returns>
    public static LocksmithOptionsBuilder UseTestSecret(this LocksmithOptionsBuilder builder, string secret = "test-secret")
    {
        return builder.UseSecretProvider(new InMemoryRotatingSecretProvider(secret));
    }
}