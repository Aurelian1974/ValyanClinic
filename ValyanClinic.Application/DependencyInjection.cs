using Microsoft.Extensions.DependencyInjection;
using ValyanClinic.Application.Interfaces;
using ValyanClinic.Application.Services;

namespace ValyanClinic.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IPatientService, PatientService>();
        services.AddScoped<IDoctorService, DoctorService>();
        services.AddScoped<IAppointmentService, AppointmentService>();
        services.AddScoped<IDashboardService, DashboardService>();

        return services;
    }
}