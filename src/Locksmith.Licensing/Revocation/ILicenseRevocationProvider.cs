using Locksmith.Core.Revocation;
using Locksmith.Licensing.Models;

namespace Locksmith.Licensing.Revocation;

public interface ILicenseRevocationProvider : IKeyRevocationProvider<LicenseDescriptor> { }
