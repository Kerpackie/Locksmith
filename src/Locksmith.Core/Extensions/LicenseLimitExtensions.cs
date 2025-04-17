using Locksmith.Core.Models;

namespace Locksmith.Core.Extensions;

public static class LicenseLimitExtensions
{
    public static bool HasLimit(this LicenseInfo license, string key, out int value)
    {
        value = 0;

        if (license?.Limits != null && license.Limits.TryGetValue(key, out var found))
        {
            value = found;
            return true;
        }

        return false;
    }

    public static int? GetLimit(this LicenseInfo license, string key)
    {
        if (license?.Limits?.TryGetValue(key, out var val) == true)
            return val;

        return null;
    }
}