using Locksmith.Licensing.Exceptions;
using Locksmith.Licensing.Models;
using Locksmith.Licensing.Validation;

namespace Locksmith.Licensing.Test;

public class FakeLicenseValidator : ILicenseValidator
{
    public List<LicenseDescriptor> ValidatedLicenses { get; } = new();
    public string? ForcedErrorMessage { get; set; }

    public void Validate(LicenseDescriptor licenseInfo)
    {
        ValidatedLicenses.Add(licenseInfo);

        if (!string.IsNullOrWhiteSpace(ForcedErrorMessage))
            throw new LicenseValidationException(ForcedErrorMessage);
    }
}
