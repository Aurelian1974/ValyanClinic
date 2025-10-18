using MediatR;
using ValyanClinic.Application.Common.Results;

namespace ValyanClinic.Application.Features.PozitieManagement.Commands.CreatePozitie;

public record CreatePozitieCommand : IRequest<Result<Guid>>
{
    public string Denumire { get; init; } = string.Empty;
    public string? Descriere { get; init; }
    public bool EsteActiv { get; init; } = true;
    public string CreatDe { get; init; } = "System";
}
