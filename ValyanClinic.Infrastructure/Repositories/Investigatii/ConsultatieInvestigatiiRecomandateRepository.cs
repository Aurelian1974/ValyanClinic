using Dapper;
using Microsoft.Extensions.Logging;
using System.Data;
using ValyanClinic.Domain.Entities.Investigatii;
using ValyanClinic.Domain.Interfaces.Repositories;
using ValyanClinic.Domain.Interfaces.Data;
using ValyanClinic.Infrastructure.Data;

namespace ValyanClinic.Infrastructure.Repositories.Investigatii;

/// <summary>
/// Repository pentru Investigații Imagistice Recomandate
/// Schema DB: Id, ConsultatieID, InvestigatieID, RegiuneAnatomica, IndicatiiClinice, 
///            Prioritate, EsteCito, TermenRecomandat, Observatii, Status, DataCreare, 
///            CreatDe, DataUltimeiModificari, ModificatDe
/// </summary>
public class ConsultatieInvestigatieImagisticaRecomandataRepository : IConsultatieInvestigatieImagisticaRecomandataRepository
{
    private readonly IDbConnectionFactory _connectionFactory;
    private readonly ILogger<ConsultatieInvestigatieImagisticaRecomandataRepository> _logger;

    public ConsultatieInvestigatieImagisticaRecomandataRepository(
        IDbConnectionFactory connectionFactory,
        ILogger<ConsultatieInvestigatieImagisticaRecomandataRepository> logger)
    {
        _connectionFactory = connectionFactory;
        _logger = logger;
    }

    public async Task<Guid> CreateAsync(ConsultatieInvestigatieImagisticaRecomandata investigatie, CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.CreateConnection();

        const string sql = @"
            INSERT INTO dbo.ConsultatieInvestigatieImagisticaRecomandata 
                (ConsultatieID, InvestigatieID, RegiuneAnatomica, IndicatiiClinice, 
                 Prioritate, EsteCito, Observatii, Status, DataCreare, CreatDe)
            OUTPUT INSERTED.Id
            VALUES 
                (@ConsultatieID, @InvestigatieID, @RegiuneAnatomica, @IndicatiiClinice,
                 @Prioritate, @EsteCito, @Observatii, @Status, @DataCreare, @CreatDe)";

        var id = await connection.QuerySingleAsync<Guid>(sql, new
        {
            investigatie.ConsultatieID,
            InvestigatieID = investigatie.InvestigatieNomenclatorID,
            investigatie.RegiuneAnatomica,
            investigatie.IndicatiiClinice,
            Prioritate = investigatie.Prioritate ?? "Normala",
            investigatie.EsteCito,
            Observatii = investigatie.ObservatiiMedic,
            Status = investigatie.Status ?? "Recomandata",
            DataCreare = DateTime.Now,
            investigatie.CreatDe
        });

        _logger.LogInformation("Investigatie imagistica recomandata creata: {Id}", id);
        return id;
    }

    public async Task<IEnumerable<ConsultatieInvestigatieImagisticaRecomandata>> GetByConsultatieIdAsync(Guid consultatieId, CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.CreateConnection();

        const string sql = @"
            SELECT 
                i.Id, i.ConsultatieID, 
                i.InvestigatieID AS InvestigatieNomenclatorID,
                n.Denumire AS DenumireInvestigatie, 
                n.Cod AS CodInvestigatie,
                i.RegiuneAnatomica, 
                i.DataCreare AS DataRecomandare,
                i.Prioritate, i.EsteCito, i.IndicatiiClinice,
                i.Observatii AS ObservatiiMedic, 
                i.Status, i.DataCreare, i.CreatDe,
                i.DataUltimeiModificari AS DataModificare, i.ModificatDe
            FROM dbo.ConsultatieInvestigatieImagisticaRecomandata i
            LEFT JOIN dbo.NomenclatorInvestigatiiImagistice n ON i.InvestigatieID = n.Id
            WHERE i.ConsultatieID = @ConsultatieID
            ORDER BY i.DataCreare DESC";

        return await connection.QueryAsync<ConsultatieInvestigatieImagisticaRecomandata>(sql, new { ConsultatieID = consultatieId });
    }

