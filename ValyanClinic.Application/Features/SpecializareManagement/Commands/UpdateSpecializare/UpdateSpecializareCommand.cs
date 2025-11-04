using MediatR;
using ValyanClinic.Application.Common.Results;

namespace ValyanClinic.Application.Features.SpecializareManagement.Commands.UpdateSpecializare;

public record UpdateSpecializareCommand : IRequest<Result<bool>>
{
    public Guid Id { get; init; }
    public string Denumire { get; init; } = string.Empty;
    public string? Categorie { get; init; }
    public string? Descriere { get; init; }
    public bool EsteActiv { get; init; }
    public string ModificatDe { get; init; } = "System";
}
