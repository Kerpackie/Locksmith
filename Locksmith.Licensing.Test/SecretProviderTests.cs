using System.Text;

namespace Locksmith.Licensing.Test;

public class SecretProviderTests
{
    [Fact]
    public void GetCurrentSecret_Should_Return_ActiveSecret()
    {
        var provider = new InMemoryRotatingSecretProvider("initial-secret");

        var current = provider.GetCurrentSecret();

        Assert.Equal(Encoding.UTF8.GetBytes("initial-secret"), current);
    }

    [Fact]
    public void GetAllValidationSecrets_Should_Include_Current_And_NonExpired()
    {
        var provider = new InMemoryRotatingSecretProvider("initial-secret", new[] { "older-secret" });

        var secrets = provider.GetAllValidationSecrets().ToList();

        Assert.Equal(2, secrets.Count);
        Assert.Contains(secrets, s => s.SequenceEqual(Encoding.UTF8.GetBytes("initial-secret")));
        Assert.Contains(secrets, s => s.SequenceEqual(Encoding.UTF8.GetBytes("older-secret")));
    }

    [Fact]
    public void RotateSecret_Should_Update_CurrentSecret_And_Preserve_Old()
    {
        var provider = new InMemoryRotatingSecretProvider("v1");
        provider.RotateSecret("v2");

        var current = provider.GetCurrentSecret();
        Assert.Equal(Encoding.UTF8.GetBytes("v2"), current);

        var all = provider.GetAllValidationSecrets().ToList();
        Assert.Equal(2, all.Count);
    }

    [Fact]
    public void Expired_Secrets_Should_Not_Be_Used_For_Validation()
    {
        var provider = new InMemoryRotatingSecretProvider("v1");
        provider.RotateSecret("v2", expiresAt: DateTime.UtcNow.AddMilliseconds(-10)); // immediately expired

        var all = provider.GetAllValidationSecrets().ToList();
        Assert.Single(all);
        Assert.Equal(Encoding.UTF8.GetBytes("v1"), all[0]);
    }
}