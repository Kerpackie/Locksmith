using System.Text;
using Locksmith.Core.Config;
using Locksmith.Core.Models;
using Locksmith.Core.Security;
using Locksmith.Core.Services;
using Locksmith.Core.Tests.Models;
using Microsoft.Extensions.DependencyInjection;

namespace Locksmith.Core.Tests.Services;

public class KeyServiceTests
{
    private ServiceProvider BuildServiceProvider(TimeSpan? clockSkew = null)
    {
        var services = new ServiceCollection();

        // Configure KeyServiceOptions with optional clock skew
        services.AddOptions<KeyServiceOptions>();
        services.Configure<KeyServiceOptions>(opts =>
        {
            opts.ClockSkew = clockSkew ?? TimeSpan.FromMinutes(5);
        });

        // Register the secret provider
        services.AddSingleton<ISecretProvider>(sp =>
            new DefaultSecretProvider("test-secret"));

        // Register the KeyService for TestDescriptor
        services.AddTransient<KeyService<TestDescriptor>>();

        return services.BuildServiceProvider();
    }

    [Fact]
    public void GenerateAndValidate_ReturnsSuccess()
    {
        // Arrange: build DI container with default skew
        var sp = BuildServiceProvider();
        var keyService = sp.GetRequiredService<KeyService<TestDescriptor>>();

        var descriptor = new TestDescriptor
        {
            IssuedTo = "Alice",
            Expiration = DateTime.UtcNow.AddMinutes(10)
        };

        // Act: generate and then validate
        var key = keyService.Generate(descriptor);
        var result = keyService.Validate(key);

        // Assert: validation succeeds and round-trips data
        Assert.True(result.IsValid);
        Assert.Equal("Alice", result.Key!.IssuedTo);
    }

    [Fact]
    public void ExpiredKey_ReturnsExpired()
    {
        // Arrange: zero clock skew to force expiration
        var sp = BuildServiceProvider(clockSkew: TimeSpan.Zero);
        var keyService = sp.GetRequiredService<KeyService<TestDescriptor>>();

        var descriptor = new TestDescriptor
        {
            IssuedTo = "Bob",
            Expiration = DateTime.UtcNow.AddMinutes(-5)
        };

        // Act: generate then validate
        var key = keyService.Generate(descriptor);
        var result = keyService.Validate(key);

        // Assert: expired
        Assert.False(result.IsValid);
        Assert.Equal("Key has expired.", result.Error);
    }
}