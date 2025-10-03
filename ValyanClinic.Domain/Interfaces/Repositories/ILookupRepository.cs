namespace ValyanClinic.Domain.Interfaces.Repositories;

/// <summary>
/// Repository interface pentru Lookup Tables (Departamente, Pozitii)
/// </summary>
public interface ILookupRepository
{
    Task<IEnumerable<(int Id, string Nume)>> GetDepartamenteAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<(int Id, string Nume)>> GetPozitiiMedicaleAsync(CancellationToken cancellationToken = default);
    Task<string?> GetDepartamentNameByIdAsync(int departamentId, CancellationToken cancellationToken = default);
    Task<string?> GetPozitieNameByIdAsync(int pozitieId, CancellationToken cancellationToken = default);
}
