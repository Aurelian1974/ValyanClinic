using MediatR;
using Microsoft.Extensions.Logging;
using ValyanClinic.Application.Common.Results;
using ValyanClinic.Domain.Entities;
using ValyanClinic.Infrastructure.Repositories.Interfaces;

namespace ValyanClinic.Application.Features.ConsultatieManagement.Commands.CreateConsultatie;

/// <summary>
/// Handler pentru CreateConsultatieCommand - folosește structură normalizată cu entități separate
/// </summary>
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

            var consultatieId = Guid.NewGuid();
            var creatDe = Guid.TryParse(request.CreatDe, out var parsedCreatDe) ? parsedCreatDe : Guid.Empty;

            // Calculate IMC if possible
            decimal? imc = null;
            if (request.Greutate.HasValue && request.Inaltime.HasValue && request.Inaltime > 0)
            {
                var inaltimeMetri = request.Inaltime.Value / 100;
                imc = Math.Round(request.Greutate.Value / (inaltimeMetri * inaltimeMetri), 2);
            }

            // 1. MASTER Entity: Consultatie (core fields only - NORMALIZED structure)
            var consultatie = new Consultatie
            {
                ConsultatieID = consultatieId,
                ProgramareID = request.ProgramareID,
                PacientID = request.PacientID,
                MedicID = request.MedicID,
                DataConsultatie = DateTime.Now.Date,
                OraConsultatie = DateTime.Now.TimeOfDay,
                TipConsultatie = request.TipConsultatie,
                Status = "Finalizata",
                DataFinalizare = DateTime.Now,
                DurataMinute = 0,
                DataCreare = DateTime.Now,
                CreatDe = creatDe
            };

            var createdId = await _repository.CreateAsync(consultatie, cancellationToken);

            if (createdId == Guid.Empty)
            {
                _logger.LogWarning("[CreateConsultatieCommandHandler] Failed to create master Consultatie");
                return Result<Guid>.Failure("Eroare la crearea consultatiei în baza de date");
            }

            // 2. ConsultatieMotivePrezentare (1:1)
            await _repository.UpsertMotivePrezentareAsync(consultatieId, new ConsultatieMotivePrezentare
            {
                ConsultatieID = consultatieId,
                MotivPrezentare = request.MotivPrezentare,
                IstoricBoalaActuala = request.IstoricBoalaActuala,
                CreatDe = creatDe,
                DataCreare = DateTime.Now
            });

            // 3. ConsultatieAntecedente (1:1) - SIMPLIFIED
            await _repository.UpsertAntecedenteAsync(consultatieId, new ConsultatieAntecedente
            {
                ConsultatieID = consultatieId,
                IstoricMedicalPersonal = request.IstoricMedicalPersonal,
                IstoricFamilial = request.IstoricFamilial,
                CreatDe = creatDe,
                DataCreare = DateTime.Now
            });

            // 4. ConsultatieExamenObiectiv (1:1)
            await _repository.UpsertExamenObiectivAsync(consultatieId, new ConsultatieExamenObiectiv
            {
                ConsultatieID = consultatieId,
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
                CreatDe = creatDe,
                DataCreare = DateTime.Now
            });

            // 5. ConsultatieInvestigatii (1:1)
            await _repository.UpsertInvestigatiiAsync(consultatieId, new ConsultatieInvestigatii
            {
                ConsultatieID = consultatieId,
                InvestigatiiLaborator = request.InvestigatiiLaborator,
                InvestigatiiImagistice = request.InvestigatiiImagistice,
                InvestigatiiEKG = request.InvestigatiiEKG,
                AlteInvestigatii = request.AlteInvestigatii,
                CreatDe = creatDe,
                DataCreare = DateTime.Now
            });

            // 6. ConsultatieDiagnostic (1:1)
            await _repository.UpsertDiagnosticAsync(consultatieId, new ConsultatieDiagnostic
            {
                ConsultatieID = consultatieId,
                DiagnosticPozitiv = request.DiagnosticPozitiv,
                DiagnosticDiferential = request.DiagnosticDiferential,
                DiagnosticEtiologic = request.DiagnosticEtiologic,
                CoduriICD10 = request.CoduriICD10,
                CoduriICD10Secundare = request.CoduriICD10Secundare,
                CreatDe = creatDe,
                DataCreare = DateTime.Now
            });

            // 7. ConsultatieTratament (1:1)
            await _repository.UpsertTratamentAsync(consultatieId, new ConsultatieTratament
            {
                ConsultatieID = consultatieId,
                TratamentMedicamentos = request.TratamentMedicamentos,
                TratamentNemedicamentos = request.TratamentNemedicamentos,
                RecomandariDietetice = request.RecomandariDietetice,
                RecomandariRegimViata = request.RecomandariRegimViata,
                InvestigatiiRecomandate = request.InvestigatiiRecomandate,
                ConsulturiSpecialitate = request.ConsulturiSpecialitate,
                DataUrmatoareiProgramari = request.DataUrmatoareiProgramari,
                RecomandariSupraveghere = request.RecomandariSupraveghere,
                CreatDe = creatDe,
                DataCreare = DateTime.Now
            });

            // 8. ConsultatieConcluzii (1:1)
            await _repository.UpsertConcluziiAsync(consultatieId, new ConsultatieConcluzii
            {
                ConsultatieID = consultatieId,
                Prognostic = request.Prognostic,
                Concluzie = request.Concluzie,
                ObservatiiMedic = request.ObservatiiMedic,
                NotePacient = request.NotePacient,
                CreatDe = creatDe,
                DataCreare = DateTime.Now
            });

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
