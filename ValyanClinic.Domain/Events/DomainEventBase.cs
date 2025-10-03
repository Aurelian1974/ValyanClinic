namespace ValyanClinic.Domain.Events;

/// <summary>
/// Clasa de baza pentru Domain Events
/// </summary>
public abstract class DomainEventBase : IDomainEvent
{
    public DateTime OccurredOn { get; init; } = DateTime.Now; // CORECTAT: foloseste ora locala in loc de UTC
    public Guid EventId { get; init; } = Guid.NewGuid();
}