    public async Task<bool> UpdateAsync(ConsultatieInvestigatieImagisticaRecomandata investigatie, CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.CreateConnection();

        const string sql = @"
            UPDATE dbo.ConsultatieInvestigatieImagisticaRecomandata
            SET InvestigatieID = @InvestigatieID,
                RegiuneAnatomica = @RegiuneAnatomica,
                Prioritate = @Prioritate,
                EsteCito = @EsteCito,
                IndicatiiClinice = @IndicatiiClinice,
                Observatii = @Observatii,
                Status = @Status,
                DataUltimeiModificari = @DataModificare,
                ModificatDe = @ModificatDe
            WHERE Id = @Id";

        var rows = await connection.ExecuteAsync(sql, new
        {
            investigatie.Id,
            InvestigatieID = investigatie.InvestigatieNomenclatorID,
            investigatie.RegiuneAnatomica,
            investigatie.Prioritate,
            investigatie.EsteCito,
            investigatie.IndicatiiClinice,
            Observatii = investigatie.ObservatiiMedic,
            investigatie.Status,
            DataModificare = DateTime.Now,
            investigatie.ModificatDe
        });

        return rows > 0;
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.CreateConnection();
        var rows = await connection.ExecuteAsync(
            "DELETE FROM dbo.ConsultatieInvestigatieImagisticaRecomandata WHERE Id = @Id", 
            new { Id = id });
        return rows > 0;
    }

    public async Task<bool> UpdateStatusAsync(Guid id, string status, Guid modificatDe, CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.CreateConnection();

        const string sql = @"
            UPDATE dbo.ConsultatieInvestigatieImagisticaRecomandata
            SET Status = @Status, DataUltimeiModificari = @DataModificare, ModificatDe = @ModificatDe
            WHERE Id = @Id";

        var rows = await connection.ExecuteAsync(sql, new { Id = id, Status = status, DataModificare = DateTime.Now, ModificatDe = modificatDe });
        return rows > 0;
    }
}

/// <summary>
/// Repository pentru Explorări Funcționale Recomandate
/// Schema DB: Id, ConsultatieID, ExplorareID, IndicatiiClinice, Prioritate, EsteCito,
///            TermenRecomandat, Observatii, Status, DataCreare, CreatDe, DataUltimeiModificari, ModificatDe
/// </summary>
public class ConsultatieExplorareRecomandataRepository : IConsultatieExplorareRecomandataRepository
{
    private readonly IDbConnectionFactory _connectionFactory;
    private readonly ILogger<ConsultatieExplorareRecomandataRepository> _logger;

    public ConsultatieExplorareRecomandataRepository(
        IDbConnectionFactory connectionFactory,
        ILogger<ConsultatieExplorareRecomandataRepository> logger)
    {
        _connectionFactory = connectionFactory;
        _logger = logger;
    }

    public async Task<Guid> CreateAsync(ConsultatieExplorareRecomandata explorare, CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.CreateConnection();

        const string sql = @"
            INSERT INTO dbo.ConsultatieExplorareRecomandata 
                (ConsultatieID, ExplorareID, IndicatiiClinice, Prioritate, EsteCito, 
                 Observatii, Status, DataCreare, CreatDe)
            OUTPUT INSERTED.Id
            VALUES 
                (@ConsultatieID, @ExplorareID, @IndicatiiClinice, @Prioritate, @EsteCito,
                 @Observatii, @Status, @DataCreare, @CreatDe)";

        var id = await connection.QuerySingleAsync<Guid>(sql, new
        {
            explorare.ConsultatieID,
            ExplorareID = explorare.ExplorareNomenclatorID,
            explorare.IndicatiiClinice,
            Prioritate = explorare.Prioritate ?? "Normala",
            explorare.EsteCito,
            Observatii = explorare.ObservatiiMedic,
            Status = explorare.Status ?? "Recomandata",
            DataCreare = DateTime.Now,
            explorare.CreatDe
        });

        _logger.LogInformation("Explorare functionala recomandata creata: {Id}", id);
        return id;
    }

