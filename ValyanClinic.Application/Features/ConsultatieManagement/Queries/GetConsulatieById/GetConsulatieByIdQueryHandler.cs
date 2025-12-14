using MediatR;
using Microsoft.Extensions.Logging;
using ValyanClinic.Application.Common.Results;
using ValyanClinic.Application.Features.ConsultatieManagement.DTOs;
using ValyanClinic.Infrastructure.Repositories.Interfaces;

namespace ValyanClinic.Application.Features.ConsultatieManagement.Queries.GetConsulatieById;

/// <summary>
/// Handler pentru GetConsulatieByIdQuery
/// Obtine detalii complete consultatie folosind sp_Consultatie_GetById
/// </summary>
public class GetConsulatieByIdQueryHandler : IRequestHandler<GetConsulatieByIdQuery, Result<ConsulatieDetailDto>>
{
    private readonly IConsultatieRepository _repository;
    private readonly ILogger<GetConsulatieByIdQueryHandler> _logger;

    public GetConsulatieByIdQueryHandler(
        IConsultatieRepository repository,
        ILogger<GetConsulatieByIdQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<Result<ConsulatieDetailDto>> Handle(
        GetConsulatieByIdQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation(
                "[GetConsulatieByIdHandler] Fetching consultatie ID: {ConsultatieID}",
                request.ConsultatieID);

            // Validare
            if (request.ConsultatieID == Guid.Empty)
                return Result<ConsulatieDetailDto>.Failure("ConsultatieID este obligatoriu");

            // Get from repository (executes sp_Consultatie_GetById)
            var consultatie = await _repository.GetByIdAsync(request.ConsultatieID, cancellationToken);

            if (consultatie == null)
            {
                _logger.LogWarning(
                    "[GetConsulatieByIdHandler] Consultatie not found: {ConsultatieID}",
                    request.ConsultatieID);

                return Result<ConsulatieDetailDto>.Failure(
                    $"Consultatia cu ID-ul {request.ConsultatieID} nu a fost găsită");
            }

            // Map to DetailDto (TODO: Add AutoMapper or manual mapping with JOIN data)
            // For now, basic mapping from entity
            var dto = new ConsulatieDetailDto
            {
                ConsultatieID = consultatie.ConsultatieID,
                ProgramareID = consultatie.ProgramareID,
                PacientID = consultatie.PacientID,
                MedicID = consultatie.MedicID,
                DataConsultatie = consultatie.DataConsultatie,
                OraConsultatie = consultatie.OraConsultatie,
                TipConsultatie = consultatie.TipConsultatie,
                MotivPrezentare = consultatie.MotivPrezentare,
                IstoricBoalaActuala = consultatie.IstoricBoalaActuala,
                AHC_Mama = consultatie.AHC_Mama,
                AHC_Tata = consultatie.AHC_Tata,
                AHC_Frati = consultatie.AHC_Frati,
                AHC_Bunici = consultatie.AHC_Bunici,
                AHC_Altele = consultatie.AHC_Altele,
                AF_Nastere = consultatie.AF_Nastere,
                AF_Dezvoltare = consultatie.AF_Dezvoltare,
                AF_Menstruatie = consultatie.AF_Menstruatie,
                AF_Sarcini = consultatie.AF_Sarcini,
                AF_Alaptare = consultatie.AF_Alaptare,
                APP_BoliCopilarieAdolescenta = consultatie.APP_BoliCopilarieAdolescenta,
                APP_BoliAdult = consultatie.APP_BoliAdult,
                APP_Interventii = consultatie.APP_Interventii,
                APP_Traumatisme = consultatie.APP_Traumatisme,
                APP_Transfuzii = consultatie.APP_Transfuzii,
                APP_Alergii = consultatie.APP_Alergii,
                APP_Medicatie = consultatie.APP_Medicatie,
                Profesie = consultatie.Profesie,
                ConditiiLocuinta = consultatie.ConditiiLocuinta,
                ConditiiMunca = consultatie.ConditiiMunca,
                ObiceiuriAlimentare = consultatie.ObiceiuriAlimentare,
                Toxice = consultatie.Toxice,
                StareGenerala = consultatie.StareGenerala,
                Constitutie = consultatie.Constitutie,
                Atitudine = consultatie.Atitudine,
                Facies = consultatie.Facies,
                Tegumente = consultatie.Tegumente,
                Mucoase = consultatie.Mucoase,
                GangliniLimfatici = consultatie.GangliniLimfatici,
                Greutate = consultatie.Greutate,
                Inaltime = consultatie.Inaltime,
                IMC = consultatie.IMC,
                Temperatura = consultatie.Temperatura,
                TensiuneArteriala = consultatie.TensiuneArteriala,
                Puls = consultatie.Puls,
                FreccventaRespiratorie = consultatie.FreccventaRespiratorie,
                SaturatieO2 = consultatie.SaturatieO2,
                Glicemie = consultatie.Glicemie,
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
                InvestigatiiLaborator = consultatie.InvestigatiiLaborator,
                InvestigatiiImagistice = consultatie.InvestigatiiImagistice,
                InvestigatiiEKG = consultatie.InvestigatiiEKG,
                AlteInvestigatii = consultatie.AlteInvestigatii,
                DiagnosticPozitiv = consultatie.DiagnosticPozitiv,
                DiagnosticDiferential = consultatie.DiagnosticDiferential,
                DiagnosticEtiologic = consultatie.DiagnosticEtiologic,
                CoduriICD10 = consultatie.CoduriICD10,
                TratamentMedicamentos = consultatie.TratamentMedicamentos,
                TratamentNemedicamentos = consultatie.TratamentNemedicamentos,
                RecomandariDietetice = consultatie.RecomandariDietetice,
                RecomandariRegimViata = consultatie.RecomandariRegimViata,
                InvestigatiiRecomandate = consultatie.InvestigatiiRecomandate,
                ConsulturiSpecialitate = consultatie.ConsulturiSpecialitate,
                DataUrmatoareiProgramari = consultatie.DataUrmatoareiProgramari,
                RecomandariSupraveghere = consultatie.RecomandariSupraveghere,
                Prognostic = consultatie.Prognostic,
                Concluzie = consultatie.Concluzie,
                ObservatiiMedic = consultatie.ObservatiiMedic,
                NotePacient = consultatie.NotePacient,
                Status = consultatie.Status,
                DataFinalizare = consultatie.DataFinalizare,
                DurataMinute = consultatie.DurataMinute,
                DocumenteAtatate = consultatie.DocumenteAtatate,
                DataCreare = consultatie.DataCreare,
                CreatDe = consultatie.CreatDe,
                DataUltimeiModificari = consultatie.DataUltimeiModificari,
                ModificatDe = consultatie.ModificatDe,
                
                // TODO: Add JOIN data from Pacienti and PersonalMedical tables
                PacientNumeComplet = "N/A", // Will be populated with JOIN
                MedicNumeComplet = "N/A"    // Will be populated with JOIN
            };

            _logger.LogInformation(
                "[GetConsulatieByIdHandler] Consultatie fetched successfully: {ConsultatieID}",
                request.ConsultatieID);

            return Result<ConsulatieDetailDto>.Success(dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "[GetConsulatieByIdHandler] Error fetching consultatie ID: {ConsultatieID}",
                request.ConsultatieID);

            return Result<ConsulatieDetailDto>.Failure(
                $"Eroare la obținerea consultatiei: {ex.Message}");
        }
    }
}
