using Dapper;
using ValyanClinic.Domain.Entities;
using ValyanClinic.Domain.Interfaces.Repositories;
using ValyanClinic.Infrastructure.Data;

namespace ValyanClinic.Infrastructure.Repositories;

/// <summary>
/// Repository pentru PersonalMedical cu Dapper si Stored Procedures - ALINIAT CU DB REALA
/// </summary>
public class PersonalMedicalRepository : BaseRepository, IPersonalMedicalRepository
{
    public PersonalMedicalRepository(IDbConnectionFactory connectionFactory)
        : base(connectionFactory)
    {
    }

    public async Task<PersonalMedical?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var parameters = new { PersonalID = id };
        return await QueryFirstOrDefaultAsync<PersonalMedical>("sp_PersonalMedical_GetById", parameters, cancellationToken);
    }

    public async Task<PersonalMedical?> GetByPersonalIdAsync(Guid personalId, CancellationToken cancellationToken = default)
    {
        // PersonalID este PK, deci e aceeasi metoda
        return await GetByIdAsync(personalId, cancellationToken);
    }

    public async Task<IEnumerable<PersonalMedical>> GetAllAsync(
        int pageNumber = 1,
        int pageSize = 20,
        string? searchText = null,
        string? departament = null,
        string? pozitie = null,
        bool? esteActiv = null,
        string sortColumn = "Nume",
        string sortDirection = "ASC",
        CancellationToken cancellationToken = default)
    {
        var parameters = new
        {
            PageNumber = pageNumber,
            PageSize = pageSize,
            SearchText = searchText,
            Departament = departament,
            Pozitie = pozitie,
            EsteActiv = esteActiv,
            SortColumn = sortColumn,
            SortDirection = sortDirection
        };
        
        // SP returneaza 2 result sets: datele si count-ul
        using var connection = _connectionFactory.CreateConnection();
        using var multi = await connection.QueryMultipleAsync(
            "sp_PersonalMedical_GetAll",
            parameters,
            commandType: System.Data.CommandType.StoredProcedure);
        
        var data = await multi.ReadAsync<PersonalMedical>();
        // Skip count result set
        return data;
    }

    public async Task<int> GetCountAsync(
        string? searchText = null,
        string? departament = null,
        string? pozitie = null,
        bool? esteActiv = null,
        CancellationToken cancellationToken = default)
    {
        var parameters = new
        {
            PageNumber = 1,
            PageSize = 1,
            SearchText = searchText,
            Departament = departament,
            Pozitie = pozitie,
            EsteActiv = esteActiv,
            SortColumn = "Nume",
            SortDirection = "ASC"
        };
        
        using var connection = _connectionFactory.CreateConnection();
        using var multi = await connection.QueryMultipleAsync(
            "sp_PersonalMedical_GetAll",
            parameters,
            commandType: System.Data.CommandType.StoredProcedure);
        
        await multi.ReadAsync<PersonalMedical>();
        
        var countResult = await multi.ReadFirstOrDefaultAsync<CountResult>();
        return countResult?.TotalCount ?? 0;
    }

    public async Task<Guid> CreateAsync(PersonalMedical personalMedical, CancellationToken cancellationToken = default)
    {
        var parameters = new
        {
            personalMedical.Nume,
            personalMedical.Prenume,
            personalMedical.Specializare,
            personalMedical.NumarLicenta,
            personalMedical.Telefon,
            personalMedical.Email,
            personalMedical.Departament,
            personalMedical.Pozitie,
            personalMedical.EsteActiv,
            personalMedical.CategorieID,
            personalMedical.SpecializareID,
            personalMedical.SubspecializareID
        };

        // SP returneaza personal medical creat
        var result = await QueryFirstOrDefaultAsync<PersonalMedical>("sp_PersonalMedical_Create", parameters, cancellationToken);
        return result?.PersonalID ?? Guid.Empty;
    }

    public async Task<bool> UpdateAsync(PersonalMedical personalMedical, CancellationToken cancellationToken = default)
    {
        var parameters = new
        {
            personalMedical.PersonalID,
            personalMedical.Nume,
            personalMedical.Prenume,
            personalMedical.Specializare,
            personalMedical.NumarLicenta,
            personalMedical.Telefon,
            personalMedical.Email,
            personalMedical.Departament,
            personalMedical.Pozitie,
            personalMedical.EsteActiv,
            personalMedical.CategorieID,
            personalMedical.SpecializareID,
            personalMedical.SubspecializareID
        };

        // SP returneaza personal medical actualizat
        var result = await QueryFirstOrDefaultAsync<PersonalMedical>("sp_PersonalMedical_Update", parameters, cancellationToken);
        return result != null;
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var parameters = new { PersonalID = id };
        
        // SP face soft delete (seteaza EsteActiv = 0)
        var result = await QueryFirstOrDefaultAsync<SuccessResult>("sp_PersonalMedical_Delete", parameters, cancellationToken);
        return result?.Success == 1;
    }

    // DTO pentru maparea rezultatului delete
    private class SuccessResult
    {
        public int Success { get; set; }
    }

    private class CountResult
    {
        public int TotalCount { get; set; }
    }
}
