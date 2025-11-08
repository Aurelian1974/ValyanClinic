using MediatR;
using Microsoft.Extensions.Logging;
using ValyanClinic.Application.Common.Results;
using ValyanClinic.Application.Features.ProgramareManagement.DTOs;
using ValyanClinic.Domain.Interfaces.Repositories;

namespace ValyanClinic.Application.Features.ProgramareManagement.Queries.GetProgramariByDoctor;

public class GetProgramariByDoctorQueryHandler : IRequestHandler<GetProgramariByDoctorQuery, Result<IEnumerable<ProgramareListDto>>>
{
    private readonly IProgramareRepository _programareRepository;
    private readonly ILogger<GetProgramariByDoctorQueryHandler> _logger;

    public GetProgramariByDoctorQueryHandler(
 IProgramareRepository programareRepository,
        ILogger<GetProgramariByDoctorQueryHandler> logger)
    {
      _programareRepository = programareRepository;
        _logger = logger;
    }

    public async Task<Result<IEnumerable<ProgramareListDto>>> Handle(
        GetProgramariByDoctorQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
     _logger.LogInformation(
        "Obținere programări doctor: {DoctorID}, DataStart={DataStart}, DataEnd={DataEnd}",
             request.DoctorID, request.DataStart, request.DataEnd);

            var programari = await _programareRepository.GetByDoctorAsync(
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

         _logger.LogInformation("Găsite {Count} programări pentru doctor {DoctorID}", programariDto.Count, request.DoctorID);

     return Result<IEnumerable<ProgramareListDto>>.Success(
            programariDto,
           $"Au fost găsite {programariDto.Count} programări.");
        }
      catch (Exception ex)
    {
    _logger.LogError(ex, "Eroare la obținerea programărilor pentru doctor {DoctorID}", request.DoctorID);
            return Result<IEnumerable<ProgramareListDto>>.Failure($"Eroare: {ex.Message}");
        }
    }
}
