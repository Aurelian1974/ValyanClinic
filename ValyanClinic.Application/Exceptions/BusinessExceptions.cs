using System.Runtime.Serialization;

namespace ValyanClinic.Application.Exceptions;

/// <summary>
/// Exceptie de baza pentru toate exceptiile de business din ValyanClinic
/// Provides structured error handling with Romanian messages
/// </summary>
[Serializable]
public abstract class BusinessException : Exception
{
    /// <summary>
    /// Codul de eroare specific pentru identificarea problemei
    /// </summary>
    public string ErrorCode { get; protected set; } = string.Empty;

    /// <summary>
    /// Detalii suplimentare despre eroare pentru logging
    /// </summary>
    public string? Details { get; protected set; }

    /// <summary>
    /// Timestamp cand a aparut eroarea
    /// </summary>
    public DateTime Timestamp { get; protected set; } = DateTime.Now;

    protected BusinessException() { }

    protected BusinessException(string message) : base(message) { }

    protected BusinessException(string message, Exception innerException) 
        : base(message, innerException) { }

    protected BusinessException(string message, string errorCode, string? details = null) 
        : base(message)
    {
        ErrorCode = errorCode;
        Details = details;
    }

    protected BusinessException(SerializationInfo info, StreamingContext context) 
        : base(info, context)
    {
        ErrorCode = info.GetString(nameof(ErrorCode)) ?? string.Empty;
        Details = info.GetString(nameof(Details));
        Timestamp = info.GetDateTime(nameof(Timestamp));
    }

    public override void GetObjectData(SerializationInfo info, StreamingContext context)
    {
        base.GetObjectData(info, context);
        info.AddValue(nameof(ErrorCode), ErrorCode);
        info.AddValue(nameof(Details), Details);
        info.AddValue(nameof(Timestamp), Timestamp);
    }
}

/// <summary>
/// Exceptie pentru cand o entitate nu este gasita
/// </summary>
[Serializable]
public class NotFoundException : BusinessException
{
    public NotFoundException(string entityName, object id) 
        : base($"{entityName} cu ID-ul {id} nu a fost gasit", "NOT_FOUND", $"Entity: {entityName}, ID: {id}")
    {
    }

    public NotFoundException(string entityName, string criteria, object value) 
        : base($"{entityName} cu {criteria} {value} nu a fost gasit", "NOT_FOUND", $"Entity: {entityName}, {criteria}: {value}")
    {
    }

    public NotFoundException(string message) 
        : base(message, "NOT_FOUND")
    {
    }

    protected NotFoundException(SerializationInfo info, StreamingContext context) 
        : base(info, context) { }
}

/// <summary>
/// Exceptie pentru incalcarea regulilor de business
/// </summary>
[Serializable]
public class BusinessRuleException : BusinessException
{
    /// <summary>
    /// Lista de reguli incalcate
    /// </summary>
    public List<string> ViolatedRules { get; protected set; } = new();

    public BusinessRuleException(string message) 
        : base(message, "BUSINESS_RULE_VIOLATION")
    {
    }

    public BusinessRuleException(string message, string rule) 
        : base(message, "BUSINESS_RULE_VIOLATION", $"Violated rule: {rule}")
    {
        ViolatedRules.Add(rule);
    }

    public BusinessRuleException(string message, List<string> violatedRules) 
        : base(message, "BUSINESS_RULE_VIOLATION", $"Violated rules: {string.Join(", ", violatedRules)}")
    {
        ViolatedRules = violatedRules;
    }

    public BusinessRuleException(string message, Exception innerException) 
        : base(message, innerException)
    {
        ErrorCode = "BUSINESS_RULE_VIOLATION";
    }

    protected BusinessRuleException(SerializationInfo info, StreamingContext context) 
        : base(info, context) 
    {
        ViolatedRules = (List<string>)(info.GetValue(nameof(ViolatedRules), typeof(List<string>)) ?? new List<string>());
    }

