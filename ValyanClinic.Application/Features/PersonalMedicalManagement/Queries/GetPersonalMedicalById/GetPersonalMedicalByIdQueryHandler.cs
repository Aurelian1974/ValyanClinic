using MediatR;
using Microsoft.Extensions.Logging;
using ValyanClinic.Application.Common.Results;
using ValyanClinic.Domain.Interfaces.Repositories;

namespace ValyanClinic.Application.Features.PersonalMedicalManagement.Queries.GetPersonalMedicalById;

public class GetPersonalMedicalByIdQueryHandler : IRequestHandler<GetPersonalMedicalByIdQuery, Result<PersonalMedicalDetailDto>>
{
    private readonly IPersonalMedicalRepository _personalRepository;
    private readonly ILogger<GetPersonalMedicalByIdQueryHandler> _logger;

    public GetPersonalMedicalByIdQueryHandler(
        IPersonalMedicalRepository personalRepository,
        ILogger<GetPersonalMedicalByIdQueryHandler> logger)
    {
        _personalRepository = personalRepository;
        _logger = logger;
    }

    public async Task<Result<PersonalMedicalDetailDto>> Handle(
        GetPersonalMedicalByIdQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Fetching PersonalMedical by ID: {PersonalID}", request.PersonalID);

            var personalMedical = await _personalRepository.GetByIdAsync(request.PersonalID, cancellationToken);

            if (personalMedical == null)
            {
                _logger.LogWarning("PersonalMedical not found: {PersonalID}", request.PersonalID);
                return Result<PersonalMedicalDetailDto>.Failure("Personal medical not found");
            }

            // Map entity to DTO
            var dto = new PersonalMedicalDetailDto
            {
                PersonalID = personalMedical.PersonalID,
                PersonalId = personalMedical.PersonalId,
                NumeComplet = $"{personalMedical.Nume} {personalMedical.Prenume}",
                Nume = personalMedical.Nume,
                Prenume = personalMedical.Prenume,
                Email = personalMedical.Email,
                Telefon = personalMedical.Telefon,
                CategorieID = personalMedical.CategorieID,
                SpecializareID = personalMedical.SpecializareID,
                SubspecializareID = personalMedical.SubspecializareID,
                EsteActiv = personalMedical.EsteActiv,
                DataAngajarii = personalMedical.DataAngajarii,
                DataPlecarii = personalMedical.DataPlecarii
            };

            _logger.LogInformation(
                "PersonalMedical found: {PersonalID} - {NumeComplet}",
                dto.PersonalID,
                dto.NumeComplet);

            return Result<PersonalMedicalDetailDto>.Success(dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching PersonalMedical by ID: {PersonalID}", request.PersonalID);
            return Result<PersonalMedicalDetailDto>.Failure($"Error: {ex.Message}");
        }
    }
}
