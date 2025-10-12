using MediatR;
using Microsoft.Extensions.Logging;
using ValyanClinic.Application.Common.Results;
using ValyanClinic.Domain.Interfaces.Repositories;

namespace ValyanClinic.Application.Features.PersonalMedicalManagement.Queries.GetPersonalMedicalList;

public class GetPersonalMedicalListQueryHandler : IRequestHandler<GetPersonalMedicalListQuery, PagedResult<PersonalMedicalListDto>>
{
    private readonly IPersonalMedicalRepository _repository;
    private readonly ILogger<GetPersonalMedicalListQueryHandler> _logger;

    public GetPersonalMedicalListQueryHandler(
        IPersonalMedicalRepository repository,
        ILogger<GetPersonalMedicalListQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<PagedResult<PersonalMedicalListDto>> Handle(GetPersonalMedicalListQuery request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation(
                "Obtin lista personal medical: Page={Page}, Size={Size}, Search={Search}, Dept={Dept}, Pozitie={Pozitie}, Activ={Activ}, Sort={Sort} {Dir}",
                request.PageNumber, request.PageSize, request.GlobalSearchText,
                request.FilterDepartament, request.FilterPozitie, request.FilterEsteActiv,
                request.SortColumn, request.SortDirection);

            var totalCount = await _repository.GetCountAsync(
                searchText: request.GlobalSearchText,
                departament: request.FilterDepartament,
                pozitie: request.FilterPozitie,
                esteActiv: request.FilterEsteActiv,
                cancellationToken: cancellationToken);

            if (totalCount == 0)
            {
                _logger.LogInformation("Nu au fost gasite rezultate");
                return PagedResult<PersonalMedicalListDto>.Success(
                    Enumerable.Empty<PersonalMedicalListDto>(),
                    request.PageNumber,
                    request.PageSize,
                    0);
            }

            var personalList = await _repository.GetAllAsync(
                pageNumber: request.PageNumber,
                pageSize: request.PageSize,
                searchText: request.GlobalSearchText,
                departament: request.FilterDepartament,
                pozitie: request.FilterPozitie,
                esteActiv: request.FilterEsteActiv,
                sortColumn: request.SortColumn,
                sortDirection: request.SortDirection,
                cancellationToken: cancellationToken);

            var dtoList = personalList.Select(p => new PersonalMedicalListDto
            {
                PersonalID = p.PersonalID,
                Nume = p.Nume,
                Prenume = p.Prenume,
                Specializare = p.Specializare,
                NumarLicenta = p.NumarLicenta,
                Telefon = p.Telefon,
                Email = p.Email,
                Departament = p.Departament,
                Pozitie = p.Pozitie,
                EsteActiv = p.EsteActiv,
                DataCreare = p.DataCreare
            }).ToList();

            _logger.LogInformation(
                "Lista personal medical obtinuta: {Count} din {Total} inregistrari",
                dtoList.Count, totalCount);

            return PagedResult<PersonalMedicalListDto>.Success(
                dtoList,
                request.PageNumber,
                request.PageSize,
                totalCount,
                $"S-au gasit {totalCount} persoane din personalul medical");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Eroare la obtinerea listei de personal medical");
            return PagedResult<PersonalMedicalListDto>.Failure($"Eroare la obtinerea listei: {ex.Message}");
        }
    }
}
