using Locksmith.Licensing.Extensions;
using Locksmith.Licensing.Models;

namespace Locksmith.Licensing.Test;

public class LicenseFeatureFlagTests : TestBase
{
    private LicenseDescriptor CreateLicense(params string[] scopes)
    {
        return new LicenseDescriptor
        {
            Name = "Feature Test",
            ProductId = "feature-product",
            Expiration = DateTime.UtcNow.AddDays(10),
            Scopes = scopes.ToList()
        };
    }

    [Fact]
    public void HasFeature_Should_Return_True_If_Present()
    {
        var license = CreateLicense("feature:export", "tier:pro");
        Assert.True(license.HasFeature("feature:export"));
    }
    
    [Fact]
    public void HasFeature_Should_Return_False_If_Not_Present()
    {
        var license = CreateLicense("feature:export", "tier:pro");
        Assert.False(license.HasFeature("feature:import"));
    }

    [Fact]
    public void HasFeature_Should_Return_True_If_At_Least_One_Present()
    {
        var license = CreateLicense("feature:import", "analytics");
        Assert.True(license.HasAnyFeature("analytics", "export"));
    }

    [Fact]
    public void HasFeature_Should_Return_False_If_Any_Missing()
    {
        var license = CreateLicense("export", "pro");
        Assert.False(license.HasAllFeatures("export", "analytics"));
    }

    [Fact]
    public void HasFeature_Should_Null_License_Gracefully()
    {
        LicenseDescriptor? license = null;
        Assert.False(license.HasFeature("anything"));
    }

    [Fact]
    public void HasFeature_Should_Handle_Null_Scopes_Gracefully()
    {
        var license = new LicenseDescriptor
        {
            Scopes = null
        };
        
        Assert.False(license.HasFeature("anything"));
    }
}