namespace ValyanClinic.Domain.Interfaces.Repositories;

/// <summary>
/// Repository interface pentru Locatii (Judete si Localitati)
/// </summary>
public interface ILocationRepository
{
    Task<IEnumerable<(int Id, string Nume)>> GetJudeteAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<(int Id, string Nume)>> GetLocalitatiByJudetIdAsync(int judetId, CancellationToken cancellationToken = default);
    Task<string?> GetJudetNameByIdAsync(int judetId, CancellationToken cancellationToken = default);
    Task<string?> GetLocalitateNameByIdAsync(int localitateId, CancellationToken cancellationToken = default);
}
