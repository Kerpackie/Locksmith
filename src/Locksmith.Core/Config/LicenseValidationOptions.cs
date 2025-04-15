namespace Locksmith.Core.Config;

public class LicenseValidationOptions
{
    /// <summary>
    /// Whether validation should throw exceptions or return error results.
    /// </summary>
    public bool ThrowOnValidationError { get; set; } = false;

    /// <summary>
    /// Allows some buffer time when validating the license.
    /// </summary>
    public TimeSpan ClockSkew { get; set; } = TimeSpan.FromMinutes(5);
    
    public bool ValidateLicenseFields { get; set; } = false;
}