    public override void GetObjectData(SerializationInfo info, StreamingContext context)
    {
        base.GetObjectData(info, context);
        info.AddValue(nameof(ViolatedRules), ViolatedRules);
    }
}

/// <summary>
/// Exceptie pentru conflicte (ex: date duplicate, constrangeri unicitate)
/// </summary>
[Serializable]
public class ConflictException : BusinessException
{
    /// <summary>
    /// Tipul de conflict identificat
    /// </summary>
    public string ConflictType { get; protected set; } = string.Empty;

    /// <summary>
    /// Valoarea care a cauzat conflictul
    /// </summary>
    public string? ConflictingValue { get; protected set; }

    public ConflictException(string message) 
        : base(message, "CONFLICT")
    {
    }

    public ConflictException(string message, string conflictType, string? conflictingValue = null) 
        : base(message, "CONFLICT", $"Type: {conflictType}, Value: {conflictingValue}")
    {
        ConflictType = conflictType;
        ConflictingValue = conflictingValue;
    }

    public ConflictException(string message, Exception innerException) 
        : base(message, innerException)
    {
        ErrorCode = "CONFLICT";
    }

    protected ConflictException(SerializationInfo info, StreamingContext context) 
        : base(info, context) 
    {
        ConflictType = info.GetString(nameof(ConflictType)) ?? string.Empty;
        ConflictingValue = info.GetString(nameof(ConflictingValue));
    }

    public override void GetObjectData(SerializationInfo info, StreamingContext context)
    {
        base.GetObjectData(info, context);
        info.AddValue(nameof(ConflictType), ConflictType);
        info.AddValue(nameof(ConflictingValue), ConflictingValue);
    }
}

/// <summary>
/// Exceptie pentru probleme de validare
/// </summary>
[Serializable]
public class ValidationException : BusinessException
{
    /// <summary>
    /// Lista de erori de validare
    /// </summary>
    public Dictionary<string, List<string>> ValidationErrors { get; protected set; } = new();

    public ValidationException(string message) 
        : base(message, "VALIDATION_ERROR")
    {
    }

    public ValidationException(string property, string error) 
        : base($"Validare esuata pentru {property}: {error}", "VALIDATION_ERROR")
    {
        ValidationErrors.Add(property, new List<string> { error });
    }

    public ValidationException(Dictionary<string, List<string>> errors) 
        : base("Validare esuata", "VALIDATION_ERROR", $"Properties: {string.Join(", ", errors.Keys)}")
    {
        ValidationErrors = errors;
    }

    public ValidationException(string message, Dictionary<string, List<string>> errors) 
        : base(message, "VALIDATION_ERROR", $"Properties: {string.Join(", ", errors.Keys)}")
    {
        ValidationErrors = errors;
    }

    protected ValidationException(SerializationInfo info, StreamingContext context) 
        : base(info, context) 
    {
        ValidationErrors = (Dictionary<string, List<string>>)(info.GetValue(nameof(ValidationErrors), typeof(Dictionary<string, List<string>>)) ?? new Dictionary<string, List<string>>());
    }

    public override void GetObjectData(SerializationInfo info, StreamingContext context)
    {
        base.GetObjectData(info, context);
        info.AddValue(nameof(ValidationErrors), ValidationErrors);
    }

    /// <summary>
    /// Obtine toate erorile ca string formatat
    /// </summary>
    public string GetFormattedErrors()
    {
        return string.Join(Environment.NewLine, 
            ValidationErrors.SelectMany(kv => kv.Value.Select(v => $"• {kv.Key}: {v}")));
    }
}

/// <summary>
/// Exceptie pentru operatii neautorizate
/// </summary>
[Serializable]
public class UnauthorizedException : BusinessException
{
    /// <summary>
    /// Actiunea care a fost incercata
    /// </summary>
    public string? AttemptedAction { get; protected set; }

