using Locksmith.Core.Exceptions;
using Locksmith.Core.Models;

namespace Locksmith.Core.Validation;

public class DefaultLicenseValidator : ILicenseValidator
{
    public void Validate(LicenseInfo licenseInfo)
    {
        if (licenseInfo == null)
        {
            throw new LicenseValidationException("License informaion is missing.");
        }

        if (string.IsNullOrWhiteSpace(licenseInfo.Name))
        {
            throw new LicenseValidationException("Name is required.");
        }
        
        if (string.IsNullOrWhiteSpace(licenseInfo.ProductId))
        {
            throw new LicenseValidationException("Product ID is required.");
        }
        
        if (licenseInfo.ExpirationDate.HasValue && licenseInfo.ExpirationDate.Value < DateTime.UtcNow)
        {
            throw new LicenseValidationException("Expiration date is in the past.");
        }
    }
}