using Dapper;
using Microsoft.Extensions.Logging;
using ValyanClinic.Domain.Entities.Investigatii;
using ValyanClinic.Domain.Interfaces.Repositories;
using ValyanClinic.Infrastructure.Data;

namespace ValyanClinic.Infrastructure.Repositories.Investigatii;

/// <summary>
/// Repository pentru Investigații Imagistice Efectuate
/// </summary>
public class ConsultatieInvestigatieImagisticaEfectuataRepository : IConsultatieInvestigatieImagisticaEfectuataRepository
{
    private readonly IDbConnectionFactory _connectionFactory;
    private readonly ILogger<ConsultatieInvestigatieImagisticaEfectuataRepository> _logger;

    public ConsultatieInvestigatieImagisticaEfectuataRepository(
        IDbConnectionFactory connectionFactory,
        ILogger<ConsultatieInvestigatieImagisticaEfectuataRepository> logger)
    {
        _connectionFactory = connectionFactory;
        _logger = logger;
    }

    public async Task<Guid> CreateAsync(ConsultatieInvestigatieImagisticaEfectuata investigatie, CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.CreateConnection();

        const string sql = @"
            INSERT INTO dbo.ConsultatieInvestigatieImagisticaEfectuata 
                (RecomandareID, ConsultatieID, PacientID, InvestigatieNomenclatorID, 
                 DenumireInvestigatie, CodInvestigatie, RegiuneAnatomica, DataEfectuare,
                 CentrulMedical, MedicExecutant, Rezultat, Concluzii, CaleFisierRezultat,
                 DataCreare, CreatDe)
            OUTPUT INSERTED.Id
            VALUES 
                (@RecomandareID, @ConsultatieID, @PacientID, @InvestigatieNomenclatorID,
                 @DenumireInvestigatie, @CodInvestigatie, @RegiuneAnatomica, @DataEfectuare,
                 @CentrulMedical, @MedicExecutant, @Rezultat, @Concluzii, @CaleFisierRezultat,
                 @DataCreare, @CreatDe)";

        var id = await connection.QuerySingleAsync<Guid>(sql, new
        {
            investigatie.RecomandareID,
            investigatie.ConsultatieID,
            investigatie.PacientID,
            investigatie.InvestigatieNomenclatorID,
            investigatie.DenumireInvestigatie,
            investigatie.CodInvestigatie,
            investigatie.RegiuneAnatomica,
            investigatie.DataEfectuare,
            investigatie.CentrulMedical,
            investigatie.MedicExecutant,
            investigatie.Rezultat,
            investigatie.Concluzii,
            investigatie.CaleFisierRezultat,
            DataCreare = DateTime.Now,
            investigatie.CreatDe
        });

        _logger.LogInformation("Investigație imagistică efectuată creată: {Id} - {Denumire}", id, investigatie.DenumireInvestigatie);
        return id;
    }

    public async Task<IEnumerable<ConsultatieInvestigatieImagisticaEfectuata>> GetByPacientIdAsync(Guid pacientId, CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.CreateConnection();

        const string sql = @"
            SELECT i.*, n.Denumire AS NomenclatorDenumire, n.Categorie AS NomenclatorCategorie
            FROM dbo.ConsultatieInvestigatieImagisticaEfectuata i
            LEFT JOIN dbo.NomenclatorInvestigatiiImagistice n ON i.InvestigatieNomenclatorID = n.Id
            WHERE i.PacientID = @PacientID
            ORDER BY i.DataEfectuare DESC";

        return await connection.QueryAsync<ConsultatieInvestigatieImagisticaEfectuata>(sql, new { PacientID = pacientId });
    }

