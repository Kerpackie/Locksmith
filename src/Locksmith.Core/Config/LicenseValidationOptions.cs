namespace Locksmith.Core.Config;

/// <summary>
/// Represents the options for license validation in the Locksmith application.
/// </summary>
public class LicenseValidationOptions
{
    /// <summary>
    /// Gets or sets a value indicating whether validation should throw exceptions 
    /// or return error results. If set to <c>true</c>, exceptions will be thrown 
    /// on validation errors; otherwise, error results will be returned.
    /// </summary>
    public bool ThrowOnValidationError { get; set; } = false;

    /// <summary>
    /// Gets or sets the allowable clock skew when validating the license. 
    /// This provides a buffer time to account for minor time differences 
    /// between systems. The default value is 5 minutes.
    /// </summary>
    public TimeSpan ClockSkew { get; set; } = TimeSpan.FromMinutes(5);

    /// <summary>
    /// Gets or sets a value indicating whether the license fields should 
    /// be validated. If set to <c>true</c>, the license fields will be 
    /// validated during the validation process.
    /// </summary>
    public bool ValidateLicenseFields { get; set; } = true;

    public bool EnforceMachineBinding { get; set; } = false;
}