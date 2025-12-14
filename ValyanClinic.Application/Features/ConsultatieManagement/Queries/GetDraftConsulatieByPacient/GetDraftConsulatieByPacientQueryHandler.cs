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

            // Map to DetailDto
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
                Greutate = consultatie.Greutate,
                Inaltime = consultatie.Inaltime,
                IMC = consultatie.IMC,
                Temperatura = consultatie.Temperatura,
                TensiuneArteriala = consultatie.TensiuneArteriala,
                Puls = consultatie.Puls,
                DiagnosticPozitiv = consultatie.DiagnosticPozitiv,
                CoduriICD10 = consultatie.CoduriICD10,
                TratamentMedicamentos = consultatie.TratamentMedicamentos,
                ObservatiiMedic = consultatie.ObservatiiMedic,
                Status = consultatie.Status,
                DataFinalizare = consultatie.DataFinalizare,
                DurataMinute = consultatie.DurataMinute,
                DataCreare = consultatie.DataCreare,
                CreatDe = consultatie.CreatDe,
                DataUltimeiModificari = consultatie.DataUltimeiModificari,
                ModificatDe = consultatie.ModificatDe,
                PacientNumeComplet = "N/A", // Will be populated separately if needed
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