    public async Task<IEnumerable<ConsultatieInvestigatieImagisticaEfectuata>> GetByConsultatieIdAsync(Guid consultatieId, CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.CreateConnection();

        const string sql = @"
            SELECT i.*, n.Denumire AS NomenclatorDenumire, n.Categorie AS NomenclatorCategorie
            FROM dbo.ConsultatieInvestigatieImagisticaEfectuata i
            LEFT JOIN dbo.NomenclatorInvestigatiiImagistice n ON i.InvestigatieNomenclatorID = n.Id
            WHERE i.ConsultatieID = @ConsultatieID
            ORDER BY i.DataEfectuare DESC";

        return await connection.QueryAsync<ConsultatieInvestigatieImagisticaEfectuata>(sql, new { ConsultatieID = consultatieId });
    }

    public async Task<bool> UpdateAsync(ConsultatieInvestigatieImagisticaEfectuata investigatie, CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.CreateConnection();

        const string sql = @"
            UPDATE dbo.ConsultatieInvestigatieImagisticaEfectuata
            SET InvestigatieNomenclatorID = @InvestigatieNomenclatorID,
                DenumireInvestigatie = @DenumireInvestigatie,
                CodInvestigatie = @CodInvestigatie,
                RegiuneAnatomica = @RegiuneAnatomica,
                DataEfectuare = @DataEfectuare,
                CentrulMedical = @CentrulMedical,
                MedicExecutant = @MedicExecutant,
                Rezultat = @Rezultat,
                Concluzii = @Concluzii,
                CaleFisierRezultat = @CaleFisierRezultat,
                DataModificare = @DataModificare,
                ModificatDe = @ModificatDe
            WHERE Id = @Id";

        var rows = await connection.ExecuteAsync(sql, new
        {
            investigatie.Id,
            investigatie.InvestigatieNomenclatorID,
            investigatie.DenumireInvestigatie,
            investigatie.CodInvestigatie,
            investigatie.RegiuneAnatomica,
            investigatie.DataEfectuare,
            investigatie.CentrulMedical,
            investigatie.MedicExecutant,
            investigatie.Rezultat,
            investigatie.Concluzii,
            investigatie.CaleFisierRezultat,
            DataModificare = DateTime.Now,
            investigatie.ModificatDe
        });

        return rows > 0;
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.CreateConnection();
        var rows = await connection.ExecuteAsync(
            "DELETE FROM dbo.ConsultatieInvestigatieImagisticaEfectuata WHERE Id = @Id", 
            new { Id = id });
        return rows > 0;
    }

    public async Task<ConsultatieInvestigatieImagisticaEfectuata?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.CreateConnection();

        const string sql = @"
            SELECT i.*, n.Denumire AS NomenclatorDenumire, n.Categorie AS NomenclatorCategorie
            FROM dbo.ConsultatieInvestigatieImagisticaEfectuata i
            LEFT JOIN dbo.NomenclatorInvestigatiiImagistice n ON i.InvestigatieNomenclatorID = n.Id
            WHERE i.Id = @Id";

        return await connection.QueryFirstOrDefaultAsync<ConsultatieInvestigatieImagisticaEfectuata>(sql, new { Id = id });
    }
}

/// <summary>
/// Repository pentru Explorări Funcționale Efectuate
/// </summary>
public class ConsultatieExplorareEfectuataRepository : IConsultatieExplorareEfectuataRepository
{
    private readonly IDbConnectionFactory _connectionFactory;
    private readonly ILogger<ConsultatieExplorareEfectuataRepository> _logger;

    public ConsultatieExplorareEfectuataRepository(
        IDbConnectionFactory connectionFactory,
        ILogger<ConsultatieExplorareEfectuataRepository> logger)
    {
        _connectionFactory = connectionFactory;
        _logger = logger;
    }

