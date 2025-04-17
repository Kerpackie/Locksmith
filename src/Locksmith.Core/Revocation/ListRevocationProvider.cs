using Locksmith.Core.Models;

namespace Locksmith.Core.Revocation;

public class ListRevocationProvider : ILicenseRevocationProvider
{
    private readonly HashSet<Guid> _revokedIds;

    public ListRevocationProvider(IEnumerable<Guid> revokedLicenseIds)
    {
        _revokedIds = new HashSet<Guid>(revokedLicenseIds);
    }

    public bool IsRevoked(LicenseInfo licenseInfo)
    {
        return licenseInfo != null && _revokedIds.Contains(licenseInfo.LicenseId);
    }
}