using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Locksmith.Core.Models;
using Locksmith.Core.Security;
using Locksmith.Core.Config;
using Locksmith.Core.Utils;
using Microsoft.Extensions.Options;

namespace Locksmith.Core.Services
{
    public class KeyService<T> where T : KeyDescriptor
    {
        private readonly ISecretProvider _secretProvider;
        private readonly KeyServiceOptions _options;

        public KeyService(
            ISecretProvider secretProvider,
            IOptions<KeyServiceOptions> options)
        {
            _secretProvider = secretProvider;
            _options = options.Value;
        }

        public string Generate(T descriptor)
        {
            // -- no extra validation --
            var json    = JsonSerializer.Serialize(descriptor);
            var payload = Encoding.UTF8.GetBytes(json);
            var sig     = ComputeHmac(payload, _secretProvider.GetCurrentSecret());
            var blob    = payload.Concat(sig).ToArray();
            return Base58Encoder.Encode(blob);
        }

        public KeyValidationResult<T> Validate(string encoded)
        {
            try
            {
                var blob      = Base58Encoder.Decode(encoded);
                var sigLen    = 32;
                var payload   = blob[..^sigLen];
                var signature = blob[^sigLen..];

                // check signature
                var validSig = _secretProvider
                    .GetAllValidationSecrets()
                    .Any(sec =>
                        CryptographicOperations.FixedTimeEquals(
                            ComputeHmac(payload, sec),
                            signature
                        )
                    );

                if (!validSig)
                    return KeyValidationResult<T>.Fail("Invalid signature.");

                // rehydrate
                var json      = Encoding.UTF8.GetString(payload);
                var descriptor = JsonSerializer.Deserialize<T>(json)
                                 ?? throw new Exception("Malformed payload");

                // check expiration
                if (descriptor.Expiration.HasValue &&
                    descriptor.Expiration.Value.Add(_options.ClockSkew) < DateTime.UtcNow)
                {
                    return KeyValidationResult<T>.Fail("Key has expired.", descriptor);
                }

                return KeyValidationResult<T>.Success(descriptor);
            }
            catch (Exception ex)
            {
                return KeyValidationResult<T>.Fail("Validation failed: " + ex.Message);
            }
        }

        private byte[] ComputeHmac(byte[] data, byte[] key)
        {
            using var hmac = new HMACSHA256(key);
            return hmac.ComputeHash(data);
        }
    }
}
