using MediatR;
using Microsoft.Extensions.Logging;
using ValyanClinic.Application.Common.Results;
using ValyanClinic.Domain.Entities;
using ValyanClinic.Infrastructure.Repositories.Interfaces;

namespace ValyanClinic.Application.Features.ConsultatieManagement.Commands.CreateConsultatie;

/// <summary>
/// Handler pentru CreateConsulatieCommand
/// Creaza o consultatie medicala noua folosind Repository pattern si Stored Procedures
/// </summary>
public class CreateConsulatieCommandHandler : IRequestHandler<CreateConsulatieCommand, Result<Guid>>
{
    private readonly IConsultatieRepository _repository;
    private readonly ILogger<CreateConsulatieCommandHandler> _logger;

    public CreateConsulatieCommandHandler(
        IConsultatieRepository repository,
        ILogger<CreateConsulatieCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<Result<Guid>> Handle(
        CreateConsulatieCommand request,
        CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation(
                "[CreateConsulatieHandler] Creating new consultatie for PacientID: {PacientID}, MedicID: {MedicID}, ProgramareID: {ProgramareID}",
                request.PacientID, request.MedicID, request.ProgramareID);

            // Validare campuri obligatorii
            if (request.ProgramareID == Guid.Empty)
                return Result<Guid>.Failure("ProgramareID este obligatoriu");

            if (request.PacientID == Guid.Empty)
                return Result<Guid>.Failure("PacientID este obligatoriu");

            if (request.MedicID == Guid.Empty)
                return Result<Guid>.Failure("MedicID este obligatoriu");

            if (request.CreatDe == Guid.Empty)
                return Result<Guid>.Failure("CreatDe este obligatoriu");

            if (string.IsNullOrWhiteSpace(request.TipConsultatie))
                return Result<Guid>.Failure("TipConsultatie este obligatoriu");

            // Calculare IMC automat daca nu e setat si avem greutate + inaltime
            var imc = request.IMC;
            if (!imc.HasValue && request.Greutate.HasValue && request.Inaltime.HasValue && request.Inaltime.Value > 0)
            {
                var inaltimeMetri = request.Inaltime.Value / 100m;
                imc = Math.Round(request.Greutate.Value / (inaltimeMetri * inaltimeMetri), 2);
            }

            // Creare entitate Domain
            var consultatie = new Consultatie
            {
                ConsultatieID = Guid.NewGuid(),
                ProgramareID = request.ProgramareID,
                PacientID = request.PacientID,
                MedicID = request.MedicID,
                DataConsultatie = request.DataConsultatie,
                OraConsultatie = request.OraConsultatie,
                TipConsultatie = request.TipConsultatie,

                // Motive Prezentare
                MotivPrezentare = request.MotivPrezentare,
                IstoricBoalaActuala = request.IstoricBoalaActuala,

                // Antecedente Heredo-Colaterale
                AHC_Mama = request.AHC_Mama,
                AHC_Tata = request.AHC_Tata,
                AHC_Frati = request.AHC_Frati,
                AHC_Bunici = request.AHC_Bunici,
                AHC_Altele = request.AHC_Altele,

                // Antecedente Fiziologice
                AF_Nastere = request.AF_Nastere,
                AF_Dezvoltare = request.AF_Dezvoltare,
                AF_Menstruatie = request.AF_Menstruatie,
                AF_Sarcini = request.AF_Sarcini,
                AF_Alaptare = request.AF_Alaptare,

                // Antecedente Personale Patologice
                APP_BoliCopilarieAdolescenta = request.APP_BoliCopilarieAdolescenta,
                APP_BoliAdult = request.APP_BoliAdult,
                APP_Interventii = request.APP_Interventii,
                APP_Traumatisme = request.APP_Traumatisme,
                APP_Transfuzii = request.APP_Transfuzii,
                APP_Alergii = request.APP_Alergii,
                APP_Medicatie = request.APP_Medicatie,

                // Conditii Socio-Economice
                Profesie = request.Profesie,
                ConditiiLocuinta = request.ConditiiLocuinta,
                ConditiiMunca = request.ConditiiMunca,
                ObiceiuriAlimentare = request.ObiceiuriAlimentare,
                Toxice = request.Toxice,

                // Examen General
                StareGenerala = request.StareGenerala,
                Constitutie = request.Constitutie,
                Atitudine = request.Atitudine,
                Facies = request.Facies,
                Tegumente = request.Tegumente,
                Mucoase = request.Mucoase,
                GangliniLimfatici = request.GangliniLimfatici,

                // Semne Vitale
                Greutate = request.Greutate,
                Inaltime = request.Inaltime,
                IMC = imc, // Calculated or provided
                Temperatura = request.Temperatura,
                TensiuneArteriala = request.TensiuneArteriala,
                Puls = request.Puls,
                FreccventaRespiratorie = request.FreccventaRespiratorie,
                SaturatieO2 = request.SaturatieO2,
                Glicemie = request.Glicemie,

                // Examen pe Aparate
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

                // Investigatii
                InvestigatiiLaborator = request.InvestigatiiLaborator,
                InvestigatiiImagistice = request.InvestigatiiImagistice,
                InvestigatiiEKG = request.InvestigatiiEKG,
                AlteInvestigatii = request.AlteInvestigatii,

                // Diagnostic
                DiagnosticPozitiv = request.DiagnosticPozitiv,
                DiagnosticDiferential = request.DiagnosticDiferential,
                DiagnosticEtiologic = request.DiagnosticEtiologic,
                CoduriICD10 = request.CoduriICD10,

                // Tratament
                TratamentMedicamentos = request.TratamentMedicamentos,
                TratamentNemedicamentos = request.TratamentNemedicamentos,
                RecomandariDietetice = request.RecomandariDietetice,
                RecomandariRegimViata = request.RecomandariRegimViata,

                // Recomandari
                InvestigatiiRecomandate = request.InvestigatiiRecomandate,
                ConsulturiSpecialitate = request.ConsulturiSpecialitate,
                DataUrmatoareiProgramari = request.DataUrmatoareiProgramari,
                RecomandariSupraveghere = request.RecomandariSupraveghere,

                // Prognostic & Concluzie
                Prognostic = request.Prognostic,
                Concluzie = request.Concluzie,
                ObservatiiMedic = request.ObservatiiMedic,
                NotePacient = request.NotePacient,

                // Status
                Status = request.Status,
                DataFinalizare = request.DataFinalizare,
                DurataMinute = 0, // Will be set by timer in UI

                // Audit
                DataCreare = DateTime.UtcNow,
                CreatDe = request.CreatDe
            };

            // Call repository (executes sp_Consultatie_Create)
            var consultatieId = await _repository.CreateAsync(consultatie, cancellationToken);

            if (consultatieId == Guid.Empty)
            {
                _logger.LogWarning("[CreateConsulatieHandler] Failed to create consultatie - Repository returned empty GUID");
                return Result<Guid>.Failure("Eroare la crearea consultatiei în baza de date");
            }

            _logger.LogInformation(
                "[CreateConsulatieHandler] Consultatie created successfully with ID: {ConsultatieID}",
                consultatieId);

            return Result<Guid>.Success(
                consultatieId,
                $"Consultatie pentru pacient {request.PacientID} a fost creată cu succes");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "[CreateConsulatieHandler] Error creating consultatie for PacientID: {PacientID}",
                request.PacientID);

            return Result<Guid>.Failure($"Eroare la crearea consultatiei: {ex.Message}");
        }
    }
}
