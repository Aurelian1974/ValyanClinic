using MediatR;
using Microsoft.Extensions.Logging;
using ValyanClinic.Application.Common.Results;
using ValyanClinic.Domain.Entities;
using ValyanClinic.Domain.Interfaces.Repositories;

namespace ValyanClinic.Application.Features.AnalizeMedicale.Commands.AddAnalizaRecomandataToConsultatie;

/// <summary>
/// Handler pentru adăugare analiză recomandată în consultație
/// </summary>
public class AddAnalizaRecomandataCommandHandler 
    : IRequestHandler<AddAnalizaRecomandataCommand, Result<Guid>>
{
    private readonly IConsultatieAnalizaRecomandataRepository _repository;
    private readonly ILogger<AddAnalizaRecomandataCommandHandler> _logger;

    public AddAnalizaRecomandataCommandHandler(
        IConsultatieAnalizaRecomandataRepository repository,
        ILogger<AddAnalizaRecomandataCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<Result<Guid>> Handle(
        AddAnalizaRecomandataCommand request,
        CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation(
                "Adding analiză recomandată {NumeAnaliza} (Categorie: {TipAnaliza}) to consultație {ConsultatieID}",
                request.NumeAnaliza, request.TipAnaliza, request.ConsultatieID);

            var analiza = new ConsultatieAnalizaRecomandataEntity
            {
                ConsultatieID = request.ConsultatieID,
                AnalizaNomenclatorID = request.AnalizaNomenclatorID,
                NumeAnaliza = request.NumeAnaliza,
                CodAnaliza = request.CodAnaliza,
                TipAnaliza = request.TipAnaliza,
                DataRecomandare = DateTime.Now,
                Prioritate = request.Prioritate,
                EsteCito = request.EsteCito,
                IndicatiiClinice = request.IndicatiiClinice,
                ObservatiiMedic = request.ObservatiiMedic,
                Status = "Recomandata",
                DataCreare = DateTime.Now,
                CreatDe = request.CreatDe
            };

            var analizaId = await _repository.CreateAsync(analiza, cancellationToken);

            _logger.LogInformation(
                "Analiză recomandată {AnalizaId} added successfully to consultație {ConsultatieID}",
                analizaId, request.ConsultatieID);

            return Result<Guid>.Success(analizaId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, 
                "Error adding analiză recomandată {NumeAnaliza} to consultație {ConsultatieID}",
                request.NumeAnaliza, request.ConsultatieID);
            
            return Result<Guid>.Failure($"Eroare la adăugare analiză: {ex.Message}");
        }
    }
}
