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

            // If ColumnFilters are present, send them to the repository for DB-side filtering (preferred)
            if (request.ColumnFilters != null && request.ColumnFilters.Any())
            {
                var cfJson = System.Text.Json.JsonSerializer.Serialize(request.ColumnFilters);
                _logger.LogInformation("Applying advanced column filters (DB-side): {Filters}", cfJson);

                var totalCountFiltered = await _repository.GetCountAsync(
                    searchText: request.GlobalSearchText,
                    departament: request.FilterDepartament,
                    pozitie: request.FilterPozitie,
                    esteActiv: request.FilterEsteActiv,
                    columnFiltersJson: cfJson,
                    cancellationToken: cancellationToken);

                if (totalCountFiltered == 0)
                {
                    return PagedResult<PersonalMedicalListDto>.Success(
                        Enumerable.Empty<PersonalMedicalListDto>(),
                        request.PageNumber,
                        request.PageSize,
                        0);
                }

                var personalListFiltered = await _repository.GetAllAsync(
                    pageNumber: request.PageNumber,
                    pageSize: request.PageSize,
                    searchText: request.GlobalSearchText,
                    departament: request.FilterDepartament,
                    pozitie: request.FilterPozitie,
                    esteActiv: request.FilterEsteActiv,
                    sortColumn: request.SortColumn,
                    sortDirection: request.SortDirection,
                    columnFiltersJson: cfJson,
                    cancellationToken: cancellationToken);

                var dtoListFiltered = personalListFiltered.Select(p => new PersonalMedicalListDto
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

                // Fetch metadata/stats as usual
                var metadataFiltered = await _repository.GetFilterMetadataAsync(cancellationToken);
                var statisticsFiltered = await _repository.GetStatisticsAsync(cancellationToken);

                var appMetadataFiltered = new PersonalMedicalFilterMetadata
                {
                    AvailableDepartamente = metadataFiltered.AvailableDepartamente.Select(d => new FilterOptionDto { Value = d.Value, Text = d.Text }).ToList(),
                    AvailablePozitii = metadataFiltered.AvailablePozitii.Select(p => new FilterOptionDto { Value = p.Value, Text = p.Text }).ToList(),
                };

                var appStatisticsFiltered = new PersonalMedicalStatistics
                {
                    TotalActiv = statisticsFiltered.TotalActiv,
                    TotalInactiv = statisticsFiltered.TotalInactiv
                };

                var combinedMetaFiltered = new PersonalMedicalListMeta
                {
                    Filters = appMetadataFiltered,
                    Stats = appStatisticsFiltered
                };

                return PagedResult<PersonalMedicalListDto>.SuccessWithMeta(
                    dtoListFiltered,
                    request.PageNumber,
                    request.PageSize,
                    totalCountFiltered,
                    combinedMetaFiltered,
                    $"S-au gasit {totalCountFiltered} persoane (filtrate)");
            }

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

            // Fetch filter metadata and statistics (separate small queries)
            var metadata = await _repository.GetFilterMetadataAsync(cancellationToken);
            var statistics = await _repository.GetStatisticsAsync(cancellationToken);

            // Map domain metadata to application-level metadata DTO
            var appMetadata = new PersonalMedicalFilterMetadata
            {
                AvailableDepartamente = metadata.AvailableDepartamente.Select(d => new FilterOptionDto { Value = d.Value, Text = d.Text }).ToList(),
                AvailablePozitii = metadata.AvailablePozitii.Select(p => new FilterOptionDto { Value = p.Value, Text = p.Text }).ToList(),
            };

            var appStatistics = new PersonalMedicalStatistics
            {
                TotalActiv = statistics.TotalActiv,
                TotalInactiv = statistics.TotalInactiv
            };

            var combinedMeta = new PersonalMedicalListMeta
            {
                Filters = appMetadata,
                Stats = appStatistics
            };

            return PagedResult<PersonalMedicalListDto>.SuccessWithMeta(
                dtoList,
                request.PageNumber,
                request.PageSize,
                totalCount,
                combinedMeta,
                $"S-au gasit {totalCount} persoane din personalul medical");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Eroare la obtinerea listei de personal medical");
            return PagedResult<PersonalMedicalListDto>.Failure($"Eroare la obtinerea listei: {ex.Message}");
        }
    }
}
