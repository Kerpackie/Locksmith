using Locksmith.Core.Enums;

namespace Locksmith.Core.Models;

/// <summary>
/// Represents information about a license, including its holder, product, type, and scopes.
/// </summary>
public class LicenseInfo
{
    /// <summary>
    /// Gets or sets the name of the license holder (e.g., individual or organization).
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Gets or sets the identifier for the product that the license is associated with.
    /// </summary>
    public string ProductId { get; set; }

    /// <summary>
    /// Gets or sets the expiration date of the license. A value of <c>null</c> indicates no expiration.
    /// </summary>
    public DateTime? ExpirationDate { get; set; }

    /// <summary>
    /// Gets or sets the type of the license. Defaults to <see cref="LicenseType.Full"/>.
    /// </summary>
    public LicenseType Type { get; set; } = LicenseType.Full;

    /// <summary>
    /// Gets or sets the list of scopes associated with the license. A value of <c>null</c> indicates no specific scopes.
    /// </summary>
    public List<string>? Scopes { get; set; }
    
    /// <summary>
    /// Gets or sets a dictionary that defines limits for specific features or resources.
    /// The key represents the feature/resource name, and the value represents the limit.
    /// A value of <c>null</c> indicates no specific limits are defined.
    /// </summary>
    public Dictionary<string, int>? Limits { get; set; }

    /// <summary>
    /// Gets or sets a dictionary that contains metadata associated with the license.
    /// The key represents the metadata field name, and the value represents the field value.
    /// A value of <c>null</c> indicates no metadata is defined.
    /// </summary>
    public Dictionary<string, string>? Metadata { get; set; }
}