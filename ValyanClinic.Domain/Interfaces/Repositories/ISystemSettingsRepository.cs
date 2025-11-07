using ValyanClinic.Domain.Entities.Settings;

namespace ValyanClinic.Domain.Interfaces.Repositories;

public interface ISystemSettingsRepository
{
    Task<SystemSetting?> GetByKeyAsync(string categorie, string cheie, CancellationToken cancellationToken = default);
    Task<IEnumerable<SystemSetting>> GetByCategoryAsync(string categorie, CancellationToken cancellationToken = default);
    Task<bool> UpdateAsync(
        string categorie, 
        string cheie, 
        string valoare, 
        string? descriere, // ✅ ADĂUGAT
        string modificatDe, 
        CancellationToken cancellationToken = default);
}
