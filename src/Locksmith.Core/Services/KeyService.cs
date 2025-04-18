using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Locksmith.Core.Models;
using Locksmith.Core.Security;
using Locksmith.Core.Utils;

namespace Locksmith.Core.Services;

public class KeyService<T> where T : KeyDescriptor
{
    private readonly ISecretProvider _secretProvider;

    public KeyService(ISecretProvider secretProvider)
    {
        _secretProvider = secretProvider;
    }

    public string Generate(T keyDescriptor)
    {
        var payloadJson = JsonSerializer.Serialize(keyDescriptor);
        var payloadBytes = System.Text.Encoding.UTF8.GetBytes(payloadJson);

        var signatureBytes = ComputeHmac(payloadBytes, _secretProvider.GetCurrentSecret());
        
        var combined = Combine(payloadBytes, signatureBytes);
        
        return Base58Encoder.Encode(combined);
    }

    public KeyValidationResult<T> Validate(string encodedKey)
    {
        try
        {
            var combined = Base58Encoder.Decode(encodedKey);
            var sigLength = 32;
            var payloadBytes = combined[..^sigLength];
            var signatureBytes = combined[^sigLength..];

            var payloadJson = Encoding.UTF8.GetString(payloadBytes);
            var descriptor = JsonSerializer.Deserialize<T>(payloadJson);

            if (descriptor == null)
                return KeyValidationResult<T>.Fail("Malformed payload.");

            foreach (var secret in _secretProvider.GetAllValidationSecrets())
            {
                var expected = ComputeHmac(payloadBytes, secret);
                if (CryptographicOperations.FixedTimeEquals(expected, signatureBytes))
                {
                    if (descriptor.Expiration.HasValue && descriptor.Expiration.Value < DateTime.UtcNow)
                        return KeyValidationResult<T>.Fail("Key has expired.", descriptor);

                    return KeyValidationResult<T>.Success(descriptor);
                }
            }

            return KeyValidationResult<T>.Fail("Invalid signature.", descriptor);
        }
        catch (Exception ex)
        {
            return KeyValidationResult<T>.Fail("Validation failed: " + ex.Message);
        }
    }

    private byte[] ComputeHmac(byte[] payload, byte[] key)
    {
        using var hmac = new HMACSHA256(key);
        return hmac.ComputeHash(payload);
    }

    private byte[] Combine(byte[] payload, byte[] signature)
    {
        var result = new byte[payload.Length + signature.Length];
        Buffer.BlockCopy(payload, 0, result, 0, payload.Length);
        Buffer.BlockCopy(signature, 0, result, payload.Length, signature.Length);
        return result;
    }
    
}