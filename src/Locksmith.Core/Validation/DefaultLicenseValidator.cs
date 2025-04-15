using Locksmith.Core.Config;
using Locksmith.Core.Exceptions;
using Locksmith.Core.Models;

namespace Locksmith.Core.Validation;

public class DefaultLicenseValidator : ILicenseValidator
{
    private readonly LicenseValidationOptions _options;

    public DefaultLicenseValidator(LicenseValidationOptions options = null)
    {
        _options = options ?? new LicenseValidationOptions();
    }

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
    }

    private void Handle(string message)
    {
        throw new LicenseValidationException(message);
    }
}