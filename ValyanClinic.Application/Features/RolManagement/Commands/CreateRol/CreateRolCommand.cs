using MediatR;
using ValyanClinic.Application.Common.Results;

namespace ValyanClinic.Application.Features.RolManagement.Commands.CreateRol;

/// <summary>
/// Command pentru crearea unui rol nou.
/// </summary>
public record CreateRolCommand : IRequest<Result<Guid>>
{
    public string Denumire { get; init; } = string.Empty;
    public string? Descriere { get; init; }
    public bool EsteActiv { get; init; } = true;
    public int OrdineAfisare { get; init; }
    public List<string> Permisiuni { get; init; } = new();
    public string? CreatDe { get; init; }
}
