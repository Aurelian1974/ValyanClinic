namespace ValyanClinic.Application.Common.Results;

/// <summary>
/// Validation result pentru rezultate de validare
/// </summary>
public class ValidationResult : Result
{
    public Dictionary<string, List<string>> ValidationErrors { get; private set; } = new();
    
    protected ValidationResult(bool isSuccess, Dictionary<string, List<string>> validationErrors)
        : base(isSuccess, validationErrors.SelectMany(x => x.Value).ToList())
    {
        ValidationErrors = validationErrors;
    }
    
    public static ValidationResult Success()
    {
        return new ValidationResult(true, new Dictionary<string, List<string>>());
    }
    
    public static ValidationResult Failure(Dictionary<string, List<string>> validationErrors)
    {
        return new ValidationResult(false, validationErrors);
    }
    
    public static ValidationResult Failure(string propertyName, string error)
    {
        var errors = new Dictionary<string, List<string>>
        {
            { propertyName, new List<string> { error } }
        };
        return new ValidationResult(false, errors);
    }
    
    public bool HasErrorsForProperty(string propertyName)
    {
        return ValidationErrors.ContainsKey(propertyName);
    }
    
    public List<string> GetErrorsForProperty(string propertyName)
    {
        return ValidationErrors.TryGetValue(propertyName, out var errors) 
            ? errors 
            : new List<string>();
    }
}
