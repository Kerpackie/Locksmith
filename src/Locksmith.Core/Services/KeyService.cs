using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Locksmith.Core.Config;
using Locksmith.Core.Models;
using Locksmith.Core.Revocation;
using Locksmith.Core.Security;
using Locksmith.Core.Utils;
using Locksmith.Core.Validation;
using Microsoft.Extensions.Options;

namespace Locksmith.Core.Services;

public class KeyService<T> where T : KeyDescriptor
{
    private readonly ISecretProvider _secretProvider;
    private readonly IKeyValidator<T>? _validator;
    private readonly IKeyRevocationProvider<T>? _revocationProvider;
    private readonly KeyServiceOptions _options;

    public KeyService(ISecretProvider secretProvider, IKeyRevocationProvider<T>? revocationProvider, KeyServiceOptions options, IKeyValidator<T>? validator = null)
    {
        _secretProvider = secretProvider;
        _revocationProvider = revocationProvider;
        _options = options;
        _validator = validator;
    }

    public string Generate(T keyDescriptor)
    {
        if (_options.EnforceLimitValidation)
        {
            _validator?.Validate(keyDescriptor);
        }
        
        var payloadJson = JsonSerializer.Serialize(keyDescriptor);
        var payloadBytes = Encoding.UTF8.GetBytes(payloadJson);

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
                    
                    if (_revocationProvider?.IsRevoked(descriptor) == true)
                        return KeyValidationResult<T>.Fail("Key has been revoked.", descriptor);

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

    public KeyGenerationResult TryGenerate(T keyDescriptor)
    {
        try
        {
            var encoded = Generate(keyDescriptor);
            return KeyGenerationResult.Ok(encoded);
        }
        catch (Exception ex)
        {
            return KeyGenerationResult.Fail(ex.Message);
        }
    }

    public byte[] ComputeHmac(byte[] payload, byte[] key)
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
