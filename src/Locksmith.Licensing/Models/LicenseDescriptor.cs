using Locksmith.Core.Models;
using Locksmith.Licensing.Enums;

namespace Locksmith.Licensing.Models;

public class LicenseDescriptor : KeyDescriptor
{
    public string ProductId { get; set; } = string.Empty;
    public List<string>? Scopes { get; set; }
    public LicenseType Type { get; set; } = LicenseType.Full;
}