using Dapper;
using System.Data;
using ValyanClinic.Domain.Entities.Settings;
using ValyanClinic.Domain.Interfaces.Repositories;
using ValyanClinic.Domain.Interfaces.Data;

namespace ValyanClinic.Infrastructure.Repositories.Settings;

public class ProgramLucruRepository : IProgramLucruRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public ProgramLucruRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IEnumerable<ProgramLucru>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.CreateConnection();
        return await connection.QueryAsync<ProgramLucru>(
            "SP_ProgramLucru_GetAll",
            commandType: CommandType.StoredProcedure);
    }

    public async Task<ProgramLucru?> GetByDayAsync(DayOfWeek ziSaptamana, CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.CreateConnection();
        return await connection.QuerySingleOrDefaultAsync<ProgramLucru>(
            "SP_ProgramLucru_GetByDay",
            new { ZiSaptamana = (int)ziSaptamana },
            commandType: CommandType.StoredProcedure);
    }

    public async Task<bool> UpdateAsync(ProgramLucru programLucru, string modificatDe, CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.CreateConnection();
        var rowsAffected = await connection.ExecuteAsync(
            "SP_ProgramLucru_Update",
            new
            {
                programLucru.Id,
                ZiSaptamana = (int)programLucru.ZiSaptamana,
                programLucru.EsteDeschis,
                OraInceput = programLucru.OraInceput?.ToString(@"hh\:mm"),
                OraSfarsit = programLucru.OraSfarsit?.ToString(@"hh\:mm"),
                PauzaInceput = programLucru.PauzaInceput?.ToString(@"hh\:mm"),
                PauzaSfarsit = programLucru.PauzaSfarsit?.ToString(@"hh\:mm"),
                programLucru.Observatii,
                ModificatDe = modificatDe
            },
            commandType: CommandType.StoredProcedure);
        return rowsAffected > 0;
    }

    public async Task<bool> EsteInProgramAsync(DateTime data, TimeSpan ora, CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.CreateConnection();
        var result = await connection.QuerySingleOrDefaultAsync<int>(
            "SP_ProgramLucru_VerificaOrar",
            new
            {
                ZiSaptamana = (int)data.DayOfWeek,
                Ora = ora.ToString(@"hh\:mm")
            },
            commandType: CommandType.StoredProcedure);
        return result == 1;
    }
}