    /// <summary>
    /// Utilizatorul care a incercat actiunea
    /// </summary>
    public string? UserId { get; protected set; }

    public UnauthorizedException(string message) 
        : base(message, "UNAUTHORIZED")
    {
    }

    public UnauthorizedException(string message, string attemptedAction, string? userId = null) 
        : base(message, "UNAUTHORIZED", $"Action: {attemptedAction}, User: {userId}")
    {
        AttemptedAction = attemptedAction;
        UserId = userId;
    }

    protected UnauthorizedException(SerializationInfo info, StreamingContext context) 
        : base(info, context) 
    {
        AttemptedAction = info.GetString(nameof(AttemptedAction));
        UserId = info.GetString(nameof(UserId));
    }

    public override void GetObjectData(SerializationInfo info, StreamingContext context)
    {
        base.GetObjectData(info, context);
        info.AddValue(nameof(AttemptedAction), AttemptedAction);
        info.AddValue(nameof(UserId), UserId);
    }
}

/// <summary>
/// Exceptie pentru operatii interzise
/// </summary>
[Serializable]
public class ForbiddenException : BusinessException
{
    /// <summary>
    /// Motivul pentru care operatia este interzisa
    /// </summary>
    public string? Reason { get; protected set; }

    public ForbiddenException(string message) 
        : base(message, "FORBIDDEN")
    {
    }

    public ForbiddenException(string message, string reason) 
        : base(message, "FORBIDDEN", reason)
    {
        Reason = reason;
    }

    protected ForbiddenException(SerializationInfo info, StreamingContext context) 
        : base(info, context) 
    {
        Reason = info.GetString(nameof(Reason));
    }

    public override void GetObjectData(SerializationInfo info, StreamingContext context)
    {
        base.GetObjectData(info, context);
        info.AddValue(nameof(Reason), Reason);
    }
}

/// <summary>
/// Exceptie pentru erori externe (servicii terte, API-uri, baze de date)
/// </summary>
[Serializable]
public class ExternalServiceException : BusinessException
{
    /// <summary>
    /// Numele serviciului extern care a cauzat eroarea
    /// </summary>
    public string ServiceName { get; protected set; } = string.Empty;

    /// <summary>
    /// Codul de eroare returnat de serviciul extern
    /// </summary>
    public string? ExternalErrorCode { get; protected set; }

    public ExternalServiceException(string serviceName, string message) 
        : base($"Eroare serviciu extern {serviceName}: {message}", "EXTERNAL_SERVICE_ERROR", $"Service: {serviceName}")
    {
        ServiceName = serviceName;
    }

    public ExternalServiceException(string serviceName, string message, string externalErrorCode) 
        : base($"Eroare serviciu extern {serviceName}: {message}", "EXTERNAL_SERVICE_ERROR", $"Service: {serviceName}, ExternalCode: {externalErrorCode}")
    {
        ServiceName = serviceName;
        ExternalErrorCode = externalErrorCode;
    }

    public ExternalServiceException(string serviceName, string message, Exception innerException) 
        : base($"Eroare serviciu extern {serviceName}: {message}", innerException)
    {
        ErrorCode = "EXTERNAL_SERVICE_ERROR";
        ServiceName = serviceName;
        Details = $"Service: {serviceName}";
    }

    protected ExternalServiceException(SerializationInfo info, StreamingContext context) 
        : base(info, context) 
    {
        ServiceName = info.GetString(nameof(ServiceName)) ?? string.Empty;
        ExternalErrorCode = info.GetString(nameof(ExternalErrorCode));
    }

    public override void GetObjectData(SerializationInfo info, StreamingContext context)
    {
        base.GetObjectData(info, context);
        info.AddValue(nameof(ServiceName), ServiceName);
        info.AddValue(nameof(ExternalErrorCode), ExternalErrorCode);
    }
}

