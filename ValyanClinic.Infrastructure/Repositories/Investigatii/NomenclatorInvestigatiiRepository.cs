using Dapper;
using Microsoft.Extensions.Logging;
using ValyanClinic.Domain.Entities.Investigatii;
using ValyanClinic.Domain.Interfaces.Repositories;
using ValyanClinic.Infrastructure.Data;

namespace ValyanClinic.Infrastructure.Repositories.Investigatii;

/// <summary>
/// Repository pentru nomenclatorul de Investigații Imagistice
/// </summary>
public class NomenclatorInvestigatiiImagisticeRepository : INomenclatorInvestigatiiImagisticeRepository
{
    private readonly IDbConnectionFactory _connectionFactory;
    private readonly ILogger<NomenclatorInvestigatiiImagisticeRepository> _logger;

    public NomenclatorInvestigatiiImagisticeRepository(
        IDbConnectionFactory connectionFactory,
        ILogger<NomenclatorInvestigatiiImagisticeRepository> logger)
    {
        _connectionFactory = connectionFactory;
        _logger = logger;
    }

    public async Task<IEnumerable<NomenclatorInvestigatieImagistica>> GetAllActiveAsync(CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.CreateConnection();

        const string sql = @"
            SELECT Id, Cod, Denumire, Categorie, Descriere, 
                   IsActive AS EsteActiv, OrdineAfisare AS Ordine, DataCreare, DataUltimeiModificari AS DataModificare
            FROM dbo.NomenclatorInvestigatiiImagistice
            WHERE IsActive = 1
            ORDER BY OrdineAfisare, Denumire";

        var result = await connection.QueryAsync<NomenclatorInvestigatieImagistica>(sql);
        _logger.LogDebug("Obținute {Count} investigații imagistice active", result.Count());
        return result;
    }

    public async Task<IEnumerable<NomenclatorInvestigatieImagistica>> GetByCategorieAsync(string categorie, CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.CreateConnection();

        const string sql = @"
            SELECT Id, Cod, Denumire, Categorie, Descriere, 
                   IsActive AS EsteActiv, OrdineAfisare AS Ordine, DataCreare, DataUltimeiModificari AS DataModificare
            FROM dbo.NomenclatorInvestigatiiImagistice
            WHERE IsActive = 1 AND Categorie = @Categorie
            ORDER BY OrdineAfisare, Denumire";

        return await connection.QueryAsync<NomenclatorInvestigatieImagistica>(sql, new { Categorie = categorie });
    }

    public async Task<IEnumerable<NomenclatorInvestigatieImagistica>> SearchAsync(string searchTerm, CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.CreateConnection();

        const string sql = @"
            SELECT Id, Cod, Denumire, Categorie, Descriere, 
                   IsActive AS EsteActiv, OrdineAfisare AS Ordine, DataCreare, DataUltimeiModificari AS DataModificare
            FROM dbo.NomenclatorInvestigatiiImagistice
            WHERE IsActive = 1 
              AND (Denumire LIKE @SearchTerm OR Cod LIKE @SearchTerm OR Descriere LIKE @SearchTerm)
            ORDER BY OrdineAfisare, Denumire";

        return await connection.QueryAsync<NomenclatorInvestigatieImagistica>(sql, new { SearchTerm = $"%{searchTerm}%" });
    }

    public async Task<NomenclatorInvestigatieImagistica?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.CreateConnection();

        const string sql = @"
            SELECT Id, Cod, Denumire, Categorie, Descriere, 
                   IsActive AS EsteActiv, OrdineAfisare AS Ordine, DataCreare, DataUltimeiModificari AS DataModificare
            FROM dbo.NomenclatorInvestigatiiImagistice
            WHERE Id = @Id";

        return await connection.QueryFirstOrDefaultAsync<NomenclatorInvestigatieImagistica>(sql, new { Id = id });
    }
}

/// <summary>
/// Repository pentru nomenclatorul de Explorări Funcționale
/// </summary>
public class NomenclatorExplorariFuncRepository : INomenclatorExplorariFuncRepository
{
    private readonly IDbConnectionFactory _connectionFactory;
    private readonly ILogger<NomenclatorExplorariFuncRepository> _logger;

    public NomenclatorExplorariFuncRepository(
        IDbConnectionFactory connectionFactory,
        ILogger<NomenclatorExplorariFuncRepository> logger)
    {
        _connectionFactory = connectionFactory;
        _logger = logger;
    }

    public async Task<IEnumerable<NomenclatorExplorareFunc>> GetAllActiveAsync(CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.CreateConnection();

        const string sql = @"
            SELECT Id, Cod, Denumire, Categorie, Descriere, 
                   IsActive AS EsteActiv, OrdineAfisare AS Ordine, DataCreare, DataUltimeiModificari AS DataModificare
            FROM dbo.NomenclatorExplorariFunc
            WHERE IsActive = 1
            ORDER BY OrdineAfisare, Denumire";

        var result = await connection.QueryAsync<NomenclatorExplorareFunc>(sql);
        _logger.LogDebug("Obținute {Count} explorări funcționale active", result.Count());
        return result;
    }

