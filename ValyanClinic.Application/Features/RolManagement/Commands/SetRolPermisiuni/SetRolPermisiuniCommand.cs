using MediatR;
using ValyanClinic.Application.Common.Results;

namespace ValyanClinic.Application.Features.RolManagement.Commands.SetRolPermisiuni;

/// <summary>
/// Command pentru setarea permisiunilor unui rol.
/// </summary>
public record SetRolPermisiuniCommand : IRequest<Result>
{
    public Guid RolId { get; init; }
    public List<string> Permisiuni { get; init; } = new();
    public string? ModificatDe { get; init; }
}
