namespace Locksmith.Core.Security;

public interface ISecretProvider
{
    byte[] GetCurrentSecret();
    IEnumerable<byte[]> GetAllValidationSecrets();
}