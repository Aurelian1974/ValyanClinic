using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Text.Json;
using ValyanClinic.Application.Exceptions;

namespace ValyanClinic.Application.Middleware;

/// <summary>
/// Middleware pentru gestionarea centralizată a excepțiilor în aplicația ValyanClinic
/// Interceptează toate excepțiile și le convertește în răspunsuri HTTP structurate
/// </summary>
public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        };
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unhandled exception occurred while processing the request");
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        // Setează content type pentru răspuns
        context.Response.ContentType = "application/json";

        // Determină status code și mesajul în funcție de tipul excepției
        var (statusCode, response) = exception switch
        {
            // Business Exceptions - mapare specifică
            NotFoundException notFoundEx => (
                HttpStatusCode.NotFound,
                CreateErrorResponse("NOT_FOUND", notFoundEx.Message, notFoundEx)
            ),

            ValidationException validationEx => (
                HttpStatusCode.BadRequest,
                CreateValidationErrorResponse(validationEx)
            ),

            BusinessRuleException businessEx => (
                HttpStatusCode.Conflict,
                CreateBusinessRuleErrorResponse(businessEx)
            ),

            ConflictException conflictEx => (
                HttpStatusCode.Conflict,
                CreateConflictErrorResponse(conflictEx)
            ),

            UnauthorizedException unauthorizedEx => (
                HttpStatusCode.Unauthorized,
                CreateErrorResponse("UNAUTHORIZED", unauthorizedEx.Message, unauthorizedEx)
            ),

            ForbiddenException forbiddenEx => (
                HttpStatusCode.Forbidden,
                CreateErrorResponse("FORBIDDEN", forbiddenEx.Message, forbiddenEx)
            ),

            ResourceInUseException resourceEx => (
                HttpStatusCode.Conflict,
                CreateResourceInUseErrorResponse(resourceEx)
            ),

            ExternalServiceException externalEx => (
                HttpStatusCode.BadGateway,
                CreateExternalServiceErrorResponse(externalEx)
            ),

            // System Exceptions
            ArgumentException argEx => (
                HttpStatusCode.BadRequest,
                CreateErrorResponse("INVALID_ARGUMENT", "Parametri de intrare invalizi", argEx)
            ),

            InvalidOperationException opEx => (
                HttpStatusCode.BadRequest,
                CreateErrorResponse("INVALID_OPERATION", "Operația nu este validă în contextul curent", opEx)
            ),

            TimeoutException timeoutEx => (
                HttpStatusCode.RequestTimeout,
                CreateErrorResponse("TIMEOUT", "Operația a depășit timpul maxim alocat", timeoutEx)
            ),

            // Generic handling pentru alte excepții
            _ => (
                HttpStatusCode.InternalServerError,
                CreateGenericErrorResponse(exception)
            )
        };

        context.Response.StatusCode = (int)statusCode;

        // Loghează excepția cu nivel corespunzător
        LogException(exception, statusCode);

        // Scrie răspunsul
        var jsonResponse = JsonSerializer.Serialize(response, _jsonOptions);
        await context.Response.WriteAsync(jsonResponse);
    }

    /// <summary>
    /// Creează răspuns de eroare generic
    /// </summary>
    private object CreateErrorResponse(string errorCode, string message, BusinessException? businessEx = null)
    {
        var response = new
        {
            error = errorCode,
            message = message,
            timestamp = DateTime.Now,
            details = businessEx?.Details
        };

        // Adaugă informații specifice pentru excepțiile de business
        if (businessEx != null)
        {
            return new
            {
                error = response.error,
                message = response.message,
                timestamp = response.timestamp,
                details = response.details,
                errorCode = businessEx.ErrorCode,
                businessTimestamp = businessEx.Timestamp
            };
        }

        return response;
    }

    /// <summary>
    /// Creează răspuns pentru erori de validare
    /// </summary>
    private object CreateValidationErrorResponse(ValidationException validationEx)
    {
        return new
        {
            error = "VALIDATION_ERROR",
            message = "Datele introduse nu sunt valide",
            timestamp = DateTime.Now,
            details = validationEx.Details,
            validationErrors = validationEx.ValidationErrors.ToDictionary(
                kv => kv.Key,
                kv => kv.Value
            ),
            formattedErrors = validationEx.GetFormattedErrors()
        };
    }

    /// <summary>
    /// Creează răspuns pentru încălcări de reguli de business
    /// </summary>
    private object CreateBusinessRuleErrorResponse(BusinessRuleException businessRuleEx)
    {
        return new
        {
            error = "BUSINESS_RULE_VIOLATION",
            message = businessRuleEx.Message,
            timestamp = DateTime.Now,
            details = businessRuleEx.Details,
            violatedRules = businessRuleEx.ViolatedRules
        };
    }

    /// <summary>
    /// Creează răspuns pentru conflicte
    /// </summary>
    private object CreateConflictErrorResponse(ConflictException conflictEx)
    {
        return new
        {
            error = "CONFLICT",
            message = conflictEx.Message,
            timestamp = DateTime.Now,
            details = conflictEx.Details,
            conflictType = conflictEx.ConflictType,
            conflictingValue = conflictEx.ConflictingValue
        };
    }

    /// <summary>
    /// Creează răspuns pentru resurse în uz
    /// </summary>
    private object CreateResourceInUseErrorResponse(ResourceInUseException resourceEx)
    {
        return new
        {
            error = "RESOURCE_IN_USE",
            message = resourceEx.Message,
            timestamp = DateTime.Now,
            details = resourceEx.Details,
            resourceType = resourceEx.ResourceType,
            resourceId = resourceEx.ResourceId,
            dependencies = resourceEx.Dependencies
        };
    }

    /// <summary>
    /// Creează răspuns pentru erori de servicii externe
    /// </summary>
    private object CreateExternalServiceErrorResponse(ExternalServiceException externalEx)
    {
        return new
        {
            error = "EXTERNAL_SERVICE_ERROR",
            message = externalEx.Message,
            timestamp = DateTime.Now,
            details = externalEx.Details,
            serviceName = externalEx.ServiceName,
            externalErrorCode = externalEx.ExternalErrorCode
        };
    }

    /// <summary>
    /// Creează răspuns generic pentru excepții necunoscute
    /// </summary>
    private object CreateGenericErrorResponse(Exception exception)
    {
        // Pentru producție, nu expunem detaliile complete ale excepției
        var isDevelopment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development";

        return new
        {
            error = "INTERNAL_SERVER_ERROR",
            message = "A apărut o eroare internă pe server",
            timestamp = DateTime.Now,
            details = isDevelopment ? exception.Message : null,
            stackTrace = isDevelopment ? exception.StackTrace : null
        };
    }

    /// <summary>
    /// Loghează excepția cu nivelul corespunzător
    /// </summary>
    private void LogException(Exception exception, HttpStatusCode statusCode)
    {
        var logLevel = statusCode switch
        {
            HttpStatusCode.InternalServerError => LogLevel.Error,
            HttpStatusCode.BadGateway => LogLevel.Error,
            HttpStatusCode.RequestTimeout => LogLevel.Warning,
            HttpStatusCode.Conflict => LogLevel.Warning,
            HttpStatusCode.BadRequest => LogLevel.Information,
            HttpStatusCode.Unauthorized => LogLevel.Warning,
            HttpStatusCode.Forbidden => LogLevel.Warning,
            HttpStatusCode.NotFound => LogLevel.Information,
            _ => LogLevel.Warning
        };

        using var scope = _logger.BeginScope(new Dictionary<string, object>
        {
            ["ExceptionType"] = exception.GetType().Name,
            ["StatusCode"] = (int)statusCode,
            ["ErrorCode"] = exception.GetErrorCode(),
            ["Timestamp"] = DateTime.Now
        });

        _logger.Log(logLevel, exception, "Exception handled by middleware: {Message}", exception.Message);

        // Log suplimentar pentru excepțiile de business
        if (exception is BusinessException businessEx)
        {
            _logger.LogInformation("Business exception details: Code={ErrorCode}, Details={Details}", 
                businessEx.ErrorCode, businessEx.Details);
        }
    }
}

