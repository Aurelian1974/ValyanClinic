using MediatR;
using ValyanClinic.Application.Common.Results;
using ValyanClinic.Application.ViewModels;

namespace ValyanClinic.Application.Features.AnalizeMedicale.Queries.GetAnalizeMedicaleByConsultatie;

/// <summary>
/// Query pentru obținere analize prescrise/efectuate într-o consultație
/// </summary>
public record GetAnalizeMedicaleByConsultatieQuery : IRequest<Result<IEnumerable<ConsultatieAnalizaMedicalaDto>>>
{
    public Guid ConsultatieId { get; init; }
}
