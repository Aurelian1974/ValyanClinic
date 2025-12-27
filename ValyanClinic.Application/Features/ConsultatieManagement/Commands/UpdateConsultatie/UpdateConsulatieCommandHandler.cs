using MediatR;
using Microsoft.Extensions.Logging;
using ValyanClinic.Application.Common.Results;
using ValyanClinic.Domain.Entities;
using ValyanClinic.Infrastructure.Repositories.Interfaces;

namespace ValyanClinic.Application.Features.ConsultatieManagement.Commands.UpdateConsultatie;

/// <summary>
/// Handler pentru UpdateConsulatieCommand
/// Actualizeaza o consultatie medicala existenta folosind Repository pattern si Stored Procedures
/// </summary>
public class UpdateConsulatieCommandHandler : IRequestHandler<UpdateConsulatieCommand, Result<bool>>
{
    private readonly IConsultatieRepository _repository;
    private readonly ILogger<UpdateConsulatieCommandHandler> _logger;

    public UpdateConsulatieCommandHandler(
        IConsultatieRepository repository,
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

            // Map to entity
            var consultatie = new Consultatie
            {
                ConsultatieID = request.ConsultatieID,
                ProgramareID = request.ProgramareID,
                PacientID = request.PacientID,
                MedicID = request.MedicID,
                DataConsultatie = request.DataConsultatie,
                OraConsultatie = request.OraConsultatie,
                TipConsultatie = request.TipConsultatie,
                MotivPrezentare = request.MotivPrezentare,
                IstoricBoalaActuala = request.IstoricBoalaActuala,
                AHC_Mama = request.AHC_Mama,
                AHC_Tata = request.AHC_Tata,
                AHC_Frati = request.AHC_Frati,
                AHC_Bunici = request.AHC_Bunici,
                AHC_Altele = request.AHC_Altele,
                AF_Nastere = request.AF_Nastere,
                AF_Dezvoltare = request.AF_Dezvoltare,
                AF_Menstruatie = request.AF_Menstruatie,
                AF_Sarcini = request.AF_Sarcini,
                AF_Alaptare = request.AF_Alaptare,
                APP_BoliCopilarieAdolescenta = request.APP_BoliCopilarieAdolescenta,
                APP_BoliAdult = request.APP_BoliAdult,
                APP_Interventii = request.APP_Interventii,
                APP_Traumatisme = request.APP_Traumatisme,
                APP_Transfuzii = request.APP_Transfuzii,
                APP_Alergii = request.APP_Alergii,
                APP_Medicatie = request.APP_Medicatie,
                Profesie = request.Profesie,
                ConditiiLocuinta = request.ConditiiLocuinta,
                ConditiiMunca = request.ConditiiMunca,
                ObiceiuriAlimentare = request.ObiceiuriAlimentare,
                Toxice = request.Toxice,
                StareGenerala = request.StareGenerala,
                Constitutie = request.Constitutie,
                Atitudine = request.Atitudine,
                Facies = request.Facies,
                Tegumente = request.Tegumente,
                Mucoase = request.Mucoase,
                GangliniLimfatici = request.GangliniLimfatici,
                Greutate = request.Greutate,
                Inaltime = request.Inaltime,
                IMC = imc,
                Temperatura = request.Temperatura,
                TensiuneArteriala = request.TensiuneArteriala,
                Puls = request.Puls,
                FreccventaRespiratorie = request.FreccventaRespiratorie,
                SaturatieO2 = request.SaturatieO2,
                Glicemie = request.Glicemie,
                ExamenCardiovascular = request.ExamenCardiovascular,
                ExamenRespiratoriu = request.ExamenRespiratoriu,
                ExamenDigestiv = request.ExamenDigestiv,
                ExamenUrinar = request.ExamenUrinar,
                ExamenNervos = request.ExamenNervos,
                ExamenLocomotor = request.ExamenLocomotor,
                ExamenEndocrin = request.ExamenEndocrin,
                ExamenORL = request.ExamenORL,
                ExamenOftalmologic = request.ExamenOftalmologic,
                ExamenDermatologic = request.ExamenDermatologic,
                InvestigatiiLaborator = request.InvestigatiiLaborator,
                InvestigatiiImagistice = request.InvestigatiiImagistice,
                InvestigatiiEKG = request.InvestigatiiEKG,
                AlteInvestigatii = request.AlteInvestigatii,
                DiagnosticPozitiv = request.DiagnosticPozitiv,
                DiagnosticDiferential = request.DiagnosticDiferential,
                DiagnosticEtiologic = request.DiagnosticEtiologic,
                CoduriICD10 = request.CoduriICD10,
                TratamentMedicamentos = request.TratamentMedicamentos,
                TratamentNemedicamentos = request.TratamentNemedicamentos,
                RecomandariDietetice = request.RecomandariDietetice,
                RecomandariRegimViata = request.RecomandariRegimViata,
                InvestigatiiRecomandate = request.InvestigatiiRecomandate,
                ConsulturiSpecialitate = request.ConsulturiSpecialitate,
                DataUrmatoareiProgramari = request.DataUrmatoareiProgramari,
                RecomandariSupraveghere = request.RecomandariSupraveghere,
                Prognostic = request.Prognostic,
                Concluzie = request.Concluzie,
                ObservatiiMedic = request.ObservatiiMedic,
                NotePacient = request.NotePacient,
                Status = request.Status,
                DataFinalizare = request.DataFinalizare,
                DurataMinute = request.DurataMinute,
                DataUltimeiModificari = DateTime.Now,
                ModificatDe = request.ModificatDe
            };

            var success = await _repository.UpdateAsync(consultatie, cancellationToken);

            if (!success)
            {
                _logger.LogWarning("[UpdateConsulatieHandler] Failed to update consultatie ID: {ConsultatieID}", request.ConsultatieID);
                return Result<bool>.Failure("Eroare la actualizarea consultatiei");
            }

            _logger.LogInformation("[UpdateConsulatieHandler] Consultatie updated successfully: {ConsultatieID}", request.ConsultatieID);
            return Result<bool>.Success(true, "Consultatie actualizată cu succes");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[UpdateConsulatieHandler] Error updating consultatie ID: {ConsultatieID}", request.ConsultatieID);
            return Result<bool>.Failure($"Eroare la actualizarea consultatiei: {ex.Message}");
        }
    }
}
