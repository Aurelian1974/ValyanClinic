using Dapper;
using Microsoft.Extensions.Logging;
using ValyanClinic.Domain.Entities;
using ValyanClinic.Domain.Interfaces.Repositories;
using ValyanClinic.Domain.Interfaces.Data;
using ValyanClinic.Infrastructure.Data;

namespace ValyanClinic.Infrastructure.Repositories;

/// <summary>
/// Repository pentru gestionarea programărilor medicale folosind Dapper și Stored Procedures.
/// Implementează pattern-ul Repository cu retry logic și error handling.
/// </summary>
public class ProgramareRepository : BaseRepository, IProgramareRepository
{
    private readonly ILogger<ProgramareRepository> _logger;

    public ProgramareRepository(
        IDbConnectionFactory connectionFactory,
  ILogger<ProgramareRepository> logger)
        : base(connectionFactory, logger)
    {
        _logger = logger;
    }

    // ==================== READ OPERATIONS - SINGLE ====================

    /// <inheritdoc />
    public async Task<Programare?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Obținere programare după ID: {ProgramareID}", id);

        // ✅ FIX: Parametru corect - SP așteaptă @ProgramareID, nu @Id!
        var parameters = new { ProgramareID = id };
        var result = await QueryFirstOrDefaultAsync<ProgramareDto>(
            "sp_Programari_GetById",
     parameters,
        cancellationToken);

        if (result == null)
        {
            _logger.LogWarning("Programarea cu ID {ProgramareID} nu a fost găsită", id);
            return null;
        }

