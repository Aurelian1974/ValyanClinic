using MediatR;
using Microsoft.Extensions.Logging;
using ValyanClinic.Application.Common.Results;
using ValyanClinic.Domain.Entities;
using IConsultatieRepository = ValyanClinic.Infrastructure.Repositories.Interfaces.IConsultatieRepository;

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

            var consultatieId = Guid.NewGuid();

            // 1. MASTER Entity: Consultatie (core fields only - NORMALIZED structure)
            var consultatie = new Consultatie
            {
                ConsultatieID = consultatieId,
                ProgramareID = request.ProgramareID,
                PacientID = request.PacientID,
                MedicID = request.MedicID,
                DataConsultatie = request.DataConsultatie,
                OraConsultatie = request.OraConsultatie,
                TipConsultatie = request.TipConsultatie,
                Status = request.Status,
                DataFinalizare = request.DataFinalizare,
                DurataMinute = 0, // Will be set by timer in UI
                DataCreare = DateTime.Now,
                CreatDe = request.CreatDe
            };

            // Call repository (executes sp_Consultatie_Create)
            var createdId = await _repository.CreateAsync(consultatie, cancellationToken);

            if (createdId == Guid.Empty)
            {
                _logger.LogWarning("[CreateConsulatieHandler] Failed to create master Consultatie");
                return Result<Guid>.Failure("Eroare la crearea consultatiei în baza de date");
            }

            // 2. ConsultatieMotivePrezentare (1:1) - always upsert for complete create
            await _repository.UpsertMotivePrezentareAsync(consultatieId, new ConsultatieMotivePrezentare
            {
                ConsultatieID = consultatieId,
                MotivPrezentare = request.MotivPrezentare,
                IstoricBoalaActuala = request.IstoricBoalaActuala,
                CreatDe = request.CreatDe,
                DataCreare = DateTime.Now
            });

            // 3. ConsultatieAntecedente (1:1) - SIMPLIFIED: only IstoricMedicalPersonal and IstoricFamilial
            await _repository.UpsertAntecedenteAsync(consultatieId, new ConsultatieAntecedente
            {
                ConsultatieID = consultatieId,
                IstoricMedicalPersonal = request.IstoricMedicalPersonal,
                IstoricFamilial = request.IstoricFamilial,
                CreatDe = request.CreatDe,
                DataCreare = DateTime.Now
            });

            // 4. ConsultatieExamenObiectiv (1:1) - always upsert for complete create
            await _repository.UpsertExamenObiectivAsync(consultatieId, new ConsultatieExamenObiectiv
            {
                ConsultatieID = consultatieId,
                StareGenerala = request.StareGenerala,
                Tegumente = request.Tegumente,
                Mucoase = request.Mucoase,
                GanglioniLimfatici = request.GanglioniLimfatici,
                Greutate = request.Greutate,
                Inaltime = request.Inaltime,
                IMC = imc,
                Temperatura = request.Temperatura,
                TensiuneArteriala = request.TensiuneArteriala,
                Puls = request.Puls,
                FreccventaRespiratorie = request.FreccventaRespiratorie,
                SaturatieO2 = request.SaturatieO2,
                Glicemie = request.Glicemie,
                CreatDe = request.CreatDe,
                DataCreare = DateTime.Now
            });

            // 5. ConsultatieInvestigatii (1:1) - always upsert for complete create
            await _repository.UpsertInvestigatiiAsync(consultatieId, new ConsultatieInvestigatii
            {
                ConsultatieID = consultatieId,
                InvestigatiiLaborator = request.InvestigatiiLaborator,
                InvestigatiiImagistice = request.InvestigatiiImagistice,
                InvestigatiiEKG = request.InvestigatiiEKG,
                AlteInvestigatii = request.AlteInvestigatii,
                CreatDe = request.CreatDe,
                DataCreare = DateTime.Now
            });

            // 6. ConsultatieDiagnostic (1:1) - always upsert for complete create
            await _repository.UpsertDiagnosticAsync(consultatieId, new ConsultatieDiagnostic
            {
                ConsultatieID = consultatieId,
                // Normalized structure
                CodICD10Principal = request.CodICD10Principal,
                NumeDiagnosticPrincipal = request.NumeDiagnosticPrincipal,
                DescriereDetaliataPrincipal = request.DescriereDetaliataPrincipal,
                // Legacy
                DiagnosticPozitiv = request.DiagnosticPozitiv,
                CoduriICD10 = request.CoduriICD10 ?? request.CodICD10Principal,
                CreatDe = request.CreatDe,
                DataCreare = DateTime.Now
            });

            // 7. ConsultatieTratament (1:1) - always upsert for complete create
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
                CreatDe = request.CreatDe,
                DataCreare = DateTime.Now
            });

            // 7.1 ConsultatieMedicament (1:N) - save medication list
            if (request.MedicationList?.Any() == true)
            {
                var medicamente = request.MedicationList
                    .Where(m => !string.IsNullOrWhiteSpace(m.Name))
                    .Select((m, index) => new ConsultatieMedicament
                    {
                        ConsultatieID = consultatieId,
                        OrdineAfisare = index,
                        NumeMedicament = m.Name,
                        Doza = m.Dose,
                        Frecventa = m.Frequency,
                        Durata = m.Duration,
                        Cantitate = m.Quantity,
                        Observatii = m.Notes,
                        CreatDe = request.CreatDe,
                        DataCreare = DateTime.Now
                    });
                await _repository.ReplaceMedicamenteAsync(consultatieId, medicamente, request.CreatDe);
            }

            // 8. ConsultatieConcluzii (1:1) - always upsert for complete create
            await _repository.UpsertConcluziiAsync(consultatieId, new ConsultatieConcluzii
            {
                ConsultatieID = consultatieId,
                Prognostic = request.Prognostic,
                Concluzie = request.Concluzie,
                ObservatiiMedic = request.ObservatiiMedic,
                NotePacient = request.NotePacient,
                CreatDe = request.CreatDe,
                DataCreare = DateTime.Now
            });

            _logger.LogInformation(
                "[CreateConsulatieHandler] Consultatie created successfully with normalized structure: {ConsultatieID}",
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
