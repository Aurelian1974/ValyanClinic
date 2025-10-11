using MediatR;
using ValyanClinic.Application.Common.Results;

namespace ValyanClinic.Application.Features.OcupatiiISCO.Queries.GetOcupatiiISCOList;

/// <summary>
/// Query pentru obținerea listei de ocupații ISCO-08 cu suport pentru paginare și filtrare
/// </summary>
public record GetOcupatiiISCOListQuery : IRequest<PagedResult<OcupatieISCOListDto>>
{
    /// <summary>
    /// Numărul paginii (1-indexed)
    /// </summary>
    public int PageNumber { get; init; } = 1;

    /// <summary>
    /// Dimensiunea paginii
    /// </summary>
    public int PageSize { get; init; } = 50;

    /// <summary>
    /// Text pentru căutare globală
    /// </summary>
    public string? SearchText { get; init; }

    /// <summary>
    /// Filtru pe nivelul ierarhic (1-4)
    /// </summary>
    public byte? NivelIerarhic { get; init; }

    /// <summary>
    /// Filtru pe grupa majoră
    /// </summary>
    public string? GrupaMajora { get; init; }

    /// <summary>
    /// Filtru pentru ocupații active/inactive
    /// </summary>
    public bool? EsteActiv { get; init; } = true;

    /// <summary>
    /// Coloana pentru sortare
    /// </summary>
    public string SortColumn { get; init; } = "Cod_ISCO";

    /// <summary>
    /// Direcția sortării (ASC/DESC)
    /// </summary>
    public string SortDirection { get; init; } = "ASC";
}
