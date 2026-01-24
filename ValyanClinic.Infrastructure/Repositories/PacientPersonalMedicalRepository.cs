using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Data;
using ValyanClinic.Domain.DTOs;
using ValyanClinic.Domain.Interfaces.Repositories;

namespace ValyanClinic.Infrastructure.Repositories;

/// <summary>
/// Implementare repository pentru gestionarea relațiilor dintre pacienti și personalul medical.
/// </summary>
public class PacientPersonalMedicalRepository : IPacientPersonalMedicalRepository
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<PacientPersonalMedicalRepository> _logger;

    /// <summary>
    /// Constructor pentru PacientPersonalMedicalRepository.
    /// </summary>
    /// <param name="configuration">
    /// Configurația aplicației pentru a accesa connection string-ul.
    /// </param>
    /// <param name="logger">
    /// Logger pentru înregistrarea operațiilor și erorilor.
    /// </param>
    public PacientPersonalMedicalRepository(
        IConfiguration configuration,
        ILogger<PacientPersonalMedicalRepository> logger)
    {
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public async Task<List<DoctorAsociatDto>> GetDoctoriByPacientAsync(
        Guid pacientId,
        bool apenumereActivi,
        CancellationToken cancellationToken = default)
    {
        if (pacientId == Guid.Empty)
        {
            throw new ArgumentException("PacientId nu poate fi Guid.Empty", nameof(pacientId));
        }

        _logger.LogInformation(
            "Obținere doctori pentru pacientId={PacientId}, apenumereActivi={ApenumereActivi}",
            pacientId, apenumereActivi);

        try
        {
            var connectionString = _configuration.GetConnectionString("DefaultConnection");
            var doctori = new List<DoctorAsociatDto>();

            using (var connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync(cancellationToken);

                using (var command = new SqlCommand("sp_PacientiPersonalMedical_GetDoctoriByPacient", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.CommandTimeout = 30;
                    command.Parameters.AddWithValue("@PacientID", pacientId);
                    command.Parameters.AddWithValue("@ApenumereActivi", apenumereActivi ? 1 : 0);

                    using (var reader = await command.ExecuteReaderAsync(cancellationToken))
                    {
                        while (await reader.ReadAsync(cancellationToken))
                        {
                            doctori.Add(MapDoctorAsociatDto(reader));
                        }
                    }
                }
            }

            _logger.LogInformation(
                "S-au găsit {Count} doctori pentru pacientId={PacientId}",
                doctori.Count, pacientId);

            return doctori;
        }
        catch (SqlException ex)
        {
            _logger.LogError(ex,
                "Eroare SQL la obținerea doctorilor pentru pacientId={PacientId}",
                pacientId);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Eroare neașteptată la obținerea doctorilor pentru pacientId={PacientId}",
                pacientId);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<List<PacientAsociatDto>> GetPacientiByDoctorAsync(
        Guid personalMedicalId,
        bool apenumereActivi,
        string? tipRelatie = null,
        CancellationToken cancellationToken = default)
    {
        if (personalMedicalId == Guid.Empty)
        {
            throw new ArgumentException("PersonalMedicalId nu poate fi Guid.Empty", nameof(personalMedicalId));
        }

        _logger.LogInformation(
            "Obținere pacienti pentru personalMedicalId={PersonalMedicalId}, apenumereActivi={ApenumereActivi}, tipRelatie={TipRelatie}",
            personalMedicalId, apenumereActivi, tipRelatie ?? "null");

        try
        {
            var connectionString = _configuration.GetConnectionString("DefaultConnection");
            var pacienti = new List<PacientAsociatDto>();

            using (var connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync(cancellationToken);

                using (var command = new SqlCommand("sp_PacientiPersonalMedical_GetPacientiByDoctor", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.CommandTimeout = 30;
                    command.Parameters.AddWithValue("@PersonalMedicalID", personalMedicalId);
                    command.Parameters.AddWithValue("@ApenumereActivi", apenumereActivi ? 1 : 0);
                    command.Parameters.AddWithValue("@TipRelatie", (object?)tipRelatie ?? DBNull.Value);

                    using (var reader = await command.ExecuteReaderAsync(cancellationToken))
                    {
                        while (await reader.ReadAsync(cancellationToken))
                        {
                            pacienti.Add(MapPacientAsociatDto(reader));
                        }
                    }
                }
            }

            _logger.LogInformation(
                "S-au găsit {Count} pacienti pentru personalMedicalId={PersonalMedicalId}",
                pacienti.Count, personalMedicalId);

            return pacienti;
        }
        catch (SqlException ex)
        {
            _logger.LogError(ex,
                "Eroare SQL la obținerea pacienților pentru personalMedicalId={PersonalMedicalId}",
                personalMedicalId);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Eroare neașteptată la obținerea pacienților pentru personalMedicalId={PersonalMedicalId}",
                personalMedicalId);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task RemoveRelatieAsync(
        Guid relatieId,
        CancellationToken cancellationToken = default)
    {
        if (relatieId == Guid.Empty)
        {
            throw new ArgumentException("RelatieId nu poate fi Guid.Empty", nameof(relatieId));
        }

        _logger.LogInformation(
            "Dezactivare relație cu relatieId={RelatieId}",
            relatieId);

        try
        {
            var connectionString = _configuration.GetConnectionString("DefaultConnection");

            using (var connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync(cancellationToken);

                using (var command = new SqlCommand("sp_PacientiPersonalMedical_RemoveRelatie", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.CommandTimeout = 30;
                    command.Parameters.AddWithValue("@RelatieID", relatieId);

                    var rowsAffected = await command.ExecuteNonQueryAsync(cancellationToken);

                    if (rowsAffected == 0)
                    {
                        _logger.LogWarning(
                            "Relația cu relatieId={RelatieId} nu a fost găsită sau este deja inactivă",
                            relatieId);

                        throw new InvalidOperationException(
                            $"Relația cu ID-ul {relatieId} nu a fost găsită sau este deja inactivă.");
                    }
                }
            }

            _logger.LogInformation(
                "Relație dezactivată cu succes: relatieId={RelatieId}",
                relatieId);
        }
        catch (SqlException ex) when (ex.Message.Contains("nu exist"))
        {
            _logger.LogWarning(ex,
                "Relația cu relatieId={RelatieId} nu a fost găsită",
                relatieId);

            throw new InvalidOperationException(
                "Relația specificată nu a fost găsită sau este deja inactivă.",
                ex);
        }
        catch (SqlException ex)
        {
            _logger.LogError(ex,
                "Eroare SQL la dezactivarea relației cu relatieId={RelatieId}",
                relatieId);
            throw;
        }
        catch (InvalidOperationException)
        {
            // Re-throw already logged InvalidOperationException
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Eroare neașteptată la dezactivarea relației cu relatieId={RelatieId}",
                relatieId);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<Guid> AddRelatieAsync(
        Guid pacientId,
        Guid personalMedicalId,
        string? tipRelatie,
        string? observatii,
        string? motiv,
        Guid? creatDe,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Adăugare relație: PacientID={PacientID}, PersonalMedicalID={PersonalMedicalID}, TipRelatie={TipRelatie}",
            pacientId, personalMedicalId, tipRelatie);

        try
        {
            var connectionString = _configuration.GetConnectionString("DefaultConnection");
            Guid newRelatieId = Guid.Empty;

            using (var connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync(cancellationToken);

                using (var command = new SqlCommand("sp_PacientiPersonalMedical_AddRelatie", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.CommandTimeout = 30;
                    command.Parameters.AddWithValue("@PacientID", pacientId);
                    command.Parameters.AddWithValue("@PersonalMedicalID", personalMedicalId);
                    command.Parameters.AddWithValue("@TipRelatie", (object?)tipRelatie ?? DBNull.Value);
                    command.Parameters.AddWithValue("@Observatii", (object?)observatii ?? DBNull.Value);
                    command.Parameters.AddWithValue("@Motiv", (object?)motiv ?? DBNull.Value);
                    command.Parameters.AddWithValue("@CreatDe", (object?)creatDe ?? DBNull.Value);

                    using (var reader = await command.ExecuteReaderAsync(cancellationToken))
                    {
                        if (await reader.ReadAsync(cancellationToken))
                        {
                            var ordinal = reader.GetOrdinal("RelatieID");
                            newRelatieId = reader.GetGuid(ordinal);

                            _logger.LogInformation(
                                "Relație creată cu succes: RelatieID={RelatieID}",
                                newRelatieId);
                        }
                    }
                }
            }

            if (newRelatieId == Guid.Empty)
            {
                throw new InvalidOperationException("Nu s-a putut crea relația.");
            }

            return newRelatieId;
        }
        catch (SqlException ex) when (ex.Message.Contains("Exista deja o relatie activa") || ex.Message.Contains("Există deja"))
        {
            _logger.LogWarning("Relație activă duplicată: {Message}", ex.Message);
            throw new InvalidOperationException("Există deja o relație activă între acest pacient și doctor.", ex);
        }
        catch (SqlException ex) when (ex.Message.Contains("nu exista") || ex.Message.Contains("nu există"))
        {
            _logger.LogError("Entitate inexistentă: {Message}", ex.Message);
            throw new InvalidOperationException(ex.Message, ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Eroare la adăugarea relației");
            throw;
        }
    }

    /// <inheritdoc />
    public async Task ActivateRelatieAsync(
        Guid relatieId,
        string? observatii,
        string? motiv,
        Guid? modificatDe,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Reactivare relație: RelatieID={RelatieID}",
            relatieId);

        try
        {
            var connectionString = _configuration.GetConnectionString("DefaultConnection");

            using (var connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync(cancellationToken);

                using (var command = new SqlCommand("sp_PacientiPersonalMedical_ActivateRelatie", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.CommandTimeout = 30;
                    command.Parameters.AddWithValue("@RelatieID", relatieId);
                    command.Parameters.AddWithValue("@Observatii", (object?)observatii ?? DBNull.Value);
                    command.Parameters.AddWithValue("@Motiv", (object?)motiv ?? DBNull.Value);
                    command.Parameters.AddWithValue("@ModificatDe", (object?)modificatDe ?? DBNull.Value);

                    await command.ExecuteNonQueryAsync(cancellationToken);

                    _logger.LogInformation(
                        "Relație reactivată cu succes: RelatieID={RelatieID}",
                        relatieId);
                }
            }
        }
        catch (SqlException ex) when (ex.Message.Contains("nu exist") || ex.Message.Contains("nu a fost găsită"))
        {
            _logger.LogWarning(ex, "Relația nu a fost găsită: RelatieID={RelatieID}", relatieId);
            throw new InvalidOperationException("Relația specificată nu a fost găsită.", ex);
        }
        catch (SqlException ex) when (ex.Message.Contains("deja activ"))
        {
            _logger.LogWarning("Relația este deja activă: RelatieID={RelatieID}", relatieId);
            throw new InvalidOperationException("Relația este deja activă.", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Eroare la reactivarea relației: RelatieID={RelatieID}", relatieId);
            throw;
        }
    }

    #region Private Mapping Methods

    /// <summary>
    /// Mapare din SqlDataReader la DoctorAsociatDto.
    /// </summary>
    /// <param name="reader">Reader-ul SQL cu datele curente.</param>
    /// <returns>Instanță de DoctorAsociatDto populată cu datele din reader.</returns>
    private DoctorAsociatDto MapDoctorAsociatDto(SqlDataReader reader)
    {
        return new DoctorAsociatDto
        {
            RelatieID = reader.GetGuid(reader.GetOrdinal("RelatieID")),
            PersonalMedicalID = reader.GetGuid(reader.GetOrdinal("PersonalMedicalID")),
            DoctorNumeComplet = reader.GetString(reader.GetOrdinal("DoctorNumeComplet")),
            DoctorSpecializare = reader.IsDBNull(reader.GetOrdinal("DoctorSpecializare"))
                ? null
                : reader.GetString(reader.GetOrdinal("DoctorSpecializare")),
            DoctorTelefon = reader.IsDBNull(reader.GetOrdinal("DoctorTelefon"))
                ? null
                : reader.GetString(reader.GetOrdinal("DoctorTelefon")),
            DoctorEmail = reader.IsDBNull(reader.GetOrdinal("DoctorEmail"))
                ? null
                : reader.GetString(reader.GetOrdinal("DoctorEmail")),
            DoctorDepartament = reader.IsDBNull(reader.GetOrdinal("DoctorDepartament"))
                ? null
                : reader.GetString(reader.GetOrdinal("DoctorDepartament")),
            TipRelatie = reader.IsDBNull(reader.GetOrdinal("TipRelatie"))
                ? null
                : reader.GetString(reader.GetOrdinal("TipRelatie")),
            DataAsocierii = reader.GetDateTime(reader.GetOrdinal("DataAsocierii")),
            DataDezactivarii = reader.IsDBNull(reader.GetOrdinal("DataDezactivarii"))
                ? null
                : reader.GetDateTime(reader.GetOrdinal("DataDezactivarii")),
            EsteActiv = reader.GetBoolean(reader.GetOrdinal("EsteActiv")),
            ZileDeAsociere = reader.GetInt32(reader.GetOrdinal("ZileDeAsociere")),
            Observatii = reader.IsDBNull(reader.GetOrdinal("Observatii"))
                ? null
                : reader.GetString(reader.GetOrdinal("Observatii")),
            Motiv = reader.IsDBNull(reader.GetOrdinal("Motiv"))
                ? null
                : reader.GetString(reader.GetOrdinal("Motiv"))
        };
    }

    /// <summary>
    /// Mapare din SqlDataReader la PacientAsociatDto.
    /// </summary>
    /// <param name="reader">Reader-ul SQL cu datele curente.</param>
    /// <returns>Instanță de PacientAsociatDto populată cu datele din reader.</returns>
    private PacientAsociatDto MapPacientAsociatDto(SqlDataReader reader)
    {
        return new PacientAsociatDto
        {
            RelatieID = reader.GetGuid(reader.GetOrdinal("RelatieID")),
            PacientID = reader.GetGuid(reader.GetOrdinal("PacientID")),
            PacientCod = reader.GetString(reader.GetOrdinal("PacientCod")),
            PacientNumeComplet = reader.GetString(reader.GetOrdinal("PacientNumeComplet")),
            PacientCNP = reader.IsDBNull(reader.GetOrdinal("PacientCNP"))
                ? null
                : reader.GetString(reader.GetOrdinal("PacientCNP")),
            PacientDataNasterii = reader.GetDateTime(reader.GetOrdinal("PacientDataNasterii")),
            PacientVarsta = reader.GetInt32(reader.GetOrdinal("PacientVarsta")),
            PacientTelefon = reader.IsDBNull(reader.GetOrdinal("PacientTelefon"))
                ? null
                : reader.GetString(reader.GetOrdinal("PacientTelefon")),
            PacientEmail = reader.IsDBNull(reader.GetOrdinal("PacientEmail"))
                ? null
                : reader.GetString(reader.GetOrdinal("PacientEmail")),
            PacientJudet = reader.IsDBNull(reader.GetOrdinal("PacientJudet"))
                ? null
                : reader.GetString(reader.GetOrdinal("PacientJudet")),
            PacientLocalitate = reader.IsDBNull(reader.GetOrdinal("PacientLocalitate"))
                ? null
                : reader.GetString(reader.GetOrdinal("PacientLocalitate")),
            TipRelatie = reader.IsDBNull(reader.GetOrdinal("TipRelatie"))
                ? null
                : reader.GetString(reader.GetOrdinal("TipRelatie")),
            DataAsocierii = reader.GetDateTime(reader.GetOrdinal("DataAsocierii")),
            DataDezactivarii = reader.IsDBNull(reader.GetOrdinal("DataDezactivarii"))
                ? null
                : reader.GetDateTime(reader.GetOrdinal("DataDezactivarii")),
            EsteActiv = reader.GetBoolean(reader.GetOrdinal("EsteActiv")),
            ZileDeAsociere = reader.GetInt32(reader.GetOrdinal("ZileDeAsociere")),
            Observatii = reader.IsDBNull(reader.GetOrdinal("Observatii"))
                ? null
                : reader.GetString(reader.GetOrdinal("Observatii")),
            Motiv = reader.IsDBNull(reader.GetOrdinal("Motiv"))
                ? null
                : reader.GetString(reader.GetOrdinal("Motiv"))
        };
    }

    #endregion
}