/// <summary>
/// Exceptie pentru cand resursa este in uz si nu poate fi modificata/stearsa
/// </summary>
[Serializable]
public class ResourceInUseException : BusinessException
{
    /// <summary>
    /// Tipul resursei care este in uz
    /// </summary>
    public string ResourceType { get; protected set; } = string.Empty;

    /// <summary>
    /// ID-ul resursei care este in uz
    /// </summary>
    public string ResourceId { get; protected set; } = string.Empty;

    /// <summary>
    /// Lista de dependente care impiedica operatia
    /// </summary>
    public List<string> Dependencies { get; protected set; } = new();

    public ResourceInUseException(string resourceType, string resourceId, List<string> dependencies) 
        : base($"{resourceType} cu ID-ul {resourceId} nu poate fi {(dependencies.Any() ? "modificat/sters" : "procesat")} deoarece este in uz", 
               "RESOURCE_IN_USE", $"Type: {resourceType}, ID: {resourceId}, Dependencies: {string.Join(", ", dependencies)}")
    {
        ResourceType = resourceType;
        ResourceId = resourceId;
        Dependencies = dependencies;
    }

    public ResourceInUseException(string resourceType, string resourceId, string dependency) 
        : this(resourceType, resourceId, new List<string> { dependency })
    {
    }

    protected ResourceInUseException(SerializationInfo info, StreamingContext context) 
        : base(info, context) 
    {
        ResourceType = info.GetString(nameof(ResourceType)) ?? string.Empty;
        ResourceId = info.GetString(nameof(ResourceId)) ?? string.Empty;
        Dependencies = (List<string>)(info.GetValue(nameof(Dependencies), typeof(List<string>)) ?? new List<string>());
    }

    public override void GetObjectData(SerializationInfo info, StreamingContext context)
    {
        base.GetObjectData(info, context);
        info.AddValue(nameof(ResourceType), ResourceType);
        info.AddValue(nameof(ResourceId), ResourceId);
        info.AddValue(nameof(Dependencies), Dependencies);
    }
}

/// <summary>
/// Extension methods pentru handling-ul exceptiilor
/// </summary>
public static class BusinessExceptionExtensions
{
    /// <summary>
    /// Verifica daca exceptia este o exceptie de business
    /// </summary>
    public static bool IsBusinessException(this Exception ex) => ex is BusinessException;

    /// <summary>
    /// Obtine codul de eroare din exceptie
    /// </summary>
    public static string GetErrorCode(this Exception ex) => 
        ex is BusinessException businessEx ? businessEx.ErrorCode : "UNKNOWN_ERROR";

    /// <summary>
    /// Obtine detaliile din exceptie
    /// </summary>
    public static string? GetDetails(this Exception ex) => 
        ex is BusinessException businessEx ? businessEx.Details : null;

    /// <summary>
    /// Converteste exceptia la un obiect structurat pentru API
    /// </summary>
    public static object ToApiResponse(this BusinessException ex) => new
    {
        error = ex.ErrorCode,
        message = ex.Message,
        details = ex.Details,
        timestamp = ex.Timestamp,
        // Adauga proprietati specifice bazate pe tipul exceptiei
        additionalInfo = ex switch
        {
            ValidationException validationEx => (object)new { validationErrors = validationEx.ValidationErrors },
            ConflictException conflictEx => (object)new { conflictType = conflictEx.ConflictType, conflictingValue = conflictEx.ConflictingValue },
            BusinessRuleException businessRuleEx => (object)new { violatedRules = businessRuleEx.ViolatedRules },
            ResourceInUseException resourceEx => (object)new { resourceType = resourceEx.ResourceType, resourceId = resourceEx.ResourceId, dependencies = resourceEx.Dependencies },
            UnauthorizedException unauthorizedEx => (object)new { attemptedAction = unauthorizedEx.AttemptedAction, userId = unauthorizedEx.UserId },
            ExternalServiceException externalEx => (object)new { serviceName = externalEx.ServiceName, externalErrorCode = externalEx.ExternalErrorCode },
            _ => (object?)null
        }
    };
}
