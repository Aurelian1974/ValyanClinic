using Dapper;
using Microsoft.Extensions.Logging;
using System.Data;
using ValyanClinic.Domain.Entities;
using ValyanClinic.Domain.Interfaces.Repositories;
using ValyanClinic.Infrastructure.Data;

namespace ValyanClinic.Infrastructure.Repositories;

/// <summary>
/// Repository pentru analize medicale în cadrul consultației
/// </summary>
public class ConsultatieAnalizaMedicalaRepository : IConsultatieAnalizaMedicalaRepository
{
    private readonly IDbConnectionFactory _connectionFactory;
    private readonly ILogger<ConsultatieAnalizaMedicalaRepository> _logger;

    public ConsultatieAnalizaMedicalaRepository(
        IDbConnectionFactory connectionFactory,
        ILogger<ConsultatieAnalizaMedicalaRepository> logger)
    {
        _connectionFactory = connectionFactory;
        _logger = logger;
    }

    public async Task<Guid> CreateAsync(ConsultatieAnalizaMedicala analiza, CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.CreateConnection();
        
        var sql = @"
            INSERT INTO ConsultatieAnalizeMedicale (
                Id, ConsultatieID, TipAnaliza, NumeAnaliza, CodAnaliza,
                StatusAnaliza, DataRecomandare, DataProgramata, Prioritate, EsteCito,
                IndicatiiClinice, ObservatiiRecomandare, Pret, Decontat,
                DataCreare, CreatDe
            )
            VALUES (
                @Id, @ConsultatieID, @TipAnaliza, @NumeAnaliza, @CodAnaliza,
                @StatusAnaliza, @DataRecomandare, @DataProgramata, @Prioritate, @EsteCito,
                @IndicatiiClinice, @ObservatiiRecomandare, @Pret, @Decontat,
                @DataCreare, @CreatDe
            )";

        analiza.Id = Guid.NewGuid();
        analiza.DataCreare = DateTime.Now;

        await connection.ExecuteAsync(sql, analiza);

        _logger.LogInformation(
            "Analiză medicală creată: {Id} pentru consultație {ConsultatieID}",
            analiza.Id, analiza.ConsultatieID);

        return analiza.Id;
    }

    public async Task<bool> UpdateAsync(ConsultatieAnalizaMedicala analiza, CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.CreateConnection();
        
        var sql = @"
            UPDATE ConsultatieAnalizeMedicale
            SET 
                StatusAnaliza = @StatusAnaliza,
                DataProgramata = @DataProgramata,
                DataEfectuare = @DataEfectuare,
                LocEfectuare = @LocEfectuare,
                AreRezultate = @AreRezultate,
                DataRezultate = @DataRezultate,
                ValoareRezultat = @ValoareRezultat,
                UnitatiMasura = @UnitatiMasura,
                ValoareNormalaMin = @ValoareNormalaMin,
                ValoareNormalaMax = @ValoareNormalaMax,
                EsteInAfaraLimitelor = @EsteInAfaraLimitelor,
                InterpretareMedic = @InterpretareMedic,
                ConclusiiAnaliza = @ConclusiiAnaliza,
                CaleFisierRezultat = @CaleFisierRezultat,
                TipFisier = @TipFisier,
                DimensiuneFisier = @DimensiuneFisier,
                DataUltimeiModificari = @DataUltimeiModificari,
                ModificatDe = @ModificatDe
            WHERE Id = @Id";

        analiza.DataUltimeiModificari = DateTime.Now;

        var rowsAffected = await connection.ExecuteAsync(sql, analiza);

        _logger.LogInformation(
            "Analiză medicală actualizată: {Id}, status={Status}",
            analiza.Id, analiza.StatusAnaliza);

        return rowsAffected > 0;
    }

    public async Task<bool> DeleteAsync(Guid analizaId, CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.CreateConnection();
        
        var sql = "DELETE FROM ConsultatieAnalizeMedicale WHERE Id = @Id";
        var rowsAffected = await connection.ExecuteAsync(sql, new { Id = analizaId });

        _logger.LogInformation("Analiză medicală ștearsă: {Id}", analizaId);

        return rowsAffected > 0;
    }

