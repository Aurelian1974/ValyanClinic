using ValyanClinic.Domain.Models;
using ValyanClinic.Application.Models;

namespace ValyanClinic.Application.Services;

/// <summary>
/// Interface for Departament Medical Service
/// Manages medical departments data loaded from database (not enum-based)
/// </summary>
public interface IDepartamentMedicalService
{
    /// <summary>
    /// Gets all medical departments from database (WHERE Tip = 'Medical')
    /// </summary>
    Task<IEnumerable<DepartamentMedical>> GetAllDepartamenteMedicaleAsync();

    /// <summary>
    /// Gets medical categories (main departments)
    /// </summary>
    Task<IEnumerable<DepartamentMedical>> GetCategoriiMedicaleAsync();

    /// <summary>
    /// Gets medical specializations
    /// </summary>
    Task<IEnumerable<DepartamentMedical>> GetSpecializariMedicaleAsync();

    /// <summary>
    /// Gets medical subspecializations
    /// </summary>
    Task<IEnumerable<DepartamentMedical>> GetSubspecializariMedicaleAsync();

    /// <summary>
    /// Gets department by ID
    /// </summary>
    Task<DepartamentMedical?> GetDepartamentMedicalByIdAsync(Guid departamentId);
}
