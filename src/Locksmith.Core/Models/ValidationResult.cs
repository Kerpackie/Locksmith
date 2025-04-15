namespace Locksmith.Core.Models;

/// <summary>
/// Represents the result of a license validation operation.
/// </summary>
public class ValidationResult
{
    /// <summary>
    /// Gets a value indicating whether the license validation was successful.
    /// </summary>
    public bool IsValid { get; }

    /// <summary>
    /// Gets the error message if the validation failed; otherwise, <c>null</c>.
    /// </summary>
    public string Error { get; }

    /// <summary>
    /// Gets the license information associated with the validation result.
    /// </summary>
    public LicenseInfo LicenseInfo { get; }

    // Classification flags

    /// <summary>
    /// Gets a value indicating whether the license has expired.
    /// </summary>
    public bool IsExpired => Error == "License has expired.";

    /// <summary>
    /// Gets a value indicating whether the license has been tampered with.
    /// </summary>
    public bool IsTampered => Error == "Invalid signature.";

    /// <summary>
    /// Gets a value indicating whether the license is malformed.
    /// </summary>
    public bool IsMalformed => Error != null && Error.StartsWith("Validation failed");

    /// <summary>
    /// Initializes a new instance of the <see cref="ValidationResult"/> class.
    /// </summary>
    /// <param name="isValid">Indicates whether the validation was successful.</param>
    /// <param name="error">The error message if validation failed; otherwise, <c>null</c>.</param>
    /// <param name="licenseInfo">The license information associated with the validation result.</param>
    private ValidationResult(bool isValid, string error, LicenseInfo licenseInfo)
    {
        IsValid = isValid;
        Error = error;
        LicenseInfo = licenseInfo;
    }

    /// <summary>
    /// Creates a valid <see cref="ValidationResult"/> with the specified license information.
    /// </summary>
    /// <param name="info">The license information associated with the valid result.</param>
    /// <returns>A valid <see cref="ValidationResult"/> instance.</returns>
    public static ValidationResult Valid(LicenseInfo info) =>
        new ValidationResult(true, null, info);

    /// <summary>
    /// Creates an invalid <see cref="ValidationResult"/> with the specified error message and optional partial license information.
    /// </summary>
    /// <param name="error">The error message describing the validation failure.</param>
    /// <param name="partialInfo">The partial license information, if available.</param>
    /// <returns>An invalid <see cref="ValidationResult"/> instance.</returns>
    public static ValidationResult Invalid(string error, LicenseInfo partialInfo = null) =>
        new ValidationResult(false, error, partialInfo);
}