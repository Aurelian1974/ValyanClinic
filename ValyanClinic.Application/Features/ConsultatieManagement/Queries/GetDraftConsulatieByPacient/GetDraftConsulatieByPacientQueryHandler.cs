using MediatR;
using Microsoft.Extensions.Logging;
using ValyanClinic.Application.Common.Results;
using ValyanClinic.Application.Features.ConsultatieManagement.DTOs;
using ValyanClinic.Infrastructure.Repositories.Interfaces;

namespace ValyanClinic.Application.Features.ConsultatieManagement.Queries.GetDraftConsulatieByPacient;

/// <summary>
/// Handler pentru GetDraftConsulatieByPacientQuery
/// Caută o consultație draft (nefinalizată) existentă pentru pacient
/// Folosit pentru a preveni crearea de consultații duplicate la reîntoarcerea în pagină
/// </summary>
public class GetDraftConsulatieByPacientQueryHandler : IRequestHandler<GetDraftConsulatieByPacientQuery, Result<ConsulatieDetailDto?>>
{
    private readonly IConsultatieRepository _repository;
    private readonly ILogger<GetDraftConsulatieByPacientQueryHandler> _logger;

    public GetDraftConsulatieByPacientQueryHandler(
        IConsultatieRepository repository,
        ILogger<GetDraftConsulatieByPacientQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<Result<ConsulatieDetailDto?>> Handle(
        GetDraftConsulatieByPacientQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation(
                "[GetDraftConsulatieByPacientHandler] Searching for draft consultatie - PacientID: {PacientID}, MedicID: {MedicID}, Data: {Data}",
                request.PacientID, request.MedicID, request.DataConsultatie);

            // Validare
            if (request.PacientID == Guid.Empty)
                return Result<ConsulatieDetailDto?>.Failure("PacientID este obligatoriu");

            // Caută draft existent
            var consultatie = await _repository.GetDraftByPacientAsync(
                request.PacientID,
                request.MedicID,
                request.DataConsultatie,
                request.ProgramareID,
                cancellationToken);

            // NULL is valid - nu există draft
            if (consultatie == null)
            {
                _logger.LogInformation(
                    "[GetDraftConsulatieByPacientHandler] No draft found for PacientID: {PacientID}",
                    request.PacientID);

                return Result<ConsulatieDetailDto?>.Success(
                    null,
                    "Nu există consultație draft pentru acest pacient");
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
                Constitutie = consultatie.ExamenObiectiv?.Constitutie,
                Atitudine = consultatie.ExamenObiectiv?.Atitudine,
                Facies = consultatie.ExamenObiectiv?.Facies,
                Tegumente = consultatie.ExamenObiectiv?.Tegumente,
                Mucoase = consultatie.ExamenObiectiv?.Mucoase,
                GangliniLimfatici = consultatie.ExamenObiectiv?.GangliniLimfatici,
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
                
                // Tab 2: Examen pe Aparate - from ConsultatieExamenObiectiv
                ExamenCardiovascular = consultatie.ExamenObiectiv?.ExamenCardiovascular,
                ExamenRespiratoriu = consultatie.ExamenObiectiv?.ExamenRespiratoriu,
                ExamenDigestiv = consultatie.ExamenObiectiv?.ExamenDigestiv,
                ExamenUrinar = consultatie.ExamenObiectiv?.ExamenUrinar,
                ExamenNervos = consultatie.ExamenObiectiv?.ExamenNervos,
                ExamenLocomotor = consultatie.ExamenObiectiv?.ExamenLocomotor,
                ExamenEndocrin = consultatie.ExamenObiectiv?.ExamenEndocrin,
                ExamenORL = consultatie.ExamenObiectiv?.ExamenORL,
                ExamenOftalmologic = consultatie.ExamenObiectiv?.ExamenOftalmologic,
                ExamenDermatologic = consultatie.ExamenObiectiv?.ExamenDermatologic,
                
                // Tab 2: Investigații - from ConsultatieInvestigatii
                InvestigatiiLaborator = consultatie.Investigatii?.InvestigatiiLaborator,
                InvestigatiiImagistice = consultatie.Investigatii?.InvestigatiiImagistice,
                InvestigatiiEKG = consultatie.Investigatii?.InvestigatiiEKG,
                AlteInvestigatii = consultatie.Investigatii?.AlteInvestigatii,
                
                // Tab 3: Diagnostic - from ConsultatieDiagnostic
                // NEW: Normalized structure for Scrisoare Medicală
                CodICD10Principal = consultatie.Diagnostic?.CodICD10Principal,
                NumeDiagnosticPrincipal = consultatie.Diagnostic?.NumeDiagnosticPrincipal,
                DescriereDetaliataPrincipal = consultatie.Diagnostic?.DescriereDetaliataPrincipal,
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
                
                // LEGACY fields for backwards compatibility
                DiagnosticPozitiv = consultatie.Diagnostic?.DiagnosticPozitiv,
                DiagnosticDiferential = consultatie.Diagnostic?.DiagnosticDiferential,
                DiagnosticEtiologic = consultatie.Diagnostic?.DiagnosticEtiologic,
                CoduriICD10 = consultatie.Diagnostic?.CoduriICD10,
                CoduriICD10Secundare = consultatie.Diagnostic?.CoduriICD10Secundare,
                
                // Tab 3: Tratament - from ConsultatieTratament
                TratamentMedicamentos = consultatie.Tratament?.TratamentMedicamentos,
                TratamentNemedicamentos = consultatie.Tratament?.TratamentNemedicamentos,
                RecomandariDietetice = consultatie.Tratament?.RecomandariDietetice,
                RecomandariRegimViata = consultatie.Tratament?.RecomandariRegimViata,
                InvestigatiiRecomandate = consultatie.Tratament?.InvestigatiiRecomandate,
                ConsulturiSpecialitate = consultatie.Tratament?.ConsulturiSpecialitate,
                DataUrmatoareiProgramari = consultatie.Tratament?.DataUrmatoareiProgramari,
                RecomandariSupraveghere = consultatie.Tratament?.RecomandariSupraveghere,
                
                // Tab 4: Concluzii - from ConsultatieConcluzii
                Prognostic = consultatie.Concluzii?.Prognostic,
                Concluzie = consultatie.Concluzii?.Concluzie,
                ObservatiiMedic = consultatie.Concluzii?.ObservatiiMedic,
                NotePacient = consultatie.Concluzii?.NotePacient,
                
                // Metadata - from Consultatii master table
                Status = consultatie.Status,
                DataFinalizare = consultatie.DataFinalizare,
                DurataMinute = consultatie.DurataMinute,
                DocumenteAtatate = consultatie.DocumenteAtatate,
                DataCreare = consultatie.DataCreare,
                CreatDe = consultatie.CreatDe,
                DataUltimeiModificari = consultatie.DataUltimeiModificari,
                ModificatDe = consultatie.ModificatDe,
                
                // JOIN data (populated separately if needed)
                PacientNumeComplet = "N/A",
                MedicNumeComplet = "N/A"
            };

            _logger.LogInformation(
                "[GetDraftConsulatieByPacientHandler] ✅ Draft found: {ConsultatieID} for PacientID: {PacientID}",
                consultatie.ConsultatieID, request.PacientID);

            return Result<ConsulatieDetailDto?>.Success(dto, "Consultație draft găsită");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "[GetDraftConsulatieByPacientHandler] Error searching for draft - PacientID: {PacientID}",
                request.PacientID);

            return Result<ConsulatieDetailDto?>.Failure(
                $"Eroare la căutarea consultației draft: {ex.Message}");
        }
    }
}
