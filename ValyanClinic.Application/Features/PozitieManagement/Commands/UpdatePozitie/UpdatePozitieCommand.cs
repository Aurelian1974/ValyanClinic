using MediatR;
using ValyanClinic.Application.Common.Results;

namespace ValyanClinic.Application.Features.PozitieManagement.Commands.UpdatePozitie;

public record UpdatePozitieCommand : IRequest<Result<bool>>
{
    public Guid Id { get; init; }
    public string Denumire { get; init; } = string.Empty;
    public string? Descriere { get; init; }
    public bool EsteActiv { get; init; }
    public string ModificatDe { get; init; } = "System";
}
