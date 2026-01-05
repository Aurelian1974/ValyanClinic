using Dapper;
using MediatR;
using Microsoft.Extensions.Logging;
using ValyanClinic.Application.Common.Results;
using ValyanClinic.Infrastructure.Data;

namespace ValyanClinic.Application.Features.AnalizeMedicale.Commands.DeleteAnalizaEfectuata;

/// <summary>
/// Handler pentru ștergerea unei analize efectuate
/// </summary>
public class DeleteAnalizaEfectuataCommandHandler 
    : IRequestHandler<DeleteAnalizaEfectuataCommand, Result<bool>>
{
    private readonly IDbConnectionFactory _connectionFactory;
    private readonly ILogger<DeleteAnalizaEfectuataCommandHandler> _logger;

    public DeleteAnalizaEfectuataCommandHandler(
        IDbConnectionFactory connectionFactory,
        ILogger<DeleteAnalizaEfectuataCommandHandler> logger)
    {
        _connectionFactory = connectionFactory;
        _logger = logger;
    }

    public async Task<Result<bool>> Handle(
        DeleteAnalizaEfectuataCommand request, 
        CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Deleting analiză efectuată {AnalizaId}", request.Id);
            
            const string sql = @"
                DELETE FROM ConsultatieAnalizeMedicale 
                WHERE Id = @Id";

            using var connection = _connectionFactory.CreateConnection();
            var rowsAffected = await connection.ExecuteAsync(sql, new { request.Id });

            if (rowsAffected > 0)
            {
                _logger.LogInformation("Successfully deleted analiză efectuată {AnalizaId}", request.Id);
                return Result<bool>.Success(true);
            }
            
            _logger.LogWarning("Analiză efectuată not found: {AnalizaId}", request.Id);
            return Result<bool>.Failure("Analiza efectuată nu a fost găsită");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting analiză efectuată {AnalizaId}", request.Id);
            return Result<bool>.Failure($"Eroare la ștergerea analizei: {ex.Message}");
        }
    }
}
