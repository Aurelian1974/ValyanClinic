using MediatR;
using Microsoft.Extensions.Logging;
using ValyanClinic.Application.Common.Results;
using ValyanClinic.Application.Features.ConsultatieManagement.DTOs;
using ValyanClinic.Infrastructure.Repositories.Interfaces;

namespace ValyanClinic.Application.Features.ConsultatieManagement.Queries.GetConsulatieByProgramare;

/// <summary>
/// Handler pentru GetConsulatieByProgramareQuery
/// Obtine consultatia asociata cu o programare folosind sp_Consultatie_GetByProgramare
/// </summary>
public class GetConsulatieByProgramareQueryHandler : IRequestHandler<GetConsulatieByProgramareQuery, Result<ConsulatieDetailDto?>>
{
    private readonly IConsultatieRepository _repository;
    private readonly ILogger<GetConsulatieByProgramareQueryHandler> _logger;

    public GetConsulatieByProgramareQueryHandler(
        IConsultatieRepository repository,
        ILogger<GetConsulatieByProgramareQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<Result<ConsulatieDetailDto?>> Handle(
        GetConsulatieByProgramareQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation(
                "[GetConsulatieByProgramareHandler] Fetching consultatie for ProgramareID: {ProgramareID}",
                request.ProgramareID);

            // Validare
            if (request.ProgramareID == Guid.Empty)
                return Result<ConsulatieDetailDto?>.Failure("ProgramareID este obligatoriu");

            // Get from repository (executes sp_Consultatie_GetByProgramare)
            var consultatie = await _repository.GetByProgramareIdAsync(request.ProgramareID, cancellationToken);

            // NULL is valid - programarea poate să nu aibă consultatie încă
            if (consultatie == null)
            {
                _logger.LogInformation(
                    "[GetConsulatieByProgramareHandler] No consultatie found for ProgramareID: {ProgramareID}",
                    request.ProgramareID);

                return Result<ConsulatieDetailDto?>.Success(
                    null,
                    "Programarea nu are consultație asociată");
            }

            // Map to DetailDto (basic mapping, similar to GetById)
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
                // ... (copy full mapping from GetByIdHandler)
                Greutate = consultatie.Greutate,
                Inaltime = consultatie.Inaltime,
                IMC = consultatie.IMC,
                Temperatura = consultatie.Temperatura,
                TensiuneArteriala = consultatie.TensiuneArteriala,
                Puls = consultatie.Puls,
                DiagnosticPozitiv = consultatie.DiagnosticPozitiv,
                CoduriICD10 = consultatie.CoduriICD10,
                TratamentMedicamentos = consultatie.TratamentMedicamentos,
                Status = consultatie.Status,
                DataFinalizare = consultatie.DataFinalizare,
                DurataMinute = consultatie.DurataMinute,
                DataCreare = consultatie.DataCreare,
                CreatDe = consultatie.CreatDe,
                DataUltimeiModificari = consultatie.DataUltimeiModificari,
                ModificatDe = consultatie.ModificatDe,
                PacientNumeComplet = "N/A",
                MedicNumeComplet = "N/A"
            };

            _logger.LogInformation(
                "[GetConsulatieByProgramareHandler] Consultatie found: {ConsultatieID} for ProgramareID: {ProgramareID}",
                consultatie.ConsultatieID, request.ProgramareID);

            return Result<ConsulatieDetailDto?>.Success(dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "[GetConsulatieByProgramareHandler] Error fetching consultatie for ProgramareID: {ProgramareID}",
                request.ProgramareID);

            return Result<ConsulatieDetailDto?>.Failure(
                $"Eroare la obținerea consultatiei pentru programare: {ex.Message}");
        }
    }
}