    public async Task<IEnumerable<NomenclatorExplorareFunc>> GetByCategorieAsync(string categorie, CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.CreateConnection();

        const string sql = @"
            SELECT Id, Cod, Denumire, Categorie, Descriere, 
                   IsActive AS EsteActiv, OrdineAfisare AS Ordine, DataCreare, DataUltimeiModificari AS DataModificare
            FROM dbo.NomenclatorExplorariFunc
            WHERE IsActive = 1 AND Categorie = @Categorie
            ORDER BY OrdineAfisare, Denumire";

        return await connection.QueryAsync<NomenclatorExplorareFunc>(sql, new { Categorie = categorie });
    }

    public async Task<IEnumerable<NomenclatorExplorareFunc>> SearchAsync(string searchTerm, CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.CreateConnection();

        const string sql = @"
            SELECT Id, Cod, Denumire, Categorie, Descriere, 
                   IsActive AS EsteActiv, OrdineAfisare AS Ordine, DataCreare, DataUltimeiModificari AS DataModificare
            FROM dbo.NomenclatorExplorariFunc
            WHERE IsActive = 1 
              AND (Denumire LIKE @SearchTerm OR Cod LIKE @SearchTerm OR Descriere LIKE @SearchTerm)
            ORDER BY OrdineAfisare, Denumire";

        return await connection.QueryAsync<NomenclatorExplorareFunc>(sql, new { SearchTerm = $"%{searchTerm}%" });
    }

    public async Task<NomenclatorExplorareFunc?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.CreateConnection();

        const string sql = @"
            SELECT Id, Cod, Denumire, Categorie, Descriere, 
                   IsActive AS EsteActiv, OrdineAfisare AS Ordine, DataCreare, DataUltimeiModificari AS DataModificare
            FROM dbo.NomenclatorExplorariFunc
            WHERE Id = @Id";

        return await connection.QueryFirstOrDefaultAsync<NomenclatorExplorareFunc>(sql, new { Id = id });
    }
}

/// <summary>
/// Repository pentru nomenclatorul de Endoscopii
/// </summary>
public class NomenclatorEndoscopiiRepository : INomenclatorEndoscopiiRepository
{
    private readonly IDbConnectionFactory _connectionFactory;
    private readonly ILogger<NomenclatorEndoscopiiRepository> _logger;

    public NomenclatorEndoscopiiRepository(
        IDbConnectionFactory connectionFactory,
        ILogger<NomenclatorEndoscopiiRepository> logger)
    {
        _connectionFactory = connectionFactory;
        _logger = logger;
    }

    public async Task<IEnumerable<NomenclatorEndoscopie>> GetAllActiveAsync(CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.CreateConnection();

        const string sql = @"
            SELECT Id, Cod, Denumire, Categorie, Descriere, 
                   IsActive AS EsteActiv, OrdineAfisare AS Ordine, DataCreare, DataUltimeiModificari AS DataModificare
            FROM dbo.NomenclatorEndoscopii
            WHERE IsActive = 1
            ORDER BY OrdineAfisare, Denumire";

        var result = await connection.QueryAsync<NomenclatorEndoscopie>(sql);
        _logger.LogDebug("Obținute {Count} endoscopii active", result.Count());
        return result;
    }

    public async Task<IEnumerable<NomenclatorEndoscopie>> GetByCategorieAsync(string categorie, CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.CreateConnection();

        const string sql = @"
            SELECT Id, Cod, Denumire, Categorie, Descriere, 
                   IsActive AS EsteActiv, OrdineAfisare AS Ordine, DataCreare, DataUltimeiModificari AS DataModificare
            FROM dbo.NomenclatorEndoscopii
            WHERE IsActive = 1 AND Categorie = @Categorie
            ORDER BY OrdineAfisare, Denumire";

        return await connection.QueryAsync<NomenclatorEndoscopie>(sql, new { Categorie = categorie });
    }

    public async Task<IEnumerable<NomenclatorEndoscopie>> SearchAsync(string searchTerm, CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.CreateConnection();

        const string sql = @"
            SELECT Id, Cod, Denumire, Categorie, Descriere, 
                   IsActive AS EsteActiv, OrdineAfisare AS Ordine, DataCreare, DataUltimeiModificari AS DataModificare
            FROM dbo.NomenclatorEndoscopii
            WHERE IsActive = 1 
              AND (Denumire LIKE @SearchTerm OR Cod LIKE @SearchTerm OR Descriere LIKE @SearchTerm)
            ORDER BY OrdineAfisare, Denumire";

        return await connection.QueryAsync<NomenclatorEndoscopie>(sql, new { SearchTerm = $"%{searchTerm}%" });
    }

    public async Task<NomenclatorEndoscopie?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.CreateConnection();

        const string sql = @"
            SELECT Id, Cod, Denumire, Categorie, Descriere, 
                   IsActive AS EsteActiv, OrdineAfisare AS Ordine, DataCreare, DataUltimeiModificari AS DataModificare
            FROM dbo.NomenclatorEndoscopii
            WHERE Id = @Id";

        return await connection.QueryFirstOrDefaultAsync<NomenclatorEndoscopie>(sql, new { Id = id });
    }
}
