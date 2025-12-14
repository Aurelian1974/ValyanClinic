using MediatR;
using ValyanClinic.Application.Common.Results;
using ValyanClinic.Application.Features.ConsultatieManagement.DTOs;

namespace ValyanClinic.Application.Features.ConsultatieManagement.Queries.GetConsultatiiByMedic;

/// <summary>
/// Query pentru obtinerea listei de consultatii pentru un medic
/// Ordonate descrescator dupa data (cele mai recente primul)
/// Util pentru dashboard medic, rapoarte, statistici
/// </summary>
public record GetConsultatiiByMedicQuery(Guid MedicID) : IRequest<Result<List<ConsulatieListDto>>>;
