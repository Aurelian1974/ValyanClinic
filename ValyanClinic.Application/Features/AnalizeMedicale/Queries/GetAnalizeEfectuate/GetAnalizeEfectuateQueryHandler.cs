using MediatR;
using Microsoft.Extensions.Logging;
using ValyanClinic.Application.Common.Results;
using ValyanClinic.Application.ViewModels;
using ValyanClinic.Domain.Interfaces.Repositories;

namespace ValyanClinic.Application.Features.AnalizeMedicale.Queries.GetAnalizeEfectuate;

/// <summary>
/// Handler pentru obținerea analizelor efectuate
/// </summary>
public class GetAnalizeEfectuateQueryHandler : IRequestHandler<GetAnalizeEfectuateQuery, Result<IEnumerable<ConsultatieAnalizaMedicalaDto>>>
{
    private readonly IConsultatieAnalizaMedicalaRepository _repository;
    private readonly ILogger<GetAnalizeEfectuateQueryHandler> _logger;

    public GetAnalizeEfectuateQueryHandler(
        IConsultatieAnalizaMedicalaRepository repository,
        ILogger<GetAnalizeEfectuateQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<Result<IEnumerable<ConsultatieAnalizaMedicalaDto>>> Handle(
        GetAnalizeEfectuateQuery request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting analize efectuate for consultație {ConsultatieId}", request.ConsultatieId);

        var analize = await _repository.GetByConsultatieIdAsync(request.ConsultatieId, cancellationToken);
        
        // Filtrăm doar analizele cu status Finalizata sau care au rezultate
        var analizeEfectuate = analize
            .Where(a => a.AreRezultate || a.StatusAnaliza == "Finalizata")
            .Select(a => new ConsultatieAnalizaMedicalaDto
            {
                Id = a.Id,
                ConsultatieID = a.ConsultatieID,
                TipAnaliza = a.TipAnaliza,
                NumeAnaliza = a.NumeAnaliza,
                CodAnaliza = a.CodAnaliza,
                StatusAnaliza = a.StatusAnaliza,
                DataRecomandare = a.DataRecomandare,
                DataProgramata = a.DataProgramata,
                DataEfectuare = a.DataEfectuare,
                LocEfectuare = a.LocEfectuare,
                Prioritate = a.Prioritate,
                EsteCito = a.EsteCito,
                IndicatiiClinice = a.IndicatiiClinice,
                ObservatiiRecomandare = a.ObservatiiRecomandare,
                AreRezultate = a.AreRezultate,
                DataRezultate = a.DataRezultate,
                ValoareRezultat = a.ValoareRezultat,
                UnitatiMasura = a.UnitatiMasura,
                ValoareNormalaMin = a.ValoareNormalaMin,
                ValoareNormalaMax = a.ValoareNormalaMax,
                EsteInAfaraLimitelor = a.EsteInAfaraLimitelor,
                InterpretareMedic = a.InterpretareMedic,
                ConclusiiAnaliza = a.ConclusiiAnaliza,
                CaleFisierRezultat = a.CaleFisierRezultat,
                TipFisier = a.TipFisier,
                Pret = a.Pret,
                Decontat = a.Decontat
            })
            .OrderByDescending(a => a.DataEfectuare ?? a.DataRecomandare)
            .ToList();

        _logger.LogInformation("Found {Count} analize efectuate for consultație {ConsultatieId}", 
            analizeEfectuate.Count, request.ConsultatieId);

        return Result<IEnumerable<ConsultatieAnalizaMedicalaDto>>.Success(analizeEfectuate);
    }
}