    public async Task<IEnumerable<ConsultatieExplorareRecomandata>> GetByConsultatieIdAsync(Guid consultatieId, CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.CreateConnection();

        const string sql = @"
            SELECT 
                e.Id, e.ConsultatieID, 
                e.ExplorareID AS ExplorareNomenclatorID,
                n.Denumire AS DenumireExplorare, 
                n.Cod AS CodExplorare,
                e.DataCreare AS DataRecomandare,
                e.Prioritate, e.EsteCito, e.IndicatiiClinice,
                e.Observatii AS ObservatiiMedic, 
                e.Status, e.DataCreare, e.CreatDe,
                e.DataUltimeiModificari AS DataModificare, e.ModificatDe
            FROM dbo.ConsultatieExplorareRecomandata e
            LEFT JOIN dbo.NomenclatorExplorariFunc n ON e.ExplorareID = n.Id
            WHERE e.ConsultatieID = @ConsultatieID
            ORDER BY e.DataCreare DESC";

        return await connection.QueryAsync<ConsultatieExplorareRecomandata>(sql, new { ConsultatieID = consultatieId });
    }

    public async Task<bool> UpdateAsync(ConsultatieExplorareRecomandata explorare, CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.CreateConnection();

        const string sql = @"
            UPDATE dbo.ConsultatieExplorareRecomandata
            SET ExplorareID = @ExplorareID,
                Prioritate = @Prioritate,
                EsteCito = @EsteCito,
                IndicatiiClinice = @IndicatiiClinice,
                Observatii = @Observatii,
                Status = @Status,
                DataUltimeiModificari = @DataModificare,
                ModificatDe = @ModificatDe
            WHERE Id = @Id";

        var rows = await connection.ExecuteAsync(sql, new
        {
            explorare.Id,
            ExplorareID = explorare.ExplorareNomenclatorID,
            explorare.Prioritate,
            explorare.EsteCito,
            explorare.IndicatiiClinice,
            Observatii = explorare.ObservatiiMedic,
            explorare.Status,
            DataModificare = DateTime.Now,
            explorare.ModificatDe
        });

        return rows > 0;
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.CreateConnection();
        var rows = await connection.ExecuteAsync(
            "DELETE FROM dbo.ConsultatieExplorareRecomandata WHERE Id = @Id", 
            new { Id = id });
        return rows > 0;
    }

    public async Task<bool> UpdateStatusAsync(Guid id, string status, Guid modificatDe, CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.CreateConnection();

        const string sql = @"
            UPDATE dbo.ConsultatieExplorareRecomandata
            SET Status = @Status, DataUltimeiModificari = @DataModificare, ModificatDe = @ModificatDe
            WHERE Id = @Id";

        var rows = await connection.ExecuteAsync(sql, new { Id = id, Status = status, DataModificare = DateTime.Now, ModificatDe = modificatDe });
        return rows > 0;
    }
}

/// <summary>
/// Repository pentru Endoscopii Recomandate
/// Schema DB: Id, ConsultatieID, EndoscopieID, IndicatiiClinice, Prioritate, EsteCito,
///            TermenRecomandat, Observatii, Status, DataCreare, CreatDe, DataUltimeiModificari, ModificatDe
/// </summary>
public class ConsultatieEndoscopieRecomandataRepository : IConsultatieEndoscopieRecomandataRepository
{
    private readonly IDbConnectionFactory _connectionFactory;
    private readonly ILogger<ConsultatieEndoscopieRecomandataRepository> _logger;

    public ConsultatieEndoscopieRecomandataRepository(
        IDbConnectionFactory connectionFactory,
        ILogger<ConsultatieEndoscopieRecomandataRepository> logger)
    {
        _connectionFactory = connectionFactory;
        _logger = logger;
    }

