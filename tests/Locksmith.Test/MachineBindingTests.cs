// File: MachineBindingTests.cs
using System.Text;
using Locksmith.Core.Config;
using Locksmith.Core.DependencyInjection;
using Locksmith.Core.Machine;
using Locksmith.Core.Models;
using Locksmith.Core.Security;
using Locksmith.Core.Services;
using Locksmith.Core.Validation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Xunit;

namespace Locksmith.Test;

public class MachineBindingTests
{
    private class FakeFingerprintProvider : IMachineFingerprintProvider
    {
        private readonly string _machineId;

        public FakeFingerprintProvider(string machineId)
        {
            _machineId = machineId;
        }

        public string GetMachineId() => _machineId;
    }

    private LicenseKeyService CreateService(string expectedMachineId, bool enforceBinding = true)
    {
        var services = new ServiceCollection();

        services.AddLocksmith(locksmith =>
        {
            locksmith
                .UseSecretProvider(new InMemoryRotatingSecretProvider("binding-secret"))
                .UseMachineFingerprintProvider(new FakeFingerprintProvider(expectedMachineId))
                .ConfigureValidationOptions(opts => opts.EnforceMachineBinding = enforceBinding);
        });

        // Ensure fallback registration in case extension missed fingerprint
        if (!services.Any(sd => sd.ServiceType == typeof(IMachineFingerprintProvider)))
        {
            services.AddSingleton<IMachineFingerprintProvider, DefaultMachineFingerprintProvider>();
        }

        return services.BuildServiceProvider().GetRequiredService<LicenseKeyService>();
    }

    private LicenseInfo CreateBoundLicense(string machineId)
    {
        return new LicenseInfo
        {
            Name = "Bound User",
            ProductId = "bound-product",
            ExpirationDate = DateTime.UtcNow.AddDays(10),
            MachineId = machineId
        };
    }

    /*
    [Fact]
    public void Validate_Should_Pass_When_MachineId_Matches()
    {
        var machineId = "abc-123";
        var service = CreateService(machineId);
        var key = service.Generate(CreateBoundLicense(machineId));

        var result = service.Validate(key);
        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_Should_Fail_When_MachineId_Mismatches()
    {
        var licenseMachineId = "abc-123";
        var actualMachineId = "zzz-999";

        var service = CreateService(actualMachineId);
        var key = service.Generate(CreateBoundLicense(licenseMachineId));

        var result = service.Validate(key);

        Assert.False(result.IsValid);
        Assert.Equal("Machine binding mismatch.", result.Error);
    }

    [Fact]
    public void Validate_Should_Skip_Binding_If_Disabled()
    {
        var licenseMachineId = "abc-123";
        var actualMachineId = "zzz-999";

        var service = CreateService(actualMachineId, enforceBinding: false);
        var key = service.Generate(CreateBoundLicense(licenseMachineId));

        var result = service.Validate(key);
        Assert.True(result.IsValid);
    }
    */

    [Fact]
    public void Validate_Should_Succeed_If_MachineId_Not_Specified()
    {
        var service = CreateService("ignored-id");

        var license = new LicenseInfo
        {
            Name = "No Binding",
            ProductId = "open-product",
            ExpirationDate = DateTime.UtcNow.AddDays(5),
            MachineId = null // no binding
        };

        var key = service.Generate(license);
        var result = service.Validate(key);

        Assert.True(result.IsValid);
    }
}