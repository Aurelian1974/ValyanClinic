using Dapper;
using ValyanClinic.Domain.Entities;
using ValyanClinic.Domain.Interfaces.Repositories;
using ValyanClinic.Infrastructure.Data;

namespace ValyanClinic.Infrastructure.Repositories;

/// <summary>
/// Repository pentru Personal cu Dapper si Stored Procedures - ALINIAT CU DB REALA
/// </summary>
public class PersonalRepository : BaseRepository, IPersonalRepository
{
    public PersonalRepository(IDbConnectionFactory connectionFactory)
        : base(connectionFactory)
    {
    }

    public async Task<Personal?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var parameters = new { Id_Personal = id };
        return await QueryFirstOrDefaultAsync<Personal>("sp_Personal_GetById", parameters, cancellationToken);
    }

    public async Task<IEnumerable<Personal>> GetAllAsync(
        int pageNumber = 1,
        int pageSize = 20,
        string? searchText = null,
        string? departament = null,
        string? status = null,
        string? functie = null,
        string? judet = null,
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
            Status = status,
            Functie = functie,
            Judet = judet,
            SortColumn = sortColumn,
            SortDirection = sortDirection
        };

        // sp_Personal_GetAll returneaza 2 result sets: datele si count-ul
        using var connection = _connectionFactory.CreateConnection();
        using var multi = await connection.QueryMultipleAsync(
            "sp_Personal_GetAll",
            parameters,
            commandType: System.Data.CommandType.StoredProcedure);

        var data = await multi.ReadAsync<Personal>();
        // Skip count result set for now (used in GetCountAsync)
        return data;
    }

    public async Task<int> GetCountAsync(
        string? searchText = null,
        string? departament = null,
        string? status = null,
        string? functie = null,
        string? judet = null,
        CancellationToken cancellationToken = default)
    {
        var parameters = new
        {
            SearchText = searchText,
            Departament = departament,
            Status = status,
            Functie = functie,
            Judet = judet
        };

        using var connection = _connectionFactory.CreateConnection();

        // sp_Personal_GetCount returneaza un scalar
        var result = await connection.ExecuteScalarAsync<int>(
            "sp_Personal_GetCount",
            parameters,
            commandType: System.Data.CommandType.StoredProcedure);

        return result;
    }

    public async Task<(string name, int value, string iconClass, string colorClass)[]> GetStatisticsAsync(
        CancellationToken cancellationToken = default)
    {
        var results = await QueryAsync<StatisticDto>("sp_Personal_GetStatistics", cancellationToken: cancellationToken);
        return results.Select(r => (r.StatisticName, r.Value, r.IconClass, r.ColorClass)).ToArray();
    }

    public async Task<(string[] departamente, string[] functii, string[] judete)> GetDropdownOptionsAsync(
        CancellationToken cancellationToken = default)
    {
        // sp_Personal_GetDropdownOptions returneaza 3 result sets
        using var connection = _connectionFactory.CreateConnection();
        using var multi = await connection.QueryMultipleAsync(
            "sp_Personal_GetDropdownOptions",
            commandType: System.Data.CommandType.StoredProcedure);

        var departamente = (await multi.ReadAsync<DropdownOption>()).Select(d => d.Value).ToArray();
        var functii = (await multi.ReadAsync<DropdownOption>()).Select(f => f.Value).ToArray();
        var judete = (await multi.ReadAsync<DropdownOption>()).Select(j => j.Value).ToArray();

        return (departamente, functii, judete);
    }

    public async Task<Guid> CreateAsync(Personal personal, CancellationToken cancellationToken = default)
    {
        var parameters = new
        {
            personal.Cod_Angajat,
            personal.CNP,
            personal.Nume,
            personal.Prenume,
            personal.Nume_Anterior,
            personal.Data_Nasterii,
            personal.Locul_Nasterii,
            personal.Nationalitate,
            personal.Cetatenie,
            personal.Telefon_Personal,
            personal.Telefon_Serviciu,
            personal.Email_Personal,
            personal.Email_Serviciu,
            personal.Adresa_Domiciliu,
            personal.Judet_Domiciliu,
            personal.Oras_Domiciliu,
            personal.Cod_Postal_Domiciliu,
            personal.Adresa_Resedinta,
            personal.Judet_Resedinta,
            personal.Oras_Resedinta,
            personal.Cod_Postal_Resedinta,
            personal.Stare_Civila,
            personal.Functia,
            personal.Departament,
            personal.Serie_CI,
            personal.Numar_CI,
            personal.Eliberat_CI_De,
            personal.Data_Eliberare_CI,
            personal.Valabil_CI_Pana,
            personal.Status_Angajat,
            personal.Observatii,
            personal.Creat_De
        };

        // SP returneaza persoana creata
        var result = await QueryFirstOrDefaultAsync<Personal>("sp_Personal_Create", parameters, cancellationToken);
        return result?.Id_Personal ?? Guid.Empty;
    }

    public async Task<bool> UpdateAsync(Personal personal, CancellationToken cancellationToken = default)
    {
        var parameters = new
        {
            personal.Id_Personal,
            personal.Cod_Angajat,
            personal.CNP,
            personal.Nume,
            personal.Prenume,
            personal.Nume_Anterior,
            personal.Data_Nasterii,
            personal.Locul_Nasterii,
            personal.Nationalitate,
            personal.Cetatenie,
            personal.Telefon_Personal,
            personal.Telefon_Serviciu,
            personal.Email_Personal,
            personal.Email_Serviciu,
            personal.Adresa_Domiciliu,
            personal.Judet_Domiciliu,
            personal.Oras_Domiciliu,
            personal.Cod_Postal_Domiciliu,
            personal.Adresa_Resedinta,
            personal.Judet_Resedinta,
            personal.Oras_Resedinta,
            personal.Cod_Postal_Resedinta,
            personal.Stare_Civila,
            personal.Functia,
            personal.Departament,
            personal.Serie_CI,
            personal.Numar_CI,
            personal.Eliberat_CI_De,
            personal.Data_Eliberare_CI,
            personal.Valabil_CI_Pana,
            personal.Status_Angajat,
            personal.Observatii,
            personal.Modificat_De
        };

        // SP returneaza persoana actualizata
        var result = await QueryFirstOrDefaultAsync<Personal>("sp_Personal_Update", parameters, cancellationToken);
        return result != null;
    }

    public async Task<bool> DeleteAsync(Guid id, string modificatDe, CancellationToken cancellationToken = default)
    {
        var parameters = new
        {
            Id_Personal = id,
            Modificat_De = modificatDe
        };

        // SP face soft delete (seteaza Status_Angajat = 'Inactiv')
        var result = await QueryFirstOrDefaultAsync<SuccessResult>("sp_Personal_Delete", parameters, cancellationToken);
        return result?.Success == 1;
    }

    public async Task<(bool cnpExists, bool codAngajatExists)> CheckUniqueAsync(
        string? cnp = null,
        string? codAngajat = null,
        Guid? excludeId = null,
        CancellationToken cancellationToken = default)
    {
        var parameters = new
        {
            CNP = cnp,
            Cod_Angajat = codAngajat,
            ExcludeId = excludeId
        };

        var result = await QueryFirstOrDefaultAsync<UniquenessCheckResult>(
            "sp_Personal_CheckUnique",
            parameters,
            cancellationToken);

        return (result?.CNP_Exists == 1, result?.CodAngajat_Exists == 1);
    }

    // DTOs pentru maparea rezultatelor SP-urilor
    private class StatisticDto
    {
        public string StatisticName { get; set; } = string.Empty;
        public int Value { get; set; }
        public string IconClass { get; set; } = string.Empty;
        public string ColorClass { get; set; } = string.Empty;
    }

    private class DropdownOption
    {
        public string Value { get; set; } = string.Empty;
        public string Text { get; set; } = string.Empty;
    }

    private class UniquenessCheckResult
    {
        public int CNP_Exists { get; set; }
        public int CodAngajat_Exists { get; set; }
    }

    private class SuccessResult
    {
        public int Success { get; set; }
    }
}