/// <summary>
/// Extension methods pentru înregistrarea middleware-ului
/// </summary>
public static class ExceptionHandlingMiddlewareExtensions
{
    /// <summary>
    /// Înregistrează middleware-ul de gestionare a excepțiilor
    /// </summary>
    public static IApplicationBuilder UseExceptionHandling(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<ExceptionHandlingMiddleware>();
    }
}

/// <summary>
/// Helper pentru crearea rapidă de excepții cu context
/// </summary>
public static class ExceptionFactory
{
    /// <summary>
    /// Creează o excepție NotFoundException cu context
    /// </summary>
    public static NotFoundException EntityNotFound<T>(object id) where T : class
    {
        return new NotFoundException(typeof(T).Name, id);
    }

    /// <summary>
    /// Creează o excepție ConflictException pentru duplicate
    /// </summary>
    public static ConflictException DuplicateEntry(string entityName, string field, string value)
    {
        return new ConflictException(
            $"{entityName} cu {field} '{value}' există deja", 
            "DUPLICATE_ENTRY", 
            value
        );
    }

    /// <summary>
    /// Creează o excepție BusinessRuleException
    /// </summary>
    public static BusinessRuleException BusinessRule(string rule, string message)
    {
        return new BusinessRuleException(message, rule);
    }

    /// <summary>
    /// Creează o excepție ValidationException din FluentValidation results
    /// </summary>
    public static ValidationException FromValidationResult(FluentValidation.Results.ValidationResult validationResult)
    {
        var errors = validationResult.Errors
            .GroupBy(x => x.PropertyName)
            .ToDictionary(
                g => g.Key,
                g => g.Select(x => x.ErrorMessage).ToList()
            );

        return new ValidationException("Validarea a eșuat", errors);
    }

    /// <summary>
    /// Creează o excepție ResourceInUseException
    /// </summary>
    public static ResourceInUseException ResourceInUse<T>(object id, params string[] dependencies) where T : class
    {
        return new ResourceInUseException(typeof(T).Name, id.ToString() ?? string.Empty, dependencies.ToList());
    }
}
