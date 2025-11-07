using Dapper;
using System.Data;
using ValyanClinic.Domain.Entities.Settings;
using ValyanClinic.Domain.Interfaces.Repositories;
using ValyanClinic.Infrastructure.Data;

namespace ValyanClinic.Infrastructure.Repositories.Settings;

public class AuditLogRepository : IAuditLogRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public AuditLogRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<(IEnumerable<AuditLog> Items, int TotalCount)> GetAllAsync(
        int pageNumber, int pageSize, string? searchText, Guid? utilizatorId, string? actiune,
    DateTime? dataStart, DateTime? dataEnd, string sortColumn, string sortDirection,
  CancellationToken cancellationToken = default)
    {
    using var connection = _connectionFactory.CreateConnection();
  using var multi = await connection.QueryMultipleAsync(
         "SP_AuditLog_GetAll",
new
         {
        PageNumber = pageNumber, PageSize = pageSize, SearchText = searchText,
    UtilizatorID = utilizatorId, Actiune = actiune, DataStart = dataStart,
      DataEnd = dataEnd, SortColumn = sortColumn, SortDirection = sortDirection
            },
            commandType: CommandType.StoredProcedure);

    var items = await multi.ReadAsync<AuditLog>();
var count = await multi.ReadFirstOrDefaultAsync<int>();
        return (items, count);
    }

    public async Task<Guid> CreateAsync(string userName, string actiune, string? entitate, string? entitateId,
        string? valoareVeche, string? valoareNoua, string? adresaIp, string? userAgent,
  string? dispozitiv, string statusActiune, string? detaliiEroare, Guid? utilizatorId,
        CancellationToken cancellationToken = default)
    {
using var connection = _connectionFactory.CreateConnection();
        var result = await connection.QuerySingleOrDefaultAsync<AuditLog>(
  "SP_AuditLog_Create",
       new
         {
        UserName = userName, Actiune = actiune, Entitate = entitate, EntitateID = entitateId,
   ValoareVeche = valoareVeche, ValoareNoua = valoareNoua, AdresaIP = adresaIp,
       UserAgent = userAgent, Dispozitiv = dispozitiv, StatusActiune = statusActiune,
 DetaliiEroare = detaliiEroare, UtilizatorID = utilizatorId
   },
            commandType: CommandType.StoredProcedure);
     return result?.AuditID ?? Guid.Empty;
  }
}
