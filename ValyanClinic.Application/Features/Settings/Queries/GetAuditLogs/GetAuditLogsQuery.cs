using MediatR;
using ValyanClinic.Application.Common.Results;

namespace ValyanClinic.Application.Features.Settings.Queries.GetAuditLogs;

public record GetAuditLogsQuery(
  int PageNumber = 1,
    int PageSize = 50,
    string? SearchText = null,
    Guid? UtilizatorID = null,
 string? Actiune = null,
    DateTime? DataStart = null,
  DateTime? DataEnd = null,
    string SortColumn = "DataActiune",
    string SortDirection = "DESC"
) : IRequest<Result<(List<AuditLogDto> Items, int TotalCount)>>;
