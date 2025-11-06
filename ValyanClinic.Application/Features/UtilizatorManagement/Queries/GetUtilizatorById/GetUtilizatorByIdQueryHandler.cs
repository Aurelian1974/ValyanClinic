using MediatR;
using Microsoft.Extensions.Logging;
using ValyanClinic.Application.Common.Results;
using ValyanClinic.Domain.Interfaces.Repositories;

namespace ValyanClinic.Application.Features.UtilizatorManagement.Queries.GetUtilizatorById;

public class GetUtilizatorByIdQueryHandler : IRequestHandler<GetUtilizatorByIdQuery, Result<UtilizatorDetailDto>>
{
    private readonly IUtilizatorRepository _repository;
    private readonly ILogger<GetUtilizatorByIdQueryHandler> _logger;

    public GetUtilizatorByIdQueryHandler(
  IUtilizatorRepository repository,
        ILogger<GetUtilizatorByIdQueryHandler> logger)
  {
     _repository = repository;
  _logger = logger;
    }

 public async Task<Result<UtilizatorDetailDto>> Handle(GetUtilizatorByIdQuery request, CancellationToken cancellationToken)
    {
        try
        {
    _logger.LogInformation("Obtin detalii utilizator: {UtilizatorID}", request.UtilizatorID);

   var utilizator = await _repository.GetByIdAsync(request.UtilizatorID, cancellationToken);

          if (utilizator == null)
     {
     _logger.LogWarning("Utilizatorul nu a fost gasit: {UtilizatorID}", request.UtilizatorID);
        return Result<UtilizatorDetailDto>.Failure("Utilizatorul nu a fost gasit");
      }

     var dto = new UtilizatorDetailDto
      {
       UtilizatorID = utilizator.UtilizatorID,
             PersonalMedicalID = utilizator.PersonalMedicalID,
   Username = utilizator.Username,
       Email = utilizator.Email,
    Rol = utilizator.Rol,
     EsteActiv = utilizator.EsteActiv,
    DataCreare = utilizator.DataCreare,
             DataUltimaAutentificare = utilizator.DataUltimaAutentificare,
  NumarIncercariEsuate = utilizator.NumarIncercariEsuate,
    DataBlocare = utilizator.DataBlocare,
           TokenResetareParola = utilizator.TokenResetareParola,
DataExpirareToken = utilizator.DataExpirareToken,
CreatDe = utilizator.CreatDe,
          DataCrearii = utilizator.DataCrearii,
      ModificatDe = utilizator.ModificatDe,
           DataUltimeiModificari = utilizator.DataUltimeiModificari,
           NumeCompletPersonalMedical = utilizator.PersonalMedical?.NumeComplet ?? string.Empty,
      Nume = utilizator.PersonalMedical?.Nume,
                Prenume = utilizator.PersonalMedical?.Prenume,
   Specializare = utilizator.PersonalMedical?.Specializare,
  Departament = utilizator.PersonalMedical?.Departament,
                Pozitie = utilizator.PersonalMedical?.Pozitie,
Telefon = utilizator.PersonalMedical?.Telefon,
 EmailPersonalMedical = utilizator.PersonalMedical?.Email
         };

            _logger.LogInformation("Detalii utilizator obtinute: {Username}", utilizator.Username);
         return Result<UtilizatorDetailDto>.Success(dto);
        }
        catch (Exception ex)
        {
       _logger.LogError(ex, "Eroare la obtinerea detaliilor utilizatorului: {UtilizatorID}", request.UtilizatorID);
            return Result<UtilizatorDetailDto>.Failure($"Eroare: {ex.Message}");
  }
    }
}
