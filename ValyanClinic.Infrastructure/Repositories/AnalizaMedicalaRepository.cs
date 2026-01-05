using Dapper;
using Microsoft.Extensions.Logging;
using System.Data;
using ValyanClinic.Domain.Entities;
using ValyanClinic.Domain.Interfaces.Repositories;
using ValyanClinic.Infrastructure.Data;

namespace ValyanClinic.Infrastructure.Repositories;

/// <summary>
/// Repository pentru nomenclator analize medicale
/// Folosește stored procedures pentru queries optimizate
/// </summary>
public class AnalizaMedicalaRepository : IAnalizaMedicalaRepository
{
    private readonly IDbConnectionFactory _connectionFactory;
    private readonly ILogger<AnalizaMedicalaRepository> _logger;

    public AnalizaMedicalaRepository(
        IDbConnectionFactory connectionFactory,
        ILogger<AnalizaMedicalaRepository> logger)
    {
        _connectionFactory = connectionFactory;
        _logger = logger;
    }

    public async Task<(IEnumerable<AnalizaMedicala> Items, int TotalCount)> SearchAsync(
        string? searchTerm,
        Guid? categorieId,
        Guid? laboratorId,
        bool doarActive = true,
        int pageNumber = 1,
        int pageSize = 50,
        CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.CreateConnection();
        
        var parameters = new DynamicParameters();
        parameters.Add("@SearchTerm", searchTerm);
        parameters.Add("@CategorieID", categorieId);
        parameters.Add("@LaboratorID", laboratorId);
        parameters.Add("@DoarActive", doarActive);
        parameters.Add("@PageNumber", pageNumber);
        parameters.Add("@PageSize", pageSize);

        using var multi = await connection.QueryMultipleAsync(
            "dbo.AnalizeMedicale_Search",
            parameters,
            commandType: CommandType.StoredProcedure);

        var items = await multi.ReadAsync<AnalizaMedicala>();
        var totalCount = await multi.ReadFirstOrDefaultAsync<int>();

        _logger.LogInformation(
            "Search analize: searchTerm={SearchTerm}, found={Count}/{Total}",
            searchTerm, items.Count(), totalCount);

        return (items, totalCount);
    }

    public async Task<AnalizaMedicala?> GetByIdAsync(Guid analizaId, CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.CreateConnection();
        
        var analiza = await connection.QueryFirstOrDefaultAsync<AnalizaMedicala>(
            "dbo.AnalizeMedicale_GetById",
            new { AnalizaID = analizaId },
            commandType: CommandType.StoredProcedure);

        return analiza;
    }

    public async Task<IEnumerable<AnalizaMedicalaCategorie>> GetCategoriiAsync(
        bool doarActive = true,
        CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.CreateConnection();
        
        var categorii = await connection.QueryAsync<AnalizaMedicalaCategorie>(
            "dbo.AnalizeMedicaleCategorii_GetAll",
            new { DoarActive = doarActive },
            commandType: CommandType.StoredProcedure);

        return categorii;
    }

    public async Task<IEnumerable<AnalizaMedicalaLaborator>> GetLaboratoareAsync(
        bool doarActive = true,
        CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.CreateConnection();
        
        var sql = @"
            SELECT 
                LaboratorID,
                NumeLaborator,
                Acronim,
                Localitate,
                Telefon,
                Email,
                Website,
                EsteActiv
            FROM dbo.AnalizeMedicaleLaboratoare
            WHERE (@DoarActive = 0 OR EsteActiv = 1)
            ORDER BY NumeLaborator";

        var laboratoare = await connection.QueryAsync<AnalizaMedicalaLaborator>(
            sql,
            new { DoarActive = doarActive });

        return laboratoare;
    }

    public async Task<IEnumerable<AnalizaMedicala>> AutocompleteAsync(
        string searchTerm,
        int maxResults = 10,
        CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.CreateConnection();
        
        var sql = @"
            SELECT TOP (@MaxResults)
                AnalizaID,
                NumeAnaliza,
                NumeScurt,
                Acronime,
                CategorieID,
                Pret,
                Moneda
            FROM dbo.AnalizeMedicale
            WHERE EsteActiv = 1
              AND (
                  NumeAnaliza LIKE '%' + @SearchTerm + '%'
                  OR Acronime LIKE '%' + @SearchTerm + '%'
                  OR NumeScurt LIKE '%' + @SearchTerm + '%'
              )
            ORDER BY 
                CASE 
                    WHEN NumeAnaliza LIKE @SearchTerm + '%' THEN 1
                    WHEN Acronime LIKE @SearchTerm + '%' THEN 2
                    ELSE 3
                END,
                NumeAnaliza";

        var analize = await connection.QueryAsync<AnalizaMedicala>(
            sql,
            new { SearchTerm = searchTerm, MaxResults = maxResults });

        return analize;
    }
}
