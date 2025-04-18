using Locksmith.Core.Extensions;
using Locksmith.Licensing.Models;

namespace Locksmith.Licensing.Test;

public class LicenseUsageLimitTests : TestBase
{
    private LicenseDescriptor CreateLicense(Dictionary<string, int>? limits = null)
    {
        return new LicenseDescriptor
        {
            Name = "Usage Test",
            ProductId = "usage.test",
            Expiration = DateTime.Now.AddDays(10),
            Limits = limits
        };
    }

    [Fact]
    public void HasLimit_Should_Return_True_When_Limit_Is_Defined()
    {
        var license = CreateLicense(
            new Dictionary<string, int>
            {
                {
                    "MaxUsers", 10
                }
            });

        Assert.True(license.HasLimit("MaxUsers", out var val));
        Assert.Equal(10, val);
    }

    [Fact]
    public void HasLimit_Should_Return_False_When_Limit_Is_Not_Defined()
    {
        var license = CreateLicense(
            new Dictionary<string, int>
            {
                {
                    "MaxUsers", 10
                }
            });
        
        Assert.False(license.HasLimit("MaxProjects", out _));
    }

    [Fact]
    public void GetLimit_Should_Return_Value_When_Limit_Exits()
    {
        var license = CreateLicense(
            new Dictionary<string, int>
            {
                {
                    "MaxDevices", 5
                }
            });
        
        Assert.Equal(5, license.GetLimit("MaxDevices"));
    }

    [Fact]
    public void GetLimit_Should_Return_Null_When_Limit_Not_Exist()
    {
        var license = CreateLicense(
            new Dictionary<string, int>
            {
                {
                    "MaxDevices", 5
                }
            });
        
        Assert.Null(license.GetLimit("Nonexistent"));
    }

    [Fact]
    public void GetLimit_Should_Handle_Null_Limits_Gracefully()
    {
        var license = CreateLicense(null);
        Assert.Null(license.GetLimit("Any"));
    }

    [Fact]
    public void HasLimit_Should_Handle_Null_License_Gracefully()
    {
        LicenseDescriptor? license = null;
        Assert.False(license.HasLimit("Any", out _));
    }
    
    [Fact]
    public void HasLimit_Should_Handle_Null_Limits_Gracefully()
    {
        var license = new LicenseDescriptor
        {
            Limits = null
        };
        
        Assert.False(license.HasLimit("Any", out _));
    }
}