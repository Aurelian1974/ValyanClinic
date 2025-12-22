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

            // Map to DetailDto - COMPLETE mapping including all UI fields
            var dto = new ConsulatieDetailDto
            {
                // Primary Keys
                ConsultatieID = consultatie.ConsultatieID,
                ProgramareID = consultatie.ProgramareID,
                PacientID = consultatie.PacientID,
                MedicID = consultatie.MedicID,
                DataConsultatie = consultatie.DataConsultatie,
                OraConsultatie = consultatie.OraConsultatie,
                TipConsultatie = consultatie.TipConsultatie,
                
                // Tab 1: Motiv & Antecedente
                MotivPrezentare = consultatie.MotivPrezentare,
                IstoricBoalaActuala = consultatie.IstoricBoalaActuala,
                APP_Medicatie = consultatie.APP_Medicatie,
                
                // Tab 2: Examen General
                StareGenerala = consultatie.StareGenerala,
                Constitutie = consultatie.Constitutie,
                Atitudine = consultatie.Atitudine,
                Facies = consultatie.Facies,
                Tegumente = consultatie.Tegumente,
                Mucoase = consultatie.Mucoase,
                GangliniLimfatici = consultatie.GangliniLimfatici,
                Edeme = consultatie.Edeme,
                
                // Tab 2: Semne Vitale
                Greutate = consultatie.Greutate,
                Inaltime = consultatie.Inaltime,
                IMC = consultatie.IMC,
                Temperatura = consultatie.Temperatura,
                TensiuneArteriala = consultatie.TensiuneArteriala,
                Puls = consultatie.Puls,
                FreccventaRespiratorie = consultatie.FreccventaRespiratorie,
                SaturatieO2 = consultatie.SaturatieO2,
                Glicemie = consultatie.Glicemie,
                
                // Tab 2: Examen pe Aparate
                ExamenCardiovascular = consultatie.ExamenCardiovascular,
                ExamenRespiratoriu = consultatie.ExamenRespiratoriu,
                ExamenDigestiv = consultatie.ExamenDigestiv,
                ExamenUrinar = consultatie.ExamenUrinar,
                ExamenNervos = consultatie.ExamenNervos,
                ExamenLocomotor = consultatie.ExamenLocomotor,
                ExamenEndocrin = consultatie.ExamenEndocrin,
                ExamenORL = consultatie.ExamenORL,
                ExamenOftalmologic = consultatie.ExamenOftalmologic,
                ExamenDermatologic = consultatie.ExamenDermatologic,
                
                // Tab 2: Investigații
                InvestigatiiLaborator = consultatie.InvestigatiiLaborator,
                InvestigatiiImagistice = consultatie.InvestigatiiImagistice,
                InvestigatiiEKG = consultatie.InvestigatiiEKG,
                AlteInvestigatii = consultatie.AlteInvestigatii,
                
                // Tab 3: Diagnostic
                DiagnosticPozitiv = consultatie.DiagnosticPozitiv,
                DiagnosticDiferential = consultatie.DiagnosticDiferential,
                DiagnosticEtiologic = consultatie.DiagnosticEtiologic,
                CoduriICD10 = consultatie.CoduriICD10,
                CoduriICD10Secundare = consultatie.CoduriICD10Secundare,
                
                // Tab 3: Tratament
                TratamentMedicamentos = consultatie.TratamentMedicamentos,
                TratamentNemedicamentos = consultatie.TratamentNemedicamentos,
                RecomandariDietetice = consultatie.RecomandariDietetice,
                RecomandariRegimViata = consultatie.RecomandariRegimViata,
                InvestigatiiRecomandate = consultatie.InvestigatiiRecomandate,
                ConsulturiSpecialitate = consultatie.ConsulturiSpecialitate,
                
                // Tab 4: Concluzii
                DataUrmatoareiProgramari = consultatie.DataUrmatoareiProgramari,
                RecomandariSupraveghere = consultatie.RecomandariSupraveghere,
                Prognostic = consultatie.Prognostic,
                Concluzie = consultatie.Concluzie,
                ObservatiiMedic = consultatie.ObservatiiMedic,
                NotePacient = consultatie.NotePacient,
                
                // Metadata
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
