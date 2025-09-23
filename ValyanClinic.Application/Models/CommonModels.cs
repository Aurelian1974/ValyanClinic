using ValyanClinic.Domain.Models;

namespace ValyanClinic.Application.Models;

/// <summary>
/// Item pentru liste dropdown comune
/// </summary>
public record DropdownItem(string Value, string Text);

/// <summary>
/// Clasa de baza pentru rezultate de validare
/// </summary>
public class ValidationResultBase
{
    public bool IsValid => !Errors.Any();
    public List<string> Errors { get; init; } = new();
    
    protected ValidationResultBase() { }
    
    protected ValidationResultBase(params string[] errors)
    {
        Errors = errors.ToList();
    }
    
    public void AddError(string error)
    {
        Errors.Add(error);
    }
}

/// <summary>
/// Clasa de baza pentru rezultate de operatii CRUD
/// </summary>
public abstract class OperationResultBase
{
    public bool IsSuccess { get; protected set; }
    public string? ErrorMessage { get; protected set; }
    public List<string> ValidationErrors { get; protected set; } = new();
}
