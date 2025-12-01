using MediatR;
using Microsoft.Extensions.Logging;
using ValyanClinic.Application.Common.Results;
using ValyanClinic.Domain.Entities;
using ValyanClinic.Domain.Interfaces.Repositories;

namespace ValyanClinic.Application.Features.PersonalMedicalManagement.Commands.UpdatePersonalMedical;

public class UpdatePersonalMedicalCommandHandler : IRequestHandler<UpdatePersonalMedicalCommand, Result<bool>>
{
    private readonly IPersonalMedicalRepository _repository;
    private readonly ILogger<UpdatePersonalMedicalCommandHandler> _logger;

    public UpdatePersonalMedicalCommandHandler(
        IPersonalMedicalRepository repository,
        ILogger<UpdatePersonalMedicalCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<Result<bool>> Handle(
        UpdatePersonalMedicalCommand request,
        CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Updating PersonalMedical: {PersonalID}", request.PersonalID);

            // DEBUG: Log incoming request values
            _logger.LogInformation("Request values: Pozitie='{Pozitie}', PozitieID={PozitieID}",
              request.Pozitie, request.PozitieID);

            var existing = await _repository.GetByIdAsync(request.PersonalID, cancellationToken);
            if (existing == null)
            {
                _logger.LogWarning("PersonalMedical not found for update: {PersonalID}", request.PersonalID);
                return Result<bool>.Failure("Personal medical not found");
            }

            // DEBUG: Log existing values
            _logger.LogInformation("Existing values: Pozitie='{Pozitie}', PozitieID={PozitieID}",
              existing.Pozitie, existing.PozitieID);

            existing.Nume = request.Nume;
            existing.Prenume = request.Prenume;
            existing.Specializare = request.Specializare;
            existing.NumarLicenta = request.NumarLicenta;
            existing.Telefon = request.Telefon;
            existing.Email = request.Email;
            existing.Departament = request.Departament;
            existing.Pozitie = request.Pozitie;
            existing.EsteActiv = request.EsteActiv;
            existing.CategorieID = request.CategorieID;
            existing.PozitieID = request.PozitieID;
            existing.SpecializareID = request.SpecializareID;
            existing.SubspecializareID = request.SubspecializareID;

            // DEBUG: Log updated values before save
            _logger.LogInformation("Updated values before save: Pozitie='{Pozitie}', PozitieID={PozitieID}",
    existing.Pozitie, existing.PozitieID);

            var success = await _repository.UpdateAsync(existing, cancellationToken);

            if (!success)
            {
                _logger.LogWarning("Failed to update PersonalMedical: {PersonalID}", request.PersonalID);
                return Result<bool>.Failure("Failed to update personal medical");
            }

            _logger.LogInformation("PersonalMedical updated successfully: {PersonalID}", request.PersonalID);
            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating PersonalMedical: {PersonalID}", request.PersonalID);
            return Result<bool>.Failure($"Error: {ex.Message}");
        }
    }
}