    public async Task<Guid> CreateAsync(ConsultatieExplorareEfectuata explorare, CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.CreateConnection();

        const string sql = @"
            INSERT INTO dbo.ConsultatieExplorareEfectuata 
                (RecomandareID, ConsultatieID, PacientID, ExplorareNomenclatorID, 
                 DenumireExplorare, CodExplorare, DataEfectuare,
                 CentrulMedical, MedicExecutant, Rezultat, Concluzii, ParametriMasurati,
                 CaleFisierRezultat, DataCreare, CreatDe)
            OUTPUT INSERTED.Id
            VALUES 
                (@RecomandareID, @ConsultatieID, @PacientID, @ExplorareNomenclatorID,
                 @DenumireExplorare, @CodExplorare, @DataEfectuare,
                 @CentrulMedical, @MedicExecutant, @Rezultat, @Concluzii, @ParametriMasurati,
                 @CaleFisierRezultat, @DataCreare, @CreatDe)";

        var id = await connection.QuerySingleAsync<Guid>(sql, new
        {
            explorare.RecomandareID,
            explorare.ConsultatieID,
            explorare.PacientID,
            explorare.ExplorareNomenclatorID,
            explorare.DenumireExplorare,
            explorare.CodExplorare,
            explorare.DataEfectuare,
            explorare.CentrulMedical,
            explorare.MedicExecutant,
            explorare.Rezultat,
            explorare.Concluzii,
            explorare.ParametriMasurati,
            explorare.CaleFisierRezultat,
            DataCreare = DateTime.Now,
            explorare.CreatDe
        });

        _logger.LogInformation("Explorare funcțională efectuată creată: {Id} - {Denumire}", id, explorare.DenumireExplorare);
        return id;
    }

    public async Task<IEnumerable<ConsultatieExplorareEfectuata>> GetByPacientIdAsync(Guid pacientId, CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.CreateConnection();

        const string sql = @"
            SELECT e.*, n.Denumire AS NomenclatorDenumire, n.Categorie AS NomenclatorCategorie
            FROM dbo.ConsultatieExplorareEfectuata e
            LEFT JOIN dbo.NomenclatorExplorariFunc n ON e.ExplorareNomenclatorID = n.Id
            WHERE e.PacientID = @PacientID
            ORDER BY e.DataEfectuare DESC";

        return await connection.QueryAsync<ConsultatieExplorareEfectuata>(sql, new { PacientID = pacientId });
    }

    public async Task<IEnumerable<ConsultatieExplorareEfectuata>> GetByConsultatieIdAsync(Guid consultatieId, CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.CreateConnection();

        const string sql = @"
            SELECT e.*, n.Denumire AS NomenclatorDenumire, n.Categorie AS NomenclatorCategorie
            FROM dbo.ConsultatieExplorareEfectuata e
            LEFT JOIN dbo.NomenclatorExplorariFunc n ON e.ExplorareNomenclatorID = n.Id
            WHERE e.ConsultatieID = @ConsultatieID
            ORDER BY e.DataEfectuare DESC";

        return await connection.QueryAsync<ConsultatieExplorareEfectuata>(sql, new { ConsultatieID = consultatieId });
    }

    public async Task<bool> UpdateAsync(ConsultatieExplorareEfectuata explorare, CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.CreateConnection();

        const string sql = @"
            UPDATE dbo.ConsultatieExplorareEfectuata
            SET ExplorareNomenclatorID = @ExplorareNomenclatorID,
                DenumireExplorare = @DenumireExplorare,
                CodExplorare = @CodExplorare,
                DataEfectuare = @DataEfectuare,
                CentrulMedical = @CentrulMedical,
                MedicExecutant = @MedicExecutant,
                Rezultat = @Rezultat,
                Concluzii = @Concluzii,
                ParametriMasurati = @ParametriMasurati,
                CaleFisierRezultat = @CaleFisierRezultat,
                DataModificare = @DataModificare,
                ModificatDe = @ModificatDe
            WHERE Id = @Id";

        var rows = await connection.ExecuteAsync(sql, new
        {
            explorare.Id,
            explorare.ExplorareNomenclatorID,
            explorare.DenumireExplorare,
            explorare.CodExplorare,
            explorare.DataEfectuare,
            explorare.CentrulMedical,
            explorare.MedicExecutant,
            explorare.Rezultat,
            explorare.Concluzii,
            explorare.ParametriMasurati,
            explorare.CaleFisierRezultat,
            DataModificare = DateTime.Now,
            explorare.ModificatDe
        });

        return rows > 0;
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.CreateConnection();
        var rows = await connection.ExecuteAsync(
            "DELETE FROM dbo.ConsultatieExplorareEfectuata WHERE Id = @Id", 
            new { Id = id });
        return rows > 0;
    }

