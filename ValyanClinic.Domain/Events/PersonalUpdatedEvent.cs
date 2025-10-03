using ValyanClinic.Domain.Entities;

namespace ValyanClinic.Domain.Events;

/// <summary>
/// Event declansat cand un Personal este actualizat
/// </summary>
public class PersonalUpdatedEvent : DomainEventBase
{
    public Personal Personal { get; init; }
    public string UserId { get; init; }

    public PersonalUpdatedEvent(Personal personal, string userId)
    {
        Personal = personal;
        UserId = userId;
        PersonalId = personal.Id_Personal; // CORECTAT: foloseste Id_Personal
        PersonalName = personal.NumeComplet;
        UpdatedBy = personal.Modificat_De ?? string.Empty; // CORECTAT: foloseste Modificat_De
    }

    public Guid PersonalId { get; init; }
    public string PersonalName { get; init; } = string.Empty;
    public string UpdatedBy { get; init; } = string.Empty;
}
