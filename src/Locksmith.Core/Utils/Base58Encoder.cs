using System.Numerics;
using System.Text;

namespace Locksmith.Core.Utils;

public static class Base58Encoder
{
    private const string Alphabet = "123456789ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz";
    private static readonly BigInteger Base = new BigInteger(58);

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