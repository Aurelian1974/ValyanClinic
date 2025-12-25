using Dapper;
using ValyanClinic.Domain.Entities;
using ValyanClinic.Domain.Interfaces.Repositories;
using ValyanClinic.Infrastructure.Data;
using Microsoft.Extensions.Logging;

namespace ValyanClinic.Infrastructure.Repositories;

/// <summary>
/// Implementare repository pentru gestionarea Rolurilor și Permisiunilor.
/// Folosește Dapper + Stored Procedures.
/// </summary>
public class RolRepository : BaseRepository, IRolRepository
{
    private readonly ILogger<RolRepository> _logger;

    public RolRepository(IDbConnectionFactory connectionFactory, ILogger<RolRepository> logger)
        : base(connectionFactory, logger)
    {
        _logger = logger;
    }

    #region Roluri CRUD

    public async Task<Rol?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var parameters = new { Id = id };
        var result = await QueryFirstOrDefaultAsync<RolDto>("sp_Roluri_GetById", parameters, cancellationToken);

        return result == null ? null : MapToEntity(result);
    }

    public async Task<IEnumerable<Rol>> GetAllAsync(
        int pageNumber = 1,
        int pageSize = 20,
        string? searchText = null,
        bool? esteActiv = null,
        string sortColumn = "OrdineAfisare",
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
        var data = await connection.QueryAsync<RolListDto>(
            "sp_Roluri_GetAll",
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
            "sp_Roluri_GetCount",
            parameters,
            commandType: System.Data.CommandType.StoredProcedure);

        return result;
    }

    public async Task<Guid> CreateAsync(Rol rol, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Creating Rol in repository: {Denumire}", rol.Denumire);

            var parameters = new
            {
                rol.Denumire,
                rol.Descriere,
                EsteActiv = rol.EsteActiv,
                OrdineAfisare = rol.OrdineAfisare,
                CreatDe = rol.CreatDe
            };

            var result = await QueryFirstOrDefaultAsync<RolDto>("sp_Roluri_Create", parameters, cancellationToken);

            if (result == null)
            {
                throw new InvalidOperationException("Nu s-a putut crea rolul.");
            }

            _logger.LogInformation("Rol created successfully: {Id}", result.RolID);
            return result.RolID;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating Rol: {Denumire}", rol.Denumire);
            throw;
        }
    }

    public async Task<bool> UpdateAsync(Rol rol, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Updating Rol: {Id}", rol.RolID);

            var parameters = new
            {
                Id = rol.RolID,
                rol.Denumire,
                rol.Descriere,
                EsteActiv = rol.EsteActiv,
                OrdineAfisare = rol.OrdineAfisare,
                ModificatDe = rol.ModificatDe
            };

            using var connection = _connectionFactory.CreateConnection();
            var result = await connection.QueryFirstOrDefaultAsync<int>(
                "sp_Roluri_Update",
                parameters,
                commandType: System.Data.CommandType.StoredProcedure);

            return result > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating Rol: {Id}", rol.RolID);
            throw;
        }
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Deleting Rol: {Id}", id);

            var parameters = new { Id = id };

            using var connection = _connectionFactory.CreateConnection();
            var result = await connection.QueryFirstOrDefaultAsync<int>(
                "sp_Roluri_Delete",
                parameters,
                commandType: System.Data.CommandType.StoredProcedure);

            return result > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting Rol: {Id}", id);
            throw;
        }
    }

    public async Task<bool> CheckUniqueAsync(
        string denumire,
        Guid? excludeId = null,
        CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.CreateConnection();
        
        var sql = excludeId.HasValue
            ? "SELECT COUNT(*) FROM dbo.Roluri WHERE [Denumire] = @Denumire AND [RolID] != @ExcludeId"
            : "SELECT COUNT(*) FROM dbo.Roluri WHERE [Denumire] = @Denumire";
        
        var count = await connection.ExecuteScalarAsync<int>(sql, new { Denumire = denumire, ExcludeId = excludeId });
        
        return count == 0;
    }

    #endregion

    #region Permisiuni per Rol

    public async Task<IEnumerable<string>> GetPermisiuniForRolAsync(
        Guid rolId,
        CancellationToken cancellationToken = default)
    {
        var parameters = new { RolID = rolId };

        using var connection = _connectionFactory.CreateConnection();
        var data = await connection.QueryAsync<RolPermisiuneDto>(
            "sp_Roluri_GetPermisiuni",
            parameters,
            commandType: System.Data.CommandType.StoredProcedure);

        return data.Where(p => p.EsteAcordat).Select(p => p.Permisiune);
    }

    public async Task<IEnumerable<string>> GetPermisiuniForRolByDenumireAsync(
        string denumireRol,
        CancellationToken cancellationToken = default)
    {
        var parameters = new { Denumire = denumireRol };

        using var connection = _connectionFactory.CreateConnection();
        var data = await connection.QueryAsync<string>(
            "sp_Roluri_GetByDenumire",
            parameters,
            commandType: System.Data.CommandType.StoredProcedure);

        return data;
    }

    public async Task<bool> SetPermisiuniForRolAsync(
        Guid rolId,
        IEnumerable<string> permisiuni,
        string? creatDe = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Setting permisiuni for Rol: {RolId}", rolId);

            var parameters = new
            {
                RolID = rolId,
                Permisiuni = string.Join(",", permisiuni),
                CreatDe = creatDe
            };

            using var connection = _connectionFactory.CreateConnection();
            var result = await connection.QueryFirstOrDefaultAsync<int>(
                "sp_Roluri_SetPermisiuni",
                parameters,
                commandType: System.Data.CommandType.StoredProcedure);

            return result >= 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting permisiuni for Rol: {RolId}", rolId);
            throw;
        }
    }

    public async Task<bool> AddPermisiuneToRolAsync(
        Guid rolId,
        string permisiune,
        string? creatDe = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();
            
            var sql = @"
                IF NOT EXISTS (SELECT 1 FROM dbo.RoluriPermisiuni WHERE [RolID] = @RolID AND [Permisiune] = @Permisiune)
                BEGIN
                    INSERT INTO dbo.RoluriPermisiuni ([RolID], [Permisiune], [Creat_De])
                    VALUES (@RolID, @Permisiune, @CreatDe)
                END";
            
            await connection.ExecuteAsync(sql, new { RolID = rolId, Permisiune = permisiune, CreatDe = creatDe });
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding permisiune {Permisiune} to Rol: {RolId}", permisiune, rolId);
            throw;
        }
    }

    public async Task<bool> RemovePermisiuneFromRolAsync(
        Guid rolId,
        string permisiune,
        CancellationToken cancellationToken = default)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();
            
            var sql = "DELETE FROM dbo.RoluriPermisiuni WHERE [RolID] = @RolID AND [Permisiune] = @Permisiune";
            
            var result = await connection.ExecuteAsync(sql, new { RolID = rolId, Permisiune = permisiune });
            return result > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing permisiune {Permisiune} from Rol: {RolId}", permisiune, rolId);
            throw;
        }
    }

    #endregion

    #region Definiții Permisiuni

    public async Task<IEnumerable<PermisiuneDefinitie>> GetAllPermisiuniDefinitiiAsync(
        CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.CreateConnection();
        var data = await connection.QueryAsync<PermisiuneDefinitieDto>(
            "sp_PermisiuniDefinitii_GetAll",
            commandType: System.Data.CommandType.StoredProcedure);

        return data.Select(MapToEntity);
    }

    public async Task<IEnumerable<string>> GetCategoriiPermisiuniAsync(
        CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.CreateConnection();
        var data = await connection.QueryAsync<string>(
            "sp_PermisiuniDefinitii_GetCategorii",
            commandType: System.Data.CommandType.StoredProcedure);

        return data;
    }

    #endregion

    #region Helpers

    public async Task<IEnumerable<(Guid Id, string Denumire)>> GetDropdownOptionsAsync(
        bool esteActiv = true,
        CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.CreateConnection();
        
        var sql = "SELECT [RolID] AS Id, [Denumire] FROM dbo.Roluri WHERE [Este_Activ] = @EsteActiv ORDER BY [Ordine_Afisare]";
        
        var data = await connection.QueryAsync<(Guid Id, string Denumire)>(sql, new { EsteActiv = esteActiv });
        return data;
    }

    #endregion

    #region Mapping

    private static Rol MapToEntity(RolDto dto)
    {
        return new Rol
        {
            RolID = dto.RolID,
            Denumire = dto.Denumire,
            Descriere = dto.Descriere,
            EsteActiv = dto.EsteActiv,
            EsteSistem = dto.EsteSistem,
            OrdineAfisare = dto.OrdineAfisare,
            DataCrearii = dto.DataCrearii,
            DataUltimeiModificari = dto.DataUltimeiModificari,
            CreatDe = dto.CreatDe,
            ModificatDe = dto.ModificatDe
        };
    }

    private static Rol MapToEntity(RolListDto dto)
    {
        return new Rol
        {
            RolID = dto.RolID,
            Denumire = dto.Denumire,
            Descriere = dto.Descriere,
            EsteActiv = dto.EsteActiv,
            EsteSistem = dto.EsteSistem,
            OrdineAfisare = dto.OrdineAfisare,
            DataCrearii = dto.DataCrearii,
            DataUltimeiModificari = dto.DataUltimeiModificari,
            CreatDe = dto.CreatDe,
            ModificatDe = dto.ModificatDe
        };
    }

    private static PermisiuneDefinitie MapToEntity(PermisiuneDefinitieDto dto)
    {
        return new PermisiuneDefinitie
        {
            PermisiuneDefinitieID = dto.PermisiuneDefinitieID,
            Cod = dto.Cod,
            Categorie = dto.Categorie,
            Denumire = dto.Denumire,
            Descriere = dto.Descriere,
            OrdineAfisare = dto.OrdineAfisare,
            EsteActiv = dto.EsteActiv
        };
    }

    #endregion

    #region Internal DTOs

    private class RolDto
    {
        public Guid RolID { get; set; }
        public string Denumire { get; set; } = string.Empty;
        public string? Descriere { get; set; }
        public bool EsteActiv { get; set; }
        public bool EsteSistem { get; set; }
        public int OrdineAfisare { get; set; }
        public DateTime DataCrearii { get; set; }
        public DateTime DataUltimeiModificari { get; set; }
        public string? CreatDe { get; set; }
        public string? ModificatDe { get; set; }
    }

    private class RolListDto : RolDto
    {
        public int NumarPermisiuni { get; set; }
        public int NumarUtilizatori { get; set; }
    }

    private class RolPermisiuneDto
    {
        public Guid RolPermisiuneID { get; set; }
        public Guid RolID { get; set; }
        public string Permisiune { get; set; } = string.Empty;
        public bool EsteAcordat { get; set; }
        public DateTime DataCrearii { get; set; }
        public string? CreatDe { get; set; }
        public string? Categorie { get; set; }
        public string? PermisiuneDenumire { get; set; }
        public string? PermisiuneDescriere { get; set; }
    }

    private class PermisiuneDefinitieDto
    {
        public Guid PermisiuneDefinitieID { get; set; }
        public string Cod { get; set; } = string.Empty;
        public string Categorie { get; set; } = string.Empty;
        public string Denumire { get; set; } = string.Empty;
        public string? Descriere { get; set; }
        public int OrdineAfisare { get; set; }
        public bool EsteActiv { get; set; }
    }

    #endregion
}
