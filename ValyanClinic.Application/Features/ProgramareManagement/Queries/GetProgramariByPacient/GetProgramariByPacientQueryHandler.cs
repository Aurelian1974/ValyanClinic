using MediatR;
using Microsoft.Extensions.Logging;
using ValyanClinic.Application.Common.Results;
using ValyanClinic.Application.Features.ProgramareManagement.DTOs;
using ValyanClinic.Domain.Interfaces.Repositories;

namespace ValyanClinic.Application.Features.ProgramareManagement.Queries.GetProgramariByPacient;

public class GetProgramariByPacientQueryHandler : IRequestHandler<GetProgramariByPacientQuery, Result<IEnumerable<ProgramareListDto>>>
{
    private readonly IProgramareRepository _programareRepository;
    private readonly ILogger<GetProgramariByPacientQueryHandler> _logger;

    public GetProgramariByPacientQueryHandler(
    IProgramareRepository programareRepository,
 ILogger<GetProgramariByPacientQueryHandler> logger)
    {
     _programareRepository = programareRepository;
        _logger = logger;
    }

    public async Task<Result<IEnumerable<ProgramareListDto>>> Handle(
        GetProgramariByPacientQuery request,
    CancellationToken cancellationToken)
    {
        try
    {
         _logger.LogInformation("Obținere istoric programări pacient: {PacientID}", request.PacientID);

            var programari = await _programareRepository.GetByPacientAsync(request.PacientID, cancellationToken);

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

 _logger.LogInformation("Găsite {Count} programări pentru pacient {PacientID}", programariDto.Count, request.PacientID);

        return Result<IEnumerable<ProgramareListDto>>.Success(
programariDto,
     $"Au fost găsite {programariDto.Count} programări în istoric.");
 }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Eroare la obținerea istoricului pacient {PacientID}", request.PacientID);
            return Result<IEnumerable<ProgramareListDto>>.Failure($"Eroare: {ex.Message}");
        }
    }
}
