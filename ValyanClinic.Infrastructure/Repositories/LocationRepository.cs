using ValyanClinic.Domain.Interfaces.Repositories;
using ValyanClinic.Domain.Interfaces.Data;
using ValyanClinic.Infrastructure.Data;

namespace ValyanClinic.Infrastructure.Repositories;

/// <summary>
/// Repository pentru Location (Judete si Localitati)
/// </summary>
public class LocationRepository : BaseRepository, ILocationRepository
{
    public LocationRepository(IDbConnectionFactory connectionFactory)
        : base(connectionFactory)
    {
    }

    public async Task<IEnumerable<(int Id, string Nume)>> GetJudeteAsync(CancellationToken cancellationToken = default)
    {
        var results = await QueryAsync<JudetDto>("sp_Location_GetJudete", cancellationToken: cancellationToken);
        return results.Select(x => (x.IdJudet, x.Nume));
    }

    public async Task<IEnumerable<(int Id, string Nume)>> GetLocalitatiByJudetIdAsync(
        int judetId,
        CancellationToken cancellationToken = default)
    {
        var parameters = new { JudetId = judetId };
        var results = await QueryAsync<LocalitateDto>("sp_Location_GetLocalitatiByJudetId", parameters, cancellationToken);
        return results.Select(x => (x.IdOras, x.Nume));
    }

    public async Task<string?> GetJudetNameByIdAsync(int judetId, CancellationToken cancellationToken = default)
    {
        var parameters = new { JudetId = judetId };
        return await ExecuteScalarAsync<string>("sp_Location_GetJudetNameById", parameters, cancellationToken);
    }

    public async Task<string?> GetLocalitateNameByIdAsync(int localitateId, CancellationToken cancellationToken = default)
    {
        var parameters = new { LocalitateId = localitateId };
        return await ExecuteScalarAsync<string>("sp_Location_GetLocalitateNameById", parameters, cancellationToken);
    }

    // DTOs pentru mapping cu coloanele din SP-uri
    private class JudetDto
    {
        public int IdJudet { get; set; }
        public string Nume { get; set; } = string.Empty;
    }

    private class LocalitateDto
    {
        public int IdOras { get; set; }
        public string Nume { get; set; } = string.Empty;
    }
}
