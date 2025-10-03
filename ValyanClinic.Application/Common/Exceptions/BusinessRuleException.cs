namespace ValyanClinic.Application.Common.Exceptions;

/// <summary>
/// Exceptie pentru incalcarea regulilor de business
/// </summary>
public class BusinessRuleException : Exception
{
    public BusinessRuleException(string message)
        : base(message)
    {
    }
    
    public BusinessRuleException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
