using MediatR;
using Microsoft.Extensions.Logging;
using ValyanClinic.Application.Common.Results;
using ValyanClinic.Domain.Entities;
using ValyanClinic.Domain.Interfaces.Repositories;

namespace ValyanClinic.Application.Features.AnalizeMedicale.Commands.AddAnalizaToConsultatie;

/// <summary>
/// Handler pentru adăugare analiză în consultație
/// </summary>
public class AddAnalizaToConsultatieCommandHandler 
    : IRequestHandler<AddAnalizaToConsultatieCommand, Result<Guid>>
{
    private readonly IConsultatieAnalizaMedicalaRepository _repository;
    private readonly ILogger<AddAnalizaToConsultatieCommandHandler> _logger;

    public AddAnalizaToConsultatieCommandHandler(
        IConsultatieAnalizaMedicalaRepository repository,
        ILogger<AddAnalizaToConsultatieCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<Result<Guid>> Handle(
        AddAnalizaToConsultatieCommand request,
        CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation(
                "Adding analiză {NumeAnaliza} to consultație {ConsultatieID}",
                request.NumeAnaliza, request.ConsultatieID);

            var analiza = new ConsultatieAnalizaMedicala
            {
                ConsultatieID = request.ConsultatieID,
                TipAnaliza = "Laborator",
                NumeAnaliza = request.NumeAnaliza,
                CodAnaliza = request.CodAnaliza,
                StatusAnaliza = "Recomandata",
                Prioritate = request.Prioritate,
                EsteCito = request.EsteCito,
                DataRecomandare = DateTime.Now,
                IndicatiiClinice = request.IndicatiiMedic,
                DataProgramata = request.DataProgramata,
                DataCreare = DateTime.Now,
                CreatDe = request.CreatDe
            };

            var analizaId = await _repository.CreateAsync(analiza, cancellationToken);

            _logger.LogInformation(
                "Analiză {AnalizaId} added successfully to consultație {ConsultatieID}",
                analizaId, request.ConsultatieID);

            return Result<Guid>.Success(analizaId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, 
                "Error adding analiză {NumeAnaliza} to consultație {ConsultatieID}",
                request.NumeAnaliza, request.ConsultatieID);
            
            return Result<Guid>.Failure($"Eroare la adăugare analiză: {ex.Message}");
        }
    }
}
