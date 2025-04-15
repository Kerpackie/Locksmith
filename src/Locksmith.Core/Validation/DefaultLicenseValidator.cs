using Locksmith.Core.Config;
using Locksmith.Core.Exceptions;
using Locksmith.Core.Machine;
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

    private readonly IMachineFingerprintProvider _fingerprintProvider;

    /// <summary>
    /// Initializes a new instance of the <see cref="DefaultLicenseValidator"/> class.
    /// </summary>
    /// <param name="options">The license validation options. If null, default options are used.</param>
    public DefaultLicenseValidator(LicenseValidationOptions options = null, IMachineFingerprintProvider fingerprintProvider = null)
    {
        _fingerprintProvider = fingerprintProvider ?? new DefaultMachineFingerprintProvider();
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
        if (licenseInfo == null)
            Handle("License information is missing.");

        if (string.IsNullOrWhiteSpace(licenseInfo.Name))
            Handle("License holder's name is required.");

        if (string.IsNullOrWhiteSpace(licenseInfo.ProductId))
            Handle("Product ID is required.");

        if (licenseInfo.ExpirationDate.HasValue && licenseInfo.ExpirationDate.Value < DateTime.UtcNow - _options.ClockSkew)
            Handle("Expiration date is in the past.");
        
        if (!string.IsNullOrWhiteSpace(licenseInfo.MachineId))
        {
            var actualMachineId = _fingerprintProvider.GetMachineId();
            if (!string.Equals(actualMachineId, licenseInfo.MachineId, StringComparison.OrdinalIgnoreCase))
                throw new LicenseValidationException("Machine binding mismatch.");
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