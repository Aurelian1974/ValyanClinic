using MediatR;
using ValyanClinic.Application.Common.Results;
using ValyanClinic.Application.Features.ProgramareManagement.DTOs;

namespace ValyanClinic.Application.Features.ProgramareManagement.Queries.GetProgramareList;

/// <summary>
/// Query pentru obținerea unei liste paginate de programări cu filtrare și sortare.
/// Implementează pattern-ul CQRS cu MediatR.
/// </summary>
public class GetProgramareListQuery : IRequest<PagedResult<ProgramareListDto>>
{
    /// <summary>
    /// Numărul paginii (începe de la 1).
    /// </summary>
    public int PageNumber { get; set; } = 1;

  /// <summary>
    /// Numărul de elemente per pagină.
    /// </summary>
    public int PageSize { get; set; } = 50;

    /// <summary>
    /// Text pentru căutare globală (pacient, doctor, observații).
    /// </summary>
    public string? GlobalSearchText { get; set; }

  /// <summary>
    /// Filtru după ID-ul medicului.
    /// </summary>
    public Guid? FilterDoctorID { get; set; }

    /// <summary>
    /// Filtru după ID-ul pacientului.
    /// </summary>
    public Guid? FilterPacientID { get; set; }

    /// <summary>
    /// Data de început pentru interval (implicit: azi).
    /// </summary>
    public DateTime? FilterDataStart { get; set; }

    /// <summary>
    /// Data de sfârșit pentru interval (implicit: azi + 30 zile).
    /// </summary>
    public DateTime? FilterDataEnd { get; set; }

    /// <summary>
    /// Filtru după status programare.
    /// </summary>
    public string? FilterStatus { get; set; }

    /// <summary>
    /// Filtru după tipul programării.
    /// </summary>
    public string? FilterTipProgramare { get; set; }

    /// <summary>
    /// Coloana pentru sortare.
  /// Valori permise: DataProgramare, OraInceput, Status, PacientNume, DoctorNume, DataCreare.
    /// </summary>
    public string SortColumn { get; set; } = "DataProgramare";

    /// <summary>
    /// Direcția sortării (ASC sau DESC).
    /// </summary>
    public string SortDirection { get; set; } = "ASC";
}
