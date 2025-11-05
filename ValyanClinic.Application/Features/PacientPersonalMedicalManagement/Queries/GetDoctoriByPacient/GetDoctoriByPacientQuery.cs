using MediatR;
using ValyanClinic.Application.Common.Results;
using ValyanClinic.Application.Features.PacientPersonalMedicalManagement.DTOs;

namespace ValyanClinic.Application.Features.PacientPersonalMedicalManagement.Queries.GetDoctoriByPacient;

/// <summary>
/// Query pentru obtinerea tuturor doctorilor asociati cu un pacient
/// </summary>
public record GetDoctoriByPacientQuery(
    Guid PacientID,
    bool ApenumereActivi = true
) : IRequest<Result<List<DoctorAsociatDto>>>;
