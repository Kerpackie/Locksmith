namespace Locksmith.Core.Exceptions;

/// <summary>
/// Represents an exception that occurs during license validation.
/// </summary>
public class LicenseValidationException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="LicenseValidationException"/> class
    /// with a specified error message.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    public LicenseValidationException(string message) : base(message) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="LicenseValidationException"/> class
    /// with a specified error message and a reference to the inner exception that is
    /// the cause of this exception.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    /// <param name="innerException">The exception that is the cause of the current exception.</param>
    public LicenseValidationException(string message, Exception innerException) : base(message, innerException) { }
}