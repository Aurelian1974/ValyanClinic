namespace ValyanClinic.Application.Common.Exceptions;

/// <summary>
/// Exceptie pentru erori de validare
/// </summary>
public class ValidationException : Exception
{
    public Dictionary<string, List<string>> ValidationErrors { get; }

    public ValidationException(Dictionary<string, List<string>> validationErrors)
        : base("Au aparut una sau mai multe erori de validare")
    {
        ValidationErrors = validationErrors;
    }

    public ValidationException(string propertyName, string error)
        : base($"Eroare de validare: {error}")
    {
        ValidationErrors = new Dictionary<string, List<string>>
        {
            { propertyName, new List<string> { error } }
        };
    }
}
