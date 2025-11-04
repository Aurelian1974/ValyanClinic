using MediatR;
using ValyanClinic.Application.Common.Results;

namespace ValyanClinic.Application.Features.SpecializareManagement.Commands.CreateSpecializare;

public record CreateSpecializareCommand : IRequest<Result<Guid>>
{
    public string Denumire { get; init; } = string.Empty;
    public string? Categorie { get; init; }
    public string? Descriere { get; init; }
    public bool EsteActiv { get; init; } = true;
    public string CreatDe { get; init; } = "System";
}
