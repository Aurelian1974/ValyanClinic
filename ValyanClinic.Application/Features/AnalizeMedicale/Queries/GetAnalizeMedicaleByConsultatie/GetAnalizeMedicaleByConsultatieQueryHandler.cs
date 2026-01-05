using MediatR;
using Microsoft.Extensions.Logging;
using ValyanClinic.Application.Common.Results;
using ValyanClinic.Application.ViewModels;
using ValyanClinic.Domain.Interfaces.Repositories;

namespace ValyanClinic.Application.Features.AnalizeMedicale.Queries.GetAnalizeMedicaleByConsultatie;

/// <summary>
/// Handler pentru obținere analize dintr-o consultație
/// </summary>
public class GetAnalizeMedicaleByConsultatieQueryHandler 
    : IRequestHandler<GetAnalizeMedicaleByConsultatieQuery, Result<IEnumerable<ConsultatieAnalizaMedicalaDto>>>
{
    private readonly IConsultatieAnalizaMedicalaRepository _repository;
    private readonly ILogger<GetAnalizeMedicaleByConsultatieQueryHandler> _logger;

    public GetAnalizeMedicaleByConsultatieQueryHandler(
        IConsultatieAnalizaMedicalaRepository repository,
        ILogger<GetAnalizeMedicaleByConsultatieQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<Result<IEnumerable<ConsultatieAnalizaMedicalaDto>>> Handle(
        GetAnalizeMedicaleByConsultatieQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation(
                "Getting analize for consultație {ConsultatieId}",
                request.ConsultatieId);

            var analize = await _repository.GetByConsultatieIdAsync(
                request.ConsultatieId,
                cancellationToken);

            var dtos = analize.Select(a => new ConsultatieAnalizaMedicalaDto
            {
                Id = a.Id,
                ConsultatieID = a.ConsultatieID,
                TipAnaliza = a.TipAnaliza ?? "Laborator", // Folosește valoarea din DB
                NumeAnaliza = a.NumeAnaliza,
                CodAnaliza = a.CodAnaliza,
                StatusAnaliza = a.StatusAnaliza, // Domain entity folosește StatusAnaliza (string)
                Prioritate = a.Prioritate,
                EsteCito = a.EsteCito,
                DataRecomandare = a.DataRecomandare,
                DataProgramata = a.DataProgramata,
                IndicatiiClinice = a.IndicatiiClinice,
                ObservatiiRecomandare = a.ObservatiiRecomandare,
                AreRezultate = a.AreRezultate,
                DataEfectuare = a.DataEfectuare,
                DataRezultate = a.DataRezultate,
                InterpretareMedic = a.InterpretareMedic
            }).ToList();

            _logger.LogInformation(
                "Found {Count} analize for consultație {ConsultatieId}",
                dtos.Count, request.ConsultatieId);

            return Result<IEnumerable<ConsultatieAnalizaMedicalaDto>>.Success(dtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, 
                "Error getting analize for consultație {ConsultatieId}",
                request.ConsultatieId);
            
            return Result<IEnumerable<ConsultatieAnalizaMedicalaDto>>.Failure(
                $"Eroare la obținere analize: {ex.Message}");
        }
    }
}
