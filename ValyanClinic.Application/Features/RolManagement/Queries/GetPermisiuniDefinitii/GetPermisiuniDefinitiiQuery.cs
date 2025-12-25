using MediatR;
using ValyanClinic.Application.Common.Results;

namespace ValyanClinic.Application.Features.RolManagement.Queries.GetPermisiuniDefinitii;

/// <summary>
/// Query pentru obținerea tuturor definițiilor de permisiuni grupate pe categorii.
/// </summary>
public record GetPermisiuniDefinitiiQuery : IRequest<Result<List<CategoriePermisiuniDto>>>;
