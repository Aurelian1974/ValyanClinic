using ValyanClinic.Application.DTOs;
using ValyanClinic.Application.Interfaces;
using ValyanClinic.Domain.Enums;

namespace ValyanClinic.Application.Services;

public class DashboardService : IDashboardService
{
    private readonly IPatientRepository _patientRepository;
    private readonly IAppointmentRepository _appointmentRepository;

    public DashboardService(
        IPatientRepository patientRepository,
        IAppointmentRepository appointmentRepository)
    {
        _patientRepository = patientRepository;
        _appointmentRepository = appointmentRepository;
    }

    public async Task<DashboardStatsDto> GetDashboardStatsAsync()
    {
        var today = DateTime.Today;
        var tomorrow = today.AddDays(1);

        // Get basic stats
        var allPatients = await _patientRepository.GetAllAsync();
        var todayAppointments = await _appointmentRepository.GetByDateRangeAsync(today, tomorrow);
        
        var totalPatients = allPatients.Count();
        var todayAppointmentsCount = todayAppointments.Count();
        var pendingAppointments = todayAppointments.Count(a => a.Status == AppointmentStatus.Scheduled);
        var todayRevenue = todayAppointments.Where(a => a.Cost.HasValue).Sum(a => a.Cost.Value);

        // Get recent activities
        var recentActivities = await GetRecentActivitiesAsync();
        
        // Get quick actions
        var quickActions = GetQuickActions();

        return new DashboardStatsDto
        {
            TotalPatients = totalPatients,
            TodayAppointments = todayAppointmentsCount,
            PendingAppointments = pendingAppointments,
            TodayRevenue = todayRevenue,
            RecentActivities = recentActivities,
            QuickActions = quickActions
        };
    }

    private async Task<List<RecentActivityDto>> GetRecentActivitiesAsync()
    {
        var today = DateTime.Today;
        var appointments = await _appointmentRepository.GetByDateRangeAsync(today.AddDays(-7), today.AddDays(1));
        
        return appointments
            .OrderByDescending(a => a.CreatedAt)
            .Take(5)
            .Select(a => new RecentActivityDto
            {
                Title = "Programare nou?",
                Description = $"Consulta?ie programat? pentru {a.AppointmentDate:dd.MM.yyyy} la {a.StartTime}",
                DateTime = a.CreatedAt,
                Icon = "calendar",
                Color = "primary"
            })
            .ToList();
    }

    private static List<QuickActionDto> GetQuickActions()
    {
        return new List<QuickActionDto>
        {
            new()
            {
                Title = "Adaug? Pacient",
                Description = "Înregistreaz? un pacient nou",
                Icon = "user-plus",
                Color = "success",
                Action = "ADAUGA_PACIENT"
            },
            new()
            {
                Title = "Programeaz? Consulta?ie",
                Description = "Creeaz? o programare nou?",
                Icon = "calendar-plus",
                Color = "info",
                Action = "PROGRAMEAZA_CONSULTATIE"
            },
            new()
            {
                Title = "Gestioneaz? Medicamente",
                Description = "Actualizeaz? stocul de medicamente",
                Icon = "pill",
                Color = "warning",
                Action = "GESTIONEAZA_MEDICAMENTE"
            },
            new()
            {
                Title = "Vezi Rapoarte",
                Description = "Genereaz? rapoarte financiare",
                Icon = "chart-bar",
                Color = "secondary",
                Action = "VEZI_RAPOARTE"
            }
        };
    }
}