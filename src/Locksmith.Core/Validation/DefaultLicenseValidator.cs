using Locksmith.Core.Config;
using Locksmith.Core.Enums;
using Locksmith.Core.Exceptions;
using Locksmith.Core.Models;

namespace Locksmith.Core.Validation;

/// <summary>
/// Provides a default implementation of the <see cref="ILicenseValidator"/> interface
/// for validating license information based on predefined rules.
/// </summary>
public class DefaultLicenseValidator : ILicenseValidator
{
    /// <summary>
    /// The options used for license validation, such as clock skew tolerance.
    /// </summary>
    private readonly LicenseValidationOptions _options;

    /// <summary>
    /// Initializes a new instance of the <see cref="DefaultLicenseValidator"/> class.
    /// </summary>
    /// <param name="options">The license validation options. If null, default options are used.</param>
    public DefaultLicenseValidator(LicenseValidationOptions options = null)
    {
        _options = options ?? new LicenseValidationOptions();
    }

    /// <summary>
    /// Validates the provided license information.
    /// </summary>
    /// <param name="licenseInfo">The license information to validate.</param>
    /// <exception cref="LicenseValidationException">
    /// Thrown if the license information is invalid, such as missing required fields
    /// or an expiration date in the past.
    /// </exception>
    public void Validate(LicenseInfo licenseInfo)
    {
        ValidatePresence(licenseInfo);
        ValidateExpiration(licenseInfo);
        ValidateLicenseTypeRules(licenseInfo);
    }

    private void ValidatePresence(LicenseInfo licenseInfo)
    {
        if (licenseInfo == null)
            Handle("License information is missing.");

        if (string.IsNullOrWhiteSpace(licenseInfo.Name))
            Handle("License holder's name is required.");

        if (string.IsNullOrWhiteSpace(licenseInfo.ProductId))
            Handle("Product ID is required.");
    }

    private void ValidateExpiration(LicenseInfo licenseInfo)
    {
        if (licenseInfo.ExpirationDate.HasValue &&
            licenseInfo.ExpirationDate.Value < DateTime.UtcNow - _options.ClockSkew)
        {
            Handle("Expiration date is in the past.");
        }
    }

    private void ValidateLicenseTypeRules(LicenseInfo licenseInfo)
    {
        if (!_options.EnforceLicenseTypeRules)
            return;

        switch (licenseInfo.Type)
        {
            case LicenseType.Trial when !licenseInfo.ExpirationDate.HasValue:
                Handle("Trial licenses must have an expiration date.");
                break;
            case LicenseType.Subscription when !licenseInfo.ExpirationDate.HasValue:
                Handle("Subscription licenses must have an expiration date.");
                break;
            case LicenseType.Full:
            case LicenseType.OEM:
            case LicenseType.Enterprise:
            case LicenseType.Academic:
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(licenseInfo.Type), licenseInfo.Type, "Unsupported license type.");
        }
    }


    /// <summary>
    /// Handles validation errors by throwing a <see cref="LicenseValidationException"/>.
    /// </summary>
    /// <param name="message">The error message to include in the exception.</param>
    /// <exception cref="LicenseValidationException">Always thrown with the provided error message.</exception>
    private void Handle(string message)
    {
        throw new LicenseValidationException(message);
    }
    
}