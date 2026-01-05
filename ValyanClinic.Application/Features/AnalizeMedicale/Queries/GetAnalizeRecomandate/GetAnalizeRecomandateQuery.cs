using MediatR;
using ValyanClinic.Application.Common.Results;
using ValyanClinic.Application.ViewModels;

namespace ValyanClinic.Application.Features.AnalizeMedicale.Queries.GetAnalizeRecomandate;

/// <summary>
/// Query pentru obținere analize recomandate dintr-o consultație
/// </summary>
public record GetAnalizeRecomandateQuery(Guid ConsultatieId) 
    : IRequest<Result<IEnumerable<ConsultatieAnalizaRecomandataDto>>>;
