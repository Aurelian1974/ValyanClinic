using MediatR;
using Microsoft.Extensions.Logging;
using ValyanClinic.Application.Common.Results;
using ValyanClinic.Application.Features.ProgramareManagement.DTOs;
using ValyanClinic.Domain.Interfaces.Repositories;

namespace ValyanClinic.Application.Features.ProgramareManagement.Queries.GetDoctorSchedule;

public class GetDoctorScheduleQueryHandler : IRequestHandler<GetDoctorScheduleQuery, Result<IEnumerable<ProgramareListDto>>>
{
    private readonly IProgramareRepository _programareRepository;
    private readonly ILogger<GetDoctorScheduleQueryHandler> _logger;

    public GetDoctorScheduleQueryHandler(
IProgramareRepository programareRepository,
ILogger<GetDoctorScheduleQueryHandler> logger)
    {
        _programareRepository = programareRepository;
   _logger = logger;
    }

    public async Task<Result<IEnumerable<ProgramareListDto>>> Handle(
   GetDoctorScheduleQuery request,
        CancellationToken cancellationToken)
    {
try
        {
      _logger.LogInformation(
     "Obținere program doctor: {DoctorID}, De la {DataStart} până la {DataEnd}",
 request.DoctorID,
   request.DataStart.ToString("yyyy-MM-dd"),
       request.DataEnd.ToString("yyyy-MM-dd"));

 var programari = await _programareRepository.GetDoctorScheduleAsync(
     request.DoctorID,
      request.DataStart,
       request.DataEnd,
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
        "Găsite {Count} programări pentru doctor {DoctorID} în intervalul {DataStart} - {DataEnd}",
     programariDto.Count, request.DoctorID,
    request.DataStart.ToString("yyyy-MM-dd"),
   request.DataEnd.ToString("yyyy-MM-dd"));

 return Result<IEnumerable<ProgramareListDto>>.Success(
   programariDto,
$"Au fost găsite {programariDto.Count} programări în intervalul specificat.");
        }
        catch (Exception ex)
        {
       _logger.LogError(ex, "Eroare la obținerea programului doctor {DoctorID}", request.DoctorID);
    return Result<IEnumerable<ProgramareListDto>>.Failure($"Eroare: {ex.Message}");
}
    }
}
