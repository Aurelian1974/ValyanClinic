namespace ValyanClinic.Domain.Events;

/// <summary>
/// Interfata pentru Domain Events
/// </summary>
public interface IDomainEvent
{
    DateTime OccurredOn { get; }
    Guid EventId { get; }
}
