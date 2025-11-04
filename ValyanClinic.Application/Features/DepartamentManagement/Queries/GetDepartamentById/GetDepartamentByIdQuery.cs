using MediatR;
using ValyanClinic.Application.Common.Results;

namespace ValyanClinic.Application.Features.DepartamentManagement.Queries.GetDepartamentById;

public record GetDepartamentByIdQuery(Guid IdDepartament) : IRequest<Result<DepartamentDetailDto>>;
