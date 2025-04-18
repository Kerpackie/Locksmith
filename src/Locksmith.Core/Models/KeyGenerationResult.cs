namespace Locksmith.Core.Models;

/// <summary>
/// Represents the result of a key generation attempt.
/// </summary>
public class KeyGenerationResult
{
    public bool Success { get; }
    public string? EncodedKey { get; }
    public string? Error { get; }

    private KeyGenerationResult(bool success, string? encodedKey = null, string? error = null)
    {
        Success = success;
        EncodedKey = encodedKey;
        Error = error;
    }

    public static KeyGenerationResult Ok(string key) => new(true, encodedKey: key);

    public static KeyGenerationResult Fail(string error) => new(false, error: error);
}