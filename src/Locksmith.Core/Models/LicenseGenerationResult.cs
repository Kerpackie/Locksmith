namespace Locksmith.Core.Models;

/// <summary>
/// Represents the result of a license generation operation.
/// </summary>
public class LicenseGenerationResult
{
    /// <summary>
    /// Gets a value indicating whether the license generation was successful.
    /// </summary>
    public bool Success { get; }

    /// <summary>
    /// Gets the generated license key if the operation was successful; otherwise, <c>null</c>.
    /// </summary>
    public string LicenseKey { get; }

    /// <summary>
    /// Gets the error message if the operation failed; otherwise, <c>null</c>.
    /// </summary>
    public string Error { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="LicenseGenerationResult"/> class.
    /// </summary>
    /// <param name="success">Indicates whether the operation was successful.</param>
    /// <param name="licenseKey">The generated license key, or <c>null</c> if the operation failed.</param>
    /// <param name="error">The error message, or <c>null</c> if the operation was successful.</param>
    private LicenseGenerationResult(bool success, string licenseKey, string error)
    {
        Success = success;
        LicenseKey = licenseKey;
        Error = error;
    }

    /// <summary>
    /// Creates a successful <see cref="LicenseGenerationResult"/> with the specified license key.
    /// </summary>
    /// <param name="key">The generated license key.</param>
    /// <returns>A successful <see cref="LicenseGenerationResult"/> instance.</returns>
    public static LicenseGenerationResult Ok(string key) => new(true, key, null);

    /// <summary>
    /// Creates a failed <see cref="LicenseGenerationResult"/> with the specified error message.
    /// </summary>
    /// <param name="error">The error message describing the failure.</param>
    /// <returns>A failed <see cref="LicenseGenerationResult"/> instance.</returns>
    public static LicenseGenerationResult Fail(string error) => new(false, null, error);
}