using MediatR;
using Microsoft.Extensions.Logging;
using ValyanClinic.Application.Common.Results;
using ValyanClinic.Application.Features.ProgramareManagement.DTOs;
using ValyanClinic.Domain.Interfaces.Repositories;

namespace ValyanClinic.Application.Features.ProgramareManagement.Queries.GetProgramareStatistics;

public class GetProgramareStatisticsQueryHandler : IRequestHandler<GetProgramareStatisticsQuery, Result<ProgramareStatisticsDto>>
{
    private readonly IProgramareRepository _programareRepository;
    private readonly ILogger<GetProgramareStatisticsQueryHandler> _logger;

    public GetProgramareStatisticsQueryHandler(
        IProgramareRepository programareRepository,
      ILogger<GetProgramareStatisticsQueryHandler> logger)
    {
        _programareRepository = programareRepository;
        _logger = logger;
    }

    public async Task<Result<ProgramareStatisticsDto>> Handle(
     GetProgramareStatisticsQuery request,
        CancellationToken cancellationToken)
    {
        try
   {
            _logger.LogInformation(
             "Obținere statistici programări: DataStart={DataStart}, DataEnd={DataEnd}, DoctorID={DoctorID}",
    request.DataStart, request.DataEnd, request.DoctorID);

 // ==================== OBȚINERE STATISTICI DIN REPOSITORY ====================

      Dictionary<string, object> statistics;

       if (request.DoctorID.HasValue)
{
     // Statistici pentru un medic specific
         statistics = await _programareRepository.GetDoctorStatisticsAsync(
      request.DoctorID.Value,
 request.DataStart,
      request.DataEnd,
           cancellationToken);
     }
       else
  {
        // Statistici globale
          statistics = await _programareRepository.GetStatisticsAsync(
        request.DataStart,
        request.DataEnd,
      cancellationToken);
            }

            // ==================== MAPARE LA DTO ====================

  var statisticsDto = new ProgramareStatisticsDto
     {
         DataStart = request.DataStart,
          DataEnd = request.DataEnd,
   TotalProgramari = GetIntValue(statistics, "TotalProgramari"),
     
                // Statistici pe status
        Programate = GetIntValue(statistics, "Programate"),
           Confirmate = GetIntValue(statistics, "Confirmate"),
      CheckedIn = GetIntValue(statistics, "CheckedIn"),
       InConsultatie = GetIntValue(statistics, "InConsultatie"),
      Finalizate = GetIntValue(statistics, "Finalizate"),
   Anulate = GetIntValue(statistics, "Anulate"),
            NoShow = GetIntValue(statistics, "NoShow"),
       
  // Statistici pe tip
  ConsultatiiInitiale = GetIntValue(statistics, "ConsultatiiInitiale"),
        ControalePeriodice = GetIntValue(statistics, "ControalePeriodice"),
     Consultatii = GetIntValue(statistics, "Consultatii"),
        Investigatii = GetIntValue(statistics, "Investigatii"),
      Proceduri = GetIntValue(statistics, "Proceduri"),
        Urgente = GetIntValue(statistics, "Urgente"),
            Telemedicina = GetIntValue(statistics, "Telemedicina"),
       LaDomiciliu = GetIntValue(statistics, "LaDomiciliu"),
   
            // Statistici avansate
        MediciActivi = GetIntValue(statistics, "MediciActivi"),
         PacientiUnici = GetIntValue(statistics, "PacientiUnici"),
  DurataMedieMinute = GetDoubleValue(statistics, "DurataMedieMinute")
  };

            _logger.LogInformation(
     "Statistici obținute: Total={Total}, Finalizate={Finalizate}, RataPrezentare={Rata}%",
     statisticsDto.TotalProgramari, statisticsDto.Finalizate, statisticsDto.RataPrezentare);

 return Result<ProgramareStatisticsDto>.Success(
             statisticsDto,
         $"Statistici obținute cu succes pentru perioada {statisticsDto.PerioadaFormatata}.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Eroare la obținerea statisticilor programări");
            return Result<ProgramareStatisticsDto>.Failure($"Eroare: {ex.Message}");
 }
    }

    // ==================== HELPER METHODS ====================

    private static int GetIntValue(Dictionary<string, object> dictionary, string key)
    {
        if (dictionary.TryGetValue(key, out var value))
      {
            return value switch
            {
      int intValue => intValue,
          long longValue => (int)longValue,
                string strValue => int.TryParse(strValue, out var parsed) ? parsed : 0,
             _ => 0
         };
   }
        return 0;
    }

    private static double GetDoubleValue(Dictionary<string, object> dictionary, string key)
    {
      if (dictionary.TryGetValue(key, out var value))
     {
   return value switch
            {
  double doubleValue => doubleValue,
    decimal decimalValue => (double)decimalValue,
  int intValue => intValue,
     string strValue => double.TryParse(strValue, out var parsed) ? parsed : 0,
         _ => 0
         };
        }
        return 0;
    }
}
