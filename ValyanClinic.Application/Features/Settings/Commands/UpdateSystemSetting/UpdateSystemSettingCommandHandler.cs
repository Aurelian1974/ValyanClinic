using MediatR;
using Microsoft.Extensions.Logging;
using ValyanClinic.Application.Common.Results;
using ValyanClinic.Domain.Interfaces.Repositories;

namespace ValyanClinic.Application.Features.Settings.Commands.UpdateSystemSetting;

public class UpdateSystemSettingCommandHandler : IRequestHandler<UpdateSystemSettingCommand, Result>
{
    private readonly ISystemSettingsRepository _repository;
    private readonly ILogger<UpdateSystemSettingCommandHandler> _logger;

    public UpdateSystemSettingCommandHandler(
      ISystemSettingsRepository repository,
    ILogger<UpdateSystemSettingCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<Result> Handle(UpdateSystemSettingCommand request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation(
                 "Updating system setting: {Categorie}.{Cheie} = {Valoare}, Descriere = {Descriere}",
               request.Categorie, request.Cheie, request.Valoare, request.Descriere);

            var success = await _repository.UpdateAsync(
              request.Categorie,
                request.Cheie,
            request.Valoare,
            request.Descriere, // ✅ ADĂUGAT
            request.ModificatDe,
                   cancellationToken);

            if (success)
            {
                _logger.LogInformation("System setting updated successfully: {Categorie}.{Cheie}",
                    request.Categorie, request.Cheie);
                return Result.Success("Setarea a fost actualizata cu succes");
            }

            _logger.LogWarning("Failed to update system setting: {Categorie}.{Cheie}",
request.Categorie, request.Cheie);
            return Result.Failure("Actualizarea setarii a esuat");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating system setting: {Categorie}.{Cheie}",
           request.Categorie, request.Cheie);
            return Result.Failure($"Eroare: {ex.Message}");
        }
    }
}
