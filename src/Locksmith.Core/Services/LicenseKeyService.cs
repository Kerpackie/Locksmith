using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Locksmith.Core.Models;
using Locksmith.Core.Utils;
using Locksmith.Core.Validation;

namespace Locksmith.Core.Services;

public class LicenseKeyService
{
    private readonly byte[] _secretKey;
    private readonly ILicenseValidator _licenseValidator;

    public LicenseKeyService(string secretKey, ILicenseValidator licenseValidator = null)
    {
        _licenseValidator = licenseValidator ?? new DefaultLicenseValidator();
        _secretKey = Encoding.UTF8.GetBytes(secretKey);
    }

    public string Generate(LicenseInfo licenseInfo)
    {
        _licenseValidator.Validate(licenseInfo);
        
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

            var sigLength = 32;
            var payloadBytes = combined[..^sigLength];
            var signatureBytes = combined[^sigLength..];

            var payloadJson = Encoding.UTF8.GetString(payloadBytes);
            
            var licenseInfo = JsonSerializer.Deserialize<LicenseInfo>(payloadJson);

            var expectedSignature = ComputeHmac(payloadBytes);
            if (!CryptographicOperations.FixedTimeEquals(signatureBytes, expectedSignature))
            {
                return ValidationResult.Invalid("Invalid signature.", licenseInfo);
            }

            if (licenseInfo.ExpirationDate is DateTime expiryDate && expiryDate < DateTime.UtcNow)
            {
                return ValidationResult.Invalid("License has expired.", licenseInfo);
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