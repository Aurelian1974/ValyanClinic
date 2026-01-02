using Dapper;
using MediatR;
using ValyanClinic.Application.Common.Results;
using ValyanClinic.Infrastructure.Data;

namespace ValyanClinic.Application.Features.AnalizeMedicale.Commands;

/// <summary>
/// Handler pentru crearea unei analize medicale recomandate
/// </summary>
public class CreateAnalizaMedicalaCommandHandler 
    : IRequestHandler<CreateAnalizaMedicalaCommand, Result<Guid>>
{
    private readonly IDbConnectionFactory _connectionFactory;

    public CreateAnalizaMedicalaCommandHandler(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<Result<Guid>> Handle(
        CreateAnalizaMedicalaCommand request, 
        CancellationToken cancellationToken)
    {
        try
        {
            const string sql = @"
                INSERT INTO ConsultatieAnalizeMedicale (
                    ConsultatieID,
                    TipAnaliza,
                    NumeAnaliza,
                    CodAnaliza,
                    StatusAnaliza,
                    DataRecomandare,
                    Prioritate,
                    EsteCito,
                    IndicatiiClinice,
                    ObservatiiRecomandare,
                    AreRezultate,
                    EsteInAfaraLimitelor,
                    Decontat,
                    DataCreare,
                    CreatDe
                ) VALUES (
                    @ConsultatieID,
                    @TipAnaliza,
                    @NumeAnaliza,
                    @CodAnaliza,
                    'Recomandata',
                    GETDATE(),
                    @Prioritate,
                    @EsteCito,
                    @IndicatiiClinice,
                    @ObservatiiRecomandare,
                    0,
                    0,
                    0,
                    GETDATE(),
                    @CreatDe
                );
                SELECT CAST(SCOPE_IDENTITY() AS UNIQUEIDENTIFIER);";

            using var connection = _connectionFactory.CreateConnection();
            var id = await connection.ExecuteScalarAsync<Guid>(sql, request);

            return Result<Guid>.Success(id);
        }
        catch (Exception ex)
        {
            return Result<Guid>.Failure($"Eroare la crearea analizei: {ex.Message}");
        }
    }
}
