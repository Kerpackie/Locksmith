namespace Locksmith.Licensing.Test;

public class LicenseMetadataTests : TestBase
{
    private LicenseInfo CreateLicense(Dictionary<string, string> metadata = null)
    {
        return new LicenseInfo
        {
            Name = "Metadata User",
            ProductId = "metadata.product",
            ExpirationDate = DateTime.Now.AddDays(10),
            Metadata = metadata
        };
    }

    [Fact]
    public void GetMetadata_Should_Return_Correct_Value_If_Key_Exists()
    {
        var license = CreateLicense(new Dictionary<string, string>
        {
            { "Region", "EU" },
            { "TenantId", "acme" }
        });
        
        Assert.Equal("EU", license.GetMetadata("Region"));
        Assert.Equal("acme", license.GetMetadata("TenantId"));
    }

    [Fact]
    public void GetMetadata_Should_Return_Null_If_Key_Not_Found()
    {
        var license = CreateLicense(
            new Dictionary<string, string>
            {
                { "Environment", "prod" }
            });
        
        Assert.Null(license.GetMetadata("MissingKey"));
    }

    [Fact]
    public void HasMetadata_Should_Return_True_If_Key_Exists()
    {
        var license = CreateLicense(
            new Dictionary<string, string>
            {
                { "IssuedBy", "admin" }
            });
        
        Assert.True(license.HasMetadata("IssuedBy"));
    }
    
    [Fact]
    public void HasMetadata_Should_Return_False_If_Key_Not_Exists()
    {
        var license = CreateLicense(new Dictionary<string, string>());
        Assert.False(license.HasMetadata("NotThere"));
    }

    [Fact]
    public void GetMetadata_Should_Handle_Null_Metadata_Gracefully()
    {
        var license = CreateLicense(null);
        Assert.Null(license.GetMetadata("Anything"));
    }

    [Fact]
    public void HasMetadata_Should_Handle_Null_Metadata_Gracefully()
    {
        var license = CreateLicense(null);
        Assert.False(license.HasMetadata("Missing"));
    }

    [Fact]
    public void HasMetadata_Should_Handle_Null_License_Gracefully()
    {
        LicenseInfo? license = null;
        Assert.False(license.HasMetadata("TenantId"));
    }
}