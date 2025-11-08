using MediatR;
using Microsoft.Extensions.Logging;
using ValyanClinic.Application.Common.Results;
using ValyanClinic.Domain.Entities;
using ValyanClinic.Domain.Interfaces.Repositories;

namespace ValyanClinic.Application.Features.ProgramareManagement.Commands.CreateProgramare;

/// <summary>
/// Handler pentru comanda CreateProgramareCommand.
/// Gestionează logica de business pentru crearea unei programări.
/// </summary>
public class CreateProgramareCommandHandler : IRequestHandler<CreateProgramareCommand, Result<Guid>>
{
    private readonly IProgramareRepository _programareRepository;
    private readonly IPacientRepository _pacientRepository;
    private readonly IPersonalMedicalRepository _personalMedicalRepository;
    private readonly ILogger<CreateProgramareCommandHandler> _logger;

    public CreateProgramareCommandHandler(
        IProgramareRepository programareRepository,
        IPacientRepository pacientRepository,
        IPersonalMedicalRepository personalMedicalRepository,
        ILogger<CreateProgramareCommandHandler> logger)
    {
   _programareRepository = programareRepository;
        _pacientRepository = pacientRepository;
        _personalMedicalRepository = personalMedicalRepository;
        _logger = logger;
    }

    public async Task<Result<Guid>> Handle(
        CreateProgramareCommand request,
    CancellationToken cancellationToken)
    {
      try
   {
 _logger.LogInformation(
        "Creare programare: Pacient={PacientID}, Doctor={DoctorID}, Data={Data}, Ora={Ora}",
                request.PacientID, request.DoctorID,
       request.DataProgramare.ToString("yyyy-MM-dd"),
    request.OraInceput.ToString(@"hh\:mm"));

   // ==================== VALIDĂRI BUSINESS ====================

            // 1. Verificare existență pacient
            var pacient = await _pacientRepository.GetByIdAsync(request.PacientID, cancellationToken);
      if (pacient == null)
  {
          _logger.LogWarning("Pacientul {PacientID} nu există", request.PacientID);
    return Result<Guid>.Failure("Pacientul specificat nu există în sistem.");
   }

   if (!pacient.Activ)
         {
_logger.LogWarning("Pacientul {PacientID} este inactiv", request.PacientID);
      return Result<Guid>.Failure("Pacientul este inactiv și nu poate fi programat.");
    }

        // 2. Verificare existență doctor
      var doctor = await _personalMedicalRepository.GetByIdAsync(request.DoctorID, cancellationToken);
   if (doctor == null)
            {
      _logger.LogWarning("Medicul {DoctorID} nu există", request.DoctorID);
         return Result<Guid>.Failure("Medicul specificat nu există în sistem.");
            }

            if (!doctor.EsteActiv.GetValueOrDefault(true))
     {
      _logger.LogWarning("Medicul {DoctorID} este inactiv", request.DoctorID);
       return Result<Guid>.Failure("Medicul este inactiv și nu poate primi programări.");
            }

  // 3. Verificare conflict de programare (suprapunere interval orar)
            var hasConflict = await _programareRepository.CheckConflictAsync(
    request.DoctorID,
  request.DataProgramare,
      request.OraInceput,
  request.OraSfarsit,
          excludeProgramareID: null, // null pentru creare (nu excludem nimic)
                cancellationToken);

    if (hasConflict)
          {
    _logger.LogWarning(
        "Conflict detectat pentru doctor {DoctorID} la data {Data} în intervalul {Interval}",
       request.DoctorID,
          request.DataProgramare.ToString("yyyy-MM-dd"),
          $"{request.OraInceput:hh\\:mm}-{request.OraSfarsit:hh\\:mm}");

   return Result<Guid>.Failure(
          $"Medicul {doctor.Nume} {doctor.Prenume} are deja o programare în intervalul orar selectat. " +
            $"Vă rugăm să alegeți un alt interval.");
       }

         // ==================== CREARE ENTITY ====================

       var programare = new Programare
            {
   PacientID = request.PacientID,
                DoctorID = request.DoctorID,
    DataProgramare = request.DataProgramare.Date, // Asigurăm că e doar dată
          OraInceput = request.OraInceput,
           OraSfarsit = request.OraSfarsit,
                TipProgramare = request.TipProgramare,
    Status = request.Status,
           Observatii = request.Observatii,
      DataCreare = DateTime.UtcNow,
       CreatDe = request.CreatDe
    };

            // ==================== SALVARE ÎN DB ====================

var createdProgramare = await _programareRepository.CreateAsync(programare, cancellationToken);

        _logger.LogInformation(
           "Programare creată cu succes: {ProgramareID} pentru pacient {PacientNume} cu doctor {DoctorNume}",
      createdProgramare.ProgramareID,
       $"{pacient.Nume} {pacient.Prenume}",
       $"{doctor.Nume} {doctor.Prenume}");

  return Result<Guid>.Success(
 createdProgramare.ProgramareID,
             $"Programarea a fost creată cu succes pentru {pacient.Nume} {pacient.Prenume} " +
       $"cu Dr. {doctor.Nume} {doctor.Prenume} pe data de {request.DataProgramare:dd.MM.yyyy} " +
       $"la ora {request.OraInceput:hh\\:mm}.");
}
        catch (Exception ex)
        {
   _logger.LogError(ex, "Eroare la crearea programării");
  return Result<Guid>.Failure($"Eroare la crearea programării: {ex.Message}");
        }
    }
}
