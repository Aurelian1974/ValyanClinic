using MediatR;
using ValyanClinic.Application.Common.Results;

namespace ValyanClinic.Application.Features.RolManagement.Queries.GetRolById;

/// <summary>
/// Query pentru obținerea unui rol după ID.
/// </summary>
public record GetRolByIdQuery(Guid Id) : IRequest<Result<RolDetailDto>>;
