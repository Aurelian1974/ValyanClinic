using ValyanClinic.Application.DTOs;
using ValyanClinic.Application.Interfaces;
using ValyanClinic.Domain.Entities;
using ValyanClinic.Domain.Enums;

namespace ValyanClinic.Application.Services;

// Placeholder pentru implementare viitoare
// În momentul de fa?? ne concentr?m doar pe Utilizatori

public interface IAppointmentService
{
    Task<string> GetStatusAsync();
}

public class AppointmentService : IAppointmentService
{
    public async Task<string> GetStatusAsync()
    {
        await Task.Delay(100);
        return "Serviciul pentru program?ri va fi implementat în curând. Focus actual: Utilizatori.";
    }
}