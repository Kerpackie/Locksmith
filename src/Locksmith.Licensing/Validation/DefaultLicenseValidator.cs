using Locksmith.Licensing.Config;
using Locksmith.Licensing.Enums;
using Locksmith.Licensing.Exceptions;
using Locksmith.Licensing.Models;

namespace Locksmith.Licensing.Validation;

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
    public void Validate(LicenseDescriptor licenseInfo)
    {
        ValidatePresence(licenseInfo);
        ValidateExpiration(licenseInfo);
        ValidateLicenseTypeRules(licenseInfo);
        ValidateLicenseScopes(licenseInfo);
        ValidateLicenseLimits(licenseInfo);
    }

    /// <summary>
    /// Validates the presence of required fields in the license information.
    /// </summary>
    /// <param name="licenseInfo">The license information to validate.</param>
    /// <exception cref="LicenseValidationException">Thrown if required fields are missing.</exception>
    private void ValidatePresence(LicenseDescriptor licenseInfo)
    {
        if (licenseInfo == null)
            Handle("License information is missing.");

        if (string.IsNullOrWhiteSpace(licenseInfo.Name))
            Handle("License holder's name is required.");

        if (string.IsNullOrWhiteSpace(licenseInfo.ProductId))
            Handle("Product ID is required.");
    }

    /// <summary>
    /// Validates the expiration date of the license.
    /// </summary>
    /// <param name="licenseInfo">The license information to validate.</param>
    /// <exception cref="LicenseValidationException">Thrown if the expiration date is in the past.</exception>
    private void ValidateExpiration(LicenseDescriptor licenseInfo)
    {
        if (licenseInfo.Expiration.HasValue &&
            licenseInfo.Expiration.Value < DateTime.UtcNow - _options.ClockSkew)
        {
            Handle("Expiration date is in the past.");
        }
    }

    /// <summary>
    /// Validates the license type rules based on the license type.
    /// </summary>
    /// <param name="licenseInfo">The license information to validate.</param>
    /// <exception cref="LicenseValidationException">Thrown if license type rules are violated.</exception>
    private void ValidateLicenseTypeRules(LicenseDescriptor licenseInfo)
    {
        if (!_options.EnforceLicenseTypeRules)
            return;

        switch (licenseInfo.Type)
        {
            case LicenseType.Trial when !licenseInfo.Expiration.HasValue:
                Handle("Trial licenses must have an expiration date.");
                break;
            case LicenseType.Subscription when !licenseInfo.Expiration.HasValue:
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
    /// Validates the license scopes to ensure all required scopes are present.
    /// </summary>
    /// <param name="licenseInfo">The license information to validate.</param>
    /// <exception cref="LicenseValidationException">Thrown if required scopes are missing.</exception>
    private void ValidateLicenseScopes(LicenseDescriptor licenseInfo)
    {
        if (_options.EnforceScopes && _options.RequiredScopes?.Count > 0)
        {
            if (licenseInfo.Scopes == null || !_options.RequiredScopes.All(s => licenseInfo.Scopes.Contains(s)))
            {
                Handle("Required license scopes not present.");
            }
        }
    }

    /// <summary>
    /// Validates the license limits to ensure all defined limits are non-negative.
    /// </summary>
    /// <param name="licenseInfo">The license information to validate.</param>
    /// <exception cref="LicenseValidationException">
    /// Thrown if any limit value is negative.
    /// </exception>
    private void ValidateLicenseLimits(LicenseDescriptor licenseInfo)
    {
        if (_options.EnforceLimitValidation)
        {
            if (licenseInfo.Limits?.Any(kv => kv.Value < 0) == true)
                throw new LicenseValidationException("Limit values must be non-negative.");
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