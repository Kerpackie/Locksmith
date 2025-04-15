namespace Locksmith.Core.Models;

public class LicenseGenerationResult
{
    public bool Success { get; }
    public string LicenseKey { get; }
    public string Error { get; }

    private LicenseGenerationResult(bool success, string licenseKey, string error)
    {
        Success = success;
        LicenseKey = licenseKey;
        Error = error;
    }

    public static LicenseGenerationResult Ok(string key) => new(true, key, null);
    public static LicenseGenerationResult Fail(string error) => new(false, null, error);
}
