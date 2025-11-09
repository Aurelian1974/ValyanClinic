using MediatR;
using Microsoft.Extensions.Logging;
using ValyanClinic.Application.Common.Results;
using ValyanClinic.Domain.Interfaces.Repositories;

namespace ValyanClinic.Application.Features.DashboardManagement.Queries.GetReceptionerStats;

public class GetReceptionerStatsQueryHandler : IRequestHandler<GetReceptionerStatsQuery, Result<ReceptionerStatsDto>>
{
    private readonly IProgramareRepository _programareRepository;
    private readonly ILogger<GetReceptionerStatsQueryHandler> _logger;

    public GetReceptionerStatsQueryHandler(
        IProgramareRepository programareRepository,
        ILogger<GetReceptionerStatsQueryHandler> logger)
    {
        _programareRepository = programareRepository;
        _logger = logger;
    }

    public async Task<Result<ReceptionerStatsDto>> Handle(
        GetReceptionerStatsQuery request,
        CancellationToken cancellationToken)
 {
        try
  {
   _logger.LogInformation("Obținere statistici dashboard receptioner pentru data: {Date}",
      request.Date.ToString("yyyy-MM-dd"));

// 1. Programări astăzi
 var programariAstazi = await _programareRepository.GetByDateAsync(
           request.Date,
          null, // Toți medicii
       cancellationToken);

     var programariList = programariAstazi.ToList();
 var totalAstazi = programariList.Count;

     // 2. Programări ieri (pentru calculul growth)
      var ieri = request.Date.AddDays(-1);
       var programariIeri = await _programareRepository.GetByDateAsync(
     ieri,
    null,
       cancellationToken);

    var totalIeri = programariIeri.Count();
    
   // ✅ FIX: Calcul growth REAL - chiar dacă ieri = 0
       int growth;
 if (totalIeri == 0 && totalAstazi == 0)
      {
    // Ambele zile = 0 → nu există creștere
   growth = 0;
      _logger.LogInformation("Growth: 0% (ieri: 0, astăzi: 0)");
            }
            else if (totalIeri == 0 && totalAstazi > 0)
            {
  // ✅ Ieri = 0, Astăzi > 0 → Creștere INFINITĂ, afișăm un procent mare reprezentativ
             // Ex: Astăzi 3 programări → +300% (3 * 100)
        growth = totalAstazi * 100;
           _logger.LogInformation("Growth: +{Growth}% (ieri: 0, astăzi: {Astazi} - primele programări!)", 
       growth, totalAstazi);
        }
            else
{
    // Calcul normal: ((astăzi - ieri) / ieri) * 100
        growth = (int)Math.Round(((double)(totalAstazi - totalIeri) / totalIeri) * 100);
  _logger.LogInformation("Growth: {Growth}% (ieri: {Ieri}, astăzi: {Astazi})", 
        growth, totalIeri, totalAstazi);
       }

            // 3. Pacienți în așteptare (status: CheckedIn)
       var pacientiInAsteptare = programariList.Count(p => 
    p.Status.Equals("CheckedIn", StringComparison.OrdinalIgnoreCase));

      // 4. ✅ Timp mediu de așteptare - returnăm -1 pentru "N/A"
      // TODO: Implementare calcul real bazat pe diferența dintre DataCheckin și DataStartConsultatie
         // Când vom avea timestamps (DataCheckin, DataStartConsultatie), vom calcula:
            // AVG(DATEDIFF(MINUTE, DataCheckin, DataStartConsultatie)) pentru programările cu CheckedIn
   var timpMediuAsteptare = -1; // -1 = "N/A - în dezvoltare"
         
       _logger.LogInformation("Timp mediu așteptare: N/A (funcționalitate în dezvoltare)");

 // 5. Programări finalizate
            var programariFinalizate = programariList.Count(p => 
      p.Status.Equals("Finalizata", StringComparison.OrdinalIgnoreCase));

         // 6. Programări rămase (toate care nu sunt Finalizate sau Anulate)
     var programariRamase = programariList.Count(p => 
 !p.Status.Equals("Finalizata", StringComparison.OrdinalIgnoreCase) &&
      !p.Status.Equals("Anulata", StringComparison.OrdinalIgnoreCase));

      // 7. Pacienți noi (săptămâna curentă) - prima programare a pacientului
            var startOfWeek = request.Date.AddDays(-(int)request.Date.DayOfWeek);
            var endOfWeek = startOfWeek.AddDays(7);

            // Obține toate programările din săptămâna curentă
  var programariSaptamana = await _programareRepository.GetAllAsync(
      pageNumber: 1,
                pageSize: 1000,
                dataStart: startOfWeek,
          dataEnd: endOfWeek,
             cancellationToken: cancellationToken);

            // Count unique pacienți din această săptămână
  // TODO: Implementare verificare dacă este PRIMA programare a pacientului (istoric complet)
     var pacientiUnici = programariSaptamana
             .Select(p => p.PacientID)
          .Distinct()
    .Count();

            var stats = new ReceptionerStatsDto
{
            ProgramariAstazi = totalAstazi,
           ProgramariGrowth = growth,
           PacientiInAsteptare = pacientiInAsteptare,
         TimpMediuAsteptare = timpMediuAsteptare,
      ProgramariFinalizate = programariFinalizate,
                ProgramariRamase = programariRamase,
     PacientiNoi = pacientiUnici
        };

            _logger.LogInformation(
      "Statistici obținute: Astăzi={Total}, Growth={Growth}%, InAșteptare={InAsteptare}, Finalizate={Finalizate}, Noi={Noi}",
      totalAstazi, growth, pacientiInAsteptare, programariFinalizate, pacientiUnici);

      return Result<ReceptionerStatsDto>.Success(stats, "Statistici obținute cu succes.");
        }
catch (Exception ex)
        {
   _logger.LogError(ex, "Eroare la obținerea statisticilor dashboard receptioner");
       return Result<ReceptionerStatsDto>.Failure($"Eroare: {ex.Message}");
        }
    }
}
