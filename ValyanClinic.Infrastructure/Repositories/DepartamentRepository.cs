using Dapper;
using ValyanClinic.Domain.Entities;
using ValyanClinic.Domain.Interfaces.Repositories;
using ValyanClinic.Infrastructure.Data;

namespace ValyanClinic.Infrastructure.Repositories;

public class DepartamentRepository : BaseRepository, IDepartamentRepository
{
    public DepartamentRepository(IDbConnectionFactory connectionFactory)
        : base(connectionFactory)
    {
    }

    public async Task<Departament?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var parameters = new { IdDepartament = id };
        var dto = await QueryFirstOrDefaultAsync<DepartamentDto>("sp_Departamente_GetById", parameters, cancellationToken);
        
        if (dto == null)
            return null;
        
        return new Departament
        {
            IdDepartament = dto.IdDepartament,
            IdTipDepartament = dto.IdTipDepartament,
            DenumireDepartament = dto.DenumireDepartament,
            DescriereDepartament = dto.DescriereDepartament,
            TipDepartament = dto.DenumireTipDepartament != null ? new TipDepartament 
            { 
                IdTipDepartament = dto.IdTipDepartament ?? Guid.Empty,
                DenumireTipDepartament = dto.DenumireTipDepartament 
            } : null
        };
    }

    public async Task<IEnumerable<Departament>> GetAllAsync(
        int pageNumber = 1,
        int pageSize = 20,
        string? searchText = null,
        Guid? idTipDepartament = null,
        string sortColumn = "DenumireDepartament",
        string sortDirection = "ASC",
        CancellationToken cancellationToken = default)
    {
        var parameters = new
        {
            PageNumber = pageNumber,
            PageSize = pageSize,
            SearchText = searchText,
            IdTipDepartament = idTipDepartament,
            SortColumn = sortColumn,
            SortDirection = sortDirection
        };
        
        using var connection = _connectionFactory.CreateConnection();
        var data = await connection.QueryAsync<DepartamentDto>(
            "sp_Departamente_GetAll",
            parameters,
            commandType: System.Data.CommandType.StoredProcedure);
        
        return data.Select(dto => new Departament
        {
            IdDepartament = dto.IdDepartament,
            IdTipDepartament = dto.IdTipDepartament,
            DenumireDepartament = dto.DenumireDepartament,
            DescriereDepartament = dto.DescriereDepartament,
            TipDepartament = dto.DenumireTipDepartament != null ? new TipDepartament 
            { 
                IdTipDepartament = dto.IdTipDepartament ?? Guid.Empty,
                DenumireTipDepartament = dto.DenumireTipDepartament 
            } : null
        });
    }

    public async Task<int> GetCountAsync(
        string? searchText = null,
        Guid? idTipDepartament = null,
        CancellationToken cancellationToken = default)
    {
        var parameters = new
        {
            SearchText = searchText,
            IdTipDepartament = idTipDepartament
        };
        
        using var connection = _connectionFactory.CreateConnection();
        var result = await connection.ExecuteScalarAsync<int>(
            "sp_Departamente_GetCount",
            parameters,
            commandType: System.Data.CommandType.StoredProcedure);
        
        return result;
    }

    public async Task<Guid> CreateAsync(Departament departament, CancellationToken cancellationToken = default)
    {
        var parameters = new
        {
            departament.IdTipDepartament,
            departament.DenumireDepartament,
            departament.DescriereDepartament
        };

        var result = await QueryFirstOrDefaultAsync<DepartamentDto>("sp_Departamente_Create", parameters, cancellationToken);
        return result?.IdDepartament ?? Guid.Empty;
    }

    public async Task<bool> UpdateAsync(Departament departament, CancellationToken cancellationToken = default)
    {
        var parameters = new
        {
            departament.IdDepartament,
            departament.IdTipDepartament,
            departament.DenumireDepartament,
            departament.DescriereDepartament
        };

        var result = await QueryFirstOrDefaultAsync<DepartamentDto>("sp_Departamente_Update", parameters, cancellationToken);
        return result != null;
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var parameters = new { IdDepartament = id };
        var result = await QueryFirstOrDefaultAsync<SuccessResult>("sp_Departamente_Delete", parameters, cancellationToken);
        return result?.Success == 1;
    }

    public async Task<bool> CheckUniqueAsync(
        string denumire,
        Guid? excludeId = null,
        CancellationToken cancellationToken = default)
    {
        var parameters = new
        {
            DenumireDepartament = denumire,
            ExcludeId = excludeId
        };
        
        var result = await QueryFirstOrDefaultAsync<UniquenessCheckResult>(
            "sp_Departamente_CheckUnique", 
            parameters, 
            cancellationToken);
        
        return result?.Denumire_Exists == 1;
    }

    private class DepartamentDto
    {
        public Guid IdDepartament { get; set; }
        public Guid? IdTipDepartament { get; set; }
        public string DenumireDepartament { get; set; } = string.Empty;
        public string? DescriereDepartament { get; set; }
        public string? DenumireTipDepartament { get; set; }
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
