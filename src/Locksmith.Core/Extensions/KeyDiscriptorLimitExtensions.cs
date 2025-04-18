using Locksmith.Core.Models;

namespace Locksmith.Core.Extensions;

/// <summary>
/// Provides extension methods for working with license limits in the <see cref="LicenseInfo"/> class.
/// </summary>
public static class KeyDescriptorLimitExtensions
{
    /// <summary>
    /// Determines whether the license has a specific limit defined for a given key.
    /// </summary>
    /// <param name="license">The license to check.</param>
    /// <param name="key">The key representing the feature or resource to check.</param>
    /// <param name="value">
    /// When this method returns, contains the limit value associated with the specified key,
    /// if the key is found; otherwise, 0.
    /// </param>
    /// <returns>
    /// <c>true</c> if the license has a limit defined for the specified key; otherwise, <c>false</c>.
    /// </returns>
    public static bool HasLimit(this KeyDescriptor keyDescriptor, string key, out int value)
    {
        value = 0;

        if (keyDescriptor?.Limits != null && keyDescriptor.Limits.TryGetValue(key, out var found))
        {
            value = found;
            return true;
        }

        return false;
    }

    /// <summary>
    /// Retrieves the limit value associated with a specific key in the license.
    /// </summary>
    /// <param name="license">The license to check.</param>
    /// <param name="key">The key representing the feature or resource to retrieve the limit for.</param>
    /// <returns>
    /// The limit value associated with the specified key, or <c>null</c> if no limit is defined.
    /// </returns>
    public static int? GetLimit(this KeyDescriptor keyDescriptor, string key)
    {
        if (keyDescriptor?.Limits?.TryGetValue(key, out var val) == true)
            return val;

        return null;
    }
}