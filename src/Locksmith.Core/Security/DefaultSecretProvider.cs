using System.Text;

namespace Locksmith.Core.Security;

public class DefaultSecretProvider : ISecretProvider
{
    private readonly byte[] _currentSecret;
    private readonly List<byte[]> _allSecrets;

    public DefaultSecretProvider(string currentSecret, IEnumerable<string>? additionalSecrets = null)
    {
        _currentSecret = Encoding.UTF8.GetBytes(currentSecret);
        _allSecrets = new List<byte[]> { _currentSecret };

        if (additionalSecrets != null)
        {
            _allSecrets.AddRange(additionalSecrets.Select(Encoding.UTF8.GetBytes));
        }
    }

    public byte[] GetCurrentSecret() => _currentSecret;

    public IEnumerable<byte[]> GetAllValidationSecrets() => _allSecrets;
}