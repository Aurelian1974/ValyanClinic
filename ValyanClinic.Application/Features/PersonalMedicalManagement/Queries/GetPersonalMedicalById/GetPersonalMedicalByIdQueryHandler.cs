using MediatR;
using Microsoft.Extensions.Logging;
using ValyanClinic.Application.Common.Results;
using ValyanClinic.Domain.Interfaces.Repositories;

namespace ValyanClinic.Application.Features.PersonalMedicalManagement.Queries.GetPersonalMedicalById;

public class GetPersonalMedicalByIdQueryHandler : IRequestHandler<GetPersonalMedicalByIdQuery, Result<PersonalMedicalDetailDto>>
{
    private readonly IPersonalMedicalRepository _repository;
    private readonly ILogger<GetPersonalMedicalByIdQueryHandler> _logger;

    public GetPersonalMedicalByIdQueryHandler(
        IPersonalMedicalRepository repository,
        ILogger<GetPersonalMedicalByIdQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<Result<PersonalMedicalDetailDto>> Handle(
        GetPersonalMedicalByIdQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Fetching PersonalMedical by ID: {PersonalID}", request.PersonalID);

            var personalMedical = await _repository.GetByIdAsync(request.PersonalID, cancellationToken);

            if (personalMedical == null)
            {
                _logger.LogWarning("PersonalMedical not found: {PersonalID}", request.PersonalID);
                return Result<PersonalMedicalDetailDto>.Failure("Personal medical not found");
            }

            var dto = new PersonalMedicalDetailDto
            {
                PersonalID = personalMedical.PersonalID,
                Nume = personalMedical.Nume,
                Prenume = personalMedical.Prenume,
                Specializare = personalMedical.Specializare,
                NumarLicenta = personalMedical.NumarLicenta,
                Telefon = personalMedical.Telefon,
                Email = personalMedical.Email,
                Departament = personalMedical.Departament,
                Pozitie = personalMedical.Pozitie,
                EsteActiv = personalMedical.EsteActiv,
                DataCreare = personalMedical.DataCreare,
                CategorieID = personalMedical.CategorieID,
                SpecializareID = personalMedical.SpecializareID,
                SubspecializareID = personalMedical.SubspecializareID
            };

            _logger.LogInformation("PersonalMedical found: {PersonalID} - {NumeComplet}", 
                dto.PersonalID, dto.NumeComplet);

            return Result<PersonalMedicalDetailDto>.Success(dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching PersonalMedical by ID: {PersonalID}", request.PersonalID);
            return Result<PersonalMedicalDetailDto>.Failure($"Error: {ex.Message}");
        }
    }
}
