namespace Locksmith.Core.Models;

public class ValidationResult
{
    public bool IsValid { get; }
    public string Error { get; }
    public LicenseInfo LicenseInfo { get; }

    // Classification flags
    public bool IsExpired => Error == "License has expired.";
    public bool IsTampered => Error == "Invalid signature.";
    public bool IsMalformed => Error != null && Error.StartsWith("Validation failed");

    private ValidationResult(bool isValid, string error, LicenseInfo licenseInfo)
    {
        IsValid = isValid;
        Error = error;
        LicenseInfo = licenseInfo;
    }

    public static ValidationResult Valid(LicenseInfo info) =>
        new ValidationResult(true, null, info);

    public static ValidationResult Invalid(string error, LicenseInfo partialInfo = null) =>
        new ValidationResult(false, error, partialInfo);
}