    public async Task<ConsultatieExplorareEfectuata?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.CreateConnection();

        const string sql = @"
            SELECT e.*, n.Denumire AS NomenclatorDenumire, n.Categorie AS NomenclatorCategorie
            FROM dbo.ConsultatieExplorareEfectuata e
            LEFT JOIN dbo.NomenclatorExplorariFunc n ON e.ExplorareNomenclatorID = n.Id
            WHERE e.Id = @Id";

        return await connection.QueryFirstOrDefaultAsync<ConsultatieExplorareEfectuata>(sql, new { Id = id });
    }
}

/// <summary>
/// Repository pentru Endoscopii Efectuate
/// </summary>
public class ConsultatieEndoscopieEfectuataRepository : IConsultatieEndoscopieEfectuataRepository
{
    private readonly IDbConnectionFactory _connectionFactory;
    private readonly ILogger<ConsultatieEndoscopieEfectuataRepository> _logger;

    public ConsultatieEndoscopieEfectuataRepository(
        IDbConnectionFactory connectionFactory,
        ILogger<ConsultatieEndoscopieEfectuataRepository> logger)
    {
        _connectionFactory = connectionFactory;
        _logger = logger;
    }

    public async Task<Guid> CreateAsync(ConsultatieEndoscopieEfectuata endoscopie, CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.CreateConnection();

        const string sql = @"
            INSERT INTO dbo.ConsultatieEndoscopieEfectuata 
                (RecomandareID, ConsultatieID, PacientID, EndoscopieNomenclatorID, 
                 DenumireEndoscopie, CodEndoscopie, DataEfectuare,
                 CentrulMedical, MedicExecutant, Rezultat, Concluzii, BiopsiiPrelevate,
                 RezultatHistopatologic, CaleFisierRezultat, DataCreare, CreatDe)
            OUTPUT INSERTED.Id
            VALUES 
                (@RecomandareID, @ConsultatieID, @PacientID, @EndoscopieNomenclatorID,
                 @DenumireEndoscopie, @CodEndoscopie, @DataEfectuare,
                 @CentrulMedical, @MedicExecutant, @Rezultat, @Concluzii, @BiopsiiPrelevate,
                 @RezultatHistopatologic, @CaleFisierRezultat, @DataCreare, @CreatDe)";

        var id = await connection.QuerySingleAsync<Guid>(sql, new
        {
            endoscopie.RecomandareID,
            endoscopie.ConsultatieID,
            endoscopie.PacientID,
            endoscopie.EndoscopieNomenclatorID,
            endoscopie.DenumireEndoscopie,
            endoscopie.CodEndoscopie,
            endoscopie.DataEfectuare,
            endoscopie.CentrulMedical,
            endoscopie.MedicExecutant,
            endoscopie.Rezultat,
            endoscopie.Concluzii,
            endoscopie.BiopsiiPrelevate,
            endoscopie.RezultatHistopatologic,
            endoscopie.CaleFisierRezultat,
            DataCreare = DateTime.Now,
            endoscopie.CreatDe
        });

        _logger.LogInformation("Endoscopie efectuată creată: {Id} - {Denumire}", id, endoscopie.DenumireEndoscopie);
        return id;
    }

