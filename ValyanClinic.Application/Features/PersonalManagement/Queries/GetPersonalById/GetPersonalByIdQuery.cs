using MediatR;
using ValyanClinic.Application.Common.Results;

namespace ValyanClinic.Application.Features.PersonalManagement.Queries.GetPersonalById;

/// <summary>
/// Query pentru detalii personal dupa ID
/// </summary>
public record GetPersonalByIdQuery(Guid Id) : IRequest<Result<PersonalDetailDto>>;
