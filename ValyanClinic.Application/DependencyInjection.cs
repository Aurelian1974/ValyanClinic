using Microsoft.Extensions.DependencyInjection;

namespace ValyanClinic.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // Momentan nu adaug?m servicii în Application layer
        // Ne concentr?m pe serviciile din proiectul Blazor pentru simplitate
        // services.AddScoped<IPatientService, PatientService>();
        // services.AddScoped<IDoctorService, DoctorService>();
        // services.AddScoped<IAppointmentService, AppointmentService>();
        // services.AddScoped<IDashboardService, DashboardService>();

        return services;
    }
}