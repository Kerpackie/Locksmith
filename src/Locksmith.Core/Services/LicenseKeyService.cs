using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Locksmith.Core.Config;
using Locksmith.Core.Exceptions;
using Locksmith.Core.Models;
using Locksmith.Core.Utils;
using Locksmith.Core.Validation;

namespace Locksmith.Core.Services;

public class LicenseKeyService
{
    private readonly byte[] _secretKey;
    private readonly ILicenseValidator _licenseValidator;
    private readonly LicenseValidationOptions _options;

    public LicenseKeyService(string secretKey, LicenseValidationOptions options = null, ILicenseValidator validator = null)
    {
        _secretKey = Encoding.UTF8.GetBytes(secretKey);
        _options = options ?? new LicenseValidationOptions();
        _licenseValidator = validator ?? new DefaultLicenseValidator(_options);
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
    
    public LicenseGenerationResult TryGenerate(LicenseInfo licenseInfo)
    {
        try
        {
            var key = Generate(licenseInfo); // uses existing method
            return LicenseGenerationResult.Ok(key);
        }
        catch (LicenseValidationException ex)
        {
            return LicenseGenerationResult.Fail(ex.Message);
        }
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
            
            if (_options.ValidateLicenseFields)
            {
                _licenseValidator.Validate(licenseInfo);
            }

            return ValidationResult.Valid(licenseInfo);
        }
        catch (LicenseValidationException ex)
        {
            return ValidationResult.Invalid(ex.Message);
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