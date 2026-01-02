using Dapper;
using MediatR;
using ValyanClinic.Application.Common.Results;
using ValyanClinic.Infrastructure.Data;

namespace ValyanClinic.Application.Features.AnalizeMedicale.Commands;

/// <summary>
/// Handler pentru ștergerea unei analize medicale
/// </summary>
public class DeleteAnalizaMedicalaCommandHandler 
    : IRequestHandler<DeleteAnalizaMedicalaCommand, Result<bool>>
{
    private readonly IDbConnectionFactory _connectionFactory;

    public DeleteAnalizaMedicalaCommandHandler(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<Result<bool>> Handle(
        DeleteAnalizaMedicalaCommand request, 
        CancellationToken cancellationToken)
    {
        try
        {
            const string sql = @"
                DELETE FROM ConsultatieAnalizeMedicale 
                WHERE Id = @Id";

            using var connection = _connectionFactory.CreateConnection();
            var rowsAffected = await connection.ExecuteAsync(sql, new { request.Id });

            return rowsAffected > 0 
                ? Result<bool>.Success(true)
                : Result<bool>.Failure("Analiza nu a fost găsită");
        }
        catch (Exception ex)
        {
            return Result<bool>.Failure($"Eroare la ștergerea analizei: {ex.Message}");
        }
    }
}
