using MediatR;
using Microsoft.Extensions.Logging;
using ValyanClinic.Application.Common.Results;
using ValyanClinic.Domain.Entities;
using ValyanClinic.Domain.Interfaces.Repositories;

namespace ValyanClinic.Application.Features.AnalizeMedicale.Commands.ImportAnalizeEfectuate;

/// <summary>
/// Handler pentru importarea analizelor efectuate din PDF
/// </summary>
public class ImportAnalizeEfectuateCommandHandler : IRequestHandler<ImportAnalizeEfectuateCommand, Result<int>>
{
    private readonly IConsultatieAnalizaMedicalaRepository _repository;
    private readonly ILogger<ImportAnalizeEfectuateCommandHandler> _logger;

    public ImportAnalizeEfectuateCommandHandler(
        IConsultatieAnalizaMedicalaRepository repository,
        ILogger<ImportAnalizeEfectuateCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<Result<int>> Handle(ImportAnalizeEfectuateCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Importing {Count} analize efectuate for consultație {ConsultatieId} from {Laborator}",
            request.Analize.Count, request.ConsultatieID, request.Laborator);

        var importedCount = 0;
        DateTime? dataRecoltare = null;
        
        // Parse data recoltare dacă există
        if (!string.IsNullOrEmpty(request.DataRecoltare))
        {
            if (DateTime.TryParse(request.DataRecoltare, out var parsed))
            {
                dataRecoltare = parsed;
            }
        }

        foreach (var analiza in request.Analize)
        {
            try
            {
                var entity = new ConsultatieAnalizaMedicala
                {
                    ConsultatieID = request.ConsultatieID,
                    TipAnaliza = analiza.TipAnaliza ?? "Laborator",
                    NumeAnaliza = analiza.NumeAnaliza,
                    CodAnaliza = analiza.CodAnaliza,
                    StatusAnaliza = "Finalizata", // Analizele importate sunt deja efectuate
                    DataRecomandare = dataRecoltare ?? DateTime.Now,
                    DataEfectuare = dataRecoltare ?? DateTime.Now,
                    LocEfectuare = request.Laborator,
                    Prioritate = "Normala",
                    EsteCito = false,
                    
                    // Rezultate
                    AreRezultate = true,
                    DataRezultate = dataRecoltare ?? DateTime.Now,
                    ValoareRezultat = analiza.Valoare,
                    UnitatiMasura = analiza.UnitatiMasura,
                    ValoareNormalaMin = analiza.ValoareNormalaMin,
                    ValoareNormalaMax = analiza.ValoareNormalaMax,
                    EsteInAfaraLimitelor = analiza.EsteInAfaraLimitelor,
                    
                    // Observații
                    ObservatiiRecomandare = $"Import din buletin: {request.NumarBuletin}",
                    
                    // Audit
                    CreatDe = request.CreatDe
                };

                await _repository.CreateAsync(entity, cancellationToken);
                importedCount++;

                _logger.LogDebug(
                    "Imported analiza: {NumeAnaliza} = {Valoare} {UM}",
                    analiza.NumeAnaliza, analiza.Valoare, analiza.UnitatiMasura);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error importing analiza {NumeAnaliza}", analiza.NumeAnaliza);
            }
        }

        _logger.LogInformation(
            "Successfully imported {Count}/{Total} analize for consultație {ConsultatieId}",
            importedCount, request.Analize.Count, request.ConsultatieID);

        return Result<int>.Success(importedCount);
    }
}
