using MediatR;
using Microsoft.Extensions.Logging;
using ValyanClinic.Application.Common.Results;
using ValyanClinic.Application.Features.ConsultatieManagement.DTOs;
using ValyanClinic.Domain.Interfaces.Repositories;

namespace ValyanClinic.Application.Features.ConsultatieManagement.Queries.GetConsulatieByProgramare;

/// <summary>
/// Handler pentru GetConsulatieByProgramareQuery
/// Obtine consultatia asociata cu o programare folosind sp_Consultatie_GetByProgramare
/// </summary>
public class GetConsulatieByProgramareQueryHandler : IRequestHandler<GetConsulatieByProgramareQuery, Result<ConsulatieDetailDto?>>
{
    private readonly IConsultatieRepository _repository;
    private readonly ILogger<GetConsulatieByProgramareQueryHandler> _logger;

    public GetConsulatieByProgramareQueryHandler(
        IConsultatieRepository repository,
        ILogger<GetConsulatieByProgramareQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<Result<ConsulatieDetailDto?>> Handle(
        GetConsulatieByProgramareQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation(
                "[GetConsulatieByProgramareHandler] Fetching consultatie for ProgramareID: {ProgramareID}",
                request.ProgramareID);

            // Validare
            if (request.ProgramareID == Guid.Empty)
                return Result<ConsulatieDetailDto?>.Failure("ProgramareID este obligatoriu");

            // Get from repository (executes sp_Consultatie_GetByProgramare)
            var consultatie = await _repository.GetByProgramareIdAsync(request.ProgramareID, cancellationToken);

            // NULL is valid - programarea poate să nu aibă consultatie încă
            if (consultatie == null)
            {
                _logger.LogInformation(
                    "[GetConsulatieByProgramareHandler] No consultatie found for ProgramareID: {ProgramareID}",
                    request.ProgramareID);

                return Result<ConsulatieDetailDto?>.Success(
                    null,
                    "Programarea nu are consultație asociată");
            }

            // Map to DetailDto - NORMALIZED structure with null-safe navigation
            var dto = new ConsulatieDetailDto
            {
                // Primary Keys - from Consultatii master table
                ConsultatieID = consultatie.ConsultatieID,
                ProgramareID = consultatie.ProgramareID,
                PacientID = consultatie.PacientID,
                MedicID = consultatie.MedicID,
                DataConsultatie = consultatie.DataConsultatie,
                OraConsultatie = consultatie.OraConsultatie,
                TipConsultatie = consultatie.TipConsultatie,
                
                // Tab 1: Motiv Prezentare - from ConsultatieMotivePrezentare
                MotivPrezentare = consultatie.MotivePrezentare?.MotivPrezentare,
                IstoricBoalaActuala = consultatie.MotivePrezentare?.IstoricBoalaActuala,
                
                // Tab 1: Antecedente (SIMPLIFIED) - from ConsultatieAntecedente
                IstoricMedicalPersonal = consultatie.Antecedente?.IstoricMedicalPersonal,
                IstoricFamilial = consultatie.Antecedente?.IstoricFamilial,
                TratamentAnterior = consultatie.Antecedente?.TratamentAnterior,
                FactoriDeRisc = consultatie.Antecedente?.FactoriDeRisc,
                Alergii = consultatie.Antecedente?.Alergii,
                
                // Tab 2: Examen General - from ConsultatieExamenObiectiv
                StareGenerala = consultatie.ExamenObiectiv?.StareGenerala,
                Tegumente = consultatie.ExamenObiectiv?.Tegumente,
                Mucoase = consultatie.ExamenObiectiv?.Mucoase,
                GanglioniLimfatici = consultatie.ExamenObiectiv?.GanglioniLimfatici,
                Edeme = consultatie.ExamenObiectiv?.Edeme,
                
                // Tab 2: Semne Vitale - from ConsultatieExamenObiectiv
                Greutate = consultatie.ExamenObiectiv?.Greutate,
                Inaltime = consultatie.ExamenObiectiv?.Inaltime,
                IMC = consultatie.ExamenObiectiv?.IMC,
                Temperatura = consultatie.ExamenObiectiv?.Temperatura,
                TensiuneArteriala = consultatie.ExamenObiectiv?.TensiuneArteriala,
                Puls = consultatie.ExamenObiectiv?.Puls,
                FreccventaRespiratorie = consultatie.ExamenObiectiv?.FreccventaRespiratorie,
                SaturatieO2 = consultatie.ExamenObiectiv?.SaturatieO2,
                Glicemie = consultatie.ExamenObiectiv?.Glicemie,
                
                // Tab 2: Examen Obiectiv Detaliat - from ConsultatieExamenObiectiv
                ExamenObiectivDetaliat = consultatie.ExamenObiectiv?.ExamenObiectivDetaliat,
                AlteObservatiiClinice = consultatie.ExamenObiectiv?.AlteObservatiiClinice,
                
                // Tab 2: Investigații - from ConsultatieInvestigatii
                InvestigatiiLaborator = consultatie.Investigatii?.InvestigatiiLaborator,
                InvestigatiiImagistice = consultatie.Investigatii?.InvestigatiiImagistice,
                InvestigatiiEKG = consultatie.Investigatii?.InvestigatiiEKG,
                AlteInvestigatii = consultatie.Investigatii?.AlteInvestigatii,
                
                // Tab 3: Diagnostic - from ConsultatieDiagnostic
                DiagnosticPozitiv = consultatie.Diagnostic?.DiagnosticPozitiv,
                CoduriICD10 = consultatie.Diagnostic?.CoduriICD10,
                // Normalized fields
                CodICD10Principal = consultatie.Diagnostic?.CodICD10Principal,
                NumeDiagnosticPrincipal = consultatie.Diagnostic?.NumeDiagnosticPrincipal,
                DescriereDetaliataPrincipal = consultatie.Diagnostic?.DescriereDetaliataPrincipal,
                // Secondary diagnoses
                DiagnosticeSecundare = consultatie.Diagnostic?.DiagnosticeSecundare?
                    .OrderBy(d => d.OrdineAfisare)
                    .Select(d => new DiagnosticSecundarDetailDto
                    {
                        Id = d.Id,
                        OrdineAfisare = d.OrdineAfisare,
                        CodICD10 = d.CodICD10,
                        NumeDiagnostic = d.NumeDiagnostic,
                        Descriere = d.Descriere
                    })
                    .ToList(),
                
                // Tab 3: Tratament - from ConsultatieTratament
                TratamentMedicamentos = consultatie.Tratament?.TratamentMedicamentos,
                TratamentNemedicamentos = consultatie.Tratament?.TratamentNemedicamentos,
                RecomandariDietetice = consultatie.Tratament?.RecomandariDietetice,
                RecomandariRegimViata = consultatie.Tratament?.RecomandariRegimViata,
                InvestigatiiRecomandate = consultatie.Tratament?.InvestigatiiRecomandate,
                ConsulturiSpecialitate = consultatie.Tratament?.ConsulturiSpecialitate,
                DataUrmatoareiProgramari = consultatie.Tratament?.DataUrmatoareiProgramari,
                RecomandariSupraveghere = consultatie.Tratament?.RecomandariSupraveghere,
                
                // Medication List from ConsultatieMedicament (1:N)
                MedicationList = consultatie.Medicamente?
                    .OrderBy(m => m.OrdineAfisare)
                    .Select(m => new DTOs.MedicationRowDto
                    {
                        Id = m.Id,
                        Name = m.NumeMedicament,
                        Dose = m.Doza ?? "",
                        Frequency = m.Frecventa ?? "",
                        Duration = m.Durata ?? "",
                        Quantity = m.Cantitate ?? "",
                        Notes = m.Observatii
                    })
                    .ToList(),
                
                // Tab 4: Concluzii - from ConsultatieConcluzii
                Prognostic = consultatie.Concluzii?.Prognostic,
                Concluzie = consultatie.Concluzii?.Concluzie,
                ObservatiiMedic = consultatie.Concluzii?.ObservatiiMedic,
                NotePacient = consultatie.Concluzii?.NotePacient,
                
                // Scrisoare Medicala - Anexa 43
                EsteAfectiuneOncologica = consultatie.Concluzii?.EsteAfectiuneOncologica ?? false,
                DetaliiAfectiuneOncologica = consultatie.Concluzii?.DetaliiAfectiuneOncologica,
                AreIndicatieInternare = consultatie.Concluzii?.AreIndicatieInternare ?? false,
                TermenInternare = consultatie.Concluzii?.TermenInternare,
                SaEliberatPrescriptie = consultatie.Concluzii?.SaEliberatPrescriptie,
                SeriePrescriptie = consultatie.Concluzii?.SeriePrescriptie,
                SaEliberatConcediuMedical = consultatie.Concluzii?.SaEliberatConcediuMedical,
                SerieConcediuMedical = consultatie.Concluzii?.SerieConcediuMedical,
                SaEliberatIngrijiriDomiciliu = consultatie.Concluzii?.SaEliberatIngrijiriDomiciliu,
                SaEliberatDispozitiveMedicale = consultatie.Concluzii?.SaEliberatDispozitiveMedicale,
                TransmiterePrinEmail = consultatie.Concluzii?.TransmiterePrinEmail ?? false,
                EmailTransmitere = consultatie.Concluzii?.EmailTransmitere,
                
                // Metadata - from Consultatii master table
                Status = consultatie.Status,
                DataFinalizare = consultatie.DataFinalizare,
                DurataMinute = consultatie.DurataMinute,
                DocumenteAtatate = consultatie.Concluzii?.DocumenteAtatate,
                DataCreare = consultatie.DataCreare,
                CreatDe = consultatie.CreatDe,
                DataUltimeiModificari = consultatie.DataUltimeiModificari,
                ModificatDe = consultatie.ModificatDe,
                
                // JOIN data (populated separately if needed)
                PacientNumeComplet = "N/A",
                MedicNumeComplet = "N/A"
            };

            _logger.LogInformation(
                "[GetConsulatieByProgramareHandler] Consultatie found: {ConsultatieID} for ProgramareID: {ProgramareID}",
                consultatie.ConsultatieID, request.ProgramareID);

            return Result<ConsulatieDetailDto?>.Success(dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "[GetConsulatieByProgramareHandler] Error fetching consultatie for ProgramareID: {ProgramareID}",
                request.ProgramareID);

            return Result<ConsulatieDetailDto?>.Failure(
                $"Eroare la obținerea consultatiei pentru programare: {ex.Message}");
        }
    }
}
