using MediatR;
using Microsoft.Extensions.Logging;
using ValyanClinic.Application.Common.Results;
using ValyanClinic.Domain.Interfaces.Repositories;

namespace ValyanClinic.Application.Features.ProgramareManagement.Queries.CheckProgramareConflict;

/// <summary>
/// Handler pentru CheckProgramareConflictQuery.
/// Verifică dacă există programări conflictuale pentru un medic într-un anumit interval.
/// </summary>
public class CheckProgramareConflictQueryHandler : IRequestHandler<CheckProgramareConflictQuery, Result<bool>>
{
    private readonly IProgramareRepository _programareRepository;
    private readonly ILogger<CheckProgramareConflictQueryHandler> _logger;

    public CheckProgramareConflictQueryHandler(
        IProgramareRepository programareRepository,
  ILogger<CheckProgramareConflictQueryHandler> logger)
    {
        _programareRepository = programareRepository;
        _logger = logger;
    }

    public async Task<Result<bool>> Handle(
        CheckProgramareConflictQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation(
              "Verificare conflict programare: Doctor={DoctorID}, Data={Data}, Interval={Interval}, Exclude={Exclude}",
     request.DoctorID,
              request.DataProgramare.ToString("yyyy-MM-dd"),
         $"{request.OraInceput:hh\\:mm}-{request.OraSfarsit:hh\\:mm}",
   request.ExcludeProgramareID);

            // Apelăm metoda CheckConflictAsync din repository
            var hasConflict = await _programareRepository.CheckConflictAsync(
   request.DoctorID,
        request.DataProgramare,
       request.OraInceput,
       request.OraSfarsit,
 request.ExcludeProgramareID,
           cancellationToken);

            if (hasConflict)
            {
                _logger.LogWarning(
                    "Conflict detectat pentru doctor {DoctorID} la data {Data} în intervalul {Interval}",
                        request.DoctorID,
               request.DataProgramare.ToString("yyyy-MM-dd"),
                        $"{request.OraInceput:hh\\:mm}-{request.OraSfarsit:hh\\:mm}");
            }
            else
            {
                _logger.LogInformation("Nu există conflict - intervalul este liber");
            }

            return Result<bool>.Success(hasConflict);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Eroare la verificarea conflictului de programare");
            return Result<bool>.Failure("Eroare la verificarea disponibilității. Vă rugăm să încercați din nou.");
        }
    }
}
