using MediatR;
using Microsoft.Extensions.Logging;
using ValyanClinic.Application.Common.Results;
using ValyanClinic.Domain.Interfaces.Repositories;

namespace ValyanClinic.Application.Features.SpecializareManagement.Commands.UpdateSpecializare;

public class UpdateSpecializareCommandHandler : IRequestHandler<UpdateSpecializareCommand, Result<bool>>
{
    private readonly ISpecializareRepository _repository;
    private readonly ILogger<UpdateSpecializareCommandHandler> _logger;

    public UpdateSpecializareCommandHandler(
        ISpecializareRepository repository,
        ILogger<UpdateSpecializareCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<Result<bool>> Handle(UpdateSpecializareCommand request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Updating Specializare: {Id}", request.Id);

            var specializare = await _repository.GetByIdAsync(request.Id, cancellationToken);
            if (specializare == null)
            {
                _logger.LogWarning("Specializare not found: {Id}", request.Id);
                return Result<bool>.Failure(new List<string> { "Specializarea nu a fost gasita" });
            }

            specializare.Denumire = request.Denumire;
            specializare.Categorie = request.Categorie;
            specializare.Descriere = request.Descriere;
            specializare.EsteActiv = request.EsteActiv;
            specializare.DataUltimeiModificari = DateTime.Now;
            specializare.ModificatDe = request.ModificatDe;

            var success = await _repository.UpdateAsync(specializare, cancellationToken);

            if (success)
            {
                _logger.LogInformation("Specializare updated successfully: {Id}", request.Id);
                return Result<bool>.Success(true);
            }

            _logger.LogWarning("Failed to update Specializare: {Id}", request.Id);
            return Result<bool>.Failure(new List<string> { "Actualizarea a esuat" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating Specializare: {Id}", request.Id);
            return Result<bool>.Failure(new List<string> { ex.Message });
        }
    }
}
