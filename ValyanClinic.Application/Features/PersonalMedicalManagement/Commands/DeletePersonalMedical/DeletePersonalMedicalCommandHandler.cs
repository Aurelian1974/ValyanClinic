using MediatR;
using Microsoft.Extensions.Logging;
using ValyanClinic.Application.Common.Results;
using ValyanClinic.Domain.Interfaces.Repositories;

namespace ValyanClinic.Application.Features.PersonalMedicalManagement.Commands.DeletePersonalMedical;

public class DeletePersonalMedicalCommandHandler : IRequestHandler<DeletePersonalMedicalCommand, Result<bool>>
{
    private readonly IPersonalMedicalRepository _repository;
    private readonly ILogger<DeletePersonalMedicalCommandHandler> _logger;
    private readonly ValyanClinic.Application.Interfaces.IPersonalMedicalNotifier? _notifier;

    public DeletePersonalMedicalCommandHandler(
        IPersonalMedicalRepository repository,
        ILogger<DeletePersonalMedicalCommandHandler> logger,
        ValyanClinic.Application.Interfaces.IPersonalMedicalNotifier? notifier = null)
    {
        _repository = repository;
        _logger = logger;
        _notifier = notifier;
    }

    public async Task<Result<bool>> Handle(DeletePersonalMedicalCommand request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Stergere personal medical: {PersonalID}", request.PersonalID);

            var success = await _repository.DeleteAsync(request.PersonalID, cancellationToken);

            if (success)
            {
                _logger.LogInformation("Personal medical sters cu succes: {PersonalID}", request.PersonalID);

                try
                {
                    if (_notifier != null)
                    {
                        await _notifier.NotifyPersonalChangedAsync("Deleted", request.PersonalID, cancellationToken);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogDebug(ex, "Notifier failed for Deleted event");
                }

                return Result<bool>.Success(true, "Personal medical sters cu succes");
            }

            _logger.LogWarning("Nu s-a putut sterge personalul medical: {PersonalID}", request.PersonalID);
            return Result<bool>.Failure("Nu s-a putut sterge personalul medical");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Eroare la stergerea personalului medical: {PersonalID}", request.PersonalID);
            return Result<bool>.Failure($"Eroare la stergere: {ex.Message}");
        }
    }
}
