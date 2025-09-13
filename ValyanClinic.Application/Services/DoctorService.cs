using ValyanClinic.Application.DTOs;
using ValyanClinic.Application.Interfaces;
using ValyanClinic.Domain.Entities;

namespace ValyanClinic.Application.Services;

// Placeholder pentru implementare viitoare
// În momentul de fa?? ne concentr?m doar pe Utilizatori

public interface IDoctorService  
{
    Task<string> GetStatusAsync();
}

public class DoctorService : IDoctorService
{
    public async Task<string> GetStatusAsync()
    {
        await Task.Delay(100);
        return "Serviciul pentru doctori va fi implementat în curând. Focus actual: Utilizatori.";
    }
}