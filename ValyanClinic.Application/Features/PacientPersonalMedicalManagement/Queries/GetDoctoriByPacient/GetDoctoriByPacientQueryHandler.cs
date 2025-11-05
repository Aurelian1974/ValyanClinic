using MediatR;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Data;
using ValyanClinic.Application.Common.Results;
using ValyanClinic.Application.Features.PacientPersonalMedicalManagement.DTOs;

namespace ValyanClinic.Application.Features.PacientPersonalMedicalManagement.Queries.GetDoctoriByPacient;

public class GetDoctoriByPacientQueryHandler : IRequestHandler<GetDoctoriByPacientQuery, Result<List<DoctorAsociatDto>>>
{
    private readonly IConfiguration _configuration;

    public GetDoctoriByPacientQueryHandler(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task<Result<List<DoctorAsociatDto>>> Handle(GetDoctoriByPacientQuery request, CancellationToken cancellationToken)
    {
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
   command.Parameters.AddWithValue("@PacientID", request.PacientID);
               command.Parameters.AddWithValue("@ApenumereActivi", request.ApenumereActivi ? 1 : 0);

     using (var reader = await command.ExecuteReaderAsync(cancellationToken))
           {
             while (await reader.ReadAsync(cancellationToken))
    {
    doctori.Add(new DoctorAsociatDto
   {
  RelatieID = reader.GetGuid(reader.GetOrdinal("RelatieID")),
           PersonalMedicalID = reader.GetGuid(reader.GetOrdinal("PersonalMedicalID")),
    DoctorNumeComplet = reader.GetString(reader.GetOrdinal("DoctorNumeComplet")),
          DoctorSpecializare = reader.IsDBNull(reader.GetOrdinal("DoctorSpecializare")) ? null : reader.GetString(reader.GetOrdinal("DoctorSpecializare")),
       DoctorTelefon = reader.IsDBNull(reader.GetOrdinal("DoctorTelefon")) ? null : reader.GetString(reader.GetOrdinal("DoctorTelefon")),
         DoctorEmail = reader.IsDBNull(reader.GetOrdinal("DoctorEmail")) ? null : reader.GetString(reader.GetOrdinal("DoctorEmail")),
         DoctorDepartament = reader.IsDBNull(reader.GetOrdinal("DoctorDepartament")) ? null : reader.GetString(reader.GetOrdinal("DoctorDepartament")),
          TipRelatie = reader.IsDBNull(reader.GetOrdinal("TipRelatie")) ? null : reader.GetString(reader.GetOrdinal("TipRelatie")),
        DataAsocierii = reader.GetDateTime(reader.GetOrdinal("DataAsocierii")),
 DataDezactivarii = reader.IsDBNull(reader.GetOrdinal("DataDezactivarii")) ? null : reader.GetDateTime(reader.GetOrdinal("DataDezactivarii")),
      EsteActiv = reader.GetBoolean(reader.GetOrdinal("EsteActiv")),
     ZileDeAsociere = reader.GetInt32(reader.GetOrdinal("ZileDeAsociere")),
     Observatii = reader.IsDBNull(reader.GetOrdinal("Observatii")) ? null : reader.GetString(reader.GetOrdinal("Observatii")),
          Motiv = reader.IsDBNull(reader.GetOrdinal("Motiv")) ? null : reader.GetString(reader.GetOrdinal("Motiv"))
      });
    }
         }
    }
        }

            return Result<List<DoctorAsociatDto>>.Success(doctori);
        }
        catch (Exception ex)
        {
   return Result<List<DoctorAsociatDto>>.Failure($"Eroare la obtinerea doctorilor: {ex.Message}");
  }
    }
}
