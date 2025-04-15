using System.Text;
using System.Text.Json;

namespace Locksmith.Core.Security;

public class FileBasedSecretProvider : ISecretProvider
{
    private readonly string _filePath;
    private readonly Lazy<List<string>> _cachedSecrets;

    public FileBasedSecretProvider(string filePath)
    {
        _filePath = filePath;
        _cachedSecrets = new Lazy<List<string>>(LoadSecrets);
    }

    public byte[] GetCurrentSecret()
    {
        var secrets = _cachedSecrets.Value;
        if (secrets.Count == 0) throw new InvalidOperationException("No secrets found in file.");
        return Encoding.UTF8.GetBytes(secrets[0]); // First is current
    }

    public IEnumerable<byte[]> GetAllValidationSecrets()
    {
        return _cachedSecrets.Value.Select(Encoding.UTF8.GetBytes);
    }

    private List<string> LoadSecrets()
    {
        if (!File.Exists(_filePath)) throw new FileNotFoundException("Secret file not found.", _filePath);
        var json = File.ReadAllText(_filePath);
        var secrets = JsonSerializer.Deserialize<List<string>>(json);
        return secrets ?? new List<string>();
    }
}
