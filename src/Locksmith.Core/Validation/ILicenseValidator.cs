using Locksmith.Core.Models;

namespace Locksmith.Core.Validation;

public interface ILicenseValidator
{
    void Validate(LicenseInfo licenseInfo);
}