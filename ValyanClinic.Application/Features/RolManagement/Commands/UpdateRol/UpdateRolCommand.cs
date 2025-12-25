using MediatR;
using ValyanClinic.Application.Common.Results;

namespace ValyanClinic.Application.Features.RolManagement.Commands.UpdateRol;

/// <summary>
/// Command pentru actualizarea unui rol existent.
/// </summary>
public record UpdateRolCommand : IRequest<Result>
{
    public Guid Id { get; init; }
    public string Denumire { get; init; } = string.Empty;
    public string? Descriere { get; init; }
    public bool EsteActiv { get; init; } = true;
    public int OrdineAfisare { get; init; }
    public List<string> Permisiuni { get; init; } = new();
    public string? ModificatDe { get; init; }
}
