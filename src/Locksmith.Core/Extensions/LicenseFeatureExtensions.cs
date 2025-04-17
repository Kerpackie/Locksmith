using Locksmith.Core.Models;

namespace Locksmith.Core.Extensions;

/// <summary>
/// Provides extension methods for working with license features in the <see cref="LicenseInfo"/> class.
/// </summary>
public static class LicenseFeatureExtensions
{
    /// <summary>
    /// Determines whether the license includes a specific feature.
    /// </summary>
    /// <param name="license">The license to check.</param>
    /// <param name="feature">The feature to look for.</param>
    /// <returns><c>true</c> if the license includes the specified feature; otherwise, <c>false</c>.</returns>
    public static bool HasFeature(this LicenseInfo license, string feature)
    {
        return license?.Scopes?.Contains(feature) == true;
    }

    /// <summary>
    /// Determines whether the license includes any of the specified features.
    /// </summary>
    /// <param name="license">The license to check.</param>
    /// <param name="features">An array of features to look for.</param>
    /// <returns><c>true</c> if the license includes any of the specified features; otherwise, <c>false</c>.</returns>
    public static bool HasAnyFeature(this LicenseInfo license, params string[] features)
    {
        return license?.Scopes.Intersect(features).Any() == true;
    }

    /// <summary>
    /// Determines whether the license includes all of the specified features.
    /// </summary>
    /// <param name="license">The license to check.</param>
    /// <param name="features">An array of features to look for.</param>
    /// <returns><c>true</c> if the license includes all of the specified features; otherwise, <c>false</c>.</returns>
    public static bool HasAllFeatures(this LicenseInfo license, params string[] features)
    {
        return license?.Scopes?.Intersect(features).Count() == features.Length;
    }
}