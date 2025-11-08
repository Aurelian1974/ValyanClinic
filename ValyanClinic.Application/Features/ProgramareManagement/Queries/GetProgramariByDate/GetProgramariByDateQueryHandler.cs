using MediatR;
using Microsoft.Extensions.Logging;
using ValyanClinic.Application.Common.Results;
using ValyanClinic.Application.Features.ProgramareManagement.DTOs;
using ValyanClinic.Domain.Interfaces.Repositories;

namespace ValyanClinic.Application.Features.ProgramareManagement.Queries.GetProgramariByDate;

public class GetProgramariByDateQueryHandler : IRequestHandler<GetProgramariByDateQuery, Result<IEnumerable<ProgramareListDto>>>
{
    private readonly IProgramareRepository _programareRepository;
    private readonly ILogger<GetProgramariByDateQueryHandler> _logger;

    public GetProgramariByDateQueryHandler(
   IProgramareRepository programareRepository,
   ILogger<GetProgramariByDateQueryHandler> logger)
    {
        _programareRepository = programareRepository;
        _logger = logger;
    }

    public async Task<Result<IEnumerable<ProgramareListDto>>> Handle(
        GetProgramariByDateQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
   _logger.LogInformation(
  "Obținere programări pentru data: {Date}, Doctor={DoctorID}",
request.Date.ToString("yyyy-MM-dd"), request.DoctorID);

        var programari = await _programareRepository.GetByDateAsync(
                request.Date,
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
       "Găsite {Count} programări pentru data {Date}",
       programariDto.Count, request.Date.ToString("yyyy-MM-dd"));

          return Result<IEnumerable<ProgramareListDto>>.Success(
programariDto,
 $"Au fost găsite {programariDto.Count} programări pentru data {request.Date:dd.MM.yyyy}.");
        }
 catch (Exception ex)
{
            _logger.LogError(ex, "Eroare la obținerea programărilor pentru data {Date}", request.Date);
            return Result<IEnumerable<ProgramareListDto>>.Failure($"Eroare: {ex.Message}");
        }
    }
}
