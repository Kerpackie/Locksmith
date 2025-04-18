namespace Locksmith.Licensing.Enums;

/// <summary>
/// Represents the different types of licenses available in the Locksmith application.
/// </summary>
public enum LicenseType
{
    /// <summary>
    /// A trial license, typically used for evaluation purposes with limited features or time.
    /// </summary>
    Trial,

    /// <summary>
    /// A full license, providing complete access to all features without restrictions.
    /// </summary>
    Full,

    /// <summary>
    /// A subscription-based license, requiring periodic renewal for continued use.
    /// </summary>
    Subscription,

    /// <summary>
    /// An OEM (Original Equipment Manufacturer) license, typically bundled with hardware or software.
    /// </summary>
    OEM,

    /// <summary>
    /// An enterprise license, designed for large organizations with multiple users or installations.
    /// </summary>
    Enterprise,

    /// <summary>
    /// An academic license, intended for educational institutions or students.
    /// </summary>
    Academic
}