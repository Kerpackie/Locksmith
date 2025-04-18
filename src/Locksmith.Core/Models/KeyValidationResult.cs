namespace Locksmith.Core.Models;

public class KeyValidationResult<T> where T : KeyDescriptor
{
    public bool IsValid { get; }
    public string? Error { get; }
    public T? Key { get; }

    private KeyValidationResult(bool isValid, string? error, T? key)
    {
        IsValid = isValid;
        Error = error;
        Key = key;
    }

    public static KeyValidationResult<T> Success(T key) => new(true, null, key);

    public static KeyValidationResult<T> Fail(string error, T? key = null) => new(false, error, key);

    public bool IsExpired => Error == "Key has expired.";
    public bool IsTampered => Error == "Invalid signature.";
    public bool IsMalformed => Error != null && Error.StartsWith("Validation failed") || Error == "Malformed payload.";
}