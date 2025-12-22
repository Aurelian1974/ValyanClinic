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
