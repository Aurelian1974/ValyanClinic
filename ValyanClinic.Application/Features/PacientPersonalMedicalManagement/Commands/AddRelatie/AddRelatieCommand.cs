using MediatR;
using ValyanClinic.Application.Common.Results;

namespace ValyanClinic.Application.Features.PacientPersonalMedicalManagement.Commands.AddRelatie;

/// <summary>
/// Command pentru adaugarea unei relatii noi intre pacient si doctor
/// </summary>
public record AddRelatieCommand(
    Guid PacientID,
    Guid PersonalMedicalID,
 string? TipRelatie,
    string? Observatii,
    string? Motiv,
    string? CreatDe
) : IRequest<Result<Guid>>;
