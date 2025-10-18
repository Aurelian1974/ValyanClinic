using MediatR;
using ValyanClinic.Application.Common.Results;

namespace ValyanClinic.Application.Features.PozitieManagement.Queries.GetPozitieById;

public record GetPozitieByIdQuery(Guid Id) : IRequest<Result<PozitieDetailDto>>;
