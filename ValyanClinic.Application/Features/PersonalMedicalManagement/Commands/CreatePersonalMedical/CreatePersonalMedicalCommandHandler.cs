using MediatR;
using Microsoft.Extensions.Logging;
using ValyanClinic.Application.Common.Results;
using ValyanClinic.Domain.Entities;
using ValyanClinic.Domain.Interfaces.Repositories;

namespace ValyanClinic.Application.Features.PersonalMedicalManagement.Commands.CreatePersonalMedical;

public class CreatePersonalMedicalCommandHandler : IRequestHandler<CreatePersonalMedicalCommand, Result<Guid>>
{
    private readonly IPersonalMedicalRepository _repository;
    private readonly ILogger<CreatePersonalMedicalCommandHandler> _logger;

    public CreatePersonalMedicalCommandHandler(
        IPersonalMedicalRepository repository,
        ILogger<CreatePersonalMedicalCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<Result<Guid>> Handle(
        CreatePersonalMedicalCommand request,
        CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Creating new PersonalMedical: {Nume} {Prenume}",
                request.Nume, request.Prenume);

            var personalMedical = new PersonalMedical
            {
                PersonalID = Guid.NewGuid(),
                Nume = request.Nume,
                Prenume = request.Prenume,
                Specializare = request.Specializare,
                NumarLicenta = request.NumarLicenta,
                Telefon = request.Telefon,
                Email = request.Email,
                Departament = request.Departament,
                Pozitie = request.Pozitie,
                EsteActiv = request.EsteActiv,
                CategorieID = request.CategorieID,
                PozitieID = request.PozitieID,
                SpecializareID = request.SpecializareID,
                SubspecializareID = request.SubspecializareID,
                DataCreare = DateTime.UtcNow
            };

            var newId = await _repository.CreateAsync(personalMedical, cancellationToken);

            if (newId == Guid.Empty)
            {
                _logger.LogWarning("Failed to create PersonalMedical");
                return Result<Guid>.Failure("Failed to create personal medical");
            }

            _logger.LogInformation("PersonalMedical created successfully: {PersonalID}", newId);
            return Result<Guid>.Success(newId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating PersonalMedical");
            return Result<Guid>.Failure($"Error: {ex.Message}");
        }
    }
}
