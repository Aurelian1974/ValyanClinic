using MediatR;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Data;
using ValyanClinic.Application.Common.Results;

namespace ValyanClinic.Application.Features.PacientPersonalMedicalManagement.Commands.ActivateRelatie;

public class ActivateRelatieCommandHandler : IRequestHandler<ActivateRelatieCommand, Result>
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<ActivateRelatieCommandHandler> _logger;

    public ActivateRelatieCommandHandler(
        IConfiguration configuration,
        ILogger<ActivateRelatieCommandHandler> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<Result> Handle(ActivateRelatieCommand request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation(
"Reactivare relație doctor-pacient: RelatieID={RelatieID}",
                request.RelatieID);

            var connectionString = _configuration.GetConnectionString("DefaultConnection");

            using (var connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync(cancellationToken);

                using (var command = new SqlCommand("sp_PacientiPersonalMedical_ActivateRelatie", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;

                    // Parameters
                    command.Parameters.AddWithValue("@RelatieID", request.RelatieID);
                    command.Parameters.AddWithValue("@Observatii", (object?)request.Observatii ?? DBNull.Value);
                    command.Parameters.AddWithValue("@Motiv", (object?)request.Motiv ?? DBNull.Value);
                    command.Parameters.AddWithValue("@ModificatDe", (object?)request.ModificatDe ?? DBNull.Value);

                    await command.ExecuteNonQueryAsync(cancellationToken);

                    _logger.LogInformation(
                           "Relație reactivată cu succes: RelatieID={RelatieID}",
                request.RelatieID);
                }
            }

            return Result.Success("Relația doctor-pacient a fost reactivată cu succes.");
        }
        catch (SqlException ex) when (ex.Message.Contains("nu exist") || ex.Message.Contains("nu a fost găsită"))
        {
            _logger.LogWarning(ex, "Relația nu a fost găsită: RelatieID={RelatieID}", request.RelatieID);
            return Result.Failure("Relația specificată nu a fost găsită.");
        }
        catch (SqlException ex) when (ex.Message.Contains("deja activ"))
        {
            _logger.LogWarning("Relația este deja activă: RelatieID={RelatieID}", request.RelatieID);
            return Result.Failure("Relația este deja activă.");
        }
        catch (SqlException ex)
        {
            _logger.LogError(ex, "Eroare SQL la reactivarea relației: RelatieID={RelatieID}", request.RelatieID);
            return Result.Failure($"Eroare SQL: {ex.Message}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Eroare neașteptată la reactivarea relației: RelatieID={RelatieID}", request.RelatieID);
            return Result.Failure($"Eroare la reactivarea relației: {ex.Message}");
        }
    }
}
