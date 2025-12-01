using MediatR;
using ValyanClinic.Application.Common.Results;

namespace ValyanClinic.Application.Features.DashboardManagement.Queries.GetReceptionerStats;

/// <summary>
/// Query pentru obținerea statisticilor pentru dashboard-ul receptioner.
/// Returnează date pentru: programări astăzi, pacienti în așteptare, finalizate, pacienți noi.
/// </summary>
public class GetReceptionerStatsQuery : IRequest<Result<ReceptionerStatsDto>>
{
    /// <summary>
    /// Data pentru care se obțin statisticile (implicit: azi).
    /// </summary>
    public DateTime Date { get; set; } = DateTime.Today;

    public GetReceptionerStatsQuery()
    {
    }

    public GetReceptionerStatsQuery(DateTime date)
    {
        Date = date;
    }
}

/// <summary>
/// DTO pentru statisticile dashboard-ului receptioner.
/// </summary>
public class ReceptionerStatsDto
{
    /// <summary>
    /// Număr total programări pentru azi.
    /// </summary>
    public int ProgramariAstazi { get; set; }

    /// <summary>
    /// Procentaj growth față de ieri.
    /// </summary>
    public int ProgramariGrowth { get; set; }

    /// <summary>
    /// Număr pacienți în sala de așteptare (status: CheckedIn).
    /// </summary>
    public int PacientiInAsteptare { get; set; }

    /// <summary>
    /// Timp mediu de așteptare (minute).
    /// </summary>
    public int TimpMediuAsteptare { get; set; }

    /// <summary>
    /// Număr programări finalizate azi.
    /// </summary>
    public int ProgramariFinalizate { get; set; }

    /// <summary>
    /// Număr programări rămase pentru azi.
    /// </summary>
    public int ProgramariRamase { get; set; }

    /// <summary>
    /// Număr pacienți noi (prima programare) în săptămâna curentă.
    /// </summary>
    public int PacientiNoi { get; set; }
}
