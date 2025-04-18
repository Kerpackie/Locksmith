namespace Locksmith.Core.Models;

/// <summary>
/// Represents a descriptor for a cryptographic key, including its unique identifier, 
/// the entity it was issued to, expiration details, and associated metadata.
/// </summary>
public abstract class KeyDescriptor
{
    /// <summary>
    /// Gets or sets the unique identifier for the key.
    /// Defaults to a new GUID when the object is instantiated.
    /// </summary>
    public Guid KeyId { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Gets or sets the name of the entity to which the key was issued.
    /// Defaults to an empty string.
    /// </summary>
    public string IssuedTo { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the expiration date and time of the key.
    /// A value of <c>null</c> indicates that the key does not expire.
    /// </summary>
    public DateTime? Expiration { get; set; }

    /// <summary>
    /// Gets or sets a dictionary containing metadata associated with the key.
    /// The key represents the metadata field name, and the value represents the field value.
    /// A value of <c>null</c> indicates no metadata is defined.
    /// </summary>
    public Dictionary<string, string>? Metadata { get; set; }

    /// <summary>
    /// Gets or sets a dictionary that defines limits for specific features or resources.
    /// The key represents the feature/resource name, and the value represents the limit.
    /// A value of <c>null</c> indicates no specific limits are defined.
    /// </summary>
    public Dictionary<string, int>? Limits { get; set; }
}