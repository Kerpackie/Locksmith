using Locksmith.Licensing.Models;

namespace Locksmith.Licensing.Revocation;

public class ListRevocationProvider : ILicenseRevocationProvider
{
    private readonly HashSet<Guid> _revokedIds;

    public ListRevocationProvider(IEnumerable<Guid> revokedLicenseIds)
    {
        _revokedIds = new HashSet<Guid>(revokedLicenseIds);
    }

    public bool IsRevoked(LicenseDescriptor licenseInfo)
    {
        return licenseInfo != null && _revokedIds.Contains(licenseInfo.KeyId);
    }
}