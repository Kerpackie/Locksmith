using Locksmith.Core.Exceptions;
using Locksmith.Core.Models;
using Locksmith.Core.Validation;

namespace Locksmith.Test;

public class FakeLicenseValidator : ILicenseValidator
{
    public List<LicenseInfo> ValidatedLicenses { get; } = new();
    public string? ForcedErrorMessage { get; set; }

    public void Validate(LicenseInfo licenseInfo)
    {
        ValidatedLicenses.Add(licenseInfo);

        if (!string.IsNullOrWhiteSpace(ForcedErrorMessage))
            throw new LicenseValidationException(ForcedErrorMessage);
    }
}
