using MediatR;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Data;
using ValyanClinic.Application.Common.Results;
using ValyanClinic.Application.Features.PacientPersonalMedicalManagement.DTOs;

namespace ValyanClinic.Application.Features.PacientPersonalMedicalManagement.Queries.GetPacientiByDoctor;

public class GetPacientiByDoctorQueryHandler : IRequestHandler<GetPacientiByDoctorQuery, Result<List<PacientAsociatDto>>>
{
    private readonly IConfiguration _configuration;

    public GetPacientiByDoctorQueryHandler(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task<Result<List<PacientAsociatDto>>> Handle(GetPacientiByDoctorQuery request, CancellationToken cancellationToken)
    {
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
                    command.Parameters.AddWithValue("@PersonalMedicalID", request.PersonalMedicalID);
                    command.Parameters.AddWithValue("@ApenumereActivi", request.ApenumereActivi ? 1 : 0);
                    command.Parameters.AddWithValue("@TipRelatie", (object?)request.TipRelatie ?? DBNull.Value);

                    using (var reader = await command.ExecuteReaderAsync(cancellationToken))
                    {
                        while (await reader.ReadAsync(cancellationToken))
                        {
                            pacienti.Add(new PacientAsociatDto
                            {
                                RelatieID = reader.GetGuid(reader.GetOrdinal("RelatieID")),
                                PacientID = reader.GetGuid(reader.GetOrdinal("PacientID")),
                                PacientCod = reader.GetString(reader.GetOrdinal("PacientCod")),
                                PacientNumeComplet = reader.GetString(reader.GetOrdinal("PacientNumeComplet")),
                                PacientCNP = reader.IsDBNull(reader.GetOrdinal("PacientCNP")) ? null : reader.GetString(reader.GetOrdinal("PacientCNP")),
                                PacientDataNasterii = reader.GetDateTime(reader.GetOrdinal("PacientDataNasterii")),
                                PacientVarsta = reader.GetInt32(reader.GetOrdinal("PacientVarsta")),
                                PacientTelefon = reader.IsDBNull(reader.GetOrdinal("PacientTelefon")) ? null : reader.GetString(reader.GetOrdinal("PacientTelefon")),
                                PacientEmail = reader.IsDBNull(reader.GetOrdinal("PacientEmail")) ? null : reader.GetString(reader.GetOrdinal("PacientEmail")),
                                PacientJudet = reader.IsDBNull(reader.GetOrdinal("PacientJudet")) ? null : reader.GetString(reader.GetOrdinal("PacientJudet")),
                                PacientLocalitate = reader.IsDBNull(reader.GetOrdinal("PacientLocalitate")) ? null : reader.GetString(reader.GetOrdinal("PacientLocalitate")),
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

            return Result<List<PacientAsociatDto>>.Success(pacienti);
        }
        catch (Exception ex)
        {
            return Result<List<PacientAsociatDto>>.Failure($"Eroare la obtinerea pacientilor: {ex.Message}");
        }
    }
}
