using System.Numerics;
using System.Text;

namespace Locksmith.Core.Utils;

/// <summary>
/// Provides methods for encoding and decoding data using the Base58 encoding scheme.
/// </summary>
public static class Base58Encoder
{
    /// <summary>
    /// The alphabet used for Base58 encoding, which excludes characters that are visually similar.
    /// </summary>
    private const string Alphabet = "123456789ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz";

    /// <summary>
    /// The base value for the Base58 encoding scheme.
    /// </summary>
    private static readonly BigInteger Base = new BigInteger(58);

    /// <summary>
    /// Encodes a byte array into a Base58 string.
    /// </summary>
    /// <param name="data">The byte array to encode.</param>
    /// <returns>The Base58-encoded string.</returns>
    public static string Encode(byte[] data)
    {
        var intData = new BigInteger(data, isUnsigned: true, isBigEndian: true);

        var result = new StringBuilder();
        while (intData > 0)
        {
            var remainder = (int)(intData % Base);
            intData /= Base;
            result.Insert(0, Alphabet[remainder]);
        }

        // Handle leading zeros
        foreach (var b in data)
        {
            if (b == 0)
            {
                result.Insert(0, Alphabet[0]);
            }
            else
            {
                break;
            }
        }

        return result.ToString();
    }

    /// <summary>
    /// Decodes a Base58-encoded string into a byte array.
    /// </summary>
    /// <param name="base58">The Base58-encoded string to decode.</param>
    /// <returns>The decoded byte array.</returns>
    /// <exception cref="FormatException">Thrown if the input string contains invalid Base58 characters.</exception>
    public static byte[] Decode(string base58)
    {
        var intData = BigInteger.Zero;

        foreach (var c in base58)
        {
            var digit = Alphabet.IndexOf(c);

            if (digit < 0)
            {
                throw new FormatException($"Invalid Base58 character '{c}'");
            }

            intData = intData * Base + digit;
        }

        // Convert to byte array.
        var bytesWithoutLeadingZeros = intData.ToByteArray();
        var bytes = bytesWithoutLeadingZeros.Reverse().ToArray();

        // Remove Sign byte if present
        if (bytes.Length >= 2 && bytes[0] == 0 && bytes[1] >= 0x80)
        {
            bytes = bytes[1..];
        }

        // Handle leading zeros
        var leadingZeros = base58.TakeWhile(c => c == Alphabet[0]).Count();

        return Enumerable.Repeat((byte)0, leadingZeros).Concat(bytes).ToArray();
    }
}