    public async Task<IEnumerable<ConsultatieEndoscopieEfectuata>> GetByPacientIdAsync(Guid pacientId, CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.CreateConnection();

        const string sql = @"
            SELECT e.*, n.Denumire AS NomenclatorDenumire, n.Categorie AS NomenclatorCategorie
            FROM dbo.ConsultatieEndoscopieEfectuata e
            LEFT JOIN dbo.NomenclatorEndoscopii n ON e.EndoscopieNomenclatorID = n.Id
            WHERE e.PacientID = @PacientID
            ORDER BY e.DataEfectuare DESC";

        return await connection.QueryAsync<ConsultatieEndoscopieEfectuata>(sql, new { PacientID = pacientId });
    }

    public async Task<IEnumerable<ConsultatieEndoscopieEfectuata>> GetByConsultatieIdAsync(Guid consultatieId, CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.CreateConnection();

        const string sql = @"
            SELECT e.*, n.Denumire AS NomenclatorDenumire, n.Categorie AS NomenclatorCategorie
            FROM dbo.ConsultatieEndoscopieEfectuata e
            LEFT JOIN dbo.NomenclatorEndoscopii n ON e.EndoscopieNomenclatorID = n.Id
            WHERE e.ConsultatieID = @ConsultatieID
            ORDER BY e.DataEfectuare DESC";

        return await connection.QueryAsync<ConsultatieEndoscopieEfectuata>(sql, new { ConsultatieID = consultatieId });
    }

    public async Task<bool> UpdateAsync(ConsultatieEndoscopieEfectuata endoscopie, CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.CreateConnection();

        const string sql = @"
            UPDATE dbo.ConsultatieEndoscopieEfectuata
            SET EndoscopieNomenclatorID = @EndoscopieNomenclatorID,
                DenumireEndoscopie = @DenumireEndoscopie,
                CodEndoscopie = @CodEndoscopie,
                DataEfectuare = @DataEfectuare,
                CentrulMedical = @CentrulMedical,
                MedicExecutant = @MedicExecutant,
                Rezultat = @Rezultat,
                Concluzii = @Concluzii,
                BiopsiiPrelevate = @BiopsiiPrelevate,
                RezultatHistopatologic = @RezultatHistopatologic,
                CaleFisierRezultat = @CaleFisierRezultat,
                DataModificare = @DataModificare,
                ModificatDe = @ModificatDe
            WHERE Id = @Id";

        var rows = await connection.ExecuteAsync(sql, new
        {
            endoscopie.Id,
            endoscopie.EndoscopieNomenclatorID,
            endoscopie.DenumireEndoscopie,
            endoscopie.CodEndoscopie,
            endoscopie.DataEfectuare,
            endoscopie.CentrulMedical,
            endoscopie.MedicExecutant,
            endoscopie.Rezultat,
            endoscopie.Concluzii,
            endoscopie.BiopsiiPrelevate,
            endoscopie.RezultatHistopatologic,
            endoscopie.CaleFisierRezultat,
            DataModificare = DateTime.Now,
            endoscopie.ModificatDe
        });

        return rows > 0;
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.CreateConnection();
        var rows = await connection.ExecuteAsync(
            "DELETE FROM dbo.ConsultatieEndoscopieEfectuata WHERE Id = @Id", 
            new { Id = id });
        return rows > 0;
    }

    public async Task<ConsultatieEndoscopieEfectuata?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.CreateConnection();

        const string sql = @"
            SELECT e.*, n.Denumire AS NomenclatorDenumire, n.Categorie AS NomenclatorCategorie
            FROM dbo.ConsultatieEndoscopieEfectuata e
            LEFT JOIN dbo.NomenclatorEndoscopii n ON e.EndoscopieNomenclatorID = n.Id
            WHERE e.Id = @Id";

        return await connection.QueryFirstOrDefaultAsync<ConsultatieEndoscopieEfectuata>(sql, new { Id = id });
    }
}
