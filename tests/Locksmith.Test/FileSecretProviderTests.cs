using System.Text;
using System.Text.Json;
using Locksmith.Core.Security;

namespace Locksmith.Test;

public class FileSecretProviderTests
{
    [Fact]
    public void Should_Load_Secrets_From_Json_File()
    {
        var path = Path.GetTempFileName();
        var secrets = new[] { "secret-1", "secret-2" };
        File.WriteAllText(path, JsonSerializer.Serialize(secrets));

        var provider = new FileBasedSecretProvider(path);

        Assert.Equal(Encoding.UTF8.GetBytes("secret-1"), provider.GetCurrentSecret());

        var allSecrets = provider.GetAllValidationSecrets().ToList();
        Assert.Equal(2, allSecrets.Count);
        Assert.Contains(allSecrets, s => s.SequenceEqual(Encoding.UTF8.GetBytes("secret-1")));
        Assert.Contains(allSecrets, s => s.SequenceEqual(Encoding.UTF8.GetBytes("secret-2")));

        File.Delete(path);
    }

    [Fact]
    public void Should_Throw_If_File_Missing()
    {
        var path = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".json");
        Assert.Throws<FileNotFoundException>(() => new FileBasedSecretProvider(path).GetCurrentSecret());
    }

    [Fact]
    public void Should_Throw_If_File_Has_No_Secrets()
    {
        var path = Path.GetTempFileName();
        File.WriteAllText(path, "[]");

        var provider = new FileBasedSecretProvider(path);
        Assert.Throws<InvalidOperationException>(() => provider.GetCurrentSecret());

        File.Delete(path);
    }
}