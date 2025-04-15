namespace Locksmith.Core.Exceptions;

public class LicenseValidationException : Exception
{
    public LicenseValidationException(string message) : base(message) { }
    
    public LicenseValidationException(string message, Exception innerException) : base(message, innerException) { }
}