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

/// <summary>
/// Provides functionality for generating and validating license keys.
/// </summary>
public class LicenseKeyService
{
    private readonly ISecretProvider _secretProvider;
    private readonly ILicenseValidator _licenseValidator;
    private readonly LicenseValidationOptions _options;

    /// <summary>
    /// Initializes a new instance of the <see cref="LicenseKeyService"/> class.
    /// </summary>
    /// <param name="secretProvider">The secret provider used for HMAC operations.</param>
    /// <param name="licenseValidator">The license validator for validating license fields.</param>
    /// <param name="options">The license validation options.</param>
    public LicenseKeyService(
        ISecretProvider secretProvider,
        ILicenseValidator licenseValidator,
        IOptions<LicenseValidationOptions> options)
    {
        _secretProvider = secretProvider;
        _licenseValidator = licenseValidator;
        _options = options?.Value ?? new LicenseValidationOptions();
    }

    /// <summary>
    /// Generates a license key for the given license information.
    /// </summary>
    /// <param name="licenseInfo">The license information to encode in the license key.</param>
    /// <returns>The generated license key as a string.</returns>
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

    /// <summary>
    /// Attempts to generate a license key and returns the result.
    /// </summary>
    /// <param name="licenseInfo">The license information to encode in the license key.</param>
    /// <returns>A <see cref="LicenseGenerationResult"/> indicating success or failure.</returns>
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

    /// <summary>
    /// Validates a given license key and returns the validation result.
    /// </summary>
    /// <param name="licenseKey">The license key to validate.</param>
    /// <returns>A <see cref="ValidationResult"/> indicating the validation outcome.</returns>
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

    /// <summary>
    /// Computes the HMAC for the given payload using the specified secret.
    /// </summary>
    /// <param name="payloadBytes">The payload as a byte array.</param>
    /// <param name="secret">The secret key as a byte array.</param>
    /// <returns>The computed HMAC as a byte array.</returns>
    internal byte[] ComputeHmac(byte[] payloadBytes, byte[] secret)
    {
        using var hmac = new HMACSHA256(secret);
        return hmac.ComputeHash(payloadBytes);
    }

    /// <summary>
    /// Combines the payload and signature into a single byte array.
    /// </summary>
    /// <param name="payloadBytes">The payload as a byte array.</param>
    /// <param name="signatureBytes">The signature as a byte array.</param>
    /// <returns>The combined byte array.</returns>
    private byte[] Combine(byte[] payloadBytes, byte[] signatureBytes)
    {
        var result = new byte[payloadBytes.Length + signatureBytes.Length];

        Buffer.BlockCopy(payloadBytes, 0, result, 0, payloadBytes.Length);
        Buffer.BlockCopy(signatureBytes, 0, result, payloadBytes.Length, signatureBytes.Length);

        return result;
    }
}