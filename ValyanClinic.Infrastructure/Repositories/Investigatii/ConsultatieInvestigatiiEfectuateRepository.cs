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
                (RecomandareID, ConsultatieID, PacientID, InvestigatieID, 
                 NumeInvestigatie, RegiuneAnatomica, DataEfectuare,
                 Laborator, MedicRadiolog, Rezultat, Concluzie, CaleDocument,
                 DataCreare, CreatDe)
            OUTPUT INSERTED.Id
            VALUES 
                (@RecomandareID, @ConsultatieID, @PacientID, @InvestigatieID,
                 @NumeInvestigatie, @RegiuneAnatomica, @DataEfectuare,
                 @Laborator, @MedicRadiolog, @Rezultat, @Concluzie, @CaleDocument,
                 @DataCreare, @CreatDe)";

        var id = await connection.QuerySingleAsync<Guid>(sql, new
        {
            investigatie.RecomandareID,
            investigatie.ConsultatieID,
            investigatie.PacientID,
            InvestigatieID = investigatie.InvestigatieNomenclatorID,
            NumeInvestigatie = investigatie.DenumireInvestigatie,
            investigatie.RegiuneAnatomica,
            investigatie.DataEfectuare,
            Laborator = investigatie.CentrulMedical,
            MedicRadiolog = investigatie.MedicExecutant,
            investigatie.Rezultat,
            Concluzie = investigatie.Concluzii,
            CaleDocument = investigatie.CaleFisierRezultat,
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
            SELECT 
                i.Id, i.ConsultatieID, i.PacientID, i.RecomandareID,
                i.InvestigatieID AS InvestigatieNomenclatorID,
                i.NumeInvestigatie AS DenumireInvestigatie,
                i.RegiuneAnatomica, i.DataEfectuare,
                i.Laborator AS CentrulMedical,
                i.MedicRadiolog AS MedicExecutant,
                i.Rezultat, i.Concluzie AS Concluzii,
                i.CaleDocument AS CaleFisierRezultat,
                i.DataCreare, i.CreatDe,
                n.Denumire AS NomenclatorDenumire, n.Categorie AS NomenclatorCategorie
            FROM dbo.ConsultatieInvestigatieImagisticaEfectuata i
            LEFT JOIN dbo.NomenclatorInvestigatiiImagistice n ON i.InvestigatieID = n.Id
            WHERE i.PacientID = @PacientID
            ORDER BY i.DataEfectuare DESC";

        return await connection.QueryAsync<ConsultatieInvestigatieImagisticaEfectuata>(sql, new { PacientID = pacientId });
    }

    public async Task<IEnumerable<ConsultatieInvestigatieImagisticaEfectuata>> GetByConsultatieIdAsync(Guid consultatieId, CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.CreateConnection();

        const string sql = @"
            SELECT 
                i.Id, i.ConsultatieID, i.PacientID, i.RecomandareID,
                i.InvestigatieID AS InvestigatieNomenclatorID,
                i.NumeInvestigatie AS DenumireInvestigatie,
                i.RegiuneAnatomica, i.DataEfectuare,
                i.Laborator AS CentrulMedical,
                i.MedicRadiolog AS MedicExecutant,
                i.Rezultat, i.Concluzie AS Concluzii,
                i.CaleDocument AS CaleFisierRezultat,
                i.DataCreare, i.CreatDe,
                n.Denumire AS NomenclatorDenumire, n.Categorie AS NomenclatorCategorie
            FROM dbo.ConsultatieInvestigatieImagisticaEfectuata i
            LEFT JOIN dbo.NomenclatorInvestigatiiImagistice n ON i.InvestigatieID = n.Id
            WHERE i.ConsultatieID = @ConsultatieID
            ORDER BY i.DataEfectuare DESC";

        return await connection.QueryAsync<ConsultatieInvestigatieImagisticaEfectuata>(sql, new { ConsultatieID = consultatieId });
    }

    public async Task<bool> UpdateAsync(ConsultatieInvestigatieImagisticaEfectuata investigatie, CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.CreateConnection();

        const string sql = @"
            UPDATE dbo.ConsultatieInvestigatieImagisticaEfectuata
            SET InvestigatieID = @InvestigatieID,
                NumeInvestigatie = @NumeInvestigatie,
                RegiuneAnatomica = @RegiuneAnatomica,
                DataEfectuare = @DataEfectuare,
                Laborator = @Laborator,
                MedicRadiolog = @MedicRadiolog,
                Rezultat = @Rezultat,
                Concluzie = @Concluzie,
                CaleDocument = @CaleDocument,
                DataUltimeiModificari = @DataModificare,
                ModificatDe = @ModificatDe
            WHERE Id = @Id";

        var rows = await connection.ExecuteAsync(sql, new
        {
            investigatie.Id,
            InvestigatieID = investigatie.InvestigatieNomenclatorID,
            NumeInvestigatie = investigatie.DenumireInvestigatie,
            investigatie.RegiuneAnatomica,
            investigatie.DataEfectuare,
            Laborator = investigatie.CentrulMedical,
            MedicRadiolog = investigatie.MedicExecutant,
            investigatie.Rezultat,
            Concluzie = investigatie.Concluzii,
            CaleDocument = investigatie.CaleFisierRezultat,
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
            SELECT 
                i.Id, i.ConsultatieID, i.PacientID, i.RecomandareID,
                i.InvestigatieID AS InvestigatieNomenclatorID,
                i.NumeInvestigatie AS DenumireInvestigatie,
                i.RegiuneAnatomica, i.DataEfectuare,
                i.Laborator AS CentrulMedical,
                i.MedicRadiolog AS MedicExecutant,
                i.Rezultat, i.Concluzie AS Concluzii,
                i.CaleDocument AS CaleFisierRezultat,
                i.DataCreare, i.CreatDe,
                n.Denumire AS NomenclatorDenumire, n.Categorie AS NomenclatorCategorie
            FROM dbo.ConsultatieInvestigatieImagisticaEfectuata i
            LEFT JOIN dbo.NomenclatorInvestigatiiImagistice n ON i.InvestigatieID = n.Id
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

        // Schema DB: ExplorareID, NumeExplorare, Laborator, Concluzie, CaleDocument
        // Entity: ExplorareNomenclatorID, DenumireExplorare, CentrulMedical, Concluzii, CaleFisierRezultat
        const string sql = @"
            INSERT INTO dbo.ConsultatieExplorareEfectuata 
                (RecomandareID, ConsultatieID, PacientID, ExplorareID, 
                 NumeExplorare, DataEfectuare,
                 Laborator, MedicExecutant, Rezultat, Concluzie, ParametriMasurati,
                 CaleDocument, DataCreare, CreatDe)
            OUTPUT INSERTED.Id
            VALUES 
                (@RecomandareID, @ConsultatieID, @PacientID, @ExplorareID,
                 @NumeExplorare, @DataEfectuare,
                 @Laborator, @MedicExecutant, @Rezultat, @Concluzie, @ParametriMasurati,
                 @CaleDocument, @DataCreare, @CreatDe)";

        var id = await connection.QuerySingleAsync<Guid>(sql, new
        {
            explorare.RecomandareID,
            explorare.ConsultatieID,
            explorare.PacientID,
            ExplorareID = explorare.ExplorareNomenclatorID,
            NumeExplorare = explorare.DenumireExplorare,
            explorare.DataEfectuare,
            Laborator = explorare.CentrulMedical,
            explorare.MedicExecutant,
            explorare.Rezultat,
            Concluzie = explorare.Concluzii,
            explorare.ParametriMasurati,
            CaleDocument = explorare.CaleFisierRezultat,
            DataCreare = DateTime.Now,
            explorare.CreatDe
        });

        _logger.LogInformation("Explorare funcțională efectuată creată: {Id} - {Denumire}", id, explorare.DenumireExplorare);
        return id;
    }

    public async Task<IEnumerable<ConsultatieExplorareEfectuata>> GetByPacientIdAsync(Guid pacientId, CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.CreateConnection();

        // Map DB columns to entity properties
        const string sql = @"
            SELECT e.Id, e.RecomandareID, e.ConsultatieID, e.PacientID, 
                   e.ExplorareID AS ExplorareNomenclatorID,
                   e.NumeExplorare AS DenumireExplorare,
                   e.DataEfectuare, 
                   e.Laborator AS CentrulMedical, 
                   e.MedicExecutant,
                   e.Rezultat, 
                   e.ParametriMasurati, 
                   e.Interpretare,
                   e.Concluzie AS Concluzii, 
                   e.CaleDocument AS CaleFisierRezultat,
                   e.DataCreare, e.CreatDe, 
                   e.DataUltimeiModificari AS DataModificare, e.ModificatDe,
                   n.Denumire AS NomenclatorDenumire, n.Categorie AS NomenclatorCategorie
            FROM dbo.ConsultatieExplorareEfectuata e
            LEFT JOIN dbo.NomenclatorExplorariFunc n ON e.ExplorareID = n.Id
            WHERE e.PacientID = @PacientID
            ORDER BY e.DataEfectuare DESC";

        return await connection.QueryAsync<ConsultatieExplorareEfectuata>(sql, new { PacientID = pacientId });
    }

    public async Task<IEnumerable<ConsultatieExplorareEfectuata>> GetByConsultatieIdAsync(Guid consultatieId, CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.CreateConnection();

        // Map DB columns to entity properties
        const string sql = @"
            SELECT e.Id, e.RecomandareID, e.ConsultatieID, e.PacientID, 
                   e.ExplorareID AS ExplorareNomenclatorID,
                   e.NumeExplorare AS DenumireExplorare,
                   e.DataEfectuare, 
                   e.Laborator AS CentrulMedical, 
                   e.MedicExecutant,
                   e.Rezultat, 
                   e.ParametriMasurati, 
                   e.Interpretare,
                   e.Concluzie AS Concluzii, 
                   e.CaleDocument AS CaleFisierRezultat,
                   e.DataCreare, e.CreatDe, 
                   e.DataUltimeiModificari AS DataModificare, e.ModificatDe,
                   n.Denumire AS NomenclatorDenumire, n.Categorie AS NomenclatorCategorie
            FROM dbo.ConsultatieExplorareEfectuata e
            LEFT JOIN dbo.NomenclatorExplorariFunc n ON e.ExplorareID = n.Id
            WHERE e.ConsultatieID = @ConsultatieID
            ORDER BY e.DataEfectuare DESC";

        return await connection.QueryAsync<ConsultatieExplorareEfectuata>(sql, new { ConsultatieID = consultatieId });
    }

    public async Task<bool> UpdateAsync(ConsultatieExplorareEfectuata explorare, CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.CreateConnection();

        // Map entity properties to DB columns
        const string sql = @"
            UPDATE dbo.ConsultatieExplorareEfectuata
            SET ExplorareID = @ExplorareID,
                NumeExplorare = @NumeExplorare,
                DataEfectuare = @DataEfectuare,
                Laborator = @Laborator,
                MedicExecutant = @MedicExecutant,
                Rezultat = @Rezultat,
                Concluzie = @Concluzie,
                ParametriMasurati = @ParametriMasurati,
                CaleDocument = @CaleDocument,
                DataUltimeiModificari = @DataModificare,
                ModificatDe = @ModificatDe
            WHERE Id = @Id";

        var rows = await connection.ExecuteAsync(sql, new
        {
            explorare.Id,
            ExplorareID = explorare.ExplorareNomenclatorID,
            NumeExplorare = explorare.DenumireExplorare,
            explorare.DataEfectuare,
            Laborator = explorare.CentrulMedical,
            explorare.MedicExecutant,
            explorare.Rezultat,
            Concluzie = explorare.Concluzii,
            explorare.ParametriMasurati,
            CaleDocument = explorare.CaleFisierRezultat,
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

        // Map DB columns to entity properties
        const string sql = @"
            SELECT e.Id, e.RecomandareID, e.ConsultatieID, e.PacientID, 
                   e.ExplorareID AS ExplorareNomenclatorID,
                   e.NumeExplorare AS DenumireExplorare,
                   e.DataEfectuare, 
                   e.Laborator AS CentrulMedical, 
                   e.MedicExecutant,
                   e.Rezultat, 
                   e.ParametriMasurati, 
                   e.Interpretare,
                   e.Concluzie AS Concluzii, 
                   e.CaleDocument AS CaleFisierRezultat,
                   e.DataCreare, e.CreatDe, 
                   e.DataUltimeiModificari AS DataModificare, e.ModificatDe,
                   n.Denumire AS NomenclatorDenumire, n.Categorie AS NomenclatorCategorie
            FROM dbo.ConsultatieExplorareEfectuata e
            LEFT JOIN dbo.NomenclatorExplorariFunc n ON e.ExplorareID = n.Id
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

        // Schema DB: EndoscopieID, NumeEndoscopie, Laborator, BiopsiePrelevata, Concluzie, CaleDocument
        // Entity: EndoscopieNomenclatorID, DenumireEndoscopie, CentrulMedical, BiopsiiPrelevate, Concluzii, CaleFisierRezultat
        const string sql = @"
            INSERT INTO dbo.ConsultatieEndoscopieEfectuata 
                (RecomandareID, ConsultatieID, PacientID, EndoscopieID, 
                 NumeEndoscopie, DataEfectuare,
                 Laborator, MedicExecutant, Rezultat, Concluzie, BiopsiePrelevata,
                 LocBiopsie, CaleDocument, DataCreare, CreatDe)
            OUTPUT INSERTED.Id
            VALUES 
                (@RecomandareID, @ConsultatieID, @PacientID, @EndoscopieID,
                 @NumeEndoscopie, @DataEfectuare,
                 @Laborator, @MedicExecutant, @Rezultat, @Concluzie, @BiopsiePrelevata,
                 @LocBiopsie, @CaleDocument, @DataCreare, @CreatDe)";

        var id = await connection.QuerySingleAsync<Guid>(sql, new
        {
            endoscopie.RecomandareID,
            endoscopie.ConsultatieID,
            endoscopie.PacientID,
            EndoscopieID = endoscopie.EndoscopieNomenclatorID,
            NumeEndoscopie = endoscopie.DenumireEndoscopie,
            endoscopie.DataEfectuare,
            Laborator = endoscopie.CentrulMedical,
            endoscopie.MedicExecutant,
            endoscopie.Rezultat,
            Concluzie = endoscopie.Concluzii,
            BiopsiePrelevata = !string.IsNullOrEmpty(endoscopie.BiopsiiPrelevate),
            LocBiopsie = endoscopie.BiopsiiPrelevate,
            CaleDocument = endoscopie.CaleFisierRezultat,
            DataCreare = DateTime.Now,
            endoscopie.CreatDe
        });

        _logger.LogInformation("Endoscopie efectuată creată: {Id} - {Denumire}", id, endoscopie.DenumireEndoscopie);
        return id;
    }

    public async Task<IEnumerable<ConsultatieEndoscopieEfectuata>> GetByPacientIdAsync(Guid pacientId, CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.CreateConnection();

        // Map DB columns to entity properties
        const string sql = @"
            SELECT e.Id, e.RecomandareID, e.ConsultatieID, e.PacientID, 
                   e.EndoscopieID AS EndoscopieNomenclatorID,
                   e.NumeEndoscopie AS DenumireEndoscopie,
                   e.DataEfectuare, 
                   e.Laborator AS CentrulMedical, 
                   e.MedicExecutant,
                   e.TipAnestezie,
                   e.DescriereProcedurii,
                   e.Rezultat, 
                   e.LocBiopsie AS BiopsiiPrelevate, 
                   e.Concluzie AS Concluzii, 
                   e.Complicatii,
                   e.CaleDocument AS CaleFisierRezultat,
                   e.DataCreare, e.CreatDe, 
                   e.DataUltimeiModificari AS DataModificare, e.ModificatDe,
                   n.Denumire AS NomenclatorDenumire, n.Categorie AS NomenclatorCategorie
            FROM dbo.ConsultatieEndoscopieEfectuata e
            LEFT JOIN dbo.NomenclatorEndoscopii n ON e.EndoscopieID = n.Id
            WHERE e.PacientID = @PacientID
            ORDER BY e.DataEfectuare DESC";

        return await connection.QueryAsync<ConsultatieEndoscopieEfectuata>(sql, new { PacientID = pacientId });
    }

    public async Task<IEnumerable<ConsultatieEndoscopieEfectuata>> GetByConsultatieIdAsync(Guid consultatieId, CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.CreateConnection();

        // Map DB columns to entity properties
        const string sql = @"
            SELECT e.Id, e.RecomandareID, e.ConsultatieID, e.PacientID, 
                   e.EndoscopieID AS EndoscopieNomenclatorID,
                   e.NumeEndoscopie AS DenumireEndoscopie,
                   e.DataEfectuare, 
                   e.Laborator AS CentrulMedical, 
                   e.MedicExecutant,
                   e.TipAnestezie,
                   e.DescriereProcedurii,
                   e.Rezultat, 
                   e.LocBiopsie AS BiopsiiPrelevate, 
                   e.Concluzie AS Concluzii, 
                   e.Complicatii,
                   e.CaleDocument AS CaleFisierRezultat,
                   e.DataCreare, e.CreatDe, 
                   e.DataUltimeiModificari AS DataModificare, e.ModificatDe,
                   n.Denumire AS NomenclatorDenumire, n.Categorie AS NomenclatorCategorie
            FROM dbo.ConsultatieEndoscopieEfectuata e
            LEFT JOIN dbo.NomenclatorEndoscopii n ON e.EndoscopieID = n.Id
            WHERE e.ConsultatieID = @ConsultatieID
            ORDER BY e.DataEfectuare DESC";

        return await connection.QueryAsync<ConsultatieEndoscopieEfectuata>(sql, new { ConsultatieID = consultatieId });
    }

    public async Task<bool> UpdateAsync(ConsultatieEndoscopieEfectuata endoscopie, CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.CreateConnection();

        // Map entity properties to DB columns
        const string sql = @"
            UPDATE dbo.ConsultatieEndoscopieEfectuata
            SET EndoscopieID = @EndoscopieID,
                NumeEndoscopie = @NumeEndoscopie,
                DataEfectuare = @DataEfectuare,
                Laborator = @Laborator,
                MedicExecutant = @MedicExecutant,
                Rezultat = @Rezultat,
                Concluzie = @Concluzie,
                BiopsiePrelevata = @BiopsiePrelevata,
                LocBiopsie = @LocBiopsie,
                CaleDocument = @CaleDocument,
                DataUltimeiModificari = @DataModificare,
                ModificatDe = @ModificatDe
            WHERE Id = @Id";

        var rows = await connection.ExecuteAsync(sql, new
        {
            endoscopie.Id,
            EndoscopieID = endoscopie.EndoscopieNomenclatorID,
            NumeEndoscopie = endoscopie.DenumireEndoscopie,
            endoscopie.DataEfectuare,
            Laborator = endoscopie.CentrulMedical,
            endoscopie.MedicExecutant,
            endoscopie.Rezultat,
            Concluzie = endoscopie.Concluzii,
            BiopsiePrelevata = !string.IsNullOrEmpty(endoscopie.BiopsiiPrelevate),
            LocBiopsie = endoscopie.BiopsiiPrelevate,
            CaleDocument = endoscopie.CaleFisierRezultat,
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

        // Map DB columns to entity properties
        const string sql = @"
            SELECT e.Id, e.RecomandareID, e.ConsultatieID, e.PacientID, 
                   e.EndoscopieID AS EndoscopieNomenclatorID,
                   e.NumeEndoscopie AS DenumireEndoscopie,
                   e.DataEfectuare, 
                   e.Laborator AS CentrulMedical, 
                   e.MedicExecutant,
                   e.TipAnestezie,
                   e.DescriereProcedurii,
                   e.Rezultat, 
                   e.LocBiopsie AS BiopsiiPrelevate, 
                   e.Concluzie AS Concluzii, 
                   e.Complicatii,
                   e.CaleDocument AS CaleFisierRezultat,
                   e.DataCreare, e.CreatDe, 
                   e.DataUltimeiModificari AS DataModificare, e.ModificatDe,
                   n.Denumire AS NomenclatorDenumire, n.Categorie AS NomenclatorCategorie
            FROM dbo.ConsultatieEndoscopieEfectuata e
            LEFT JOIN dbo.NomenclatorEndoscopii n ON e.EndoscopieID = n.Id
            WHERE e.Id = @Id";

        return await connection.QueryFirstOrDefaultAsync<ConsultatieEndoscopieEfectuata>(sql, new { Id = id });
    }
}
