using MediatR;
using Microsoft.Extensions.Logging;
using ValyanClinic.Application.Common.Results;
using ValyanClinic.Application.Features.ProgramareManagement.DTOs;
using ValyanClinic.Domain.Interfaces.Repositories;

namespace ValyanClinic.Application.Features.ProgramareManagement.Queries.GetUpcomingProgramari;

public class GetUpcomingProgramariQueryHandler : IRequestHandler<GetUpcomingProgramariQuery, Result<IEnumerable<ProgramareListDto>>>
{
    private readonly IProgramareRepository _programareRepository;
    private readonly ILogger<GetUpcomingProgramariQueryHandler> _logger;

public GetUpcomingProgramariQueryHandler(
        IProgramareRepository programareRepository,
        ILogger<GetUpcomingProgramariQueryHandler> logger)
    {
  _programareRepository = programareRepository;
        _logger = logger;
    }

    public async Task<Result<IEnumerable<ProgramareListDto>>> Handle(
      GetUpcomingProgramariQuery request,
  CancellationToken cancellationToken)
    {
        try
        {
   _logger.LogInformation(
   "Obținere programări viitoare: Zile={Days}, Doctor={DoctorID}",
          request.Days, request.DoctorID);

   var programari = await _programareRepository.GetUpcomingAsync(
    request.Days,
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
     "Găsite {Count} programări viitoare în următoarele {Days} zile",
       programariDto.Count, request.Days);

       return Result<IEnumerable<ProgramareListDto>>.Success(
    programariDto,
            $"Au fost găsite {programariDto.Count} programări viitoare.");
        }
        catch (Exception ex)
        {
   _logger.LogError(ex, "Eroare la obținerea programărilor viitoare");
      return Result<IEnumerable<ProgramareListDto>>.Failure($"Eroare: {ex.Message}");
        }
    }
}
