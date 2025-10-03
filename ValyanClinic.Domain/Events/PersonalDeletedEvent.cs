namespace ValyanClinic.Domain.Events;

/// <summary>
/// Event declansat cand un Personal este sters
/// </summary>
public class PersonalDeletedEvent : DomainEventBase
{
    public Guid PersonalId { get; init; }
    public string PersonalName { get; init; } = string.Empty;
    public string DeletedBy { get; init; } = string.Empty;
    public string UserId { get; init; }

    public PersonalDeletedEvent(Guid personalId, string personalName, string deletedBy, string userId)
    {
        PersonalId = personalId;
        PersonalName = personalName;
        DeletedBy = deletedBy;
        UserId = userId;
    }
}
