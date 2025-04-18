﻿using Locksmith.Core.Models;

namespace Locksmith.Core.Extensions;


/// <summary>
/// Provides extension methods for working with metadata in the <see cref="LicenseInfo"/> class.
/// </summary>
public static class KeyDescriptorMetadataExtensions
{
	/// <summary>
	/// Retrieves the metadata value associated with a specific key in the license.
	/// </summary>
	/// <param name="license">The license to check.</param>
	/// <param name="key">The key representing the metadata field to retrieve.</param>
	/// <returns>
	/// The metadata value associated with the specified key, or <c>null</c> if the key is not found.
	/// </returns>
	public static string? GetMetadata(this KeyDescriptor keyDescriptor, string key)
	{
		if (keyDescriptor?.Metadata != null && keyDescriptor.Metadata.TryGetValue(key, out var value))
			return value;

		return null;
	}

	/// <summary>
	/// Determines whether the license contains a specific metadata key.
	/// </summary>
	/// <param name="license">The license to check.</param>
	/// <param name="key">The key representing the metadata field to check for.</param>
	/// <returns>
	/// <c>true</c> if the metadata key exists in the license; otherwise, <c>false</c>.
	/// </returns>
	public static bool HasMetadata(this KeyDescriptor keyDescriptor, string key)
	{
		return keyDescriptor?.Metadata?.ContainsKey(key) == true;
	}
}