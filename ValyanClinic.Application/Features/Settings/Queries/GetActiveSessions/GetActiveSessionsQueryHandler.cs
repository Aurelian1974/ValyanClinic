using MediatR;
using Microsoft.Extensions.Logging;
using ValyanClinic.Application.Common.Results;
using ValyanClinic.Domain.Interfaces.Repositories;

namespace ValyanClinic.Application.Features.Settings.Queries.GetActiveSessions;

public class GetActiveSessionsQueryHandler : IRequestHandler<GetActiveSessionsQuery, Result<List<UserSessionDto>>>
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

    public async Task<Result<List<UserSessionDto>>> Handle(
     GetActiveSessionsQuery request,
   CancellationToken cancellationToken)
    {
try
        {
       _logger.LogInformation("Fetching active user sessions");

    var sessions = await _repository.GetActiveSessionsAsync(cancellationToken);

  var dtos = sessions.Select(s => new UserSessionDto
   {
     SessionID = s.SessionID,
     UtilizatorID = s.UtilizatorID,
  SessionToken = s.SessionToken,
     AdresaIP = s.AdresaIP,
  UserAgent = s.UserAgent,
  Dispozitiv = s.Dispozitiv,
    DataCreare = s.DataCreare,
      DataUltimaActivitate = s.DataUltimaActivitate,
     DataExpirare = s.DataExpirare,
    EsteActiva = s.EsteActiva
  }).ToList();

   _logger.LogInformation("Found {Count} active sessions", dtos.Count);

  return Result<List<UserSessionDto>>.Success(dtos);
        }
        catch (Exception ex)
        {
  _logger.LogError(ex, "Error fetching active sessions");
    return Result<List<UserSessionDto>>.Failure($"Error: {ex.Message}");
  }
    }
}
