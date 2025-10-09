using MediatR;
using Microsoft.Extensions.Logging;
using ValyanClinic.Application.Common.Results;
using ValyanClinic.Domain.Interfaces.Repositories;

namespace ValyanClinic.Application.Features.PersonalManagement.Queries.GetPersonalList;

/// <summary>
/// Handler pentru GetPersonalListQuery cu server-side paging, sorting si filtering
/// </summary>
public class GetPersonalListQueryHandler : IRequestHandler<GetPersonalListQuery, PagedResult<PersonalListDto>>
{
    private readonly IPersonalRepository _personalRepository;
    private readonly ILogger<GetPersonalListQueryHandler> _logger;

    public GetPersonalListQueryHandler(
        IPersonalRepository personalRepository,
        ILogger<GetPersonalListQueryHandler> logger)
    {
        _personalRepository = personalRepository;
        _logger = logger;
    }

    public async Task<PagedResult<PersonalListDto>> Handle(GetPersonalListQuery request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation(
                "Obtin lista de personal: Page={Page}, Size={Size}, Search={Search}, Status={Status}, Dept={Dept}, Functie={Functie}, Judet={Judet}, Sort={Sort} {Dir}",
                request.PageNumber, request.PageSize, request.GlobalSearchText, 
                request.FilterStatus, request.FilterDepartament, request.FilterFunctie, request.FilterJudet,
                request.SortColumn, request.SortDirection);

            // Get total count first (for pagination metadata)
            var totalCount = await _personalRepository.GetCountAsync(
                searchText: request.GlobalSearchText,
                departament: request.FilterDepartament,
                status: request.FilterStatus,
                functie: request.FilterFunctie,
                judet: request.FilterJudet,
                cancellationToken: cancellationToken);

            // If no results, return empty paged result
            if (totalCount == 0)
            {
                _logger.LogInformation("Nu au fost gasite rezultate");
                return PagedResult<PersonalListDto>.Success(
                    Enumerable.Empty<PersonalListDto>(),
                    request.PageNumber,
                    request.PageSize,
                    0);
            }

            // Get paged data with server-side filtering and sorting
            var personalList = await _personalRepository.GetAllAsync(
                pageNumber: request.PageNumber,
                pageSize: request.PageSize,
                searchText: request.GlobalSearchText,
                departament: request.FilterDepartament,
                status: request.FilterStatus,
                functie: request.FilterFunctie,
                judet: request.FilterJudet,
                sortColumn: request.SortColumn,
                sortDirection: request.SortDirection,
                cancellationToken: cancellationToken);
            
            // Map to DTOs
            var dtoList = personalList.Select(p => new PersonalListDto
            {
                Id_Personal = p.Id_Personal,
                Cod_Angajat = p.Cod_Angajat,
                Nume = p.Nume,
                Prenume = p.Prenume,
                CNP = p.CNP,
                Telefon_Personal = p.Telefon_Personal,
                Email_Personal = p.Email_Personal,
                Data_Nasterii = p.Data_Nasterii,
                Status_Angajat = p.Status_Angajat,
                Judet_Domiciliu = p.Judet_Domiciliu,
                Oras_Domiciliu = p.Oras_Domiciliu,
                Functia = p.Functia,
                Departament = p.Departament
            }).ToList();

            _logger.LogInformation(
                "Lista de personal obtinuta cu succes: {Count} din {Total} angajati",
                dtoList.Count, totalCount);

            return PagedResult<PersonalListDto>.Success(
                dtoList,
                request.PageNumber,
                request.PageSize,
                totalCount,
                $"S-au gasit {totalCount} angajati");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Eroare la obtinerea listei de personal");
            return PagedResult<PersonalListDto>.Failure($"Eroare la obtinerea listei de angajati: {ex.Message}");
        }
    }
}
