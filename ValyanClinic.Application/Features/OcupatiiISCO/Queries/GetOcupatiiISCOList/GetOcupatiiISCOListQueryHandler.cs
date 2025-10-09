using MediatR;
using Microsoft.Extensions.Logging;
using ValyanClinic.Application.Common.Results;
using ValyanClinic.Domain.Interfaces.Repositories;

namespace ValyanClinic.Application.Features.OcupatiiISCO.Queries.GetOcupatiiISCOList;

/// <summary>
/// Handler pentru obținerea listei de ocupații ISCO-08
/// Implementează paginare și filtrare avansată
/// </summary>
public class GetOcupatiiISCOListQueryHandler : IRequestHandler<GetOcupatiiISCOListQuery, PagedResult<OcupatieISCOListDto>>
{
    private readonly IOcupatieISCORepository _repository;
    private readonly ILogger<GetOcupatiiISCOListQueryHandler> _logger;

    public GetOcupatiiISCOListQueryHandler(
        IOcupatieISCORepository repository,
        ILogger<GetOcupatiiISCOListQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<PagedResult<OcupatieISCOListDto>> Handle(GetOcupatiiISCOListQuery request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Obținere listă ocupații ISCO-08: Page={Page}, Size={Size}, Search='{Search}', Nivel={Nivel}",
                request.PageNumber, request.PageSize, request.SearchText, request.NivelIerarhic);

            // Obține datele cu paginare
            var ocupatii = await _repository.GetAllAsync(
                pageNumber: request.PageNumber,
                pageSize: request.PageSize,
                searchText: request.SearchText,
                nivelIerarhic: request.NivelIerarhic,
                grupaMajora: request.GrupaMajora,
                esteActiv: request.EsteActiv,
                sortColumn: request.SortColumn,
                sortDirection: request.SortDirection,
                cancellationToken: cancellationToken);

            // Obține numărul total pentru paginare
            var totalCount = await _repository.GetCountAsync(
                searchText: request.SearchText,
                nivelIerarhic: request.NivelIerarhic,
                grupaMajora: request.GrupaMajora,
                esteActiv: request.EsteActiv,
                cancellationToken: cancellationToken);

            // Mapare la DTO
            var ocupatiiDto = ocupatii.Select(o => new OcupatieISCOListDto
            {
                Id = o.Id,
                CodISCO = o.CodISCO,
                DenumireOcupatie = o.DenumireOcupatie,
                DenumireOcupatieEN = o.DenumireOcupatieEN,
                NivelIerarhic = o.NivelIerarhic,
                CodParinte = o.CodParinte,
                GrupaMajora = o.GrupaMajora,
                GrupaMajoraDenumire = o.GrupaMajoraDenumire,
                Descriere = o.Descriere,
                EsteActiv = o.EsteActiv,
                DataCrearii = o.DataCrearii,
                CreatDe = o.CreatDe
            }).ToList();

            _logger.LogInformation("Lista ocupații ISCO-08 obținută cu succes: {Count} din {Total} înregistrări",
                ocupatiiDto.Count, totalCount);

            return PagedResult<OcupatieISCOListDto>.Success(
                items: ocupatiiDto,
                currentPage: request.PageNumber,
                pageSize: request.PageSize,
                totalCount: totalCount,
                message: $"Găsite {totalCount} ocupații ISCO-08");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Eroare la obținerea listei de ocupații ISCO-08");
            return PagedResult<OcupatieISCOListDto>.Failure(
                $"Eroare la încărcarea ocupațiilor ISCO-08: {ex.Message}");
        }
    }
}
