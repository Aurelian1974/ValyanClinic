using MediatR;
using Microsoft.Extensions.Logging;
using ValyanClinic.Application.Common.Results;
using ValyanClinic.Domain.Interfaces.Repositories;

namespace ValyanClinic.Application.Features.UserSessions.Queries.GetActiveSessions;

/// <summary>
/// Handler pentru GetActiveSessionsQuery
/// </summary>
public class GetActiveSessionsQueryHandler : IRequestHandler<GetActiveSessionsQuery, Result<IEnumerable<ActiveSessionDto>>>
{
  private readonly IUserSessionRepository _repository;
    private readonly ILogger<GetActiveSessionsQueryHandler> _logger;

    public GetActiveSessionsQueryHandler(
     IUserSessionRepository repository,
        ILogger<GetActiveSessionsQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<Result<IEnumerable<ActiveSessionDto>>> Handle(
    GetActiveSessionsQuery request,
   CancellationToken cancellationToken)
    {
    try
{
            _logger.LogInformation(
              "Getting active sessions. Utilizator: {UtilizatorID}, DoarExpiraInCurand: {DoarExpiraInCurand}",
 request.UtilizatorID, request.DoarExpiraInCurand);

    var sessions = await _repository.GetActiveSessionsWithDetailsAsync(
            request.UtilizatorID,
         request.DoarExpiraInCurand,
  request.SortColumn,
  request.SortDirection,
       cancellationToken);

            var dtoList = sessions.Select(s => new ActiveSessionDto
    {
     SessionID = s.SessionID,
           UtilizatorID = s.UtilizatorID,
                Username = s.Username,
     Email = s.Email,
            Rol = s.Rol,
    SessionToken = s.SessionToken,
    AdresaIP = s.AdresaIP,
   UserAgent = s.UserAgent,
        Dispozitiv = s.Dispozitiv,
   DataCreare = s.DataCreare,
              DataUltimaActivitate = s.DataUltimaActivitate,
           DataExpirare = s.DataExpirare,
  EsteActiva = s.EsteActiva
   }).ToList();

            _logger.LogInformation("Found {Count} active sessions", dtoList.Count);

            return Result<IEnumerable<ActiveSessionDto>>.Success(
        dtoList,
      $"S-au gasit {dtoList.Count} sesiuni active");
   }
        catch (Exception ex)
{
            _logger.LogError(ex, "Error getting active sessions");
            return Result<IEnumerable<ActiveSessionDto>>.Failure(
    new List<string> { $"Eroare la incarcarea sesiunilor: {ex.Message}" });
        }
    }
}
