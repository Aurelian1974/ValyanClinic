using MediatR;
using Microsoft.Extensions.Logging;
using ValyanClinic.Application.Common.Results;
using ValyanClinic.Application.ViewModels;
using ValyanClinic.Domain.Interfaces.Repositories;

namespace ValyanClinic.Application.Features.AnalizeMedicale.Queries.GetAnalizeRecomandate;

/// <summary>
/// Handler pentru obținere analize recomandate dintr-o consultație
/// </summary>
public class GetAnalizeRecomandateQueryHandler 
    : IRequestHandler<GetAnalizeRecomandateQuery, Result<IEnumerable<ConsultatieAnalizaRecomandataDto>>>
{
    private readonly IConsultatieAnalizaRecomandataRepository _repository;
    private readonly ILogger<GetAnalizeRecomandateQueryHandler> _logger;

    public GetAnalizeRecomandateQueryHandler(
        IConsultatieAnalizaRecomandataRepository repository,
        ILogger<GetAnalizeRecomandateQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<Result<IEnumerable<ConsultatieAnalizaRecomandataDto>>> Handle(
        GetAnalizeRecomandateQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation(
                "Getting analize recomandate for consultație {ConsultatieId}",
                request.ConsultatieId);

            var analize = await _repository.GetByConsultatieIdAsync(
                request.ConsultatieId,
                cancellationToken);

            var dtos = analize.Select(a => new ConsultatieAnalizaRecomandataDto
            {
                Id = a.Id,
                ConsultatieID = a.ConsultatieID,
                AnalizaNomenclatorID = a.AnalizaNomenclatorID,
                NumeAnaliza = a.NumeAnaliza,
                CodAnaliza = a.CodAnaliza,
                TipAnaliza = a.TipAnaliza,
                DataRecomandare = a.DataRecomandare,
                Prioritate = a.Prioritate,
                EsteCito = a.EsteCito,
                IndicatiiClinice = a.IndicatiiClinice,
                ObservatiiMedic = a.ObservatiiMedic,
                Status = a.Status,
                DataProgramata = a.DataProgramata
            }).ToList();

            _logger.LogInformation(
                "Found {Count} analize recomandate for consultație {ConsultatieId}",
                dtos.Count, request.ConsultatieId);

            return Result<IEnumerable<ConsultatieAnalizaRecomandataDto>>.Success(dtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, 
                "Error getting analize recomandate for consultație {ConsultatieId}",
                request.ConsultatieId);
            
            return Result<IEnumerable<ConsultatieAnalizaRecomandataDto>>.Failure(
                $"Eroare la obținere analize: {ex.Message}");
        }
    }
}
