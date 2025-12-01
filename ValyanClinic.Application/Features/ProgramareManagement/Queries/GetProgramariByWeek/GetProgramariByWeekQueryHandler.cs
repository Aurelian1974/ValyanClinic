using MediatR;
using Microsoft.Extensions.Logging;
using ValyanClinic.Application.Common.Results;
using ValyanClinic.Application.Features.ProgramareManagement.DTOs;
using ValyanClinic.Domain.Interfaces.Repositories;

namespace ValyanClinic.Application.Features.ProgramareManagement.Queries.GetProgramariByWeek;

public class GetProgramariByWeekQueryHandler : IRequestHandler<GetProgramariByWeekQuery, Result<IEnumerable<ProgramareListDto>>>
{
    private readonly IProgramareRepository _programareRepository;
    private readonly ILogger<GetProgramariByWeekQueryHandler> _logger;

    public GetProgramariByWeekQueryHandler(
        IProgramareRepository programareRepository,
        ILogger<GetProgramariByWeekQueryHandler> logger)
    {
        _programareRepository = programareRepository;
        _logger = logger;
    }

    public async Task<Result<IEnumerable<ProgramareListDto>>> Handle(
        GetProgramariByWeekQuery request,
    CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation(
          "⚡ GetProgramariByWeek: {StartDate} - {EndDate}, Doctor={DoctorID}",
           request.WeekStartDate.ToString("yyyy-MM-dd"),
              request.WeekEndDate.ToString("yyyy-MM-dd"),
         request.DoctorID);

            // ✅ SINGLE DB CALL - Get ALL programări for the week
            var programari = await _programareRepository.GetByDateRangeAsync(
     request.WeekStartDate,
        request.WeekEndDate,
         request.DoctorID,
      cancellationToken);

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
                PacientNumeComplet = p.PacientNumeComplet,
                PacientTelefon = p.PacientTelefon,
                PacientEmail = p.PacientEmail,
                PacientCNP = p.PacientCNP,
                DoctorNumeComplet = p.DoctorNumeComplet,
                DoctorSpecializare = p.DoctorSpecializare,
                DoctorTelefon = p.DoctorTelefon,
                DataCreare = p.DataCreare,
                CreatDeNumeComplet = p.CreatDeNumeComplet,
                DataUltimeiModificari = p.DataUltimeiModificari
            }).ToList();

            _logger.LogInformation(
                   "✅ Loaded {Count} programări for week in SINGLE query",
              programariDto.Count);

            return Result<IEnumerable<ProgramareListDto>>.Success(
             programariDto,
            $"Au fost găsite {programariDto.Count} programări pentru săptămâna selectată.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error loading programări for week");
            return Result<IEnumerable<ProgramareListDto>>.Failure($"Eroare: {ex.Message}");
        }
    }
}
