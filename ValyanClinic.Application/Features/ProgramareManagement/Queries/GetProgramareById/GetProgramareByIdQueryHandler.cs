using MediatR;
using Microsoft.Extensions.Logging;
using ValyanClinic.Application.Common.Results;
using ValyanClinic.Application.Features.ProgramareManagement.DTOs;
using ValyanClinic.Domain.Interfaces.Repositories;

namespace ValyanClinic.Application.Features.ProgramareManagement.Queries.GetProgramareById;

/// <summary>
/// Handler pentru query-ul GetProgramareByIdQuery.
/// Obține detaliile complete ale unei programări.
/// </summary>
public class GetProgramareByIdQueryHandler : IRequestHandler<GetProgramareByIdQuery, Result<ProgramareDetailDto>>
{
    private readonly IProgramareRepository _programareRepository;
    private readonly ILogger<GetProgramareByIdQueryHandler> _logger;

    public GetProgramareByIdQueryHandler(
 IProgramareRepository programareRepository,
        ILogger<GetProgramareByIdQueryHandler> logger)
    {
 _programareRepository = programareRepository;
        _logger = logger;
    }

    public async Task<Result<ProgramareDetailDto>> Handle(
  GetProgramareByIdQuery request,
        CancellationToken cancellationToken)
    {
     try
      {
        _logger.LogInformation("Obținere detalii programare: {ProgramareID}", request.ProgramareID);

        // ==================== OBȚINERE DATE DIN REPOSITORY ====================

   var programare = await _programareRepository.GetByIdAsync(request.ProgramareID, cancellationToken);

    if (programare == null)
       {
       _logger.LogWarning("Programarea {ProgramareID} nu a fost găsită", request.ProgramareID);
      return Result<ProgramareDetailDto>.Failure("Programarea specificată nu a fost găsită.");
          }

            // ==================== MAPARE LA DTO ====================

     var programareDto = new ProgramareDetailDto
  {
      ProgramareID = programare.ProgramareID,
    PacientID = programare.PacientID,
     DoctorID = programare.DoctorID,
     DataProgramare = programare.DataProgramare,
   OraInceput = programare.OraInceput,
OraSfarsit = programare.OraSfarsit,
  TipProgramare = programare.TipProgramare,
  Status = programare.Status,
Observatii = programare.Observatii,
      // Navigation properties - Pacient
       PacientNumeComplet = programare.PacientNumeComplet,
     PacientTelefon = programare.PacientTelefon,
 PacientEmail = programare.PacientEmail,
 PacientCNP = programare.PacientCNP,
    // Navigation properties - Doctor
   DoctorNumeComplet = programare.DoctorNumeComplet,
                DoctorSpecializare = programare.DoctorSpecializare,
     DoctorTelefon = programare.DoctorTelefon,
       // Audit information
     DataCreare = programare.DataCreare,
        CreatDe = programare.CreatDe,
    CreatDeNumeComplet = programare.CreatDeNumeComplet,
    DataUltimeiModificari = programare.DataUltimeiModificari,
         ModificatDe = programare.ModificatDe
    };

   _logger.LogInformation(
   "Detalii programare obținute: {ProgramareID}, Pacient={Pacient}, Doctor={Doctor}, Data={Data}",
       programare.ProgramareID,
   programare.PacientNumeComplet,
      programare.DoctorNumeComplet,
            programare.DataProgramare.ToString("yyyy-MM-dd"));

     return Result<ProgramareDetailDto>.Success(
   programareDto,
       $"Detaliile programării au fost obținute cu succes.");
    }
      catch (Exception ex)
   {
     _logger.LogError(ex, "Eroare la obținerea detaliilor programării {ProgramareID}", request.ProgramareID);
  return Result<ProgramareDetailDto>.Failure($"Eroare la obținerea detaliilor programării: {ex.Message}");
     }
    }
}
