using ValyanClinic.Domain.Interfaces.Repositories;
using ValyanClinic.Infrastructure.Data;

namespace ValyanClinic.Infrastructure.Repositories;

/// <summary>
/// Repository pentru Lookup Tables (Departamente, Pozitii Medicale)
/// </summary>
public class LookupRepository : BaseRepository, ILookupRepository
{
    public LookupRepository(IDbConnectionFactory connectionFactory)
        : base(connectionFactory)
    {
    }

    public async Task<IEnumerable<(int Id, string Nume)>> GetDepartamenteAsync(CancellationToken cancellationToken = default)
    {
        var results = await QueryAsync<LookupDto>("sp_Lookup_GetDepartamente", cancellationToken: cancellationToken);
        return results.Select(x => (x.Id, x.Nume));
    }

    public async Task<IEnumerable<(int Id, string Nume)>> GetPozitiiMedicaleAsync(CancellationToken cancellationToken = default)
    {
        var results = await QueryAsync<LookupDto>("sp_Lookup_GetPozitiiMedicale", cancellationToken: cancellationToken);
        return results.Select(x => (x.Id, x.Nume));
    }

    public async Task<string?> GetDepartamentNameByIdAsync(int departamentId, CancellationToken cancellationToken = default)
    {
        var parameters = new { DepartamentId = departamentId };
        return await ExecuteScalarAsync<string>("sp_Lookup_GetDepartamentNameById", parameters, cancellationToken);
    }

    public async Task<string?> GetPozitieNameByIdAsync(int pozitieId, CancellationToken cancellationToken = default)
    {
        var parameters = new { PozitieId = pozitieId };
        return await ExecuteScalarAsync<string>("sp_Lookup_GetPozitieNameById", parameters, cancellationToken);
    }

    // DTO pentru mapping
    private class LookupDto
    {
        public int Id { get; set; }
        public string Nume { get; set; } = string.Empty;
    }
}
