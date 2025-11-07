using ValyanClinic.Domain.Entities.Settings;

namespace ValyanClinic.Domain.Interfaces.Repositories;

public interface IAuditLogRepository
{
    Task<(IEnumerable<AuditLog> Items, int TotalCount)> GetAllAsync(
 int pageNumber, int pageSize, string? searchText, Guid? utilizatorId, string? actiune,
        DateTime? dataStart, DateTime? dataEnd, string sortColumn, string sortDirection,
        CancellationToken cancellationToken = default);

    Task<Guid> CreateAsync(string userName, string actiune, string? entitate, string? entitateId,
string? valoareVeche, string? valoareNoua, string? adresaIp, string? userAgent,
   string? dispozitiv, string statusActiune, string? detaliiEroare, Guid? utilizatorId,
        CancellationToken cancellationToken = default);
}
