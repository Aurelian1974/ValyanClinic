using ValyanClinic.Application.Common.Results;
using MediatR;
using ValyanClinic.Domain.DTOs;

namespace ValyanClinic.Application.Features.PersonalMedicalManagement.Queries.GetPacientiByDoctor;

/// <summary>
/// Query pentru obținerea pacienților asociați cu un doctor
/// </summary>
public record GetPacientiByDoctorQuery(Guid DoctorID) : IRequest<Result<List<PacientAsociatDto>>>;
