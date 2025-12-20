using MediatR;
using ValyanClinic.Application.Common.Results;
using ValyanClinic.Application.Features.ConsultatieManagement.DTOs;

namespace ValyanClinic.Application.Features.ConsultatieManagement.Queries.GetConsultatiiByPacient;

/// <summary>
/// Query pentru obtinerea listei de consultatii pentru un pacient
/// Ordonate descrescator dupa data (cele mai recente primul)
/// </summary>
public record GetConsultatiiByPacientQuery(Guid PacientID) : IRequest<Result<List<ConsulatieListDto>>>;
