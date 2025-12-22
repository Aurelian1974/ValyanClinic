using Dapper;
using MediatR;
using ValyanClinic.Application.Common.Results;
using ValyanClinic.Application.ViewModels;
using ValyanClinic.Infrastructure.Data;

namespace ValyanClinic.Application.Features.AnalizeMedicale.Queries;

/// <summary>
/// Handler pentru query-ul de obținere a analizelor medicale ale unui pacient
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
            const string sql = @"
                SELECT 
                    Id,
                    PacientId,
                    ConsultatieId,
                    DataDocument,
                    NumeDocument,
                    SursaDocument,
                    BatchId,
                    Categorie,
                    NumeAnaliza,
                    Rezultat,
                    UnitateMasura,
                    IntervalReferinta,
                    InAfaraLimitelor,
                    DataCrearii
                FROM AnalizeMedicale
                WHERE PacientId = @PacientId
                ORDER BY DataDocument DESC, Categorie, NumeAnaliza";

            using var connection = _connectionFactory.CreateConnection();
            var analize = await connection.QueryAsync<AnalizaMedicalaDto>(sql, new { request.PacientId });
            
            // Grupăm analizele pe BatchId sau DataDocument
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
