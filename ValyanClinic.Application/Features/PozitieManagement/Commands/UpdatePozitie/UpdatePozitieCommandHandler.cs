using MediatR;
using Microsoft.Extensions.Logging;
using ValyanClinic.Application.Common.Results;
using ValyanClinic.Domain.Interfaces.Repositories;

namespace ValyanClinic.Application.Features.PozitieManagement.Commands.UpdatePozitie;

public class UpdatePozitieCommandHandler : IRequestHandler<UpdatePozitieCommand, Result<bool>>
{
    private readonly IPozitieRepository _repository;
    private readonly ILogger<UpdatePozitieCommandHandler> _logger;

    public UpdatePozitieCommandHandler(
        IPozitieRepository repository,
        ILogger<UpdatePozitieCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<Result<bool>> Handle(UpdatePozitieCommand request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Updating Pozitie: {Id}", request.Id);

            var pozitie = await _repository.GetByIdAsync(request.Id, cancellationToken);
            if (pozitie == null)
            {
                _logger.LogWarning("Pozitie not found: {Id}", request.Id);
                return Result<bool>.Failure(new List<string> { "Pozitia nu a fost gasita" });
            }

            pozitie.Denumire = request.Denumire;
            pozitie.Descriere = request.Descriere;
            pozitie.EsteActiv = request.EsteActiv;
            pozitie.DataUltimeiModificari = DateTime.Now;
            pozitie.ModificatDe = request.ModificatDe;

            var success = await _repository.UpdateAsync(pozitie, cancellationToken);

            if (success)
            {
                _logger.LogInformation("Pozitie updated successfully: {Id}", request.Id);
                return Result<bool>.Success(true);
            }

            _logger.LogWarning("Failed to update Pozitie: {Id}", request.Id);
            return Result<bool>.Failure(new List<string> { "Actualizarea a esuat" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating Pozitie: {Id}", request.Id);
            return Result<bool>.Failure(new List<string> { ex.Message });
        }
    }
}
