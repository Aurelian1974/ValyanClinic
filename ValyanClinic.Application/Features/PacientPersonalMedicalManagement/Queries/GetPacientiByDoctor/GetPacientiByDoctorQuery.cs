using MediatR;
using ValyanClinic.Application.Common.Results;
using ValyanClinic.Domain.DTOs;

namespace ValyanClinic.Application.Features.PacientPersonalMedicalManagement.Queries.GetPacientiByDoctor;

/// <summary>
/// Query pentru obtinerea tuturor pacientilor asociati cu un doctor
/// </summary>
public record GetPacientiByDoctorQuery(
  Guid PersonalMedicalID,
    bool ApenumereActivi = true,
    string? TipRelatie = null
) : IRequest<Result<List<PacientAsociatDto>>>;
