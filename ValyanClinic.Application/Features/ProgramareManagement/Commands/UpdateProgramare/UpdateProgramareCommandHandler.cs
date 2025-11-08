using MediatR;
using Microsoft.Extensions.Logging;
using ValyanClinic.Application.Common.Results;
using ValyanClinic.Domain.Interfaces.Repositories;

namespace ValyanClinic.Application.Features.ProgramareManagement.Commands.UpdateProgramare;

/// <summary>
/// Handler pentru comanda UpdateProgramareCommand.
/// Gestionează logica de business pentru actualizarea unei programări.
/// </summary>
public class UpdateProgramareCommandHandler : IRequestHandler<UpdateProgramareCommand, Result<bool>>
{
    private readonly IProgramareRepository _programareRepository;
    private readonly IPacientRepository _pacientRepository;
    private readonly IPersonalMedicalRepository _personalMedicalRepository;
    private readonly ILogger<UpdateProgramareCommandHandler> _logger;

    public UpdateProgramareCommandHandler(
        IProgramareRepository programareRepository,
        IPacientRepository pacientRepository,
        IPersonalMedicalRepository personalMedicalRepository,
     ILogger<UpdateProgramareCommandHandler> logger)
    {
        _programareRepository = programareRepository;
        _pacientRepository = pacientRepository;
        _personalMedicalRepository = personalMedicalRepository;
    _logger = logger;
    }

    public async Task<Result<bool>> Handle(
        UpdateProgramareCommand request,
        CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation(
 "Actualizare programare: {ProgramareID}, Data={Data}, Ora={Ora}, Status={Status}",
          request.ProgramareID,
           request.DataProgramare.ToString("yyyy-MM-dd"),
   request.OraInceput.ToString(@"hh\:mm"),
         request.Status);

          // ==================== VALIDĂRI BUSINESS ====================

  // 1. Verificare existență programare
   var existingProgramare = await _programareRepository.GetByIdAsync(request.ProgramareID, cancellationToken);
if (existingProgramare == null)
            {
      _logger.LogWarning("Programarea {ProgramareID} nu există", request.ProgramareID);
       return Result<bool>.Failure("Programarea specificată nu există în sistem.");
   }

 // 2. Verificare dacă programarea poate fi editată (doar Programata și Confirmata)
   if (existingProgramare.Status is not ("Programata" or "Confirmata"))
        {
    _logger.LogWarning(
        "Programarea {ProgramareID} cu status {Status} nu poate fi editată",
       request.ProgramareID, existingProgramare.Status);

       return Result<bool>.Failure(
             $"Programarea cu statusul '{existingProgramare.Status}' nu mai poate fi modificată. " +
       $"Doar programările cu statusul 'Programată' sau 'Confirmată' pot fi editate.");
            }

       // 3. Verificare existență pacient (dacă s-a schimbat)
         if (request.PacientID != existingProgramare.PacientID)
   {
    var pacient = await _pacientRepository.GetByIdAsync(request.PacientID, cancellationToken);
 if (pacient == null)
  {
 _logger.LogWarning("Pacientul {PacientID} nu există", request.PacientID);
       return Result<bool>.Failure("Pacientul specificat nu există în sistem.");
      }

     if (!pacient.Activ)
       {
           _logger.LogWarning("Pacientul {PacientID} este inactiv", request.PacientID);
            return Result<bool>.Failure("Pacientul este inactiv și nu poate fi programat.");
     }
      }

       // 4. Verificare existență doctor (dacă s-a schimbat)
            if (request.DoctorID != existingProgramare.DoctorID)
            {
    var doctor = await _personalMedicalRepository.GetByIdAsync(request.DoctorID, cancellationToken);
     if (doctor == null)
       {
          _logger.LogWarning("Medicul {DoctorID} nu există", request.DoctorID);
      return Result<bool>.Failure("Medicul specificat nu există în sistem.");
      }

          if (!doctor.EsteActiv.GetValueOrDefault(true))
          {
       _logger.LogWarning("Medicul {DoctorID} este inactiv", request.DoctorID);
   return Result<bool>.Failure("Medicul este inactiv și nu poate primi programări.");
            }
}

     // 5. Verificare conflict de programare (exclude programarea curentă)
            var hasConflict = await _programareRepository.CheckConflictAsync(
   request.DoctorID,
                request.DataProgramare,
        request.OraInceput,
                request.OraSfarsit,
              excludeProgramareID: request.ProgramareID, // EXCLUDE programarea curentă
    cancellationToken);

   if (hasConflict)
  {
   _logger.LogWarning(
      "Conflict detectat pentru doctor {DoctorID} la data {Data} în intervalul {Interval}",
           request.DoctorID,
        request.DataProgramare.ToString("yyyy-MM-dd"),
         $"{request.OraInceput:hh\\:mm}-{request.OraSfarsit:hh\\:mm}");

       var doctor = await _personalMedicalRepository.GetByIdAsync(request.DoctorID, cancellationToken);
    return Result<bool>.Failure(
       $"Medicul {doctor?.Nume} {doctor?.Prenume} are deja o altă programare în intervalul orar selectat. " +
          $"Vă rugăm să alegeți un alt interval.");
            }

            // ==================== ACTUALIZARE ENTITY ====================

    existingProgramare.PacientID = request.PacientID;
   existingProgramare.DoctorID = request.DoctorID;
       existingProgramare.DataProgramare = request.DataProgramare.Date;
            existingProgramare.OraInceput = request.OraInceput;
            existingProgramare.OraSfarsit = request.OraSfarsit;
   existingProgramare.TipProgramare = request.TipProgramare;
existingProgramare.Status = request.Status;
          existingProgramare.Observatii = request.Observatii;
            existingProgramare.DataUltimeiModificari = DateTime.UtcNow;
       existingProgramare.ModificatDe = request.ModificatDe;

            // ==================== SALVARE ÎN DB ====================

var updatedProgramare = await _programareRepository.UpdateAsync(existingProgramare, cancellationToken);

     _logger.LogInformation(
             "Programare actualizată cu succes: {ProgramareID}",
    updatedProgramare.ProgramareID);

    return Result<bool>.Success(
   true,
    $"Programarea a fost actualizată cu succes.");
        }
        catch (Exception ex)
    {
            _logger.LogError(ex, "Eroare la actualizarea programării {ProgramareID}", request.ProgramareID);
            return Result<bool>.Failure($"Eroare la actualizarea programării: {ex.Message}");
     }
    }
}
