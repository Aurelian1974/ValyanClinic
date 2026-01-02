using MediatR;
using ValyanClinic.Application.Common.Results;
using ValyanClinic.Application.ViewModels;

namespace ValyanClinic.Application.Features.AnalizeMedicale.Queries;

/// <summary>
/// Query pentru obținerea analizelor medicale din consultație (recomandate + efectuate)
/// </summary>
public record GetAnalizeMedicaleByConsultatieQuery(Guid ConsultatieId) 
    : IRequest<Result<List<ConsultatieAnalizaMedicalaDto>>>;
