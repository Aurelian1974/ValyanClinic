using MediatR;
using ValyanClinic.Application.Common.Results;

namespace ValyanClinic.Application.Features.PersonalManagement.Queries.GetPersonalList;

/// <summary>
/// Query pentru lista completa de personal
/// </summary>
public record GetPersonalListQuery : IRequest<Result<IEnumerable<PersonalListDto>>>;
