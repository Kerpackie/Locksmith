namespace Locksmith.Core.Models;

public class LicenseInfo
{
    /// <summary>
    /// Name of the licence holder (e.g., individual or organisation).
    /// </summary>
    public string Name { get; set; }
    
    /// <summary>
    /// The identifier for the product which the licence is for.
    /// </summary>
    public string ProductId { get; set; }
    
    /// <summary>
    /// The expiration date of the licence. Null means no expiration.
    /// </summary>
    public DateTime? ExpirationDate { get; set; }

    /// <summary>
    /// Optional machine binding value (Eg. hashed MAC, fingerprint, etc.).
    /// </summary>
    public string? MachineId { get; set; }
}