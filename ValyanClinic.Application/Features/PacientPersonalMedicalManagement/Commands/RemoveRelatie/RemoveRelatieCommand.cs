using MediatR;
using ValyanClinic.Application.Common.Results;

namespace ValyanClinic.Application.Features.PacientPersonalMedicalManagement.Commands.RemoveRelatie;

/// <summary>
/// Command pentru dezactivarea unei relatii (soft delete)
/// </summary>
public record RemoveRelatieCommand(
    Guid? RelatieID = null,
    Guid? PacientID = null,
    Guid? PersonalMedicalID = null,
    string? ModificatDe = null
) : IRequest<Result>;
