using System.Text;

namespace Locksmith.Core.Security;

/// <summary>
/// An in-memory implementation of ISecretProvider that supports secret rotation with optional expiry metadata.
/// </summary>
public class InMemoryRotatingSecretProvider : ISecretProvider
{
    private readonly List<SecretEntry> _secrets;

    public InMemoryRotatingSecretProvider(string currentSecret, IEnumerable<string>? additionalSecrets = null)
    {
        _secrets = new List<SecretEntry>
        {
            new SecretEntry(currentSecret, DateTime.UtcNow, null, isCurrent: true)
        };

        if (additionalSecrets != null)
        {
            foreach (var s in additionalSecrets)
            {
                _secrets.Add(new SecretEntry(s, DateTime.MinValue, null, isCurrent: false));
            }
        }
    }

    public byte[] GetCurrentSecret()
    {
        var current = _secrets.FirstOrDefault(s => s.IsCurrent);
        if (current == null) throw new InvalidOperationException("No current secret configured.");
        return current.GetSecretBytes();
    }

    public IEnumerable<byte[]> GetAllValidationSecrets()
    {
        return _secrets
            .Where(s => !s.ExpiresAt.HasValue || s.ExpiresAt.Value > DateTime.UtcNow)
            .Select(s => s.GetSecretBytes());
    }

    public void RotateSecret(string newSecret, DateTime? expiresAt = null)
    {
        foreach (var s in _secrets) s.IsCurrent = false;
        _secrets.Add(new SecretEntry(newSecret, DateTime.UtcNow, expiresAt, isCurrent: true));
    }

    private class SecretEntry
    {
        public string Secret { get; }
        public DateTime AddedAt { get; }
        public DateTime? ExpiresAt { get; }
        public bool IsCurrent { get; set; }

        public SecretEntry(string secret, DateTime addedAt, DateTime? expiresAt, bool isCurrent)
        {
            Secret = secret;
            AddedAt = addedAt;
            ExpiresAt = expiresAt;
            IsCurrent = isCurrent;
        }

        public byte[] GetSecretBytes() => Encoding.UTF8.GetBytes(Secret);
    }
}
