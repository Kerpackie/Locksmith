using Locksmith.Core.Config;

namespace Locksmith.Licensing.Config;

/// <summary>
/// Represents the options for license validation in the Locksmith application.
/// </summary>
public class LicenseValidationOptions : KeyServiceOptions
{

    /// <summary>
    /// Gets or sets a value indicating whether the license fields should 
    /// be validated. If set to <c>true</c>, the license fields will be 
    /// validated during the validation process.
    /// </summary>
    public bool ValidateLicenseFields { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether license type rules should
    /// be enforced during validation. If set to <c>true</c>, specific rules
    /// for license types will be applied.
    /// </summary>
    public bool EnforceLicenseTypeRules { get; set; } = false;

    /// <summary>
    /// Gets or sets a value indicating whether license scopes should be
    /// enforced during validation. If set to <c>true</c>, the license must
    /// include all required scopes to pass validation.
    /// </summary>
    public bool EnforceScopes { get; set; } = false;

    /// <summary>
    /// Gets or sets the list of required scopes for license validation.
    /// If <see cref="EnforceScopes"/> is <c>true</c>, the license must
    /// include all scopes in this list to be considered valid.
    /// </summary>
    public List<string>? RequiredScopes { get; set; }


}