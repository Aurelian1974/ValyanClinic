using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using ValyanClinic.Application.Interfaces;
using ValyanClinic.Infrastructure.Data;
using ValyanClinic.Infrastructure.Repositories;

namespace ValyanClinic.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // Database connection
        var connectionString = configuration.GetConnectionString("DefaultConnection") 
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
        
        services.AddSingleton<IDbConnectionFactory>(provider => new SqlConnectionFactory(connectionString));

        // Repositories
        services.AddScoped<IPatientRepository, PatientRepository>();
        services.AddScoped<IDoctorRepository, DoctorRepository>();
        services.AddScoped<IAppointmentRepository, AppointmentRepository>();
        // services.AddScoped<IMedicalRecordRepository, MedicalRecordRepository>();
        // services.AddScoped<IUserRepository, UserRepository>();

        return services;
    }
}