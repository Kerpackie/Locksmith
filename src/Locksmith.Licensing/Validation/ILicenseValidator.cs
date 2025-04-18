using Locksmith.Core.Validation;
using Locksmith.Licensing.Models;

namespace Locksmith.Licensing.Validation;

public interface ILicenseValidator : IKeyValidator<LicenseDescriptor> { }
