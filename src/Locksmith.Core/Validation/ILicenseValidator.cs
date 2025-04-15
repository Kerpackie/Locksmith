using Locksmith.Core.Models;

namespace Locksmith.Core.Validation;

/// <summary>
/// Defines the contract for validating license information.
/// </summary>
public interface ILicenseValidator
{
    /// <summary>
    /// Validates the provided license information.
    /// </summary>
    /// <param name="licenseInfo">The license information to validate.</param>
    /// <exception cref="LicenseValidationException">
    /// Thrown if the license information is invalid, such as missing required fields
    /// or an expiration date in the past.
    /// </exception>
    void Validate(LicenseInfo licenseInfo);
}