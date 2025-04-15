
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Locksmith.Core.Config;
using Locksmith.Core.Exceptions;
using Locksmith.Core.Models;
using Locksmith.Core.Security;
using Locksmith.Core.Utils;
using Locksmith.Core.Validation;
using Microsoft.Extensions.Options;

[assembly: InternalsVisibleTo("Locksmith.Test")]

namespace Locksmith.Core.Services;

public class LicenseKeyService
{
    private readonly ISecretProvider _secretProvider;
    private readonly ILicenseValidator _licenseValidator;
    private readonly LicenseValidationOptions _options;

    public LicenseKeyService(
        ISecretProvider secretProvider,
        ILicenseValidator licenseValidator,
        IOptions<LicenseValidationOptions> options)
    {
        _secretProvider = secretProvider;
        _licenseValidator = licenseValidator;
        _options = options?.Value ?? new LicenseValidationOptions();
    }



    public string Generate(LicenseInfo licenseInfo)
    {
        if (_options.ValidateLicenseFields)
        {
            _licenseValidator.Validate(licenseInfo);
        }
        
        var payloadJson = JsonSerializer.Serialize(licenseInfo);
        var payloadBytes = Encoding.UTF8.GetBytes(payloadJson);
        
        var signatureBytes = ComputeHmac(payloadBytes, _secretProvider.GetCurrentSecret());
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

            var isSignatureValid = _secretProvider
                .GetAllValidationSecrets()
                .Any(secret =>
                {
                    var expected = ComputeHmac(payloadBytes, secret);
                    return CryptographicOperations.FixedTimeEquals(signatureBytes, expected);
                });

            if (!isSignatureValid)
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

    
    internal byte[] ComputeHmac(byte[] payloadBytes, byte[] secret)
    {
        using var hmac = new HMACSHA256(secret);
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