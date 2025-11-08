using MediatR;
using ValyanClinic.Application.Common.Results;

namespace ValyanClinic.Application.Features.UserSessions.Queries.GetActiveSessions;

/// <summary>
/// Query pentru obținerea sesiunilor active
/// </summary>
public record GetActiveSessionsQuery : IRequest<Result<IEnumerable<ActiveSessionDto>>>
{
    /// <summary>
    /// Filtru pentru un utilizator specific (optional)
    /// </summary>
    public Guid? UtilizatorID { get; init; }
 
    /// <summary>
    /// Doar sesiuni care expiră în curând (< 15 min)
    /// </summary>
 public bool DoarExpiraInCurand { get; init; } = false;
    
  /// <summary>
    /// Sortare - coloana
    /// </summary>
  public string SortColumn { get; init; } = "DataUltimaActivitate";
    
/// <summary>
    /// Sortare - direcție (ASC/DESC)
    /// </summary>
    public string SortDirection { get; init; } = "DESC";
}
