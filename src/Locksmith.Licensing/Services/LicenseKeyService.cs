using Locksmith.Core.Models;
using Locksmith.Core.Security;
using Locksmith.Core.Services;
using Locksmith.Licensing.Models;

namespace Locksmith.Licensing.Services;

public class LicenseKeyService
{
    private readonly KeyService<LicenseDescriptor> _inner;

    public LicenseKeyService(ISecretProvider secretProvider)
    {
        _inner = new KeyService<LicenseDescriptor>(secretProvider);
    }

    public string Generate(LicenseDescriptor descriptor) => _inner.Generate(descriptor);

    public KeyValidationResult<LicenseDescriptor> Validate(string encodedKey) => _inner.Validate(encodedKey);
}