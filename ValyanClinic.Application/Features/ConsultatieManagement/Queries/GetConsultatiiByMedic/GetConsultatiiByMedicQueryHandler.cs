using MediatR;
using Microsoft.Extensions.Logging;
using ValyanClinic.Application.Common.Results;
using ValyanClinic.Application.Features.ConsultatieManagement.DTOs;
using ValyanClinic.Domain.Interfaces.Repositories;

namespace ValyanClinic.Application.Features.ConsultatieManagement.Queries.GetConsultatiiByMedic;

/// <summary>
/// Handler pentru GetConsultatiiByMedicQuery
/// Obtine lista consultatiilor pentru un medic folosind sp_Consultatie_GetByMedic
/// </summary>
public class GetConsultatiiByMedicQueryHandler : IRequestHandler<GetConsultatiiByMedicQuery, Result<List<ConsulatieListDto>>>
{
    private readonly IConsultatieRepository _repository;
    private readonly ILogger<GetConsultatiiByMedicQueryHandler> _logger;

    public GetConsultatiiByMedicQueryHandler(
        IConsultatieRepository repository,
        ILogger<GetConsultatiiByMedicQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<Result<List<ConsulatieListDto>>> Handle(
        GetConsultatiiByMedicQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation(
                "[GetConsultatiiByMedicHandler] Fetching consultatii for MedicID: {MedicID}",
                request.MedicID);

            // Validare
            if (request.MedicID == Guid.Empty)
                return Result<List<ConsulatieListDto>>.Failure("MedicID este obligatoriu");

            // Get from repository (executes sp_Consultatie_GetByMedic)
            var consultatii = await _repository.GetByMedicIdAsync(request.MedicID, cancellationToken);

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
                "[GetConsultatiiByMedicHandler] Found {Count} consultatii for MedicID: {MedicID}",
                dtoList.Count, request.MedicID);

            return Result<List<ConsulatieListDto>>.Success(
                dtoList,
                $"Au fost găsite {dtoList.Count} consultatii");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "[GetConsultatiiByMedicHandler] Error fetching consultatii for MedicID: {MedicID}",
                request.MedicID);

            return Result<List<ConsulatieListDto>>.Failure(
                $"Eroare la obținerea consultatiilor pentru medic: {ex.Message}");
        }
    }
}
