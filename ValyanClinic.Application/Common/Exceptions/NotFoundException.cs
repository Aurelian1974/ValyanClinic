namespace ValyanClinic.Application.Common.Exceptions;

/// <summary>
/// Exceptie pentru entitati care nu au fost gasite
/// </summary>
public class NotFoundException : Exception
{
    public NotFoundException(string entityName, object key)
        : base($"Entitatea '{entityName}' cu ID-ul '{key}' nu a fost gasita.")
    {
    }
    
    public NotFoundException(string message)
        : base(message)
    {
    }
}
