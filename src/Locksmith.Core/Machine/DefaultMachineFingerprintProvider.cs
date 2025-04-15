using System.Security.Cryptography;
using System.Text;

namespace Locksmith.Core.Machine;

public class DefaultMachineFingerprintProvider : IMachineFingerprintProvider
{
    public string GetMachineId()
    {
        // Use hashed machine data (e.g., MAC address, CPU ID, volume ID)
        var rawId = Environment.MachineName; // simple placeholder
        using var sha256 = SHA256.Create();
        var hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(rawId));
        return Convert.ToHexString(hash);
    }
}
