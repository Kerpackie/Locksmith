using Locksmith.Core.Security;
using Locksmith.Licensing.Models;
using Locksmith.Licensing.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Locksmith.Licensing;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddLicensing(this IServiceCollection services)
    {
        services.AddTransient<LicenseKeyService>();
        return services;
    }

    public static IServiceCollection AddLicensing(this IServiceCollection services, ISecretProvider secretProvider)
    {
        services.AddSingleton(secretProvider);
        services.AddLicensing();
        return services;
    }
    
    public static IServiceCollection AddLicensingWithDefaults(this IServiceCollection services, string secret)
    {
        services.AddSingleton<ISecretProvider>(new DefaultSecretProvider(secret));
        return services.AddLicensing();
    }

}