using MediatR;
using Microsoft.Extensions.Logging;
using ValyanClinic.Application.Common.Results;
using ValyanClinic.Domain.Interfaces.Repositories;

namespace ValyanClinic.Application.Features.UserSessions.Commands.EndSession;

/// <summary>
/// Handler pentru EndSessionCommand
/// </summary>
public class EndSessionCommandHandler : IRequestHandler<EndSessionCommand, Result<bool>>
{
    private readonly IUserSessionRepository _repository;
    private readonly ILogger<EndSessionCommandHandler> _logger;

    public EndSessionCommandHandler(
        IUserSessionRepository repository,
  ILogger<EndSessionCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<Result<bool>> Handle(
 EndSessionCommand request,
  CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation(
                 "Ending session: {SessionID} by {ModificatDe}",
                          request.SessionID, request.ModificatDe);

            var success = await _repository.EndSessionAsync(
               request.SessionID,
                  cancellationToken);

            if (success)
            {
                _logger.LogInformation("Session {SessionID} ended successfully", request.SessionID);
                return Result<bool>.Success(true, "Sesiunea a fost inchisa cu succes");
            }

            _logger.LogWarning("Failed to end session: {SessionID}", request.SessionID);
            return Result<bool>.Failure(new List<string> { "Sesiunea nu a putut fi inchisa" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error ending session: {SessionID}", request.SessionID);
            return Result<bool>.Failure(new List<string> { $"Eroare: {ex.Message}" });
        }
    }
}
