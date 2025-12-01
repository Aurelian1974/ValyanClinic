using Dapper;
using ValyanClinic.Domain.Entities;
using ValyanClinic.Domain.Interfaces.Repositories;
using ValyanClinic.Infrastructure.Data;
using Microsoft.Extensions.Logging;

namespace ValyanClinic.Infrastructure.Repositories;

/// <summary>
/// Repository pentru Pacient cu Dapper si Stored Procedures
/// </summary>
public class PacientRepository : BaseRepository, IPacientRepository
{
    private readonly ILogger<PacientRepository> _logger;

    public PacientRepository(
        IDbConnectionFactory connectionFactory,
        ILogger<PacientRepository> logger)
        : base(connectionFactory)
    {
        _logger = logger;
    }

    public async Task<Pacient?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var parameters = new { Id = id };
        return await QueryFirstOrDefaultAsync<Pacient>("sp_Pacienti_GetById", parameters, cancellationToken);
    }

    public async Task<Pacient?> GetByCodPacientAsync(string codPacient, CancellationToken cancellationToken = default)
    {
        var parameters = new { CodPacient = codPacient };
        return await QueryFirstOrDefaultAsync<Pacient>("sp_Pacienti_GetByCodPacient", parameters, cancellationToken);
    }

    public async Task<Pacient?> GetByCNPAsync(string cnp, CancellationToken cancellationToken = default)
    {
        var parameters = new { CNP = cnp };
        return await QueryFirstOrDefaultAsync<Pacient>("sp_Pacienti_GetByCNP", parameters, cancellationToken);
    }

    public async Task<IEnumerable<Pacient>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await QueryAsync<Pacient>("sp_Pacienti_GetAll", cancellationToken: cancellationToken);
    }

    public async Task<(IEnumerable<Pacient> Items, int TotalCount)> GetPagedAsync(
        int pageNumber,
        int pageSize,
        string? searchText = null,
        string? judet = null,
        bool? asigurat = null,
        bool? activ = null,
        string sortColumn = "Nume",
        string sortDirection = "ASC",
        CancellationToken cancellationToken = default)
    {
        // ✅ ADDED: Detailed logging pentru diagnosticare
        _logger.LogWarning("🔍 [PacientRepository.GetPagedAsync] CALL START");
        _logger.LogWarning("  📥 Parameters:");
        _logger.LogWarning("    PageNumber   = {PageNumber}", pageNumber);
        _logger.LogWarning("    PageSize     = {PageSize}", pageSize);
        _logger.LogWarning("    SearchText   = {SearchText}", searchText ?? "NULL");
        _logger.LogWarning("    Judet        = {Judet}", judet ?? "NULL");
        _logger.LogWarning("    Asigurat     = {Asigurat}", asigurat?.ToString() ?? "NULL");
        _logger.LogWarning("    Activ        = {Activ}", activ?.ToString() ?? "NULL");
        _logger.LogWarning("    SortColumn   = {SortColumn}", sortColumn);
        _logger.LogWarning("    SortDirection= {SortDirection}", sortDirection);

        var parameters = new
        {
            PageNumber = pageNumber,
            PageSize = pageSize,
            SearchText = searchText,
            Judet = judet,
            Asigurat = asigurat,
            Activ = activ,
            SortColumn = sortColumn,
            SortDirection = sortDirection
        };

        _logger.LogWarning("  📦 Dapper parameters object created:");
        _logger.LogWarning("    {Parameters}", System.Text.Json.JsonSerializer.Serialize(parameters));

        using var connection = _connectionFactory.CreateConnection();
        
        _logger.LogWarning("  🔌 Connection created: {ConnectionString}", connection.ConnectionString.Substring(0, Math.Min(50, connection.ConnectionString.Length)) + "...");
        _logger.LogWarning("  📞 Calling sp_Pacienti_GetAll via Dapper...");

        using var multi = await connection.QueryMultipleAsync(
            "sp_Pacienti_GetAll",
            parameters,
            commandType: System.Data.CommandType.StoredProcedure);

        var items = await multi.ReadAsync<Pacient>();
        var itemsList = items.ToList();

        _logger.LogWarning("  📊 SP returned {Count} items", itemsList.Count);

        // Get count
        _logger.LogWarning("  📞 Calling sp_Pacienti_GetCount...");
        var count = await GetCountAsync(searchText, judet, asigurat, activ, cancellationToken);

        _logger.LogWarning("  📊 Total count: {TotalCount}", count);
        _logger.LogWarning("🔍 [PacientRepository.GetPagedAsync] CALL END");

        // ✅ ADDED: Diagnostic warning daca returneaza 0
        if (itemsList.Count == 0 && count == 0)
        {
            _logger.LogError("❌ [PacientRepository] ZERO RECORDS RETURNED!");
            _logger.LogError("   This might be caused by NULL parameter handling bug in sp_Pacienti_GetAll");
            _logger.LogError("   Check if SP has: WHERE (@Activ IS NULL OR Activ = @Activ)");
            _logger.LogError("   Not: WHERE Activ = @Activ");
            
            // Test query to verify data exists
            var totalInDb = await connection.ExecuteScalarAsync<int>(
                "SELECT COUNT(*) FROM Pacienti",
                commandType: System.Data.CommandType.Text);
            
            _logger.LogError("   Total pacienti in DB (without filters): {Total}", totalInDb);
            
            if (totalInDb > 0)
            {
                _logger.LogError("   ⚠️  CONFIRMED: SP bug - DB has {Total} records but SP returned 0", totalInDb);
                _logger.LogError("   ✅ FIX: Run DevSupport/Fixes/MASTER_FIX_AdministrarePacienti_NULL_Handling.sql");
            }
        }

        return (itemsList, count);
    }

    public async Task<int> GetCountAsync(
        string? searchText = null,
        string? judet = null,
        bool? asigurat = null,
        bool? activ = null,
        CancellationToken cancellationToken = default)
    {
        // ✅ ADDED: Logging pentru GetCountAsync
        _logger.LogWarning("🔍 [PacientRepository.GetCountAsync] CALL");
        _logger.LogWarning("  Parameters: SearchText={SearchText}, Judet={Judet}, Asigurat={Asigurat}, Activ={Activ}",
            searchText ?? "NULL", judet ?? "NULL", asigurat?.ToString() ?? "NULL", activ?.ToString() ?? "NULL");

        var parameters = new
        {
            SearchText = searchText,
            Judet = judet,
            Asigurat = asigurat,
            Activ = activ
        };

        using var connection = _connectionFactory.CreateConnection();
        var result = await connection.ExecuteScalarAsync<int>(
            "sp_Pacienti_GetCount",
            parameters,
            commandType: System.Data.CommandType.StoredProcedure);

        _logger.LogWarning("  Result: {Count}", result);

        return result;
    }

    public async Task<IEnumerable<(string Value, string Text, int NumarPacienti)>> GetJudeteAsync(
        CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.CreateConnection();
        var results = await connection.QueryAsync<JudetOption>(
            "sp_Pacienti_GetJudete",
            commandType: System.Data.CommandType.StoredProcedure);

        return results.Select(r => (r.Value, r.Text, r.NumarPacienti));
    }

    public async Task<IEnumerable<(Guid Id, string DisplayText, string? CNP, DateTime DataNasterii)>> GetDropdownOptionsAsync(
        bool activ = true,
        CancellationToken cancellationToken = default)
    {
        var parameters = new { Activ = activ };

        using var connection = _connectionFactory.CreateConnection();
        var results = await connection.QueryAsync<DropdownOption>(
            "sp_Pacienti_GetDropdownOptions",
            parameters,
            commandType: System.Data.CommandType.StoredProcedure);

        return results.Select(r => (Guid.Parse(r.Value), r.Text, r.CNP, r.Data_Nasterii));
    }

    public async Task<Pacient> CreateAsync(Pacient pacient, CancellationToken cancellationToken = default)
    {
        var parameters = new
        {
            pacient.Cod_Pacient,
            pacient.CNP,
            pacient.Nume,
            pacient.Prenume,
            pacient.Data_Nasterii,
            pacient.Sex,
            pacient.Telefon,
            pacient.Telefon_Secundar,
            pacient.Email,
            pacient.Judet,
            pacient.Localitate,
            pacient.Adresa,
            pacient.Cod_Postal,
            pacient.Asigurat,
            pacient.CNP_Asigurat,
            pacient.Nr_Card_Sanatate,
            pacient.Casa_Asigurari,
            pacient.Alergii,
            pacient.Boli_Cronice,
            pacient.Medic_Familie,
            pacient.Persoana_Contact,
            pacient.Telefon_Urgenta,
            pacient.Relatie_Contact,
            pacient.Activ,
            pacient.Observatii,
            CreatDe = pacient.Creat_De
        };

        var result = await QueryFirstOrDefaultAsync<Pacient>("sp_Pacienti_Create", parameters, cancellationToken);
        return result ?? throw new InvalidOperationException("Failed to create pacient");
    }

    public async Task<Pacient> UpdateAsync(Pacient pacient, CancellationToken cancellationToken = default)
    {
        var parameters = new
        {
            pacient.Id,
            pacient.CNP,
            pacient.Nume,
            pacient.Prenume,
            pacient.Data_Nasterii,
            pacient.Sex,
            pacient.Telefon,
            pacient.Telefon_Secundar,
            pacient.Email,
            pacient.Judet,
            pacient.Localitate,
            pacient.Adresa,
            pacient.Cod_Postal,
            pacient.Asigurat,
            pacient.CNP_Asigurat,
            pacient.Nr_Card_Sanatate,
            pacient.Casa_Asigurari,
            pacient.Alergii,
            pacient.Boli_Cronice,
            pacient.Medic_Familie,
            pacient.Persoana_Contact,
            pacient.Telefon_Urgenta,
            pacient.Relatie_Contact,
            pacient.Activ,
            pacient.Observatii,
            ModificatDe = pacient.Modificat_De
        };

        var result = await QueryFirstOrDefaultAsync<Pacient>("sp_Pacienti_Update", parameters, cancellationToken);
        return result ?? throw new InvalidOperationException("Failed to update pacient");
    }

    public async Task<bool> DeleteAsync(Guid id, string modificatDe, CancellationToken cancellationToken = default)
    {
        var parameters = new
        {
            Id = id,
            ModificatDe = modificatDe
        };

        var result = await QueryFirstOrDefaultAsync<SuccessResult>("sp_Pacienti_Delete", parameters, cancellationToken);
        return result?.Success == 1;
    }

    public async Task<bool> HardDeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var parameters = new { Id = id };
        var result = await QueryFirstOrDefaultAsync<SuccessResult>("sp_Pacienti_HardDelete", parameters, cancellationToken);
        return result?.Success == 1;
    }

    public async Task<string> GenerateNextCodPacientAsync(CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.CreateConnection();
        var result = await connection.QueryFirstOrDefaultAsync<string>(
            "sp_Pacienti_GenerateNextCodPacient",
            commandType: System.Data.CommandType.StoredProcedure);

        return result ?? "PACIENT00000001";
    }

    public async Task<(bool CnpExists, bool CodPacientExists)> CheckUniqueAsync(
        string? cnp = null,
        string? codPacient = null,
        Guid? excludeId = null,
        CancellationToken cancellationToken = default)
    {
        var parameters = new
        {
            CNP = cnp,
            Cod_Pacient = codPacient,
            ExcludeId = excludeId
        };

        var result = await QueryFirstOrDefaultAsync<UniquenessCheckResult>(
            "sp_Pacienti_CheckUnique",
            parameters,
            cancellationToken);

        return (result?.CNP_Exists == 1, result?.CodPacient_Exists == 1);
    }

    public async Task<bool> UpdateUltimaVizitaAsync(
        Guid id,
        DateTime dataVizita,
        string modificatDe,
        CancellationToken cancellationToken = default)
    {
        var parameters = new
        {
            Id = id,
            DataVizita = dataVizita,
            ModificatDe = modificatDe
        };

        var result = await QueryFirstOrDefaultAsync<SuccessResult>(
            "sp_Pacienti_UpdateUltimaVizita",
            parameters,
            cancellationToken);

        return result?.Success == 1;
    }

    public async Task<Dictionary<string, (int Total, int Activi)>> GetStatisticsAsync(
        CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.CreateConnection();
        var results = await connection.QueryAsync<StatisticResult>(
            "sp_Pacienti_GetStatistics",
            commandType: System.Data.CommandType.StoredProcedure);

        return results.ToDictionary(
            r => r.Categorie,
            r => (r.Numar, r.Activi));
    }

    public async Task<IEnumerable<Pacient>> GetBirthdaysAsync(
        DateTime? startDate = null,
        DateTime? endDate = null,
        CancellationToken cancellationToken = default)
    {
        var parameters = new
        {
            StartDate = startDate,
            EndDate = endDate
        };

        return await QueryAsync<Pacient>("sp_Pacienti_GetBirthdays", parameters, cancellationToken);
    }

    // DTOs pentru maparea rezultatelor SP-urilor
    private class JudetOption
    {
        public string Value { get; set; } = string.Empty;
        public string Text { get; set; } = string.Empty;
        public int NumarPacienti { get; set; }
    }

    private class DropdownOption
    {
        public string Value { get; set; } = string.Empty;
        public string Text { get; set; } = string.Empty;
        public string? CNP { get; set; }
        public DateTime Data_Nasterii { get; set; }
    }

    private class UniquenessCheckResult
    {
        public int CNP_Exists { get; set; }
        public int CodPacient_Exists { get; set; }
    }

    private class SuccessResult
    {
        public int Success { get; set; }
        public string? Message { get; set; }
    }

    private class StatisticResult
    {
        public string Categorie { get; set; } = string.Empty;
        public int Numar { get; set; }
        public int Activi { get; set; }
    }
}
