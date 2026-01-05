using Dapper;
using MediatR;
using ValyanClinic.Application.Common.Results;
using ValyanClinic.Application.ViewModels;
using ValyanClinic.Infrastructure.Data;

namespace ValyanClinic.Application.Features.AnalizeMedicale.Queries;

/// <summary>
/// Handler pentru query-ul de obținere a analizelor medicale ale unui pacient
/// Caută în tabela ConsultatieAnalizeMedicale (analize efectuate/importate)
/// </summary>
public class GetAnalizeMedicaleByPacientQueryHandler 
    : IRequestHandler<GetAnalizeMedicaleByPacientQuery, Result<List<AnalizeMedicaleGroupDto>>>
{
    private readonly IDbConnectionFactory _connectionFactory;

    public GetAnalizeMedicaleByPacientQueryHandler(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<Result<List<AnalizeMedicaleGroupDto>>> Handle(
        GetAnalizeMedicaleByPacientQuery request, 
        CancellationToken cancellationToken)
    {
        try
        {
            // Query pe tabela ConsultatieAnalizeMedicale cu join pe Consultatii pentru PacientId
            const string sql = @"
                SELECT 
                    a.Id,
                    c.PacientID AS PacientId,
                    a.ConsultatieID AS ConsultatieId,
                    COALESCE(a.DataEfectuare, a.DataRecomandare) AS DataDocument,
                    CONCAT('Import ', FORMAT(COALESCE(a.DataEfectuare, a.DataRecomandare), 'dd.MM.yyyy')) AS NumeDocument,
                    a.LocEfectuare AS SursaDocument,
                    a.ConsultatieID AS BatchId,
                    a.TipAnaliza AS Categorie,
                    a.NumeAnaliza,
                    a.ValoareRezultat AS Rezultat,
                    a.UnitatiMasura AS UnitateMasura,
                    CASE 
                        WHEN a.ValoareNormalaMin IS NOT NULL AND a.ValoareNormalaMax IS NOT NULL 
                        THEN CONCAT(CAST(a.ValoareNormalaMin AS VARCHAR), ' - ', CAST(a.ValoareNormalaMax AS VARCHAR))
                        WHEN a.ValoareNormalaMin IS NOT NULL 
                        THEN CONCAT('> ', CAST(a.ValoareNormalaMin AS VARCHAR))
                        WHEN a.ValoareNormalaMax IS NOT NULL 
                        THEN CONCAT('< ', CAST(a.ValoareNormalaMax AS VARCHAR))
                        ELSE NULL
                    END AS IntervalReferinta,
                    ISNULL(a.EsteInAfaraLimitelor, 0) AS InAfaraLimitelor,
                    a.DataCreare AS DataCrearii
                FROM ConsultatieAnalizeMedicale a
                INNER JOIN Consultatii c ON a.ConsultatieID = c.ConsultatieID
                WHERE c.PacientID = @PacientId
                  AND a.AreRezultate = 1
                ORDER BY COALESCE(a.DataEfectuare, a.DataRecomandare) DESC, a.TipAnaliza, a.NumeAnaliza";

            using var connection = _connectionFactory.CreateConnection();
            var analize = await connection.QueryAsync<AnalizaMedicalaDto>(sql, new { request.PacientId });
            
            // Grupăm analizele pe ConsultatieId (BatchId) și DataDocument
            var groups = analize
                .GroupBy(a => new { a.BatchId, a.DataDocument, a.NumeDocument, a.SursaDocument })
                .Select(g => new AnalizeMedicaleGroupDto
                {
                    DataDocument = g.Key.DataDocument,
                    NumeDocument = g.Key.NumeDocument,
                    SursaDocument = g.Key.SursaDocument,
                    BatchId = g.Key.BatchId,
                    Analize = g.ToList()
                })
                .OrderByDescending(g => g.DataDocument)
                .ToList();

            return Result<List<AnalizeMedicaleGroupDto>>.Success(groups);
        }
        catch (Exception ex)
        {
            return Result<List<AnalizeMedicaleGroupDto>>.Failure($"Eroare la încărcarea analizelor: {ex.Message}");
        }
    }
}
