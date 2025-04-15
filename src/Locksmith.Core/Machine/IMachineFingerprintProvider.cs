namespace Locksmith.Core.Machine;

public interface IMachineFingerprintProvider
{
    /// <summary>
    /// Returns a consistent fingerprint string for the current machine.
    /// </summary>
    string GetMachineId();
}
