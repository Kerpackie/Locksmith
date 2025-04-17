using Locksmith.Core.Models;

namespace Locksmith.Core.Revocation;

public interface ILicenseRevocationProvider
{
    bool IsRevoked(LicenseInfo licenseInfo);
}