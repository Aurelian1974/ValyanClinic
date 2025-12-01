using Dapper;
using System.Data;
using ValyanClinic.Domain.Entities.Settings;
using ValyanClinic.Domain.Interfaces.Repositories;
using ValyanClinic.Infrastructure.Data;

namespace ValyanClinic.Infrastructure.Repositories.Settings;

public class SystemSettingsRepository : ISystemSettingsRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public SystemSettingsRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<SystemSetting?> GetByKeyAsync(string categorie, string cheie, CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.CreateConnection();
        return await connection.QuerySingleOrDefaultAsync<SystemSetting>(
              "SP_GetSystemSetting",
     new { Categorie = categorie, Cheie = cheie },
              commandType: CommandType.StoredProcedure);
    }

    public async Task<IEnumerable<SystemSetting>> GetByCategoryAsync(string categorie, CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.CreateConnection();
        return await connection.QueryAsync<SystemSetting>(
                "SP_GetSystemSettingsByCategory",
         new { Categorie = categorie },
            commandType: CommandType.StoredProcedure);
    }

    public async Task<bool> UpdateAsync(string categorie, string cheie, string valoare, string? descriere, string modificatDe, CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.CreateConnection();
        var result = await connection.QuerySingleOrDefaultAsync<SystemSetting>(
                    "SP_UpdateSystemSetting",
                 new { Categorie = categorie, Cheie = cheie, Valoare = valoare, Descriere = descriere, ModificatDe = modificatDe },
                    commandType: CommandType.StoredProcedure);
        return result != null;
    }
}
