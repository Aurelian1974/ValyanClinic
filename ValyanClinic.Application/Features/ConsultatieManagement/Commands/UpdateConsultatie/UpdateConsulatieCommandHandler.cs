using MediatR;
using Microsoft.Extensions.Logging;
using ValyanClinic.Application.Common.Results;
using ValyanClinic.Domain.Entities;
using ValyanClinic.Domain.Interfaces.Repositories;

namespace ValyanClinic.Application.Features.ConsultatieManagement.Commands.UpdateConsultatie;

/// <summary>
/// Handler pentru UpdateConsulatieCommand
/// Actualizeaza o consultatie medicala existenta folosind Repository pattern si Stored Procedures
/// </summary>
public class UpdateConsulatieCommandHandler : IRequestHandler<UpdateConsulatieCommand, Result<bool>>
{
    private readonly IConsultatieBaseRepository _repository;
    private readonly ILogger<UpdateConsulatieCommandHandler> _logger;

    public UpdateConsulatieCommandHandler(
        IConsultatieBaseRepository repository,
        ILogger<UpdateConsulatieCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<Result<bool>> Handle(
        UpdateConsulatieCommand request,
        CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation(
                "[UpdateConsulatieHandler] Updating consultatie ID: {ConsultatieID}",
                request.ConsultatieID);

            // Validare
            if (request.ConsultatieID == Guid.Empty)
                return Result<bool>.Failure("ConsultatieID este obligatoriu");

            if (request.ModificatDe == Guid.Empty)
                return Result<bool>.Failure("ModificatDe este obligatoriu");

            // Calculare IMC automat
            var imc = request.IMC;
            if (!imc.HasValue && request.Greutate.HasValue && request.Inaltime.HasValue && request.Inaltime.Value > 0)
            {
                var inaltimeMetri = request.Inaltime.Value / 100m;
                imc = Math.Round(request.Greutate.Value / (inaltimeMetri * inaltimeMetri), 2);
            }

            // 1. MASTER Entity: Consultatie (core fields only - NORMALIZED structure)
            var consultatie = new Consultatie
            {
                ConsultatieID = request.ConsultatieID,
                ProgramareID = request.ProgramareID,
                PacientID = request.PacientID,
                MedicID = request.MedicID,
                DataConsultatie = request.DataConsultatie,
                OraConsultatie = request.OraConsultatie,
                TipConsultatie = request.TipConsultatie,
                Status = request.Status,
                DataFinalizare = request.DataFinalizare,
                DurataMinute = request.DurataMinute,
                DataUltimeiModificari = DateTime.Now,
                ModificatDe = request.ModificatDe
            };

            var success = await _repository.UpdateAsync(consultatie, cancellationToken);

            if (!success)
            {
                _logger.LogWarning("[UpdateConsulatieHandler] Failed to update master Consultatie");
                return Result<bool>.Failure("Eroare la actualizarea consultatiei");
            }

            // 2. ConsultatieMotivePrezentare (1:1) - always upsert for full update
            await _repository.UpsertMotivePrezentareAsync(request.ConsultatieID, new ConsultatieMotivePrezentare
            {
                ConsultatieID = request.ConsultatieID,
                MotivPrezentare = request.MotivPrezentare,
                IstoricBoalaActuala = request.IstoricBoalaActuala,
                ModificatDe = request.ModificatDe,
                DataUltimeiModificari = DateTime.Now
            });

            // 3. ConsultatieAntecedente (1:1) - SIMPLIFIED + Anexa 43
            await _repository.UpsertAntecedenteAsync(request.ConsultatieID, new ConsultatieAntecedente
            {
                ConsultatieID = request.ConsultatieID,
                IstoricMedicalPersonal = request.IstoricMedicalPersonal,
                IstoricFamilial = request.IstoricFamilial,
                TratamentAnterior = request.TratamentAnterior,
                FactoriDeRisc = request.FactoriDeRisc,
                ModificatDe = request.ModificatDe,
                DataUltimeiModificari = DateTime.Now
            });

            // 4. ConsultatieExamenObiectiv (1:1) - always upsert for full update
            await _repository.UpsertExamenObiectivAsync(request.ConsultatieID, new ConsultatieExamenObiectiv
            {
                ConsultatieID = request.ConsultatieID,
                StareGenerala = request.StareGenerala,
                Tegumente = request.Tegumente,
                Mucoase = request.Mucoase,
                GanglioniLimfatici = request.GanglioniLimfatici,
                Greutate = request.Greutate,
                Inaltime = request.Inaltime,
                IMC = imc,
                Temperatura = request.Temperatura,
                TensiuneArteriala = request.TensiuneArteriala,
                Puls = request.Puls,
                FreccventaRespiratorie = request.FreccventaRespiratorie,
                SaturatieO2 = request.SaturatieO2,
                Glicemie = request.Glicemie,
                ModificatDe = request.ModificatDe,
                DataUltimeiModificari = DateTime.Now
            });

            // 5. ConsultatieInvestigatii (1:1) - always upsert for full update
            await _repository.UpsertInvestigatiiAsync(request.ConsultatieID, new ConsultatieInvestigatii
            {
                ConsultatieID = request.ConsultatieID,
                InvestigatiiLaborator = request.InvestigatiiLaborator,
                InvestigatiiImagistice = request.InvestigatiiImagistice,
                InvestigatiiEKG = request.InvestigatiiEKG,
                AlteInvestigatii = request.AlteInvestigatii,
                ModificatDe = request.ModificatDe,
                DataUltimeiModificari = DateTime.Now
            });

            // 6. ConsultatieDiagnostic (1:1) - always upsert for full update
            await _repository.UpsertDiagnosticAsync(request.ConsultatieID, new ConsultatieDiagnostic
            {
                ConsultatieID = request.ConsultatieID,
                // Normalized structure
                CodICD10Principal = request.CodICD10Principal,
                NumeDiagnosticPrincipal = request.NumeDiagnosticPrincipal,
                DescriereDetaliataPrincipal = request.DescriereDetaliataPrincipal,
                // Legacy
                DiagnosticPozitiv = request.DiagnosticPozitiv,
                CoduriICD10 = request.CoduriICD10 ?? request.CodICD10Principal,
                ModificatDe = request.ModificatDe,
                DataUltimeiModificari = DateTime.Now
            });

            // 7. ConsultatieTratament (1:1) - always upsert for full update
            await _repository.UpsertTratamentAsync(request.ConsultatieID, new ConsultatieTratament
            {
                ConsultatieID = request.ConsultatieID,
                TratamentMedicamentos = request.TratamentMedicamentos,
                TratamentNemedicamentos = request.TratamentNemedicamentos,
                RecomandariDietetice = request.RecomandariDietetice,
                RecomandariRegimViata = request.RecomandariRegimViata,
                InvestigatiiRecomandate = request.InvestigatiiRecomandate,
                ConsulturiSpecialitate = request.ConsulturiSpecialitate,
                DataUrmatoareiProgramari = request.DataUrmatoareiProgramari,
                RecomandariSupraveghere = request.RecomandariSupraveghere,
                ModificatDe = request.ModificatDe,
                DataUltimeiModificari = DateTime.Now
            });

            // 7.1 ConsultatieMedicament (1:N) - save medication list
            var medicamente = (request.MedicationList ?? new())
                .Where(m => !string.IsNullOrWhiteSpace(m.Name))
                .Select((m, index) => new ConsultatieMedicament
                {
                    ConsultatieID = request.ConsultatieID,
                    OrdineAfisare = index,
                    NumeMedicament = m.Name,
                    Doza = m.Dose,
                    Frecventa = m.Frequency,
                    Durata = m.Duration,
                    Cantitate = m.Quantity,
                    Observatii = m.Notes,
                    ModificatDe = request.ModificatDe,
                    DataUltimeiModificari = DateTime.Now
                });
            await _repository.ReplaceMedicamenteAsync(request.ConsultatieID, medicamente, request.ModificatDe);

            // 8. ConsultatieConcluzii (1:1) - always upsert for full update
            await _repository.UpsertConcluziiAsync(request.ConsultatieID, new ConsultatieConcluzii
            {
                ConsultatieID = request.ConsultatieID,
                Prognostic = request.Prognostic,
                Concluzie = request.Concluzie,
                ObservatiiMedic = request.ObservatiiMedic,
                NotePacient = request.NotePacient,
                ModificatDe = request.ModificatDe,
                DataUltimeiModificari = DateTime.Now
            });

            _logger.LogInformation(
                "[UpdateConsulatieHandler] Consultatie updated successfully with normalized structure: {ConsultatieID}", 
                request.ConsultatieID);
            return Result<bool>.Success(true, "Consultatie actualizată cu succes");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[UpdateConsulatieHandler] Error updating consultatie ID: {ConsultatieID}", request.ConsultatieID);
            return Result<bool>.Failure($"Eroare la actualizarea consultatiei: {ex.Message}");
        }
    }
}