    public async Task<Guid> CreateAsync(ConsultatieEndoscopieRecomandata endoscopie, CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.CreateConnection();

        const string sql = @"
            INSERT INTO dbo.ConsultatieEndoscopieRecomandata 
                (ConsultatieID, EndoscopieID, IndicatiiClinice, Prioritate, EsteCito, 
                 Observatii, Status, DataCreare, CreatDe)
            OUTPUT INSERTED.Id
            VALUES 
                (@ConsultatieID, @EndoscopieID, @IndicatiiClinice, @Prioritate, @EsteCito,
                 @Observatii, @Status, @DataCreare, @CreatDe)";

        var id = await connection.QuerySingleAsync<Guid>(sql, new
        {
            endoscopie.ConsultatieID,
            EndoscopieID = endoscopie.EndoscopieNomenclatorID,
            endoscopie.IndicatiiClinice,
            Prioritate = endoscopie.Prioritate ?? "Normala",
            endoscopie.EsteCito,
            Observatii = endoscopie.ObservatiiMedic,
            Status = endoscopie.Status ?? "Recomandata",
            DataCreare = DateTime.Now,
            endoscopie.CreatDe
        });

        _logger.LogInformation("Endoscopie recomandata creata: {Id}", id);
        return id;
    }

    public async Task<IEnumerable<ConsultatieEndoscopieRecomandata>> GetByConsultatieIdAsync(Guid consultatieId, CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.CreateConnection();

        const string sql = @"
            SELECT 
                e.Id, e.ConsultatieID, 
                e.EndoscopieID AS EndoscopieNomenclatorID,
                n.Denumire AS DenumireEndoscopie, 
                n.Cod AS CodEndoscopie,
                e.DataCreare AS DataRecomandare,
                e.Prioritate, e.EsteCito, e.IndicatiiClinice,
                e.Observatii AS ObservatiiMedic, 
                e.Status, e.DataCreare, e.CreatDe,
                e.DataUltimeiModificari AS DataModificare, e.ModificatDe
            FROM dbo.ConsultatieEndoscopieRecomandata e
            LEFT JOIN dbo.NomenclatorEndoscopii n ON e.EndoscopieID = n.Id
            WHERE e.ConsultatieID = @ConsultatieID
            ORDER BY e.DataCreare DESC";

        return await connection.QueryAsync<ConsultatieEndoscopieRecomandata>(sql, new { ConsultatieID = consultatieId });
    }

    public async Task<bool> UpdateAsync(ConsultatieEndoscopieRecomandata endoscopie, CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.CreateConnection();

        const string sql = @"
            UPDATE dbo.ConsultatieEndoscopieRecomandata
            SET EndoscopieID = @EndoscopieID,
                Prioritate = @Prioritate,
                EsteCito = @EsteCito,
                IndicatiiClinice = @IndicatiiClinice,
                Observatii = @Observatii,
                Status = @Status,
                DataUltimeiModificari = @DataModificare,
                ModificatDe = @ModificatDe
            WHERE Id = @Id";

        var rows = await connection.ExecuteAsync(sql, new
        {
            endoscopie.Id,
            EndoscopieID = endoscopie.EndoscopieNomenclatorID,
            endoscopie.Prioritate,
            endoscopie.EsteCito,
            endoscopie.IndicatiiClinice,
            Observatii = endoscopie.ObservatiiMedic,
            endoscopie.Status,
            DataModificare = DateTime.Now,
            endoscopie.ModificatDe
        });

        return rows > 0;
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.CreateConnection();
        var rows = await connection.ExecuteAsync(
            "DELETE FROM dbo.ConsultatieEndoscopieRecomandata WHERE Id = @Id", 
            new { Id = id });
        return rows > 0;
    }

    public async Task<bool> UpdateStatusAsync(Guid id, string status, Guid modificatDe, CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.CreateConnection();

        const string sql = @"
            UPDATE dbo.ConsultatieEndoscopieRecomandata
            SET Status = @Status, DataUltimeiModificari = @DataModificare, ModificatDe = @ModificatDe
            WHERE Id = @Id";

        var rows = await connection.ExecuteAsync(sql, new { Id = id, Status = status, DataModificare = DateTime.Now, ModificatDe = modificatDe });
        return rows > 0;
    }
}
