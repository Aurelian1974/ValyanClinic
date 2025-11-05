using MediatR;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Data;
using ValyanClinic.Application.Common.Results;

namespace ValyanClinic.Application.Features.PacientPersonalMedicalManagement.Commands.AddRelatie;

public class AddRelatieCommandHandler : IRequestHandler<AddRelatieCommand, Result<Guid>>
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<AddRelatieCommandHandler> _logger;

 public AddRelatieCommandHandler(
  IConfiguration configuration,
     ILogger<AddRelatieCommandHandler> logger)
    {
        _configuration = configuration;
 _logger = logger;
    }

    public async Task<Result<Guid>> Handle(AddRelatieCommand request, CancellationToken cancellationToken)
    {
   try
        {
     _logger.LogInformation("[AddRelatieHandler] Adding relatie: PacientID={PacientID}, DoctorID={DoctorID}, TipRelatie={TipRelatie}",
    request.PacientID, request.PersonalMedicalID, request.TipRelatie);

            var connectionString = _configuration.GetConnectionString("DefaultConnection");
   Guid newRelatieId = Guid.Empty;

   using (var connection = new SqlConnection(connectionString))
            {
        await connection.OpenAsync(cancellationToken);

       using (var command = new SqlCommand("sp_PacientiPersonalMedical_AddRelatie", connection))
     {
          command.CommandType = CommandType.StoredProcedure;
         command.Parameters.AddWithValue("@PacientID", request.PacientID);
  command.Parameters.AddWithValue("@PersonalMedicalID", request.PersonalMedicalID);
        command.Parameters.AddWithValue("@TipRelatie", (object?)request.TipRelatie ?? DBNull.Value);
        command.Parameters.AddWithValue("@Observatii", (object?)request.Observatii ?? DBNull.Value);
        command.Parameters.AddWithValue("@Motiv", (object?)request.Motiv ?? DBNull.Value);
   command.Parameters.AddWithValue("@CreatDe", (object?)request.CreatDe ?? DBNull.Value);

     using (var reader = await command.ExecuteReaderAsync(cancellationToken))
    {
          if (await reader.ReadAsync(cancellationToken))
    {
     // ✅ FIX: Citește "RelatieID" (nu "Id")
            // SP returnează: ppm.Id AS RelatieID
          var ordinal = reader.GetOrdinal("RelatieID");
              newRelatieId = reader.GetGuid(ordinal);

       _logger.LogInformation("[AddRelatieHandler] Relatie created successfully: RelatieID={RelatieID}", newRelatieId);
          }
       else
                {
 _logger.LogWarning("[AddRelatieHandler] No result set returned from SP");
               }
        }
     }
    }

            if (newRelatieId == Guid.Empty)
  {
  _logger.LogError("[AddRelatieHandler] Failed to create relatie - RelatieID is empty");
 return Result<Guid>.Failure("Nu s-a putut crea relatia.");
       }

     _logger.LogInformation("[AddRelatieHandler] Successfully created relatie: {RelatieID}", newRelatieId);
            return Result<Guid>.Success(newRelatieId, "Doctor adăugat cu succes!");
        }
        catch (SqlException ex) when (ex.Message.Contains("Exista deja o relatie activa") || ex.Message.Contains("Există deja"))
        {
          _logger.LogWarning("[AddRelatieHandler] Duplicate active relatie: {Message}", ex.Message);
            return Result<Guid>.Failure("Există deja o relație activă între acest pacient și doctor.");
        }
        catch (SqlException ex) when (ex.Message.Contains("nu exista") || ex.Message.Contains("nu există"))
  {
      _logger.LogError("[AddRelatieHandler] Entity not found: {Message}", ex.Message);
return Result<Guid>.Failure(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[AddRelatieHandler] Exception adding relatie");
   return Result<Guid>.Failure($"Eroare la crearea relației: {ex.Message}");
      }
    }
}
