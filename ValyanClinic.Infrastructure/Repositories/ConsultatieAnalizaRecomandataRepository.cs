using Dapper;
using Microsoft.Extensions.Logging;
using System.Data;
using ValyanClinic.Domain.Entities;
using ValyanClinic.Domain.Interfaces.Repositories;
using ValyanClinic.Infrastructure.Data;

namespace ValyanClinic.Infrastructure.Repositories;

/// <summary>
/// Repository pentru analize medicale recomandate în timpul consultației
/// </summary>
public class ConsultatieAnalizaRecomandataRepository : IConsultatieAnalizaRecomandataRepository
{
    private readonly IDbConnectionFactory _connectionFactory;
    private readonly ILogger<ConsultatieAnalizaRecomandataRepository> _logger;

    public ConsultatieAnalizaRecomandataRepository(
        IDbConnectionFactory connectionFactory,
        ILogger<ConsultatieAnalizaRecomandataRepository> logger)
    {
        _connectionFactory = connectionFactory;
        _logger = logger;
    }

    public async Task<Guid> CreateAsync(ConsultatieAnalizaRecomandataEntity analiza, CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.CreateConnection();

        var parameters = new DynamicParameters();
        parameters.Add("@ConsultatieID", analiza.ConsultatieID);
        parameters.Add("@AnalizaNomenclatorID", analiza.AnalizaNomenclatorID);
        parameters.Add("@NumeAnaliza", analiza.NumeAnaliza);
        parameters.Add("@CodAnaliza", analiza.CodAnaliza);
        parameters.Add("@TipAnaliza", analiza.TipAnaliza);
        parameters.Add("@Prioritate", analiza.Prioritate);
        parameters.Add("@EsteCito", analiza.EsteCito);
        parameters.Add("@IndicatiiClinice", analiza.IndicatiiClinice);
        parameters.Add("@ObservatiiMedic", analiza.ObservatiiMedic);
        parameters.Add("@CreatDe", analiza.CreatDe);
        parameters.Add("@Id", dbType: DbType.Guid, direction: ParameterDirection.Output);

        await connection.ExecuteAsync(
            "dbo.ConsultatieAnalizeMedicaleRecomandate_Create",
            parameters,
            commandType: CommandType.StoredProcedure);

        var newId = parameters.Get<Guid>("@Id");

        _logger.LogInformation(
            "Analiză recomandată creată: {Id} - {NumeAnaliza} pentru consultație {ConsultatieID}",
            newId, analiza.NumeAnaliza, analiza.ConsultatieID);

        return newId;
    }

    public async Task<IEnumerable<ConsultatieAnalizaRecomandataEntity>> GetByConsultatieIdAsync(
        Guid consultatieId, 
        CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.CreateConnection();

        var analize = await connection.QueryAsync<ConsultatieAnalizaRecomandataEntity>(
            "dbo.ConsultatieAnalizeMedicaleRecomandate_GetByConsultatieId",
            new { ConsultatieID = consultatieId },
            commandType: CommandType.StoredProcedure);

        _logger.LogInformation(
            "Obținute {Count} analize recomandate pentru consultație {ConsultatieID}",
            analize.Count(), consultatieId);

        return analize;
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.CreateConnection();

        var result = await connection.QueryFirstOrDefaultAsync<int>(
            "dbo.ConsultatieAnalizeMedicaleRecomandate_Delete",
            new { Id = id },
            commandType: CommandType.StoredProcedure);

        var success = result > 0;

        if (success)
        {
            _logger.LogInformation("Analiză recomandată ștearsă: {Id}", id);
        }
        else
        {
            _logger.LogWarning("Analiză recomandată nu a fost găsită pentru ștergere: {Id}", id);
        }

        return success;
    }

    public async Task<bool> UpdateAsync(ConsultatieAnalizaRecomandataEntity analiza, CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.CreateConnection();

        var parameters = new DynamicParameters();
        parameters.Add("@Id", analiza.Id);
        parameters.Add("@Prioritate", analiza.Prioritate);
        parameters.Add("@EsteCito", analiza.EsteCito);
        parameters.Add("@IndicatiiClinice", analiza.IndicatiiClinice);
        parameters.Add("@ObservatiiMedic", analiza.ObservatiiMedic);
        parameters.Add("@Status", analiza.Status);
        parameters.Add("@DataProgramata", analiza.DataProgramata);
        parameters.Add("@ModificatDe", analiza.ModificatDe);

        var result = await connection.QueryFirstOrDefaultAsync<int>(
            "dbo.ConsultatieAnalizeMedicaleRecomandate_Update",
            parameters,
            commandType: CommandType.StoredProcedure);

        var success = result > 0;

        if (success)
        {
            _logger.LogInformation("Analiză recomandată actualizată: {Id}", analiza.Id);
        }

        return success;
    }
}
