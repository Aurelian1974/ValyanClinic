using MediatR;
using Microsoft.Extensions.Logging;
using ValyanClinic.Application.Common.Results;
using ValyanClinic.Domain.Entities;
using ValyanClinic.Infrastructure.Repositories.Interfaces;

namespace ValyanClinic.Application.Features.ConsultatieManagement.Commands.SaveConsultatieDraft;

/// <summary>
/// Handler pentru SaveConsultatieDraftCommand
/// Salveaza draft-ul consultatiei (auto-save optimizat cu campuri esentiale)
/// </summary>
public class SaveConsultatieDraftCommandHandler : IRequestHandler<SaveConsultatieDraftCommand, Result<Guid>>
{
    private readonly IConsultatieRepository _repository;
    private readonly ILogger<SaveConsultatieDraftCommandHandler> _logger;

    public SaveConsultatieDraftCommandHandler(
        IConsultatieRepository repository,
        ILogger<SaveConsultatieDraftCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<Result<Guid>> Handle(
        SaveConsultatieDraftCommand request,
        CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation(
                "[SaveConsultatieDraftHandler] Saving draft for ConsultatieID: {ConsultatieID}, ProgramareID: {ProgramareID} (null = walk-in patient)",
                request.ConsultatieID, request.ProgramareID);

            // ✅ Validare - ProgramareID este OPȚIONAL (consultații fără programare = walk-in)
            // ProgramareID can be null for walk-in patients

            if (request.PacientID == Guid.Empty)
                return Result<Guid>.Failure("PacientID este obligatoriu");

            if (request.MedicID == Guid.Empty)
                return Result<Guid>.Failure("MedicID este obligatoriu");

            if (request.CreatDeSauModificatDe == Guid.Empty)
                return Result<Guid>.Failure("CreatDeSauModificatDe este obligatoriu");

            // Calculare IMC automat
            var imc = request.IMC;
            if (!imc.HasValue && request.Greutate.HasValue && request.Inaltime.HasValue && request.Inaltime.Value > 0)
            {
                var inaltimeMetri = request.Inaltime.Value / 100m;
                imc = Math.Round(request.Greutate.Value / (inaltimeMetri * inaltimeMetri), 2);
            }

            // Map to entity (all fields from UI)
            var consultatie = new Consultatie
            {
                ConsultatieID = request.ConsultatieID ?? Guid.NewGuid(),
                ProgramareID = request.ProgramareID, // Can be null for walk-in patients
                PacientID = request.PacientID,
                MedicID = request.MedicID,
                DataConsultatie = request.DataConsultatie,
                OraConsultatie = request.OraConsultatie,
                TipConsultatie = request.TipConsultatie,
                
                // Tab 1: Motiv & Antecedente
                MotivPrezentare = request.MotivPrezentare,
                IstoricBoalaActuala = request.IstoricBoalaActuala,
                APP_Medicatie = request.APP_Medicatie,
                
                // Tab 2: Semne Vitale
                Greutate = request.Greutate,
                Inaltime = request.Inaltime,
                IMC = imc,
                Temperatura = request.Temperatura,
                TensiuneArteriala = request.TensiuneArteriala,
                Puls = request.Puls,
                FreccventaRespiratorie = request.FreccventaRespiratorie,
                SaturatieO2 = request.SaturatieO2,
                
                // Tab 2: Examen General
                StareGenerala = request.StareGenerala,
                Tegumente = request.Tegumente,
                Mucoase = request.Mucoase,
                Edeme = request.Edeme,
                ExamenCardiovascular = request.ExamenCardiovascular,
                
                // Tab 2: Investigații
                InvestigatiiLaborator = request.InvestigatiiLaborator,
                
                // Tab 3: Diagnostic
                DiagnosticPozitiv = request.DiagnosticPozitiv,
                DiagnosticDiferential = request.DiagnosticDiferential,
                CoduriICD10 = request.CoduriICD10,
                CoduriICD10Secundare = request.CoduriICD10Secundare,
                
                // Tab 3: Tratament
                TratamentMedicamentos = request.TratamentMedicamentos,
                RecomandariRegimViata = request.RecomandariRegimViata,
                
                // Tab 4: Concluzii
                Concluzie = request.Concluzie,
                ObservatiiMedic = request.ObservatiiMedic,
                DataUrmatoareiProgramari = request.DataUrmatoareiProgramari,
                
                // Status & Audit
                Status = "In desfasurare", // Draft always in progress
                CreatDe = request.CreatDeSauModificatDe,
                ModificatDe = request.CreatDeSauModificatDe,
                DataCreare = DateTime.UtcNow,
                DataUltimeiModificari = DateTime.UtcNow
            };

            // Call repository (executes sp_Consultatie_SaveDraft with INSERT/UPDATE logic)
            var consultatieId = await _repository.SaveDraftAsync(consultatie, cancellationToken);

            if (consultatieId == Guid.Empty)
            {
                _logger.LogWarning("[SaveConsultatieDraftHandler] Failed to save draft");
                return Result<Guid>.Failure("Eroare la salvarea draft-ului");
            }

            _logger.LogInformation(
                "[SaveConsultatieDraftHandler] Draft saved successfully: {ConsultatieID}",
                consultatieId);

            return Result<Guid>.Success(consultatieId, "Draft salvat cu succes");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[SaveConsultatieDraftHandler] Error saving draft");
            return Result<Guid>.Failure($"Eroare la salvarea draft-ului: {ex.Message}");
        }
    }
}
