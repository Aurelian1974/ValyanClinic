using Microsoft.AspNetCore.Components;
using System.Globalization;

namespace ValyanClinic.Components.Pages;

public partial class Home : ComponentBase
{
    // Dashboard statistics
    private int PatientsTodayCount { get; set; } = 42;
    private int PatientsTodayGrowth { get; set; } = 12;
    private int AppointmentsCount { get; set; } = 28;
    private int AppointmentsGrowth { get; set; } = 8;
    private int ActiveStaffCount { get; set; } = 15;
    private string MonthlyRevenue { get; set; } = "125K";
    private int RevenueGrowth { get; set; } = 18;

    protected override void OnInitialized()
    {
        // Initialize dashboard data
        // In viitor: fetch from service/repository
    }

    private string GetCurrentDate()
    {
        var culture = new CultureInfo("ro-RO");
        return DateTime.Now.ToString("dddd, dd MMMM yyyy", culture);
    }
}
