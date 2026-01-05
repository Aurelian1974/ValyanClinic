using MediatR;
using ValyanClinic.Application.Common.Results;
using ValyanClinic.Application.ViewModels;

namespace ValyanClinic.Application.Features.AnalizeMedicale.Queries.GetAnalizeEfectuate;

/// <summary>
/// Query pentru obținerea analizelor efectuate (cu rezultate) pentru o consultație
/// </summary>
public record GetAnalizeEfectuateQuery(Guid ConsultatieId) : IRequest<Result<IEnumerable<ConsultatieAnalizaMedicalaDto>>>;
