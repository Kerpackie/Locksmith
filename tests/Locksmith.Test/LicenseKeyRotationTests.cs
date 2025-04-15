using System.Security.Cryptography;
using System.Text;
using Locksmith.Core.Services;
using Locksmith.Core.Utils;
using Microsoft.Extensions.DependencyInjection;

namespace Locksmith.Test;

public class LicenseKeyRotationTests : TestBase
{
    [Fact]
    public void Validate_Should_Pass_With_OldSecret_After_Rotation()
    {
        var oldSecret = "old-secret-key";
        var newSecret = "new-secret-key";

        var serviceWithOld = BuildServiceProvider(currentSecret: oldSecret)
            .GetRequiredService<LicenseKeyService>();
        var key = serviceWithOld.Generate(CreateTestLicense(DateTime.UtcNow.AddDays(5)));

        var serviceWithNew = BuildServiceProvider(currentSecret: newSecret, additionalSecrets: new[] { oldSecret })
            .GetRequiredService<LicenseKeyService>();
        var result = serviceWithNew.Validate(key);

        Assert.True(result.IsValid);
        Assert.Equal("Alice Example", result.LicenseInfo.Name);
    }

    [Fact]
    public void Generate_Should_Use_Only_CurrentSecret()
    {
        var currentSecret = "current-secret";
        var oldSecret = "old-secret";

        var service = BuildServiceProvider(currentSecret: currentSecret, additionalSecrets: new[] { oldSecret })
            .GetRequiredService<LicenseKeyService>();

        var license = CreateTestLicense(DateTime.UtcNow.AddDays(1));
        var key = service.Generate(license);

        var combined = Base58Encoder.Decode(key);
        var payloadBytes = combined[..^32];
        var actualSignature = combined[^32..];

        var expectedSignature = service.ComputeHmac(payloadBytes, Encoding.UTF8.GetBytes(currentSecret));

        Assert.True(CryptographicOperations.FixedTimeEquals(expectedSignature, actualSignature));
    }

    [Fact]
    public void Validate_Should_Succeed_With_PreviousSecret()
    {
        var oldSecret = "secret-v1";
        var newSecret = "secret-v2";

        var oldService = BuildServiceProvider(currentSecret: oldSecret)
            .GetRequiredService<LicenseKeyService>();
        var key = oldService.Generate(CreateTestLicense(DateTime.UtcNow.AddDays(5)));

        var rotatedService = BuildServiceProvider(currentSecret: newSecret, additionalSecrets: new[] { oldSecret })
            .GetRequiredService<LicenseKeyService>();
        var result = rotatedService.Validate(key);

        Assert.True(result.IsValid);
        Assert.Equal("Alice Example", result.LicenseInfo.Name);
    }

    [Fact]
    public void Validate_Should_Fail_When_NoSecretMatches()
    {
        var goodSecret = "good-secret";
        var wrongSecret = "totally-wrong-secret";

        var goodService = BuildServiceProvider(currentSecret: goodSecret)
            .GetRequiredService<LicenseKeyService>();
        var key = goodService.Generate(CreateTestLicense(DateTime.UtcNow.AddDays(2)));

        var wrongService = BuildServiceProvider(currentSecret: wrongSecret)
            .GetRequiredService<LicenseKeyService>();
        var result = wrongService.Validate(key);

        Assert.False(result.IsValid);
        Assert.True(result.IsTampered);
        Assert.Equal("Invalid signature.", result.Error);
    }

    [Fact]
    public void Validate_Should_Fail_If_SecretList_Empty()
    {
        var emptySecret = ""; // this will technically generate a valid HMAC with empty key, but decoding will fail
        var service = BuildServiceProvider(currentSecret: emptySecret)
            .GetRequiredService<LicenseKeyService>();

        var result = service.Validate("any-invalid-key");

        Assert.False(result.IsValid);
        Assert.True(result.IsMalformed);
    }

    [Fact]
    public void Validate_Should_Succeed_With_Any_Of_MultipleSecrets()
    {
        var secrets = new[] { "v1-key", "v2-key", "v3-key" };
        var usedSecret = secrets[1];

        var signerService = BuildServiceProvider(currentSecret: usedSecret)
            .GetRequiredService<LicenseKeyService>();
        var key = signerService.Generate(CreateTestLicense(DateTime.UtcNow.AddDays(3)));

        var validatingService = BuildServiceProvider(currentSecret: "new-key", additionalSecrets: secrets)
            .GetRequiredService<LicenseKeyService>();
        var result = validatingService.Validate(key);

        Assert.True(result.IsValid);
        Assert.Equal("Alice Example", result.LicenseInfo.Name);
    }
}