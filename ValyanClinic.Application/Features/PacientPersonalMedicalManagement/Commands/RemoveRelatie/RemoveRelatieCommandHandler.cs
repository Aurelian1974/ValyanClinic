using MediatR;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Data;
using ValyanClinic.Application.Common.Results;

namespace ValyanClinic.Application.Features.PacientPersonalMedicalManagement.Commands.RemoveRelatie;

public class RemoveRelatieCommandHandler : IRequestHandler<RemoveRelatieCommand, Result>
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<RemoveRelatieCommandHandler> _logger;

    public RemoveRelatieCommandHandler(
        IConfiguration configuration,
        ILogger<RemoveRelatieCommandHandler> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

  public async Task<Result> Handle(RemoveRelatieCommand request, CancellationToken cancellationToken)
    {
        try
        {
   // ✅ VALIDATION: RelatieID trebuie specificat
   if (!request.RelatieID.HasValue)
  {
        _logger.LogWarning("RelatieID nu a fost specificat");
return Result.Failure("RelatieID este obligatoriu pentru dezactivarea relației.");
       }

        _logger.LogInformation(
        "Dezactivare relație: RelatieID={RelatieID}",
 request.RelatieID.Value);

        var connectionString = _configuration.GetConnectionString("DefaultConnection");

   using (var connection = new SqlConnection(connectionString))
        {
                await connection.OpenAsync(cancellationToken);

      // ✅ FIXED: Nume corect SP + doar 1 parametru
       using (var command = new SqlCommand("sp_PacientiPersonalMedical_RemoveRelatie", connection))
    {
      command.CommandType = CommandType.StoredProcedure;
      
// ✅ FIXED: Doar @RelatieID - așa cum cere stored procedure-ul
      command.Parameters.AddWithValue("@RelatieID", request.RelatieID.Value);

  await command.ExecuteNonQueryAsync(cancellationToken);
          
       _logger.LogInformation(
       "Relație dezactivată cu succes: RelatieID={RelatieID}",
     request.RelatieID.Value);
              }
      }

            return Result.Success();
        }
catch (SqlException ex) when (ex.Message.Contains("nu exist"))
        {
  _logger.LogWarning(ex, "Relația nu a fost găsită: RelatieID={RelatieID}", request.RelatieID);
return Result.Failure("Relația specificată nu a fost găsită sau este deja inactivă.");
        }
        catch (SqlException ex)
        {
       _logger.LogError(ex, "Eroare SQL la dezactivarea relației: RelatieID={RelatieID}", request.RelatieID);
  return Result.Failure($"Eroare SQL: {ex.Message}");
        }
        catch (Exception ex)
     {
 _logger.LogError(ex, "Eroare neașteptată la dezactivarea relației: RelatieID={RelatieID}", request.RelatieID);
    return Result.Failure($"Eroare la dezactivarea relației: {ex.Message}");
 }
    }
}
