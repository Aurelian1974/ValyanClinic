using MediatR;
using Microsoft.Extensions.Logging;
using ValyanClinic.Application.Common.Results;
using ValyanClinic.Domain.Entities;
using ValyanClinic.Infrastructure.Repositories.Interfaces;

namespace ValyanClinic.Application.Features.ConsultatieManagement.Commands.CreateConsultatie;

public class CreateConsultatieCommandHandler : IRequestHandler<CreateConsultatieCommand, Result<Guid>>
{
    private readonly IConsultatieRepository _repository;
    private readonly ILogger<CreateConsultatieCommandHandler> _logger;

    public CreateConsultatieCommandHandler(
        IConsultatieRepository repository,
        ILogger<CreateConsultatieCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<Result<Guid>> Handle(CreateConsultatieCommand request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("[CreateConsultatieCommandHandler] Creating consultatie for Programare: {ProgramareID}", request.ProgramareID);

            // Validate required fields
            if (string.IsNullOrWhiteSpace(request.MotivPrezentare))
            {
                return Result<Guid>.Failure("Motivul prezentarii este obligatoriu");
            }

            if (string.IsNullOrWhiteSpace(request.DiagnosticPozitiv))
            {
                return Result<Guid>.Failure("Diagnosticul pozitiv este obligatoriu");
            }

            // Create entity
            var consultatie = new Consultatie
            {
                ConsultatieID = Guid.NewGuid(),
                ProgramareID = request.ProgramareID,
                PacientID = request.PacientID,
                MedicID = request.MedicID,

                DataConsultatie = DateTime.Now.Date,
                OraConsultatie = DateTime.Now.TimeOfDay,
                TipConsultatie = request.TipConsultatie,

                // I. Motive Prezentare
                MotivPrezentare = request.MotivPrezentare,
                IstoricBoalaActuala = request.IstoricBoalaActuala,

                // II. Antecedente
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

                // III. Examen Obiectiv
                StareGenerala = request.StareGenerala,
                Constitutie = request.Constitutie,
                Atitudine = request.Atitudine,
                Facies = request.Facies,
                Tegumente = request.Tegumente,
                Mucoase = request.Mucoase,
                GangliniLimfatici = request.GangliniLimfatici,

                Greutate = request.Greutate,
                Inaltime = request.Inaltime,
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

                // IV. Investigatii
                InvestigatiiLaborator = request.InvestigatiiLaborator,
                InvestigatiiImagistice = request.InvestigatiiImagistice,
                InvestigatiiEKG = request.InvestigatiiEKG,
                AlteInvestigatii = request.AlteInvestigatii,

                // V. Diagnostic
                DiagnosticPozitiv = request.DiagnosticPozitiv,
                DiagnosticDiferential = request.DiagnosticDiferential,
                DiagnosticEtiologic = request.DiagnosticEtiologic,
                CoduriICD10 = request.CoduriICD10,

                // VI. Tratament
                TratamentMedicamentos = request.TratamentMedicamentos,
                TratamentNemedicamentos = request.TratamentNemedicamentos,
                RecomandariDietetice = request.RecomandariDietetice,
                RecomandariRegimViata = request.RecomandariRegimViata,

                // VII. Recomandari
                InvestigatiiRecomandate = request.InvestigatiiRecomandate,
                ConsulturiSpecialitate = request.ConsulturiSpecialitate,
                DataUrmatoareiProgramari = request.DataUrmatoareiProgramari,
                RecomandariSupraveghere = request.RecomandariSupraveghere,

                // VIII. Prognostic & Concluzie
                Prognostic = request.Prognostic,
                Concluzie = request.Concluzie,

                // IX. Observatii
                ObservatiiMedic = request.ObservatiiMedic,
                NotePacient = request.NotePacient,

                // Status
                Status = "Finalizata",
                DataFinalizare = DateTime.Now,

                // Audit
                DataCreare = DateTime.Now,
                CreatDe = Guid.Parse(request.CreatDe)
            };

            // Calculate IMC if possible
            if (consultatie.Greutate.HasValue && consultatie.Inaltime.HasValue && consultatie.Inaltime > 0)
            {
                var inaltimeMetri = consultatie.Inaltime.Value / 100;
                consultatie.IMC = Math.Round(consultatie.Greutate.Value / (inaltimeMetri * inaltimeMetri), 2);
            }

            var consultatieId = await _repository.CreateAsync(consultatie, cancellationToken);

            _logger.LogInformation("[CreateConsultatieCommandHandler] Consultatie created successfully: {ConsultatieID}", consultatieId);

            return Result<Guid>.Success(consultatieId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[CreateConsultatieCommandHandler] Error creating consultatie");
            return Result<Guid>.Failure($"Eroare la crearea consultatiei: {ex.Message}");
        }
    }
}
