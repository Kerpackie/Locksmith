namespace Locksmith.Core.Security;

/// <summary>
/// Defines the contract for a secret provider that manages secrets for validation purposes.
/// </summary>
public interface ISecretProvider
{
    /// <summary>
    /// Retrieves the current secret as a byte array.
    /// </summary>
    /// <returns>The current secret as a byte array.</returns>
    byte[] GetCurrentSecret();

    /// <summary>
    /// Retrieves all validation secrets, including the current secret and any additional secrets, as a collection of byte arrays.
    /// </summary>
    /// <returns>A collection of all validation secrets as byte arrays.</returns>
    IEnumerable<byte[]> GetAllValidationSecrets();
}