        return MapToEntity(result);
    }

    // ==================== READ OPERATIONS - COLLECTION (PAGINARE & FILTRARE) ====================

    /// <inheritdoc />
    public async Task<IEnumerable<Programare>> GetAllAsync(
        int pageNumber = 1,
     int pageSize = 50,
        string? searchText = null,
Guid? doctorID = null,
   Guid? pacientID = null,
        DateTime? dataStart = null,
        DateTime? dataEnd = null,
        string? status = null,
   string? tipProgramare = null,
    string sortColumn = "DataProgramare",
        string sortDirection = "ASC",
  CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
        "Obținere listă programări: Page={Page}, Size={Size}, Search={Search}, Doctor={Doctor}, Pacient={Pacient}, DataStart={DataStart}, DataEnd={DataEnd}, Status={Status}, Tip={Tip}",
     pageNumber, pageSize, searchText, doctorID, pacientID, dataStart, dataEnd, status, tipProgramare);

        var parameters = new
        {
            PageNumber = pageNumber,
            PageSize = pageSize,
            SearchText = searchText,
            DoctorID = doctorID,
            PacientID = pacientID,
            DataStart = dataStart,
            DataEnd = dataEnd,
            Status = status,
            TipProgramare = tipProgramare,
            SortColumn = sortColumn,
            SortDirection = sortDirection
        };

        var results = await QueryAsync<ProgramareDto>(
"sp_Programari_GetAll",
 parameters,
            cancellationToken);

        return results.Select(MapToEntity);
    }

    /// <inheritdoc />
    public async Task<int> GetCountAsync(
        string? searchText = null,
        Guid? doctorID = null,
        Guid? pacientID = null,
        DateTime? dataStart = null,
        DateTime? dataEnd = null,
        string? status = null,
  string? tipProgramare = null,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
         "Obținere număr total programări cu filtre: Search={Search}, Doctor={Doctor}, Pacient={Pacient}",
 searchText, doctorID, pacientID);

        var parameters = new
        {
            SearchText = searchText,
            DoctorID = doctorID,
            PacientID = pacientID,
            DataStart = dataStart,
            DataEnd = dataEnd,
            Status = status,
            TipProgramare = tipProgramare
        };

        using var connection = _connectionFactory.CreateConnection();
        var result = await connection.ExecuteScalarAsync<int>(
       "sp_Programari_GetCount",
      parameters,
                commandType: System.Data.CommandType.StoredProcedure);

        _logger.LogInformation("Număr total programări găsite: {Count}", result);
        return result;
    }

    // ==================== SPECIALIZED QUERIES (BUSINESS LOGIC) ====================

    /// <inheritdoc />
    public async Task<IEnumerable<Programare>> GetByDoctorAsync(
        Guid doctorID,
   DateTime? dataStart = null,
        DateTime? dataEnd = null,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
    "Obținere programări pentru doctor: {DoctorID}, DataStart={DataStart}, DataEnd={DataEnd}",
               doctorID, dataStart, dataEnd);

        var parameters = new
        {
            DoctorID = doctorID,
            DataStart = dataStart,
            DataEnd = dataEnd
        };

        var results = await QueryAsync<ProgramareDto>(
      "sp_Programari_GetByDoctor",
      parameters,
            cancellationToken);

        return results.Select(MapToEntity);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<Programare>> GetByPacientAsync(
    Guid pacientID,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Obținere istoric programări pentru pacient: {PacientID}", pacientID);

        var parameters = new { PacientID = pacientID };

        var results = await QueryAsync<ProgramareDto>(
          "sp_Programari_GetByPacient",
              parameters,
        cancellationToken);

        return results.Select(MapToEntity);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<Programare>> GetByDateAsync(
  DateTime date,
      Guid? doctorID = null,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
      "Obținere programări pentru data: {Date}, Doctor={DoctorID}",
        date.ToString("yyyy-MM-dd"), doctorID);

        // ✅ FIX: SP așteaptă @DataProgramare, nu @Data!
        var parameters = new
        {
            DataProgramare = date,
            DoctorID = doctorID
        };

        var results = await QueryAsync<ProgramareDto>(
            "sp_Programari_GetByDate", parameters, cancellationToken);

        return results.Select(MapToEntity);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<Programare>> GetByDateRangeAsync(
     DateTime startDate,
   DateTime endDate,
 Guid? doctorID = null,
     CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
          "⚡ GetByDateRange - OPTIMIZED: {StartDate} - {EndDate}, Doctor={DoctorID}",
        startDate.ToString("yyyy-MM-dd"),
          endDate.ToString("yyyy-MM-dd"),
         doctorID);

        var parameters = new
        {
            DataStart = startDate,
            DataEnd = endDate,
            DoctorID = doctorID
        };

        var results = await QueryAsync<ProgramareDto>(
             "sp_Programari_GetByDateRange",
             parameters,
             cancellationToken);

        var programari = results.Select(MapToEntity).ToList();

        _logger.LogInformation(
        "✅ Loaded {Count} programări for date range in SINGLE query",
            programari.Count);

        return programari;
    }

    /// <inheritdoc />
    public async Task<IEnumerable<Programare>> GetUpcomingAsync(
        int days = 7,
        Guid? doctorID = null,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
        "Obținere programări viitoare: Zile={Days}, Doctor={DoctorID}",
        days, doctorID);

        var parameters = new
        {
            Zile = days,
            DoctorID = doctorID
        };

        var results = await QueryAsync<ProgramareDto>(
                            "sp_Programari_GetUpcoming", parameters, cancellationToken);
        return results.Select(MapToEntity);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<Programare>> GetDoctorScheduleAsync(
        Guid doctorID,
   DateTime dataStart,
        DateTime dataEnd,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Obținere program doctor: {DoctorID}, De la {DataStart} până la {DataEnd}",
    doctorID, dataStart.ToString("yyyy-MM-dd"), dataEnd.ToString("yyyy-MM-dd"));

        var parameters = new
        {
            DoctorID = doctorID,
            DataStart = dataStart,
            DataEnd = dataEnd
        };

        var results = await QueryAsync<ProgramareDto>(
  "sp_Programari_GetDoctorSchedule",
       parameters,
            cancellationToken);

        return results.Select(MapToEntity);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<Programare>> GetPacientHistoryAsync(
Guid pacientID,
        int ultimeleZile = 365,
  CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
  "Obținere istoric pacient: {PacientID}, Ultimele {Zile} zile",
   pacientID, ultimeleZile);

        var parameters = new
        {
            PacientID = pacientID,
            UltimeleZile = ultimeleZile
        };

        var results = await QueryAsync<ProgramareDto>(
       "sp_Programari_GetPacientHistory",
            parameters,
 cancellationToken);

        return results.Select(MapToEntity);
    }

    // ==================== WRITE OPERATIONS (CRUD) ====================

    /// <inheritdoc />
    public async Task<Programare> CreateAsync(
        Programare programare,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
     "Creare programare nouă: Pacient={PacientID}, Doctor={DoctorID}, Data={Data}, Ora={Ora}",
    programare.PacientID, programare.DoctorID,
    programare.DataProgramare.ToString("yyyy-MM-dd"),
        programare.OraInceput.ToString(@"hh\:mm"));

        var parameters = new
        {
            programare.PacientID,
            programare.DoctorID,
            programare.DataProgramare,
            programare.OraInceput,
            programare.OraSfarsit,
            programare.TipProgramare,
            programare.Status,
            programare.Observatii,
            programare.CreatDe
        };

        var result = await QueryFirstOrDefaultAsync<ProgramareDto>(
                  "sp_Programari_Create",
             parameters,
             cancellationToken);

        if (result == null)
        {
            _logger.LogError("Eroare la crearea programării - SP nu a returnat rezultat");
            throw new InvalidOperationException("Crearea programării a eșuat");
        }

        _logger.LogInformation("Programare creată cu succes: {ProgramareID}", result.ProgramareID);
        return MapToEntity(result);
    }

    /// <inheritdoc />
    public async Task<Programare> UpdateAsync(
        Programare programare,
 CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Actualizare programare: {ProgramareID}, Data={Data}, Ora={Ora}, Status={Status}",
      programare.ProgramareID,
    programare.DataProgramare.ToString("yyyy-MM-dd"),
   programare.OraInceput.ToString(@"hh\:mm"),
    programare.Status);

        var parameters = new
        {
            programare.ProgramareID,
            programare.PacientID,
            programare.DoctorID,
            programare.DataProgramare,
            programare.OraInceput,
            programare.OraSfarsit,
            programare.TipProgramare,
            programare.Status,
            programare.Observatii,
            programare.ModificatDe
        };

        var result = await QueryFirstOrDefaultAsync<ProgramareDto>(
               "sp_Programari_Update",
               parameters,
             cancellationToken);

        if (result == null)
        {
            _logger.LogError("Eroare la actualizarea programării {ProgramareID} - SP nu a returnat rezultat", programare.ProgramareID);
            throw new InvalidOperationException($"Actualizarea programării {programare.ProgramareID} a eșuat");
        }

        _logger.LogInformation("Programare actualizată cu succes: {ProgramareID}", programare.ProgramareID);
        return MapToEntity(result);
    }

    /// <inheritdoc />
    public async Task<bool> DeleteAsync(
        Guid id,
        Guid modificatDe,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Ștergere (anulare) programare: {ProgramareID} de către {ModificatDe}", id, modificatDe);

        var parameters = new
        {
            ProgramareID = id,
            ModificatDe = modificatDe
        };

        var result = await QueryFirstOrDefaultAsync<SuccessResult>(
            "sp_Programari_Delete",
        parameters,
    cancellationToken);

        var success = result?.Success == 1;

        if (success)
        {
            _logger.LogInformation("Programare anulată cu succes: {ProgramareID}", id);
        }
        else
        {
            _logger.LogWarning("Anularea programării {ProgramareID} a eșuat", id);
        }

        return success;
    }

    // ==================== BUSINESS OPERATIONS (VALIDĂRI) ====================

    /// <inheritdoc />
    public async Task<bool> CheckConflictAsync(
        Guid doctorID,
        DateTime dataProgramare,
        TimeSpan oraInceput,
        TimeSpan oraSfarsit,
        Guid? excludeProgramareID = null,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Verificare conflict: Doctor={DoctorID}, Data={Data}, Interval={OraInceput}-{OraSfarsit}, Exclude={Exclude}",
 doctorID, dataProgramare.ToString("yyyy-MM-dd"),
      oraInceput.ToString(@"hh\:mm"), oraSfarsit.ToString(@"hh\:mm"),
  excludeProgramareID);

        var parameters = new
        {
            DoctorID = doctorID,
            DataProgramare = dataProgramare,
            OraInceput = oraInceput,
            OraSfarsit = oraSfarsit,
            ExcludeProgramareID = excludeProgramareID
        };

        var result = await QueryFirstOrDefaultAsync<ConflictCheckResult>(
     "sp_Programari_CheckConflict",
            parameters,
       cancellationToken);

        var hasConflict = result?.ConflictExists == 1;

        if (hasConflict)
        {
            _logger.LogWarning(
             "Conflict detectat pentru doctor {DoctorID} la data {Data} în intervalul {Interval}",
              doctorID, dataProgramare.ToString("yyyy-MM-dd"),
                  $"{oraInceput:hh\\:mm}-{oraSfarsit:hh\\:mm}");
        }
        else
        {
            _logger.LogInformation("Nu există conflict pentru intervalul verificat");
        }

        return hasConflict;
    }

    // ==================== STATISTICS (RAPORTARE) ====================

    /// <inheritdoc />
    public async Task<Dictionary<string, object>> GetStatisticsAsync(
        DateTime? dataStart = null,
    DateTime? dataEnd = null,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Obținere statistici globale: DataStart={DataStart}, DataEnd={DataEnd}",
  dataStart?.ToString("yyyy-MM-dd"), dataEnd?.ToString("yyyy-MM-dd"));

        var parameters = new
        {
            DataStart = dataStart,
            DataEnd = dataEnd
        };

        // ✅ UPDATED: Folosește sp_Programari_GetStatistics_v2 (returnează un singur row cu toate statisticile)
        using var connection = _connectionFactory.CreateConnection();
        var result = await connection.QueryFirstOrDefaultAsync<StatisticsResult>(
                "sp_Programari_GetStatistics_v2",
                    parameters,
                    commandType: System.Data.CommandType.StoredProcedure);

        if (result == null)
        {
            _logger.LogWarning("Statisticile nu au putut fi obținute - result null");
            return new Dictionary<string, object>();
        }

        // ✅ Mapare result la Dictionary<string, object> pentru handler
        var statistics = new Dictionary<string, object>
        {
            ["TotalProgramari"] = result.TotalProgramari,
            ["Programate"] = result.Programate,
            ["Confirmate"] = result.Confirmate,
            ["CheckedIn"] = result.CheckedIn,
            ["InConsultatie"] = result.InConsultatie,
            ["Finalizate"] = result.Finalizate,
            ["Anulate"] = result.Anulate,
            ["NoShow"] = result.NoShow,
            ["ConsultatiiInitiale"] = result.ConsultatiiInitiale,
            ["ControalePeriodice"] = result.ControalePeriodice,
            ["Consultatii"] = result.Consultatii,
            ["Investigatii"] = result.Investigatii,
            ["Proceduri"] = result.Proceduri,
            ["Urgente"] = result.Urgente,
            ["Telemedicina"] = result.Telemedicina,
            ["LaDomiciliu"] = result.LaDomiciliu,
            ["MediciActivi"] = result.MediciActivi,
            ["PacientiUnici"] = result.PacientiUnici,
            ["DurataMedieMinute"] = result.DurataMedieMinute ?? 0.0
        };

        _logger.LogInformation("Statistici obținute: Total={Total}, Finalizate={Finalizate}",
            result.TotalProgramari, result.Finalizate);

        return statistics;
    }

    /// <inheritdoc />
    public async Task<Dictionary<string, object>> GetDoctorStatisticsAsync(
 Guid doctorID,
        DateTime? dataStart = null,
    DateTime? dataEnd = null,
   CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Obținere statistici doctor: {DoctorID}, DataStart={DataStart}, DataEnd={DataEnd}",
            doctorID, dataStart?.ToString("yyyy-MM-dd"), dataEnd?.ToString("yyyy-MM-dd"));

        var parameters = new
        {
            DoctorID = doctorID,
            DataStart = dataStart,
            DataEnd = dataEnd
        };

        // ✅ UPDATED: Folosește sp_Programari_GetDoctorStatistics_v2
        using var connection = _connectionFactory.CreateConnection();
        var result = await connection.QueryFirstOrDefaultAsync<StatisticsResult>(
      "sp_Programari_GetDoctorStatistics_v2",
      parameters,
   commandType: System.Data.CommandType.StoredProcedure);

        if (result == null)
        {
            _logger.LogWarning("Statisticile pentru doctor {DoctorID} nu au putut fi obținute", doctorID);
            return new Dictionary<string, object>();
        }

        // ✅ Mapare result la Dictionary<string, object>
        var statistics = new Dictionary<string, object>
        {
            ["TotalProgramari"] = result.TotalProgramari,
            ["Programate"] = result.Programate,
            ["Confirmate"] = result.Confirmate,
            ["CheckedIn"] = result.CheckedIn,
            ["InConsultatie"] = result.InConsultatie,
            ["Finalizate"] = result.Finalizate,
            ["Anulate"] = result.Anulate,
            ["NoShow"] = result.NoShow,
            ["ConsultatiiInitiale"] = result.ConsultatiiInitiale,
            ["ControalePeriodice"] = result.ControalePeriodice,
            ["Consultatii"] = result.Consultatii,
            ["Investigatii"] = result.Investigatii,
            ["Proceduri"] = result.Proceduri,
            ["Urgente"] = result.Urgente,
            ["Telemedicina"] = result.Telemedicina,
            ["LaDomiciliu"] = result.LaDomiciliu,
            ["MediciActivi"] = result.MediciActivi,
            ["PacientiUnici"] = result.PacientiUnici,
            ["DurataMedieMinute"] = result.DurataMedieMinute ?? 0.0
        };

        _logger.LogInformation(
                    "Statistici doctor {DoctorID} obținute: Total={Total}, Finalizate={Finalizate}",
               doctorID, result.TotalProgramari, result.Finalizate);

        return statistics;
    }

    // ==================== PRIVATE HELPER METHODS ====================

    /// <summary>
    /// Mapează DTO-ul din baza de date către entitatea de domeniu.
    /// </summary>
    private static Programare MapToEntity(ProgramareDto dto)
    {
        return new Programare
        {
            ProgramareID = dto.ProgramareID,
            PacientID = dto.PacientID,
            DoctorID = dto.DoctorID,
            DataProgramare = dto.DataProgramare,
            OraInceput = dto.OraInceput,
            OraSfarsit = dto.OraSfarsit,
            TipProgramare = dto.TipProgramare,
            Status = dto.Status,
            Observatii = dto.Observatii,
            DataCreare = dto.DataCreare,
            CreatDe = dto.CreatDe,
            DataUltimeiModificari = dto.DataUltimeiModificari,
            ModificatDe = dto.ModificatDe,
            // Navigation properties (din JOIN-uri)
            PacientNumeComplet = dto.PacientNumeComplet,
            PacientTelefon = dto.PacientTelefon,
            PacientEmail = dto.PacientEmail,
            PacientCNP = dto.PacientCNP,
            DoctorNumeComplet = dto.DoctorNumeComplet,
            DoctorSpecializare = dto.DoctorSpecializare,
            DoctorTelefon = dto.DoctorTelefon,
            DoctorEmail = dto.DoctorEmail,  // ✅ NEW - pentru trimitere email-uri
            CreatDeNumeComplet = dto.CreatDeNumeComplet
        };
    }

    // ==================== INTERNAL DTOs (pentru mapare rezultate SP) ====================

    /// <summary>
    /// DTO pentru maparea rezultatelor din stored procedures.
    /// Structura exactă trebuie să corespundă cu coloanele returnate de SP-uri.
    /// </summary>
    private class ProgramareDto
    {
        public Guid ProgramareID { get; set; }
        public Guid PacientID { get; set; }
        public Guid DoctorID { get; set; }
        public DateTime DataProgramare { get; set; }
        public TimeSpan OraInceput { get; set; }
        public TimeSpan OraSfarsit { get; set; }
        public string? TipProgramare { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? Observatii { get; set; }
        public DateTime DataCreare { get; set; }
        public Guid CreatDe { get; set; }
        public DateTime? DataUltimeiModificari { get; set; }
        public Guid? ModificatDe { get; set; }

        // Navigation properties (din JOIN-uri ale SP-urilor)
        public string? PacientNumeComplet { get; set; }
        public string? PacientTelefon { get; set; }
        public string? PacientEmail { get; set; }
        public string? PacientCNP { get; set; }
        public string? DoctorNumeComplet { get; set; }
        public string? DoctorSpecializare { get; set; }
        public string? DoctorTelefon { get; set; }
        public string? DoctorEmail { get; set; }  // ✅ NEW - pentru trimitere email-uri
        public string? CreatDeNumeComplet { get; set; }
    }

    /// <summary>
    /// DTO pentru rezultatul verificării de conflict.
    /// </summary>
    private class ConflictCheckResult
    {
        public int ConflictExists { get; set; }
    }

    /// <summary>
    /// DTO pentru rezultatele de tip success/failure din SP-uri.
    /// </summary>
    private class SuccessResult
    {
        public int Success { get; set; }
        public string? Message { get; set; }
    }

    /// <summary>
    /// DTO pentru rezultatele statistice (v2 format - UN SINGUR ROW cu toate statisticile).
    /// </summary>
    private class StatisticsResult
    {
        // Status counts
        public int TotalProgramari { get; set; }
        public int Programate { get; set; }
        public int Confirmate { get; set; }
        public int CheckedIn { get; set; }
        public int InConsultatie { get; set; }
        public int Finalizate { get; set; }
        public int Anulate { get; set; }
        public int NoShow { get; set; }

        // Tip programare counts
        public int ConsultatiiInitiale { get; set; }
        public int ControalePeriodice { get; set; }
        public int Consultatii { get; set; }
        public int Investigatii { get; set; }
        public int Proceduri { get; set; }
        public int Urgente { get; set; }
        public int Telemedicina { get; set; }
        public int LaDomiciliu { get; set; }

        // Advanced statistics
        public int MediciActivi { get; set; }
        public int PacientiUnici { get; set; }
        public double? DurataMedieMinute { get; set; }
    }

    /// <summary>
    /// DTO DEPRECATED - păstrat pentru backwards compatibility.
    /// Folosește StatisticsResult în loc.
    /// </summary>
    [Obsolete("Use StatisticsResult instead")]
    private class StatisticResult
    {
        public string Categorie { get; set; } = string.Empty;
        public int Valoare { get; set; }
    }
}
