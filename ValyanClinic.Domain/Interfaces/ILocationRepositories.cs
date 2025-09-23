using ValyanClinic.Domain.Models;

namespace ValyanClinic.Domain.Interfaces;

/// <summary>
/// Interface pentru operatiunile cu Judete
/// </summary>
public interface IJudetRepository
{
    Task<IEnumerable<Judet>> GetAllAsync();
    Task<Judet?> GetByIdAsync(int id);
    Task<Judet?> GetByCodAsync(string codJudet);
    Task<IEnumerable<Judet>> GetOrderedByNameAsync();
}

/// <summary>
/// Interface pentru operatiunile cu Localitati
/// </summary>
public interface ILocalitateRepository
{
    Task<IEnumerable<Localitate>> GetAllAsync();
    Task<Localitate?> GetByIdAsync(int id);
    Task<IEnumerable<Localitate>> GetByJudetIdAsync(int judetId);
    Task<IEnumerable<Localitate>> GetByJudetIdOrderedAsync(int judetId);
}
