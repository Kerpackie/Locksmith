using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Locksmith.Core.Models;
using Locksmith.Core.Utils;

namespace Locksmith.Core.Services;

public class LicenseKeyService
{
    private readonly byte[] _secretKey;

    public LicenseKeyService(string secretKey)
    {
        _secretKey = Encoding.UTF8.GetBytes(secretKey);
    }

    public string Generate(LicenseInfo licenseInfo)
    {
        var payloadJson = JsonSerializer.Serialize(licenseInfo);
        var payloadBytes = Encoding.UTF8.GetBytes(payloadJson);
        
        var signatureBytes = ComputeHmac(payloadBytes);
        var combined = Combine(payloadBytes, signatureBytes);
        
        return Base58Encoder.Encode(combined);
    }
    
    public ValidationResult Validate(string licenseKey)
    {
        try
        {
            var combined = Base58Encoder.Decode(licenseKey);
            
            // Assume at least 32 bytes are HMAC-SHA256
            var sigLength = 32;
            var payloadBytes = combined[..^sigLength];
            var signatureBytes = combined[^sigLength..];
            
            var expectedSignature = ComputeHmac(payloadBytes);

            if (!CryptographicOperations.FixedTimeEquals(signatureBytes, expectedSignature))
            {
                return ValidationResult.Invalid("Invalid signature.");
            }
            
            var payloadJson = Encoding.UTF8.GetString(payloadBytes);
            var licenseInfo = JsonSerializer.Deserialize<LicenseInfo>(payloadJson);

            if (licenseInfo.ExpirationDate is DateTime expiryDate && expiryDate < DateTime.UtcNow)
            {
                return ValidationResult.Invalid("License has expired.");
            }
            
            return ValidationResult.Valid(licenseInfo);
        }
        catch (Exception exception)
        {
            return ValidationResult.Invalid($"Validation failed: {exception.Message}");
        }
    }
    
    private byte[] ComputeHmac(byte[] payloadBytes)
    {
        using var hmac = new HMACSHA256(_secretKey);
        return hmac.ComputeHash(payloadBytes);
    }
    
    private byte[] Combine(byte[] payloadBytes, byte[] signatureBytes)
    {
        var result = new byte[payloadBytes.Length + signatureBytes.Length];
        
        Buffer.BlockCopy(payloadBytes, 0, result, 0, payloadBytes.Length);
        Buffer.BlockCopy(signatureBytes, 0, result, payloadBytes.Length, signatureBytes.Length);

        return result;
    }
}