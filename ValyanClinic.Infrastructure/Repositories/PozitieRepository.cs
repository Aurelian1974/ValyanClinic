using Dapper;
using ValyanClinic.Domain.Entities;
using ValyanClinic.Domain.Interfaces.Repositories;
using ValyanClinic.Infrastructure.Data;

namespace ValyanClinic.Infrastructure.Repositories;

public class PozitieRepository : BaseRepository, IPozitieRepository
{
    public PozitieRepository(IDbConnectionFactory connectionFactory)
        : base(connectionFactory)
    {
    }

    public async Task<Pozitie?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var parameters = new { Id = id };
        var result = await QueryFirstOrDefaultAsync<PozitieDto>("sp_Pozitii_GetById", parameters, cancellationToken);
        
        if (result == null)
            return null;
            
        return MapToEntity(result);
    }

    public async Task<IEnumerable<Pozitie>> GetAllAsync(
        int pageNumber = 1,
        int pageSize = 20,
        string? searchText = null,
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
            EsteActiv = esteActiv,
            SortColumn = sortColumn,
            SortDirection = sortDirection
        };
        
        using var connection = _connectionFactory.CreateConnection();
        var data = await connection.QueryAsync<PozitieDto>(
            "sp_Pozitii_GetAll",
            parameters,
            commandType: System.Data.CommandType.StoredProcedure);
        
        return data.Select(MapToEntity);
    }

    public async Task<int> GetCountAsync(
        string? searchText = null,
        bool? esteActiv = null,
        CancellationToken cancellationToken = default)
    {
        var parameters = new
        {
            SearchText = searchText,
            EsteActiv = esteActiv
        };
        
        using var connection = _connectionFactory.CreateConnection();
        var result = await connection.ExecuteScalarAsync<int>(
            "sp_Pozitii_GetCount",
            parameters,
            commandType: System.Data.CommandType.StoredProcedure);
        
        return result;
    }

    public async Task<Guid> CreateAsync(Pozitie pozitie, CancellationToken cancellationToken = default)
    {
        var parameters = new
        {
            pozitie.Denumire,
            pozitie.Descriere,
            EsteActiv = pozitie.EsteActiv,
            CreatDe = pozitie.CreatDe
        };

        var result = await QueryFirstOrDefaultAsync<PozitieDto>("sp_Pozitii_Create", parameters, cancellationToken);
        return result?.Id ?? Guid.Empty; // SIMPLU: returnează doar ID ca DepartamentRepository și SpecializareRepository
    }

    public async Task<bool> UpdateAsync(Pozitie pozitie, CancellationToken cancellationToken = default)
    {
        var parameters = new
        {
            pozitie.Id,
            pozitie.Denumire,
            pozitie.Descriere,
            EsteActiv = pozitie.EsteActiv,
            ModificatDe = pozitie.ModificatDe
        };

        var result = await QueryFirstOrDefaultAsync<PozitieDto>("sp_Pozitii_Update", parameters, cancellationToken);
        return result != null;
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var parameters = new { Id = id, ModificatDe = "System" };
        var result = await QueryFirstOrDefaultAsync<SuccessResult>("sp_Pozitii_Delete", parameters, cancellationToken);
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
            "sp_Pozitii_CheckUnique", 
            parameters, 
            cancellationToken);
        
        return result?.Denumire_Exists == 1;
    }

    private static Pozitie MapToEntity(PozitieDto dto)
    {
        return new Pozitie
        {
            Id = dto.Id,
            Denumire = dto.Denumire,
            Descriere = dto.Descriere,
            EsteActiv = dto.Este_Activ,
            DataCrearii = dto.Data_Crearii,
            DataUltimeiModificari = dto.Data_Ultimei_Modificari,
            CreatDe = dto.Creat_De,
            ModificatDe = dto.Modificat_De
        };
    }

    private class PozitieDto
    {
        public Guid Id { get; set; }
        public string Denumire { get; set; } = string.Empty;
        public string? Descriere { get; set; }
        public bool Este_Activ { get; set; }
        public DateTime Data_Crearii { get; set; }
        public DateTime Data_Ultimei_Modificari { get; set; }
        public string? Creat_De { get; set; }
        public string? Modificat_De { get; set; }
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
