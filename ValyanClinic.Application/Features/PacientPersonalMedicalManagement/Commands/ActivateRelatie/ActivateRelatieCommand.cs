using MediatR;
using ValyanClinic.Application.Common.Results;

namespace ValyanClinic.Application.Features.PacientPersonalMedicalManagement.Commands.ActivateRelatie;

/// <summary>
/// Command pentru reactivarea unei relatii inactiva
/// Permite reluarea unei relatii doctor-pacient care a fost anterior dezactivată
/// </summary>
public record ActivateRelatieCommand(
    Guid RelatieID,
    string? Observatii = null,
    string? Motiv = null,
    Guid? ModificatDe = null
) : IRequest<Result>;
