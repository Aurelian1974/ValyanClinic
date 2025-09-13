using ValyanClinic.Application.DTOs;
using ValyanClinic.Application.Interfaces;
using ValyanClinic.Domain.Enums;

namespace ValyanClinic.Application.Services;

// Placeholder pentru implementare viitoare
// În momentul de fa?? ne concentr?m doar pe Utilizatori

public interface IDashboardService
{
    Task<string> GetStatusAsync();
}

public class DashboardService : IDashboardService
{
    public async Task<string> GetStatusAsync()
    {
        await Task.Delay(100);
        return "Serviciul pentru dashboard va fi implementat în curând. Focus actual: Utilizatori.";
    }
}