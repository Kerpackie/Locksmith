using System.Text;

namespace Locksmith.Core.Security;

/// <summary>
/// An in-memory implementation of <see cref="ISecretProvider"/> that supports secret rotation with optional expiry metadata.
/// </summary>
public class InMemoryRotatingSecretProvider : ISecretProvider
{
    /// <summary>
    /// Stores the list of secrets, including metadata such as expiry and whether the secret is current.
    /// </summary>
    private readonly List<SecretEntry> _secrets;

    /// <summary>
    /// Initializes a new instance of the <see cref="InMemoryRotatingSecretProvider"/> class.
    /// </summary>
    /// <param name="currentSecret">The current secret as a string.</param>
    /// <param name="additionalSecrets">An optional collection of additional secrets as strings.</param>
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

    /// <summary>
    /// Gets the current secret as a byte array.
    /// </summary>
    /// <returns>The current secret as a byte array.</returns>
    /// <exception cref="InvalidOperationException">Thrown if no current secret is configured.</exception>
    public byte[] GetCurrentSecret()
    {
        var current = _secrets.FirstOrDefault(s => s.IsCurrent);
        if (current == null) throw new InvalidOperationException("No current secret configured.");
        return current.GetSecretBytes();
    }

    /// <summary>
    /// Gets all validation secrets, including those that have not expired, as a collection of byte arrays.
    /// </summary>
    /// <returns>A collection of all valid secrets as byte arrays.</returns>
    public IEnumerable<byte[]> GetAllValidationSecrets()
    {
        return _secrets
            .Where(s => !s.ExpiresAt.HasValue || s.ExpiresAt.Value > DateTime.UtcNow)
            .Select(s => s.GetSecretBytes());
    }

    /// <summary>
    /// Rotates the current secret by adding a new secret and marking it as current.
    /// </summary>
    /// <param name="newSecret">The new secret to be added.</param>
    /// <param name="expiresAt">The optional expiry date for the new secret.</param>
    public void RotateSecret(string newSecret, DateTime? expiresAt = null)
    {
        foreach (var s in _secrets) s.IsCurrent = false;
        _secrets.Add(new SecretEntry(newSecret, DateTime.UtcNow, expiresAt, isCurrent: true));
    }

    /// <summary>
    /// Represents a single secret entry with metadata.
    /// </summary>
    private class SecretEntry
    {
        /// <summary>
        /// Gets the secret as a string.
        /// </summary>
        public string Secret { get; }

        /// <summary>
        /// Gets the date and time when the secret was added.
        /// </summary>
        public DateTime AddedAt { get; }

        /// <summary>
        /// Gets the optional expiry date and time of the secret.
        /// </summary>
        public DateTime? ExpiresAt { get; }

        /// <summary>
        /// Gets or sets a value indicating whether this secret is the current secret.
        /// </summary>
        public bool IsCurrent { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="SecretEntry"/> class.
        /// </summary>
        /// <param name="secret">The secret as a string.</param>
        /// <param name="addedAt">The date and time when the secret was added.</param>
        /// <param name="expiresAt">The optional expiry date and time of the secret.</param>
        /// <param name="isCurrent">Indicates whether this secret is the current secret.</param>
        public SecretEntry(string secret, DateTime addedAt, DateTime? expiresAt, bool isCurrent)
        {
            Secret = secret;
            AddedAt = addedAt;
            ExpiresAt = expiresAt;
            IsCurrent = isCurrent;
        }

        /// <summary>
        /// Gets the secret as a byte array.
        /// </summary>
        /// <returns>The secret as a byte array.</returns>
        public byte[] GetSecretBytes() => Encoding.UTF8.GetBytes(Secret);
    }
}