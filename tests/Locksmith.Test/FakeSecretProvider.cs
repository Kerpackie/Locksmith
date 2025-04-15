using System.Text;
using Locksmith.Core.Security;

namespace Locksmith.Test;

public class FakeSecretProvider : ISecretProvider
{
    public byte[] CurrentSecret { get; set; } = Encoding.UTF8.GetBytes("fake-current");
    public List<byte[]> ValidationSecrets { get; } = new()
    {
        Encoding.UTF8.GetBytes("fake-current")
    };

    public byte[] GetCurrentSecret() => CurrentSecret;

    public IEnumerable<byte[]> GetAllValidationSecrets() => ValidationSecrets;
}