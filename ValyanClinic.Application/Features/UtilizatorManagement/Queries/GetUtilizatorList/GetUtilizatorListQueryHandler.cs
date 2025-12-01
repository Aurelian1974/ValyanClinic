using MediatR;
using Microsoft.Extensions.Logging;
using ValyanClinic.Application.Common.Results;
using ValyanClinic.Domain.Interfaces.Repositories;

namespace ValyanClinic.Application.Features.UtilizatorManagement.Queries.GetUtilizatorList;

public class GetUtilizatorListQueryHandler : IRequestHandler<GetUtilizatorListQuery, PagedResult<UtilizatorListDto>>
{
    private readonly IUtilizatorRepository _repository;
    private readonly ILogger<GetUtilizatorListQueryHandler> _logger;

    public GetUtilizatorListQueryHandler(
        IUtilizatorRepository repository,
   ILogger<GetUtilizatorListQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<PagedResult<UtilizatorListDto>> Handle(GetUtilizatorListQuery request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation(
          "Obtin lista utilizatori: Page={Page}, Size={Size}, Search={Search}, Rol={Rol}, Activ={Activ}, Sort={Sort} {Dir}",
                     request.PageNumber, request.PageSize, request.GlobalSearchText,
                   request.FilterRol, request.FilterEsteActiv,
               request.SortColumn, request.SortDirection);

            var (items, totalCount) = await _repository.GetAllAsync(
  pageNumber: request.PageNumber,
   pageSize: request.PageSize,
   searchText: request.GlobalSearchText,
       rol: request.FilterRol,
        esteActiv: request.FilterEsteActiv,
           sortColumn: request.SortColumn,
                sortDirection: request.SortDirection,
                cancellationToken: cancellationToken);

            var dtoList = items.Select(u => new UtilizatorListDto
            {
                UtilizatorID = u.UtilizatorID,
                PersonalMedicalID = u.PersonalMedicalID,
                Username = u.Username,
                Email = u.Email,
                Rol = u.Rol,
                EsteActiv = u.EsteActiv,
                DataCreare = u.DataCreare,
                DataUltimaAutentificare = u.DataUltimaAutentificare,
                NumarIncercariEsuate = u.NumarIncercariEsuate,
                DataBlocare = u.DataBlocare,
                NumeCompletPersonalMedical = u.PersonalMedical?.NumeComplet ?? string.Empty,
                Specializare = u.PersonalMedical?.Specializare,
                Departament = u.PersonalMedical?.Departament,
                Telefon = u.PersonalMedical?.Telefon
            }).ToList();

            _logger.LogInformation(
              "Lista utilizatori obtinuta: {Count} din {Total} inregistrari",
               dtoList.Count, totalCount);

            return PagedResult<UtilizatorListDto>.Success(
         dtoList,
         request.PageNumber,
           request.PageSize,
              totalCount,
           $"S-au gasit {totalCount} utilizatori");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Eroare la obtinerea listei de utilizatori");
            return PagedResult<UtilizatorListDto>.Failure($"Eroare la obtinerea listei: {ex.Message}");
        }
    }
}
