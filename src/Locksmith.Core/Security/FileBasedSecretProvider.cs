using System.Text;
using System.Text.Json;

namespace Locksmith.Core.Security;

/// <summary>
/// Provides a file-based implementation of the <see cref="ISecretProvider"/> interface,
/// which retrieves secrets from a JSON file.
/// </summary>
public class FileBasedSecretProvider : ISecretProvider
{
    /// <summary>
    /// The file path to the JSON file containing the secrets.
    /// </summary>
    private readonly string _filePath;

    /// <summary>
    /// Lazily loads and caches the secrets from the file.
    /// </summary>
    private readonly Lazy<List<string>> _cachedSecrets;

    /// <summary>
    /// Initializes a new instance of the <see cref="FileBasedSecretProvider"/> class.
    /// </summary>
    /// <param name="filePath">The file path to the JSON file containing the secrets.</param>
    public FileBasedSecretProvider(string filePath)
    {
        _filePath = filePath;
        _cachedSecrets = new Lazy<List<string>>(LoadSecrets);
    }

    /// <summary>
    /// Gets the current secret as a byte array.
    /// </summary>
    /// <returns>The current secret as a byte array.</returns>
    /// <exception cref="InvalidOperationException">Thrown if no secrets are found in the file.</exception>
    public byte[] GetCurrentSecret()
    {
        var secrets = _cachedSecrets.Value;
        if (secrets.Count == 0) throw new InvalidOperationException("No secrets found in file.");
        return Encoding.UTF8.GetBytes(secrets[0]); // First is current
    }

    /// <summary>
    /// Gets all secrets, including the current secret and any additional secrets, as a collection of byte arrays.
    /// </summary>
    /// <returns>A collection of all secrets as byte arrays.</returns>
    public IEnumerable<byte[]> GetAllValidationSecrets()
    {
        return _cachedSecrets.Value.Select(Encoding.UTF8.GetBytes);
    }

    /// <summary>
    /// Loads the secrets from the JSON file.
    /// </summary>
    /// <returns>A list of secrets as strings.</returns>
    /// <exception cref="FileNotFoundException">Thrown if the secret file is not found.</exception>
    private List<string> LoadSecrets()
    {
        if (!File.Exists(_filePath)) throw new FileNotFoundException("Secret file not found.", _filePath);
        var json = File.ReadAllText(_filePath);
        var secrets = JsonSerializer.Deserialize<List<string>>(json);
        return secrets ?? new List<string>();
    }
}