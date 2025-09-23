using ValyanClinic.Application.Models;
using ValyanClinic.Domain.Models;

namespace ValyanClinic.Components.Pages.HomePage;

/// <summary>
/// Page-specific models pentru Home - ORGANIZAT iN FOLDER HomePage
/// UPDATED: Dashboard models in loc de coming soon features
/// Optimizat pentru C# 13 & .NET 9
/// </summary>
public class HomeModels
{
    public UserStatistics UserStatistics { get; set; } = new();
    
    // Dashboard Stats
    public List<DashboardStat> DashboardStats { get; private set; } = [];
    
    // Quick Actions pentru dashboard
    public List<QuickAction> QuickActions { get; private set; } = [];
    
    // Recent Activities
    public List<RecentActivity> RecentActivities { get; private set; } = [];
    
    // Active users count
    public int ActiveUsersCount { get; set; } = 12;

    public void InitializeDashboardData()
    {
        InitializeDashboardStats();
        InitializeQuickActions();
        InitializeRecentActivities();
    }

    private void InitializeDashboardStats()
    {
        DashboardStats = [
            new() { 
                Icon = "fas fa-users", 
                Label = "Utilizatori Activi", 
                Value = UserStatistics?.TotalUsers.ToString() ?? "12",
                Color = "#667eea" 
            },
            new() { 
                Icon = "fas fa-user-md", 
                Label = "Doctori", 
                Value = UserStatistics?.DoctorsCount.ToString() ?? "4",
                Color = "#28a745" 
            },
            new() { 
                Icon = "fas fa-calendar-check", 
                Label = "Programri Astzi", 
                Value = "8",
                Color = "#17a2b8" 
            },
            new() { 
                Icon = "fas fa-heartbeat", 
                Label = "Consulii Active", 
                Value = "3",
                Color = "#dc3545" 
            }
        ];
    }

    private void InitializeQuickActions()
    {
        QuickActions = [
            new()
            {
                Title = "Gestionare Utilizatori",
                Description = "Administreaz? utilizatorii sistemului",
                Icon = "fas fa-users-cog",
                Color = "#667eea",
                Route = "/utilizatori",
                IsAvailable = true
            },
            new()
            {
                Title = "Pacieni",
                Description = "Gestioneaz? pacienii ?i dosarele medicale",
                Icon = "fas fa-user-injured",
                Color = "#28a745",
                Route = "/pacienti",
                IsAvailable = false
            },
            new()
            {
                Title = "Programri",
                Description = "Calendar ?i programri medicale",
                Icon = "fas fa-calendar-alt",
                Color = "#17a2b8",
                Route = "/programari",
                IsAvailable = false
            },
            new()
            {
                Title = "Facturare",
                Description = "Gestioneaz? facturile ?i pl??ile",
                Icon = "fas fa-file-invoice-dollar",
                Color = "#ffc107",
                Route = "/facturi",
                IsAvailable = false
            },
            new()
            {
                Title = "Stocuri",
                Description = "Inventarul ?i echipamentele medicale",
                Icon = "fas fa-boxes",
                Color = "#6f42c1",
                Route = "/stocuri",
                IsAvailable = false
            },
            new()
            {
                Title = "Rapoarte",
                Description = "Analize ?i rapoarte statistice",
                Icon = "fas fa-chart-bar",
                Color = "#fd7e14",
                Route = "/rapoarte",
                IsAvailable = false
            }
        ];
    }

    private void InitializeRecentActivities()
    {
        RecentActivities = [
            new()
            {
                Icon = "fas fa-user-plus",
                Message = "Utilizator nou inregistrat: Dr. Maria Popescu",
                TimeAgo = "Acum 2 ore"
            },
            new()
            {
                Icon = "fas fa-calendar-check",
                Message = "Programare confirmat? pentru Ion Popescu",
                TimeAgo = "Acum 3 ore"
            },
            new()
            {
                Icon = "fas fa-file-medical",
                Message = "Consultaie completat? pentru Ana Ionescu",
                TimeAgo = "Acum 5 ore"
            },
            new()
            {
                Icon = "fas fa-prescription-bottle-alt",
                Message = "Actualizare stoc medicamente",
                TimeAgo = "Acum 6 ore"
            },
            new()
            {
                Icon = "fas fa-user-edit",
                Message = "Profil utilizator actualizat: Elena Vasile",
                TimeAgo = "Acum 1 zi"
            }
        ];
    }
}

// Supporting Models - optimizat cu record types pentru C# 13
public record DashboardStat
{
    public required string Icon { get; init; }
    public required string Label { get; init; }
    public required string Value { get; init; }
    public string Color { get; init; } = "#667eea";
}

public record QuickAction
{
    public required string Title { get; init; }
    public required string Description { get; init; }
    public required string Icon { get; init; }
    public required string Color { get; init; }
    public required string Route { get; init; }
    public bool IsAvailable { get; init; } = true;
}

public record RecentActivity
{
    public required string Icon { get; init; }
    public required string Message { get; init; }
    public required string TimeAgo { get; init; }
}

// Extension methods pentru enhanced functionality
public static class HomeModelsExtensions
{
    public static IEnumerable<QuickAction> GetAvailableActions(this HomeModels models) =>
        models.QuickActions.Where(a => a.IsAvailable);
    
    public static IEnumerable<QuickAction> GetComingSoonActions(this HomeModels models) =>
        models.QuickActions.Where(a => !a.IsAvailable);
    
    public static string GetDashboardSummary(this HomeModels models)
    {
        var availableActions = models.QuickActions.Count(a => a.IsAvailable);
        var totalActions = models.QuickActions.Count;
        return $"{availableActions} din {totalActions} funcii disponibile";
    }
}