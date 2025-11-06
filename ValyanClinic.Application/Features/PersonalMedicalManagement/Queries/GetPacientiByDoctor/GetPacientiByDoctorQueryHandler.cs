using Dapper;
using MediatR;
using Microsoft.Extensions.Logging;
using ValyanClinic.Application.Common.Results;
using ValyanClinic.Infrastructure.Data;

namespace ValyanClinic.Application.Features.PersonalMedicalManagement.Queries.GetPacientiByDoctor;

/// <summary>
/// Handler pentru obținerea pacienților asociați cu un doctor
/// </summary>
public class GetPacientiByDoctorQueryHandler : IRequestHandler<GetPacientiByDoctorQuery, Result<List<PacientAsociatDto>>>
{
    private readonly IDbConnectionFactory _connectionFactory;
    private readonly ILogger<GetPacientiByDoctorQueryHandler> _logger;

    public GetPacientiByDoctorQueryHandler(
        IDbConnectionFactory connectionFactory,
     ILogger<GetPacientiByDoctorQueryHandler> logger)
    {
        _connectionFactory = connectionFactory;
   _logger = logger;
    }

    public async Task<Result<List<PacientAsociatDto>>> Handle(
        GetPacientiByDoctorQuery request,
        CancellationToken cancellationToken)
 {
        try
        {
    _logger.LogInformation("Getting pacienti for doctor: {DoctorID}", request.DoctorID);

 using var connection = _connectionFactory.CreateConnection();

            // Query pentru a prelua pacienții asociați
       var sql = @"
           SELECT 
          ppm.Id AS RelatieID,
         ppm.PacientID,
   p.Nume + ' ' + p.Prenume AS PacientNumeComplet,
 p.CNP AS PacientCNP,
          p.Telefon AS PacientTelefon,
   p.Email AS PacientEmail,
             p.Data_Nasterii AS PacientDataNasterii,
      DATEDIFF(YEAR, p.Data_Nasterii, GETDATE()) - 
   CASE 
     WHEN MONTH(p.Data_Nasterii) > MONTH(GETDATE()) 
    OR (MONTH(p.Data_Nasterii) = MONTH(GETDATE()) AND DAY(p.Data_Nasterii) > DAY(GETDATE()))
     THEN 1 
          ELSE 0 
 END AS PacientVarsta,
          p.Sex AS PacientSex,
                    ppm.TipRelatie,
          ppm.DataAsocierii,
     ppm.DataDezactivarii,
         ppm.EsteActiv,
              ppm.Motiv,
         ppm.Observatii
FROM Pacienti_PersonalMedical ppm
      INNER JOIN Pacienti p ON ppm.PacientID = p.Id
         WHERE ppm.PersonalMedicalID = @DoctorID
     ORDER BY 
            ppm.EsteActiv DESC,
     ppm.DataAsocierii DESC";

  var pacienti = await connection.QueryAsync<PacientAsociatDto>(
    sql,
    new { DoctorID = request.DoctorID });

            var result = pacienti.ToList();

        _logger.LogInformation(
   "Found {Count} pacienti for doctor {DoctorID} ({Active} active, {Inactive} inactive)",
         result.Count,
       request.DoctorID,
     result.Count(p => p.EsteActiv),
       result.Count(p => !p.EsteActiv));

          return Result<List<PacientAsociatDto>>.Success(result);
        }
    catch (Exception ex)
        {
 _logger.LogError(ex, "Error getting pacienti for doctor: {DoctorID}", request.DoctorID);
        return Result<List<PacientAsociatDto>>.Failure($"Error: {ex.Message}");
        }
    }
}
