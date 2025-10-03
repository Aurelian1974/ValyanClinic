using ValyanClinic.Domain.Entities;

namespace ValyanClinic.Domain.Events;

/// <summary>
/// Event declansat cand un Personal este creat
/// </summary>
public class PersonalCreatedEvent : DomainEventBase
{
    public Personal Personal { get; init; }
    public string UserId { get; init; }

    public PersonalCreatedEvent(Personal personal, string userId)
    {
        Personal = personal;
        UserId = userId;
        PersonalId = personal.Id_Personal; // CORECTAT: foloseste Id_Personal
        PersonalName = personal.NumeComplet;
        CreatedBy = personal.Creat_De ?? string.Empty; // CORECTAT: foloseste Creat_De
    }

    public Guid PersonalId { get; init; }
    public string PersonalName { get; init; } = string.Empty;
    public string CreatedBy { get; init; } = string.Empty;
}
