using MediatR;
using ValyanClinic.Application.Common.Results;

namespace ValyanClinic.Application.Features.SpecializareManagement.Queries.GetSpecializareById;

public record GetSpecializareByIdQuery(Guid Id) : IRequest<Result<SpecializareDetailDto>>;
