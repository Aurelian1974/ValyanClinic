using MediatR;
using ValyanClinic.Application.Common.Results;

namespace ValyanClinic.Application.Features.UtilizatorManagement.Queries.GetUtilizatorById;

public record GetUtilizatorByIdQuery(Guid UtilizatorID) : IRequest<Result<UtilizatorDetailDto>>;
