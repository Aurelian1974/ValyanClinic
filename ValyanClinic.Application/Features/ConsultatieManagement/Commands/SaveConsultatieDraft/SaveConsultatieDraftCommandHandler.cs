using MediatR;
using Microsoft.Extensions.Logging;
using ValyanClinic.Application.Common.Results;
using ValyanClinic.Domain.Entities;
using ValyanClinic.Domain.Interfaces.Repositories;

namespace ValyanClinic.Application.Features.ConsultatieManagement.Commands.SaveConsultatieDraft;

/// <summary>
/// Handler pentru SaveConsultatieDraftCommand
/// Salveaza draft-ul consultatiei (auto-save optimizat cu campuri esentiale)
/// </summary>
public class SaveConsultatieDraftCommandHandler : IRequestHandler<SaveConsultatieDraftCommand, Result<Guid>>
{
    private readonly IConsultatieDraftRepository _repository;
    private readonly ILogger<SaveConsultatieDraftCommandHandler> _logger;

    /// <summary>
    /// Initializes a new instance of SaveConsultatieDraftCommandHandler with the required draft repository and logger.
    /// </summary>
    public SaveConsultatieDraftCommandHandler(
        IConsultatieDraftRepository repository,
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

            // Determină ConsultatieID (CREATE sau UPDATE)
            // ✅ FIX: Verifică dacă există draft pentru acest pacient/medic/dată înainte de a crea GUID nou
            Guid consultatieId;
            if (request.ConsultatieID.HasValue && request.ConsultatieID.Value != Guid.Empty)
            {
                // Avem deja ConsultatieID - UPDATE
                consultatieId = request.ConsultatieID.Value;
            }
            else
            {
                // Nu avem ConsultatieID - verificăm dacă există draft
                var existingDraft = await _repository.GetDraftByPacientAsync(
                    request.PacientID,
                    request.MedicID,
                    request.DataConsultatie,
                    request.ProgramareID,
                    cancellationToken);

                if (existingDraft != null)
                {
                    // Există draft - reutilizăm ID-ul
                    consultatieId = existingDraft.ConsultatieID;
                    _logger.LogInformation(
                        "[SaveConsultatieDraftHandler] Found existing draft: {ConsultatieID}", 
                        consultatieId);
                }
                else
                {
                    // Nu există draft - creăm unul nou
                    consultatieId = Guid.NewGuid();
                    _logger.LogInformation(
                        "[SaveConsultatieDraftHandler] Creating new draft: {ConsultatieID}", 
                        consultatieId);
                }
            }

            // 1. MASTER Entity: Consultatie (doar core fields)
            var consultatie = new Consultatie
            {
                ConsultatieID = consultatieId,
                ProgramareID = request.ProgramareID, // Can be null for walk-in patients
                PacientID = request.PacientID,
                MedicID = request.MedicID,
                DataConsultatie = request.DataConsultatie,
                OraConsultatie = request.OraConsultatie,
                TipConsultatie = request.TipConsultatie,
                Status = "In desfasurare", // Draft always in progress
                CreatDe = request.CreatDeSauModificatDe,
                ModificatDe = request.CreatDeSauModificatDe,
                DataCreare = DateTime.Now,
                DataUltimeiModificari = DateTime.Now
            };

            // Call repository SaveDraft pentru MASTER entity
            var savedId = await _repository.SaveDraftAsync(consultatie, cancellationToken);

            if (savedId == Guid.Empty)
            {
                _logger.LogWarning("[SaveConsultatieDraftHandler] Failed to save master Consultatie");
                return Result<Guid>.Failure("Eroare la salvarea draft-ului");
            }

            // 2. ConsultatieMotivePrezentare (1:1) - condiționat
            if (!string.IsNullOrWhiteSpace(request.MotivPrezentare) || 
                !string.IsNullOrWhiteSpace(request.IstoricBoalaActuala))
            {
                await _repository.UpsertMotivePrezentareAsync(consultatieId, new ConsultatieMotivePrezentare
                {
                    ConsultatieID = consultatieId,
                    MotivPrezentare = request.MotivPrezentare,
                    IstoricBoalaActuala = request.IstoricBoalaActuala,
                    CreatDe = request.CreatDeSauModificatDe,
                    DataCreare = DateTime.Now,
                    DataUltimeiModificari = DateTime.Now
                });
            }

            // 3. ConsultatieAntecedente (1:1) - SIMPLIFIED - condiționat
            if (!string.IsNullOrWhiteSpace(request.IstoricMedicalPersonal) ||
                !string.IsNullOrWhiteSpace(request.IstoricFamilial) ||
                !string.IsNullOrWhiteSpace(request.TratamentAnterior) ||
                !string.IsNullOrWhiteSpace(request.FactoriDeRisc) ||
                !string.IsNullOrWhiteSpace(request.Alergii))
            {
                await _repository.UpsertAntecedenteAsync(consultatieId, new ConsultatieAntecedente
                {
                    ConsultatieID = consultatieId,
                    IstoricMedicalPersonal = request.IstoricMedicalPersonal,
                    IstoricFamilial = request.IstoricFamilial,
                    TratamentAnterior = request.TratamentAnterior,
                    FactoriDeRisc = request.FactoriDeRisc,
                    Alergii = request.Alergii,
                    CreatDe = request.CreatDeSauModificatDe,
                    DataCreare = DateTime.Now,
                    DataUltimeiModificari = DateTime.Now
                });
            }

            // 4. ConsultatieExamenObiectiv (1:1) - condiționat
            if (request.Greutate.HasValue || request.Inaltime.HasValue || 
                imc.HasValue || request.Temperatura.HasValue ||
                !string.IsNullOrWhiteSpace(request.TensiuneArteriala) ||
                request.Puls.HasValue || request.FreccventaRespiratorie.HasValue ||
                request.SaturatieO2.HasValue || request.Glicemie.HasValue ||
                !string.IsNullOrWhiteSpace(request.StareGenerala) ||
                !string.IsNullOrWhiteSpace(request.Tegumente) ||
                !string.IsNullOrWhiteSpace(request.Mucoase) ||
                !string.IsNullOrWhiteSpace(request.Edeme) ||
                !string.IsNullOrWhiteSpace(request.GanglioniLimfatici) ||
                !string.IsNullOrWhiteSpace(request.ExamenObiectivDetaliat) ||
                !string.IsNullOrWhiteSpace(request.AlteObservatiiClinice))
            {
                await _repository.UpsertExamenObiectivAsync(consultatieId, new ConsultatieExamenObiectiv
                {
                    ConsultatieID = consultatieId,
                    Greutate = request.Greutate,
                    Inaltime = request.Inaltime,
                    IMC = imc,
                    Temperatura = request.Temperatura,
                    TensiuneArteriala = request.TensiuneArteriala,
                    Puls = request.Puls,
                    FreccventaRespiratorie = request.FreccventaRespiratorie,
                    SaturatieO2 = request.SaturatieO2,
                    Glicemie = request.Glicemie,
                    StareGenerala = request.StareGenerala,
                    Tegumente = request.Tegumente,
                    Mucoase = request.Mucoase,
                    Edeme = request.Edeme,
                    GanglioniLimfatici = request.GanglioniLimfatici,
                    ExamenObiectivDetaliat = request.ExamenObiectivDetaliat,
                    AlteObservatiiClinice = request.AlteObservatiiClinice,
                    CreatDe = request.CreatDeSauModificatDe,
                    DataCreare = DateTime.Now,
                    DataUltimeiModificari = DateTime.Now
                });
            }

            // 5. ConsultatieInvestigatii (1:1) - condiționat
            if (!string.IsNullOrWhiteSpace(request.InvestigatiiLaborator) ||
                !string.IsNullOrWhiteSpace(request.AlteInvestigatii))
            {
                await _repository.UpsertInvestigatiiAsync(consultatieId, new ConsultatieInvestigatii
                {
                    ConsultatieID = consultatieId,
                    InvestigatiiLaborator = request.InvestigatiiLaborator,
                    AlteInvestigatii = request.AlteInvestigatii,
                    CreatDe = request.CreatDeSauModificatDe,
                    DataCreare = DateTime.Now,
                    DataUltimeiModificari = DateTime.Now
                });
            }

            // 6. ConsultatieDiagnostic (1:1) - Diagnostic Principal
            if (!string.IsNullOrWhiteSpace(request.CodICD10Principal) ||
                !string.IsNullOrWhiteSpace(request.NumeDiagnosticPrincipal) ||
                !string.IsNullOrWhiteSpace(request.DescriereDetaliataPrincipal) ||
                !string.IsNullOrWhiteSpace(request.DiagnosticPozitiv) ||
                !string.IsNullOrWhiteSpace(request.CoduriICD10))
            {
                await _repository.UpsertDiagnosticAsync(consultatieId, new ConsultatieDiagnostic
                {
                    ConsultatieID = consultatieId,
                    // NEW: Diagnostic Principal normalizat
                    CodICD10Principal = request.CodICD10Principal,
                    NumeDiagnosticPrincipal = request.NumeDiagnosticPrincipal,
                    DescriereDetaliataPrincipal = request.DescriereDetaliataPrincipal,
                    // LEGACY: backwards compatibility
                    DiagnosticPozitiv = request.DiagnosticPozitiv,
                    CoduriICD10 = request.CoduriICD10 ?? request.CodICD10Principal, // fallback to new field
                    CreatDe = request.CreatDeSauModificatDe,
                    DataCreare = DateTime.Now,
                    DataUltimeiModificari = DateTime.Now
                });
            }

            // 6b. ConsultatieDiagnosticSecundar (1:N) - Diagnostice Secundare
            if (request.DiagnosticeSecundare != null && request.DiagnosticeSecundare.Any())
            {
                var diagnosticeSecundare = request.DiagnosticeSecundare.Select((d, index) => new ConsultatieDiagnosticSecundar
                {
                    // Id from DTO: Guid.Empty = new diagnostic, otherwise = existing (preserve for MERGE)
                    Id = d.Id,
                    ConsultatieID = consultatieId,
                    OrdineAfisare = d.OrdineAfisare > 0 ? d.OrdineAfisare : index + 1,
                    CodICD10 = d.CodICD10,
                    NumeDiagnostic = d.NumeDiagnostic,
                    Descriere = d.Descriere,
                    CreatDe = request.CreatDeSauModificatDe,
                    DataCreare = DateTime.Now
                });

                await _repository.SyncDiagnosticeSecundareAsync(
                    consultatieId, 
                    diagnosticeSecundare, 
                    request.CreatDeSauModificatDe);
            }

            // 7. ConsultatieTratament (1:1) - condiționat
            if (!string.IsNullOrWhiteSpace(request.TratamentMedicamentos) ||
                !string.IsNullOrWhiteSpace(request.RecomandariRegimViata) ||
                !string.IsNullOrWhiteSpace(request.DataUrmatoareiProgramari) ||
                !string.IsNullOrWhiteSpace(request.RecomandariSupraveghere))
            {
                await _repository.UpsertTratamentAsync(consultatieId, new ConsultatieTratament
                {
                    ConsultatieID = consultatieId,
                    TratamentMedicamentos = request.TratamentMedicamentos,
                    RecomandariRegimViata = request.RecomandariRegimViata,
                    DataUrmatoareiProgramari = request.DataUrmatoareiProgramari,
                    RecomandariSupraveghere = request.RecomandariSupraveghere,
                    CreatDe = request.CreatDeSauModificatDe,
                    DataCreare = DateTime.Now,
                    DataUltimeiModificari = DateTime.Now
                });
            }

            // 7b. ConsultatieMedicament (1:N) - Lista de medicamente prescrise
            var medicamente = (request.MedicationList ?? new())
                .Where(m => !string.IsNullOrWhiteSpace(m.Name))
                .Select((m, index) => new ConsultatieMedicament
                {
                    // Id from DTO: Guid.Empty = new medication, otherwise = existing (preserve for MERGE)
                    Id = m.Id,
                    ConsultatieID = consultatieId,
                    OrdineAfisare = index,
                    NumeMedicament = m.Name,
                    Doza = m.Dose,
                    Frecventa = m.Frequency,
                    Durata = m.Duration,
                    Cantitate = m.Quantity,
                    Observatii = m.Notes,
                    CreatDe = request.CreatDeSauModificatDe,
                    DataCreare = DateTime.Now
                });
            await _repository.ReplaceMedicamenteAsync(consultatieId, medicamente, request.CreatDeSauModificatDe);

            // 8. ConsultatieConcluzii (1:1) - condiționat - include Scrisoare Medicală Anexa 43
            if (!string.IsNullOrWhiteSpace(request.Concluzie) ||
                !string.IsNullOrWhiteSpace(request.ObservatiiMedic) ||
                request.EsteAfectiuneOncologica ||
                request.AreIndicatieInternare ||
                request.SaEliberatPrescriptie == true ||
                request.SaEliberatConcediuMedical == true ||
                request.SaEliberatIngrijiriDomiciliu == true ||
                request.SaEliberatDispozitiveMedicale == true ||
                request.TransmiterePrinEmail)
            {
                await _repository.UpsertConcluziiAsync(consultatieId, new ConsultatieConcluzii
                {
                    ConsultatieID = consultatieId,
                    Concluzie = request.Concluzie,
                    ObservatiiMedic = request.ObservatiiMedic,
                    // Scrisoare Medicala - Anexa 43
                    EsteAfectiuneOncologica = request.EsteAfectiuneOncologica,
                    DetaliiAfectiuneOncologica = request.DetaliiAfectiuneOncologica,
                    AreIndicatieInternare = request.AreIndicatieInternare,
                    TermenInternare = request.TermenInternare,
                    SaEliberatPrescriptie = request.SaEliberatPrescriptie,
                    SeriePrescriptie = request.SeriePrescriptie,
                    SaEliberatConcediuMedical = request.SaEliberatConcediuMedical,
                    SerieConcediuMedical = request.SerieConcediuMedical,
                    SaEliberatIngrijiriDomiciliu = request.SaEliberatIngrijiriDomiciliu,
                    SaEliberatDispozitiveMedicale = request.SaEliberatDispozitiveMedicale,
                    TransmiterePrinEmail = request.TransmiterePrinEmail,
                    EmailTransmitere = request.EmailTransmitere,
                    CreatDe = request.CreatDeSauModificatDe,
                    DataCreare = DateTime.Now,
                    DataUltimeiModificari = DateTime.Now
                });
            }

            _logger.LogInformation(
                "[SaveConsultatieDraftHandler] Draft saved successfully with normalized structure: {ConsultatieID}",
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