namespace ValyanClinic.Core.Exceptions;

public abstract class ValyanClinicException : Exception
{
    protected ValyanClinicException(string message) : base(message) { }
    protected ValyanClinicException(string message, Exception innerException) : base(message, innerException) { }
}

public class ValidationException : ValyanClinicException
{
    public List<string> Errors { get; }

    public ValidationException(string message) : base(message)
    {
        Errors = new List<string> { message };
    }

    public ValidationException(List<string> errors) : base("Erori de validare")
    {
        Errors = errors;
    }

    public ValidationException(string message, List<string> errors) : base(message)
    {
        Errors = errors;
    }
}

public class NotFoundException : ValyanClinicException
{
    public string EntityName { get; }
    public object EntityId { get; }

    public NotFoundException(string entityName, object entityId) 
        : base($"{entityName} cu ID-ul {entityId} nu a fost gasit")
    {
        EntityName = entityName;
        EntityId = entityId;
    }

    public NotFoundException(string message) : base(message)
    {
        EntityName = string.Empty;
        EntityId = 0;
    }
}

public class BusinessRuleException : ValyanClinicException
{
    public string RuleName { get; }

    public BusinessRuleException(string ruleName, string message) : base(message)
    {
        RuleName = ruleName;
    }

    public BusinessRuleException(string message) : base(message)
    {
        RuleName = string.Empty;
    }
}

public class ConcurrencyException : ValyanClinicException
{
    public ConcurrencyException(string message) : base(message) { }
    
    public ConcurrencyException() : base("Datele au fost modificate de alt utilizator. Va rugam reincarcati pagina.") { }
}

public class UnauthorizedException : ValyanClinicException
{
    public UnauthorizedException(string message) : base(message) { }
    
    public UnauthorizedException() : base("Nu aveti permisiunea necesara pentru aceasta operatie.") { }
}

public class ExternalServiceException : ValyanClinicException
{
    public string ServiceName { get; }

    public ExternalServiceException(string serviceName, string message) : base(message)
    {
        ServiceName = serviceName;
    }

    public ExternalServiceException(string serviceName, string message, Exception innerException) 
        : base(message, innerException)
    {
        ServiceName = serviceName;
    }
}