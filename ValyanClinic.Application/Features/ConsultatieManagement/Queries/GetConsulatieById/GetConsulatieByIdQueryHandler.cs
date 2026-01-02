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

            // Map to DetailDto using NORMALIZED structure with navigation properties
            var dto = new ConsulatieDetailDto
            {
                // MASTER fields
                ConsultatieID = consultatie.ConsultatieID,
                ProgramareID = consultatie.ProgramareID,
                PacientID = consultatie.PacientID,
                MedicID = consultatie.MedicID,
                DataConsultatie = consultatie.DataConsultatie,
                OraConsultatie = consultatie.OraConsultatie,
                TipConsultatie = consultatie.TipConsultatie,
                Status = consultatie.Status,
                DataFinalizare = consultatie.DataFinalizare,
                DurataMinute = consultatie.DurataMinute,
                DataCreare = consultatie.DataCreare,
                CreatDe = consultatie.CreatDe,
                DataUltimeiModificari = consultatie.DataUltimeiModificari,
                ModificatDe = consultatie.ModificatDe,
                
                // ConsultatieMotivePrezentare (1:1) - NULL-SAFE navigation
                MotivPrezentare = consultatie.MotivePrezentare?.MotivPrezentare,
                IstoricBoalaActuala = consultatie.MotivePrezentare?.IstoricBoalaActuala,
                
                // ConsultatieAntecedente (1:1) - NULL-SAFE navigation
                AHC_Mama = consultatie.Antecedente?.AHC_Mama,
                AHC_Tata = consultatie.Antecedente?.AHC_Tata,
                AHC_Frati = consultatie.Antecedente?.AHC_Frati,
                AHC_Bunici = consultatie.Antecedente?.AHC_Bunici,
                AHC_Altele = consultatie.Antecedente?.AHC_Altele,
                AF_Nastere = consultatie.Antecedente?.AF_Nastere,
                AF_Dezvoltare = consultatie.Antecedente?.AF_Dezvoltare,
                AF_Menstruatie = consultatie.Antecedente?.AF_Menstruatie,
                AF_Sarcini = consultatie.Antecedente?.AF_Sarcini,
                AF_Alaptare = consultatie.Antecedente?.AF_Alaptare,
                APP_BoliCopilarieAdolescenta = consultatie.Antecedente?.APP_BoliCopilarieAdolescenta,
                APP_BoliAdult = consultatie.Antecedente?.APP_BoliAdult,
                APP_Interventii = consultatie.Antecedente?.APP_Interventii,
                APP_Traumatisme = consultatie.Antecedente?.APP_Traumatisme,
                APP_Transfuzii = consultatie.Antecedente?.APP_Transfuzii,
                APP_Alergii = consultatie.Antecedente?.APP_Alergii,
                APP_Medicatie = consultatie.Antecedente?.APP_Medicatie,
                Profesie = consultatie.Antecedente?.Profesie,
                ConditiiLocuinta = consultatie.Antecedente?.ConditiiLocuinta,
                ConditiiMunca = consultatie.Antecedente?.ConditiiMunca,
                ObiceiuriAlimentare = consultatie.Antecedente?.ObiceiuriAlimentare,
                Toxice = consultatie.Antecedente?.Toxice,
                
                // ConsultatieExamenObiectiv (1:1) - NULL-SAFE navigation
                StareGenerala = consultatie.ExamenObiectiv?.StareGenerala,
                Constitutie = consultatie.ExamenObiectiv?.Constitutie,
                Atitudine = consultatie.ExamenObiectiv?.Atitudine,
                Facies = consultatie.ExamenObiectiv?.Facies,
                Tegumente = consultatie.ExamenObiectiv?.Tegumente,
                Mucoase = consultatie.ExamenObiectiv?.Mucoase,
                GangliniLimfatici = consultatie.ExamenObiectiv?.GangliniLimfatici,
                Edeme = consultatie.ExamenObiectiv?.Edeme,
                Greutate = consultatie.ExamenObiectiv?.Greutate,
                Inaltime = consultatie.ExamenObiectiv?.Inaltime,
                IMC = consultatie.ExamenObiectiv?.IMC,
                Temperatura = consultatie.ExamenObiectiv?.Temperatura,
                TensiuneArteriala = consultatie.ExamenObiectiv?.TensiuneArteriala,
                Puls = consultatie.ExamenObiectiv?.Puls,
                FreccventaRespiratorie = consultatie.ExamenObiectiv?.FreccventaRespiratorie,
                SaturatieO2 = consultatie.ExamenObiectiv?.SaturatieO2,
                Glicemie = consultatie.ExamenObiectiv?.Glicemie,
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
                
                // ConsultatieInvestigatii (1:1) - NULL-SAFE navigation
                InvestigatiiLaborator = consultatie.Investigatii?.InvestigatiiLaborator,
                InvestigatiiImagistice = consultatie.Investigatii?.InvestigatiiImagistice,
                InvestigatiiEKG = consultatie.Investigatii?.InvestigatiiEKG,
                AlteInvestigatii = consultatie.Investigatii?.AlteInvestigatii,
                
                // ConsultatieDiagnostic (1:1) - NULL-SAFE navigation
                DiagnosticPozitiv = consultatie.Diagnostic?.DiagnosticPozitiv,
                DiagnosticDiferential = consultatie.Diagnostic?.DiagnosticDiferential,
                DiagnosticEtiologic = consultatie.Diagnostic?.DiagnosticEtiologic,
                CoduriICD10 = consultatie.Diagnostic?.CoduriICD10,
                CoduriICD10Secundare = consultatie.Diagnostic?.CoduriICD10Secundare,
                
                // ConsultatieTratament (1:1) - NULL-SAFE navigation
                TratamentMedicamentos = consultatie.Tratament?.TratamentMedicamentos,
                TratamentNemedicamentos = consultatie.Tratament?.TratamentNemedicamentos,
                RecomandariDietetice = consultatie.Tratament?.RecomandariDietetice,
                RecomandariRegimViata = consultatie.Tratament?.RecomandariRegimViata,
                InvestigatiiRecomandate = consultatie.Tratament?.InvestigatiiRecomandate,
                ConsulturiSpecialitate = consultatie.Tratament?.ConsulturiSpecialitate,
                DataUrmatoareiProgramari = consultatie.Tratament?.DataUrmatoareiProgramari,
                RecomandariSupraveghere = consultatie.Tratament?.RecomandariSupraveghere,
                
                // ConsultatieConcluzii (1:1) - NULL-SAFE navigation
                Prognostic = consultatie.Concluzii?.Prognostic,
                Concluzie = consultatie.Concluzii?.Concluzie,
                ObservatiiMedic = consultatie.Concluzii?.ObservatiiMedic,
                NotePacient = consultatie.Concluzii?.NotePacient,
                DocumenteAtatate = consultatie.Concluzii?.DocumenteAtatate,
                
                // TODO: Add JOIN data from Pacienti and PersonalMedical tables
                PacientNumeComplet = "N/A", // Will be populated with JOIN in SP
                MedicNumeComplet = "N/A"    // Will be populated with JOIN in SP
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
