using ValyanClinic.Domain.Models;

namespace ValyanClinic.Infrastructure.Repositories;

/// <summary>
/// Repository interface pentru operatiile cu Personal
/// Foloseste Dapper si Stored Procedures pentru performance optimal
/// </summary>
public interface IPersonalRepository
{
    Task<(IEnumerable<Personal> Data, int TotalCount)> GetAllAsync(
        int pageNumber = 1,
        int pageSize = 20,
        string? searchText = null,
        string? departament = null,
        string? status = null,
        string sortColumn = "Nume",
        string sortDirection = "ASC");

    Task<Personal?> GetByIdAsync(Guid id);
    
    Task<Personal> CreateAsync(Personal personal, string creatDe);
    
    Task<Personal> UpdateAsync(Personal personal, string modificatDe);
    
    Task<bool> DeleteAsync(Guid id, string modificatDe);
    
    Task<(bool CnpExists, bool CodAngajatExists)> CheckUniqueAsync(
        string cnp, 
        string codAngajat, 
        Guid? excludeId = null);
    
    Task<(int TotalPersonal, int PersonalActiv, int PersonalInactiv)> GetStatisticsAsync();
    
    Task<IEnumerable<(string Value, string Text)>> GetDepartamenteAsync();
    
    Task<IEnumerable<(string Value, string Text)>> GetFunctiiAsync();
    
    Task<IEnumerable<(string Value, string Text)>> GetJudeteAsync();
}
