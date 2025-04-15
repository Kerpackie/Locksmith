namespace Locksmith.Core.Models;

public class ValidationResult
{
    public bool IsValid { get; }
    public string? Error { get; }
    public LicenseInfo LicenseInfo { get; }

    private ValidationResult(bool isValid, string? error, LicenseInfo licenseInfo)
    {
        IsValid = isValid;
        Error = error;
        LicenseInfo = licenseInfo;
    }
    
    public static ValidationResult Valid(LicenseInfo licenseInfo) 
        => new ValidationResult(true, null, licenseInfo);
    
    public static ValidationResult Invalid(string error) 
        => new ValidationResult(false, error, new LicenseInfo());
    
}