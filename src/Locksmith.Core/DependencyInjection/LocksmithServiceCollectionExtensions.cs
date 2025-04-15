using Locksmith.Core.Config;
using Locksmith.Core.Security;
using Locksmith.Core.Services;
using Locksmith.Core.Validation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Locksmith.Core.DependencyInjection;

/// <example>
/// services.AddLocksmith(locksmith =>
///{
///    locksmith
///        .UseSecretProvider(new InMemoryRotatingSecretProvider("my-current", new[] { "old-1", "old-2" }))
///        .ConfigureValidationOptions(options =>
///        {
///            options.ThrowOnValidationError = true;
///            options.ClockSkew = TimeSpan.FromMinutes(10);
///        });
///});
/// </example>
public static class LocksmithServiceCollectionExtensions
{
    public static IServiceCollection AddLocksmith(
        this IServiceCollection services,
        Action<LocksmithOptionsBuilder> configure)
    {
        var builder = new LocksmithOptionsBuilder(services);
        configure(builder);

        services.Configure(builder.ValidationOptions ?? (_ => { }));

        services.AddSingleton<ISecretProvider>(builder.SecretProvider ??
                                               new InMemoryRotatingSecretProvider("default-secret"));

        services.AddSingleton<ILicenseValidator>(sp =>
        {
            var opts = sp.GetRequiredService<IOptions<LicenseValidationOptions>>().Value;
            return new DefaultLicenseValidator(opts);
        });

        services.AddTransient<LicenseKeyService>();

        return services;
    }
}