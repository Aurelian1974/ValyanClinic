using MediatR;
using Microsoft.Extensions.Logging;
using ValyanClinic.Application.Common.Results;
using ValyanClinic.Application.Features.ProgramareManagement.DTOs;
using ValyanClinic.Domain.Interfaces.Repositories;

namespace ValyanClinic.Application.Features.ProgramareManagement.Queries.GetProgramareList;

/// <summary>
/// Handler pentru query-ul GetProgramareListQuery.
/// Obține lista paginată de programări cu filtrare și sortare.
/// </summary>
public class GetProgramareListQueryHandler : IRequestHandler<GetProgramareListQuery, PagedResult<ProgramareListDto>>
{
    private readonly IProgramareRepository _programareRepository;
    private readonly ILogger<GetProgramareListQueryHandler> _logger;

public GetProgramareListQueryHandler(
      IProgramareRepository programareRepository,
        ILogger<GetProgramareListQueryHandler> logger)
  {
     _programareRepository = programareRepository;
        _logger = logger;
    }

    public async Task<PagedResult<ProgramareListDto>> Handle(
        GetProgramareListQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation(
                "Obținere listă programări: Page={Page}, Size={Size}, Search={Search}, Doctor={Doctor}, Pacient={Pacient}, DataStart={DataStart}, DataEnd={DataEnd}, Status={Status}, Tip={Tip}, Sort={Sort} {Dir}",
      request.PageNumber, request.PageSize, request.GlobalSearchText,
   request.FilterDoctorID, request.FilterPacientID,
   request.FilterDataStart?.ToString("yyyy-MM-dd"), request.FilterDataEnd?.ToString("yyyy-MM-dd"),
                request.FilterStatus, request.FilterTipProgramare,
    request.SortColumn, request.SortDirection);

            // ==================== OBȚINERE DATE DIN REPOSITORY ====================

            // 1. Obține numărul total (pentru paginare)
            var totalCount = await _programareRepository.GetCountAsync(
     searchText: request.GlobalSearchText,
     doctorID: request.FilterDoctorID,
       pacientID: request.FilterPacientID,
       dataStart: request.FilterDataStart,
         dataEnd: request.FilterDataEnd,
  status: request.FilterStatus,
     tipProgramare: request.FilterTipProgramare,
         cancellationToken: cancellationToken);

            if (totalCount == 0)
      {
                _logger.LogInformation("Nu au fost găsite programări pentru criteriile specificate");
         return PagedResult<ProgramareListDto>.Success(
     Enumerable.Empty<ProgramareListDto>(),
               request.PageNumber,
request.PageSize,
    0,
          "Nu au fost găsite programări pentru criteriile specificate.");
            }

            // 2. Obține programările paginate
       var programari = await _programareRepository.GetAllAsync(
                pageNumber: request.PageNumber,
    pageSize: request.PageSize,
       searchText: request.GlobalSearchText,
                doctorID: request.FilterDoctorID,
         pacientID: request.FilterPacientID,
                dataStart: request.FilterDataStart,
    dataEnd: request.FilterDataEnd,
  status: request.FilterStatus,
                tipProgramare: request.FilterTipProgramare,
       sortColumn: request.SortColumn,
  sortDirection: request.SortDirection,
           cancellationToken: cancellationToken);

            // ==================== MAPARE LA DTO ====================

            var programariDto = programari.Select(p => new ProgramareListDto
            {
              ProgramareID = p.ProgramareID,
       PacientID = p.PacientID,
     DoctorID = p.DoctorID,
         DataProgramare = p.DataProgramare,
           OraInceput = p.OraInceput,
    OraSfarsit = p.OraSfarsit,
 TipProgramare = p.TipProgramare,
     Status = p.Status,
     Observatii = p.Observatii,
  // Navigation properties
       PacientNumeComplet = p.PacientNumeComplet,
     PacientTelefon = p.PacientTelefon,
      PacientEmail = p.PacientEmail,
PacientCNP = p.PacientCNP,
   DoctorNumeComplet = p.DoctorNumeComplet,
           DoctorSpecializare = p.DoctorSpecializare,
    DoctorTelefon = p.DoctorTelefon,
          // Audit
DataCreare = p.DataCreare,
        CreatDeNumeComplet = p.CreatDeNumeComplet,
   DataUltimeiModificari = p.DataUltimeiModificari
       }).ToList();

_logger.LogInformation(
          "Listă programări obținută: {Count} din {Total} înregistrări",
           programariDto.Count, totalCount);

            // ==================== RETURNARE REZULTAT ====================

   var message = totalCount == 1
           ? "A fost găsită 1 programare."
           : $"Au fost găsite {totalCount} programări.";

            return PagedResult<ProgramareListDto>.Success(
          programariDto,
    request.PageNumber,
          request.PageSize,
      totalCount,
                message);
        }
        catch (Exception ex)
  {
            _logger.LogError(ex, "Eroare la obținerea listei de programări");
 return PagedResult<ProgramareListDto>.Failure($"Eroare la obținerea listei de programări: {ex.Message}");
        }
    }
}
