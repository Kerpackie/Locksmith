using Locksmith.Core.Models;
using Locksmith.Core.Revocation;
using Locksmith.Core.Security;
using Locksmith.Core.Services;
using Locksmith.Core.Validation;
using Locksmith.Licensing.Config;
using Locksmith.Licensing.Models;
using Microsoft.Extensions.Options;

namespace Locksmith.Licensing.Services;

public class LicenseKeyService
{
    private readonly KeyService<LicenseDescriptor> _inner;

    public LicenseKeyService(
        ISecretProvider secretProvider, 
        LicenseValidationOptions options,
        IKeyValidator<LicenseDescriptor>? validator = null,
        IKeyRevocationProvider<LicenseDescriptor>? revocationProvider = null)
    {
        _inner = new KeyService<LicenseDescriptor>(
            secretProvider,
            revocationProvider,
            options,
            validator);
    }

    public string Generate(LicenseDescriptor descriptor) => _inner.Generate(descriptor);

    public KeyValidationResult<LicenseDescriptor> Validate(string encodedKey) => _inner.Validate(encodedKey);
    
    public KeyGenerationResult TryGenerate(LicenseDescriptor descriptor) => _inner.TryGenerate(descriptor);
}