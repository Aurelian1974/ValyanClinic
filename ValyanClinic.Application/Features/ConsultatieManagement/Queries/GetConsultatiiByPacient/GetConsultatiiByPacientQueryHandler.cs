using MediatR;
using Microsoft.Extensions.Logging;
using ValyanClinic.Application.Common.Results;
using ValyanClinic.Application.Features.ConsultatieManagement.DTOs;
using ValyanClinic.Domain.Interfaces.Repositories;

namespace ValyanClinic.Application.Features.ConsultatieManagement.Queries.GetConsultatiiByPacient;

/// <summary>
/// Handler pentru GetConsultatiiByPacientQuery
/// Obtine lista consultatiilor pentru un pacient folosind sp_Consultatie_GetByPacient
/// </summary>
public class GetConsultatiiByPacientQueryHandler : IRequestHandler<GetConsultatiiByPacientQuery, Result<List<ConsulatieListDto>>>
{
    private readonly IConsultatieBaseRepository _repository;
    private readonly ILogger<GetConsultatiiByPacientQueryHandler> _logger;

    public GetConsultatiiByPacientQueryHandler(
        IConsultatieBaseRepository repository,
        ILogger<GetConsultatiiByPacientQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<Result<List<ConsulatieListDto>>> Handle(
        GetConsultatiiByPacientQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation(
                "[GetConsultatiiByPacientHandler] Fetching consultatii for PacientID: {PacientID}",
                request.PacientID);

            // Validare
            if (request.PacientID == Guid.Empty)
                return Result<List<ConsulatieListDto>>.Failure("PacientID este obligatoriu");

            // Get from repository (executes sp_Consultatie_GetByPacient)
            var consultatii = await _repository.GetByPacientIdAsync(request.PacientID, cancellationToken);

            // Map to ListDto (simplified view)
            var dtoList = consultatii.Select(c => new ConsulatieListDto
            {
                ConsultatieID = c.ConsultatieID,
                ProgramareID = c.ProgramareID,
                PacientID = c.PacientID,
                MedicID = c.MedicID,
                DataConsultatie = c.DataConsultatie,
                OraConsultatie = c.OraConsultatie,
                TipConsultatie = c.TipConsultatie,
                MotivPrezentare = c.MotivPrezentare,
                DiagnosticPozitiv = c.DiagnosticPozitiv,
                Status = c.Status,
                DataFinalizare = c.DataFinalizare,
                DurataMinute = c.DurataMinute,
                DataCreare = c.DataCreare,
                
                // TODO: Add JOIN data from Pacienti and PersonalMedical
                PacientNumeComplet = "N/A", // Will be populated with JOIN
                MedicNumeComplet = "N/A"    // Will be populated with JOIN
            }).ToList();

            _logger.LogInformation(
                "[GetConsultatiiByPacientHandler] Found {Count} consultatii for PacientID: {PacientID}",
                dtoList.Count, request.PacientID);

            return Result<List<ConsulatieListDto>>.Success(
                dtoList,
                $"Au fost găsite {dtoList.Count} consultatii");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "[GetConsultatiiByPacientHandler] Error fetching consultatii for PacientID: {PacientID}",
                request.PacientID);

            return Result<List<ConsulatieListDto>>.Failure(
                $"Eroare la obținerea consultatiilor pentru pacient: {ex.Message}");
        }
    }
}
