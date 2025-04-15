using System.Text;

namespace Locksmith.Core.Security;

/// <summary>
/// Provides a default implementation of the <see cref="ISecretProvider"/> interface,
/// which manages the current secret and additional secrets for validation purposes.
/// </summary>
public class DefaultSecretProvider : ISecretProvider
{
    /// <summary>
    /// Stores the current secret as a byte array.
    /// </summary>
    private readonly byte[] _currentSecret;

    /// <summary>
    /// Stores all secrets, including the current secret and any additional secrets, as a list of byte arrays.
    /// </summary>
    private readonly List<byte[]> _allSecrets;

    /// <summary>
    /// Initializes a new instance of the <see cref="DefaultSecretProvider"/> class.
    /// </summary>
    /// <param name="currentSecret">The current secret as a string.</param>
    /// <param name="additionalSecrets">An optional collection of additional secrets as strings.</param>
    public DefaultSecretProvider(string currentSecret, IEnumerable<string>? additionalSecrets = null)
    {
        _currentSecret = Encoding.UTF8.GetBytes(currentSecret);
        _allSecrets = new List<byte[]> { _currentSecret };

        if (additionalSecrets != null)
        {
            _allSecrets.AddRange(additionalSecrets.Select(Encoding.UTF8.GetBytes));
        }
    }

    /// <summary>
    /// Gets the current secret as a byte array.
    /// </summary>
    /// <returns>The current secret.</returns>
    public byte[] GetCurrentSecret() => _currentSecret;

    /// <summary>
    /// Gets all secrets, including the current secret and any additional secrets, as a collection of byte arrays.
    /// </summary>
    /// <returns>A collection of all secrets.</returns>
    public IEnumerable<byte[]> GetAllValidationSecrets() => _allSecrets;
}