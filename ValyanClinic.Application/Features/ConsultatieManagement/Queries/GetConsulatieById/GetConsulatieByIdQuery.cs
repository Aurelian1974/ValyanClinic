using MediatR;
using ValyanClinic.Application.Common.Results;
using ValyanClinic.Application.Features.ConsultatieManagement.DTOs;

namespace ValyanClinic.Application.Features.ConsultatieManagement.Queries.GetConsulatieById;

/// <summary>
/// Query pentru obtinerea detaliilor complete ale unei consultatii dupa ID
/// Implementează pattern-ul CQRS cu MediatR
/// </summary>
public record GetConsulatieByIdQuery(Guid ConsultatieID) : IRequest<Result<ConsulatieDetailDto>>;