    public async Task<ConsultatieAnalizaMedicala?> GetByIdAsync(Guid analizaId, CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.CreateConnection();
        
        var sql = "SELECT * FROM ConsultatieAnalizeMedicale WHERE Id = @Id";
        var analiza = await connection.QueryFirstOrDefaultAsync<ConsultatieAnalizaMedicala>(
            sql,
            new { Id = analizaId });

        if (analiza != null)
        {
            // Încarcă și detaliile (parametrii)
            analiza.Detalii = (await GetDetaliiByAnalizaIdAsync(analizaId, cancellationToken)).ToList();
        }

        return analiza;
    }

    public async Task<IEnumerable<ConsultatieAnalizaMedicala>> GetByConsultatieIdAsync(
        Guid consultatieId,
        CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.CreateConnection();
        
        var sql = @"
            SELECT *
            FROM ConsultatieAnalizeMedicale
            WHERE ConsultatieID = @ConsultatieID
            ORDER BY DataRecomandare DESC";

        var analize = await connection.QueryAsync<ConsultatieAnalizaMedicala>(
            sql,
            new { ConsultatieID = consultatieId });

        return analize;
    }

    public async Task<IEnumerable<ConsultatieAnalizaMedicala>> GetByPacientIdAsync(
        Guid pacientId,
        bool doarCuRezultate = false,
        DateTime? dataStart = null,
        DateTime? dataEnd = null,
        CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.CreateConnection();
        
        var sql = @"
            SELECT a.*
            FROM ConsultatieAnalizeMedicale a
            INNER JOIN Consultatii c ON a.ConsultatieID = c.ConsultatieID
            WHERE c.PacientID = @PacientID
              AND (@DoarCuRezultate = 0 OR a.AreRezultate = 1)
              AND (@DataStart IS NULL OR a.DataRecomandare >= @DataStart)
              AND (@DataEnd IS NULL OR a.DataRecomandare <= @DataEnd)
            ORDER BY a.DataRecomandare DESC";

        var analize = await connection.QueryAsync<ConsultatieAnalizaMedicala>(
            sql,
            new 
            { 
                PacientID = pacientId,
                DoarCuRezultate = doarCuRezultate,
                DataStart = dataStart,
                DataEnd = dataEnd
            });

        return analize;
    }

    public async Task<Guid> AddDetaliu(ConsultatieAnalizaDetaliu detaliu, CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.CreateConnection();
        
        var sql = @"
            INSERT INTO ConsultatieAnalizaDetalii (
                Id, AnalizaMedicalaID, NumeParametru, CodParametru, Valoare, UnitatiMasura,
                TipValoare, ValoareNormalaMin, ValoareNormalaMax, ValoareNormalaText,
                EsteAnormal, NivelGravitate, Observatii, DataCreare
            )
            VALUES (
                @Id, @AnalizaMedicalaID, @NumeParametru, @CodParametru, @Valoare, @UnitatiMasura,
                @TipValoare, @ValoareNormalaMin, @ValoareNormalaMax, @ValoareNormalaText,
                @EsteAnormal, @NivelGravitate, @Observatii, @DataCreare
            )";

        detaliu.Id = Guid.NewGuid();
        detaliu.DataCreare = DateTime.Now;

        await connection.ExecuteAsync(sql, detaliu);

        return detaliu.Id;
    }

    public async Task<IEnumerable<ConsultatieAnalizaDetaliu>> GetDetaliiByAnalizaIdAsync(
        Guid analizaId,
        CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.CreateConnection();
        
        var sql = @"
            SELECT *
            FROM ConsultatieAnalizaDetalii
            WHERE AnalizaMedicalaID = @AnalizaID
            ORDER BY NumeParametru";

        var detalii = await connection.QueryAsync<ConsultatieAnalizaDetaliu>(
            sql,
            new { AnalizaID = analizaId });

        return detalii;
    }

    public async Task<bool> UpdateStatusAsync(
        Guid analizaId,
        string newStatus,
        CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.CreateConnection();
        
        var sql = @"
            UPDATE ConsultatieAnalizeMedicale
            SET StatusAnaliza = @NewStatus,
                DataUltimeiModificari = @DataModificare
            WHERE Id = @Id";

        var rowsAffected = await connection.ExecuteAsync(
            sql,
            new 
            { 
                Id = analizaId,
                NewStatus = newStatus,
                DataModificare = DateTime.Now
            });

        _logger.LogInformation(
            "Status analiză actualizat: {Id} → {NewStatus}",
            analizaId, newStatus);

        return rowsAffected > 0;
    }
}
