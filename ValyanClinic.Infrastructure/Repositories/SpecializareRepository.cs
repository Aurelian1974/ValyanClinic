using Dapper;
using ValyanClinic.Domain.Entities;
using ValyanClinic.Domain.Interfaces.Repositories;
using ValyanClinic.Infrastructure.Data;

namespace ValyanClinic.Infrastructure.Repositories;

public class SpecializareRepository : BaseRepository, ISpecializareRepository
{
    public SpecializareRepository(IDbConnectionFactory connectionFactory)
        : base(connectionFactory)
    {
    }

    public async Task<Specializare?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var parameters = new { Id = id };
        var result = await QueryFirstOrDefaultAsync<SpecializareDto>("sp_Specializari_GetById", parameters, cancellationToken);
        
        if (result == null)
            return null;
            
        return MapToEntity(result);
    }

    public async Task<IEnumerable<Specializare>> GetAllAsync(
        int pageNumber = 1,
        int pageSize = 20,
        string? searchText = null,
        string? categorie = null,
        bool? esteActiv = null,
        string sortColumn = "Denumire",
        string sortDirection = "ASC",
        CancellationToken cancellationToken = default)
    {
        var parameters = new
        {
            PageNumber = pageNumber,
            PageSize = pageSize,
            SearchText = searchText,
            Categorie = categorie,
            EsteActiv = esteActiv,
            SortColumn = sortColumn,
            SortDirection = sortDirection
        };
        
        using var connection = _connectionFactory.CreateConnection();
        var data = await connection.QueryAsync<SpecializareDto>(
            "sp_Specializari_GetAll",
            parameters,
            commandType: System.Data.CommandType.StoredProcedure);
        
        return data.Select(MapToEntity);
    }

    public async Task<int> GetCountAsync(
        string? searchText = null,
        string? categorie = null,
        bool? esteActiv = null,
        CancellationToken cancellationToken = default)
    {
        var parameters = new
        {
            SearchText = searchText,
            Categorie = categorie,
            EsteActiv = esteActiv
        };
        
        using var connection = _connectionFactory.CreateConnection();
        var result = await connection.ExecuteScalarAsync<int>(
            "sp_Specializari_GetCount",
            parameters,
            commandType: System.Data.CommandType.StoredProcedure);
        
        return result;
    }

    public async Task<Specializare?> CreateAsync(Specializare specializare, CancellationToken cancellationToken = default)
    {
        var parameters = new
        {
            specializare.Denumire,
            specializare.Categorie,
            specializare.Descriere,
            EsteActiv = specializare.EsteActiv,
            CreatDe = specializare.CreatDe
        };

        var result = await QueryFirstOrDefaultAsync<SpecializareDto>("sp_Specializari_Create", parameters, cancellationToken);
        return result != null ? MapToEntity(result) : null;
    }

    public async Task<bool> UpdateAsync(Specializare specializare, CancellationToken cancellationToken = default)
    {
        var parameters = new
        {
            specializare.Id,
            specializare.Denumire,
            specializare.Categorie,
            specializare.Descriere,
            EsteActiv = specializare.EsteActiv,
            ModificatDe = specializare.ModificatDe
        };

        var result = await QueryFirstOrDefaultAsync<SpecializareDto>("sp_Specializari_Update", parameters, cancellationToken);
        return result != null;
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var parameters = new { Id = id, ModificatDe = "System" };
        var result = await QueryFirstOrDefaultAsync<SuccessResult>("sp_Specializari_Delete", parameters, cancellationToken);
        return result?.Success == 1;
    }

    public async Task<bool> CheckUniqueAsync(
        string denumire,
        Guid? excludeId = null,
        CancellationToken cancellationToken = default)
    {
        var parameters = new
        {
            Denumire = denumire,
            ExcludeId = excludeId
        };
        
        var result = await QueryFirstOrDefaultAsync<UniquenessCheckResult>(
            "sp_Specializari_CheckUnique", 
            parameters, 
            cancellationToken);
        
        return result?.Denumire_Exists == 1;
    }

    public async Task<IEnumerable<string>> GetCategoriiAsync(CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.CreateConnection();
        var results = await connection.QueryAsync<CategorieDto>(
            "sp_Specializari_GetCategorii",
            commandType: System.Data.CommandType.StoredProcedure);
        
        return results.Select(c => c.Value);
    }

    private static Specializare MapToEntity(SpecializareDto dto)
    {
        return new Specializare
        {
            Id = dto.Id,
            Denumire = dto.Denumire,
            Categorie = dto.Categorie,
            Descriere = dto.Descriere,
            EsteActiv = dto.Este_Activ,
            DataCrearii = dto.Data_Crearii,
            DataUltimeiModificari = dto.Data_Ultimei_Modificari,
            CreatDe = dto.Creat_De,
            ModificatDe = dto.Modificat_De
        };
    }

    private class SpecializareDto
    {
        public Guid Id { get; set; }
        public string Denumire { get; set; } = string.Empty;
        public string? Categorie { get; set; }
        public string? Descriere { get; set; }
        public bool Este_Activ { get; set; }
        public DateTime Data_Crearii { get; set; }
        public DateTime Data_Ultimei_Modificari { get; set; }
        public string? Creat_De { get; set; }
        public string? Modificat_De { get; set; }
    }

    private class CategorieDto
    {
        public string Value { get; set; } = string.Empty;
        public string Text { get; set; } = string.Empty;
    }

    private class UniquenessCheckResult
    {
        public int Denumire_Exists { get; set; }
    }

    private class SuccessResult
    {
        public int Success { get; set; }
    }
}
