using Locksmith.Core.Models;

namespace Locksmith.Core.Revocation;

public class ListRevocationProvider<T> : IKeyRevocationProvider<T> where T : KeyDescriptor
{
    private readonly HashSet<Guid> _revoked;
    public ListRevocationProvider(IEnumerable<Guid> revokedIds)
        => _revoked = new HashSet<Guid>(revokedIds);

    public bool IsRevoked(T descriptor) => _revoked.Contains(descriptor.KeyId);
}

public class ListRevocationProvider2 : ILicenseRevocationProvider
{
    private readonly HashSet<Guid> _revokedIds;

    public ListRevocationProvider2(IEnumerable<Guid> revokedLicenseIds)
    {
        _revokedIds = new HashSet<Guid>(revokedLicenseIds);
    }

    public bool IsRevoked(LicenseInfo licenseInfo)
    {
        return licenseInfo != null && _revokedIds.Contains(licenseInfo.LicenseId);
    }
}