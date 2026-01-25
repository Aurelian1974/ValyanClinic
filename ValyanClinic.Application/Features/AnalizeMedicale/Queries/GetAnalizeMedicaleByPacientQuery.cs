using MediatR;
using ValyanClinic.Application.Common.Results;
using ValyanClinic.Application.Features.ConsultatieManagement.DTOs;

namespace ValyanClinic.Application.Features.AnalizeMedicale.Queries;

/// <summary>
/// Query pentru a obține analizele medicale ale unui pacient
/// </summary>
public record GetAnalizeMedicaleByPacientQuery(Guid PacientId) : IRequest<Result<List<AnalizeMedicaleGroupDto>>>;
