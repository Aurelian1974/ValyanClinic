using MediatR;
using Microsoft.Extensions.Logging;
using ValyanClinic.Application.Common.Results;
using ValyanClinic.Application.Features.ConsultatieManagement.DTOs;
using ValyanClinic.Domain.Interfaces.Repositories;

namespace ValyanClinic.Application.Features.ConsultatieManagement.Queries.GetConsulatieById;

/// <summary>
/// Handler pentru GetConsulatieByIdQuery
/// Obtine detalii complete consultatie folosind sp_Consultatie_GetById
/// </summary>
public class GetConsulatieByIdQueryHandler : IRequestHandler<GetConsulatieByIdQuery, Result<ConsulatieDetailDto>>
{
    private readonly IConsultatieBaseRepository _repository;
    private readonly ILogger<GetConsulatieByIdQueryHandler> _logger;

    /// <summary>
    /// Initializes a new instance of <see cref="GetConsulatieByIdQueryHandler"/> with the required repository and logger.
    /// </summary>
    public GetConsulatieByIdQueryHandler(
        IConsultatieBaseRepository repository,
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
                
                // ConsultatieAntecedente (1:1) - SIMPLIFIED - NULL-SAFE navigation
                IstoricMedicalPersonal = consultatie.Antecedente?.IstoricMedicalPersonal,
                IstoricFamilial = consultatie.Antecedente?.IstoricFamilial,
                
                // ConsultatieExamenObiectiv (1:1) - NULL-SAFE navigation
                StareGenerala = consultatie.ExamenObiectiv?.StareGenerala,
                Tegumente = consultatie.ExamenObiectiv?.Tegumente,
                Mucoase = consultatie.ExamenObiectiv?.Mucoase,
                GanglioniLimfatici = consultatie.ExamenObiectiv?.GanglioniLimfatici,
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
                
                // ConsultatieInvestigatii (1:1) - NULL-SAFE navigation
                InvestigatiiLaborator = consultatie.Investigatii?.InvestigatiiLaborator,
                InvestigatiiImagistice = consultatie.Investigatii?.InvestigatiiImagistice,
                InvestigatiiEKG = consultatie.Investigatii?.InvestigatiiEKG,
                AlteInvestigatii = consultatie.Investigatii?.AlteInvestigatii,
                
                // ConsultatieDiagnostic (1:1) - NULL-SAFE navigation
                DiagnosticPozitiv = consultatie.Diagnostic?.DiagnosticPozitiv,
                CoduriICD10 = consultatie.Diagnostic?.CoduriICD10,
                // Normalized fields
                CodICD10Principal = consultatie.Diagnostic?.CodICD10Principal,
                NumeDiagnosticPrincipal = consultatie.Diagnostic?.NumeDiagnosticPrincipal,
                DescriereDetaliataPrincipal = consultatie.Diagnostic?.DescriereDetaliataPrincipal,
                
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