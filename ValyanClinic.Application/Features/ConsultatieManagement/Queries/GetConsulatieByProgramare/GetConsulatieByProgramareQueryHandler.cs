using MediatR;
using Microsoft.Extensions.Logging;
using ValyanClinic.Application.Common.Results;
using ValyanClinic.Application.Features.ConsultatieManagement.DTOs;
using ValyanClinic.Infrastructure.Repositories.Interfaces;

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
