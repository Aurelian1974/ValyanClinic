using MediatR;
using Microsoft.Extensions.Logging;
using ValyanClinic.Application.Common.Results;
using ValyanClinic.Domain.Interfaces.Repositories;

namespace ValyanClinic.Application.Features.Settings.Queries.GetAuditLogs;

public class GetAuditLogsQueryHandler : IRequestHandler<GetAuditLogsQuery, Result<(List<AuditLogDto> Items, int TotalCount)>>
{
    private readonly IAuditLogRepository _repository;
    private readonly ILogger<GetAuditLogsQueryHandler> _logger;

    public GetAuditLogsQueryHandler(
   IAuditLogRepository repository,
   ILogger<GetAuditLogsQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<Result<(List<AuditLogDto> Items, int TotalCount)>> Handle(
     GetAuditLogsQuery request,
 CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Fetching audit logs: Page={Page}, Size={Size}",
       request.PageNumber, request.PageSize);

            var (logs, totalCount) = await _repository.GetAllAsync(
              request.PageNumber,
                  request.PageSize,
           request.SearchText,
               request.UtilizatorID,
                 request.Actiune,
              request.DataStart,
             request.DataEnd,
                request.SortColumn,
             request.SortDirection,
            cancellationToken);

            var dtos = logs.Select(log => new AuditLogDto
            {
                AuditID = log.AuditID,
                UtilizatorID = log.UtilizatorID,
                UserName = log.UserName,
                Actiune = log.Actiune,
                DataActiune = log.DataActiune,
                Entitate = log.Entitate,
                EntitateID = log.EntitateID,
                ValoareVeche = log.ValoareVeche,
                ValoareNoua = log.ValoareNoua,
                AdresaIP = log.AdresaIP,
                UserAgent = log.UserAgent,
                Dispozitiv = log.Dispozitiv,
                StatusActiune = log.StatusActiune,
                DetaliiEroare = log.DetaliiEroare
            }).ToList();

            _logger.LogInformation("Found {Count} audit logs (Total: {Total})", dtos.Count, totalCount);

            return Result<(List<AuditLogDto>, int)>.Success((dtos, totalCount));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching audit logs");
            return Result<(List<AuditLogDto>, int)>.Failure($"Error: {ex.Message}");
        }
    }
}
