using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using ValyanClinic.Application.Authorization.Handlers;
using ValyanClinic.Application.Authorization.Requirements;

namespace ValyanClinic.Application.Authorization;

/// <summary>
/// Extension methods pentru configurarea Policy-Based Authorization.
/// Folosit în Program.cs pentru înregistrarea politicilor.
/// </summary>
public static class AuthorizationConfiguration
{
    /// <summary>
    /// Înregistrează toate politicile de autorizare în servicii.
    /// Apelat din Program.cs.
    /// </summary>
    /// <param name="services">Service collection</param>
    /// <returns>Service collection pentru chaining</returns>
    public static IServiceCollection AddValyanClinicAuthorization(this IServiceCollection services)
    {
        // Înregistrează Authorization Handlers
        services.AddScoped<IAuthorizationHandler, PermissionAuthorizationHandler>();
        services.AddScoped<IAuthorizationHandler, AnyPermissionAuthorizationHandler>();
        services.AddScoped<IAuthorizationHandler, MedicalStaffAuthorizationHandler>();

        // Configurează politicile
        services.AddAuthorizationCore(options =>
        {
            ConfigurePolicies(options);
        });

        return services;
    }

    /// <summary>
    /// Configurează toate politicile de autorizare.
    /// </summary>
    private static void ConfigurePolicies(AuthorizationOptions options)
    {
        // ============================================
        // POLITICI PENTRU PACIENȚI
        // ============================================
        
        options.AddPolicy(Policies.CanViewPatients, policy =>
            policy.Requirements.Add(new PermissionRequirement(Permissions.Pacient.View)));
        
        options.AddPolicy(Policies.CanManagePatients, policy =>
            policy.Requirements.Add(new AnyPermissionRequirement(
                Permissions.Pacient.Create,
                Permissions.Pacient.Edit,
                Permissions.Pacient.Delete)));
        
        options.AddPolicy(Policies.CanViewSensitivePatientData, policy =>
            policy.Requirements.Add(new PermissionRequirement(Permissions.Pacient.ViewSensitiveData)));

        // ============================================
        // POLITICI PENTRU CONSULTAȚII
        // ============================================
        
        options.AddPolicy(Policies.CanViewConsultations, policy =>
            policy.Requirements.Add(new AnyPermissionRequirement(
                Permissions.Consultatie.View,
                Permissions.Consultatie.ViewOwn,
                Permissions.Consultatie.ViewDepartment)));
        
        options.AddPolicy(Policies.CanCreateConsultations, policy =>
        {
            policy.Requirements.Add(new PermissionRequirement(Permissions.Consultatie.Create));
            policy.Requirements.Add(new MedicalStaffRequirement(requirePersonalMedicalId: true));
        });
        
        options.AddPolicy(Policies.CanEditOwnConsultations, policy =>
            policy.Requirements.Add(new PermissionRequirement(Permissions.Consultatie.EditOwn)));
        
        options.AddPolicy(Policies.CanPrescribe, policy =>
        {
            policy.Requirements.Add(new PermissionRequirement(Permissions.Consultatie.Prescribe));
            policy.Requirements.Add(new MedicalStaffRequirement(requirePersonalMedicalId: true));
        });

        // ============================================
        // POLITICI PENTRU PROGRAMĂRI
        // ============================================
        
        options.AddPolicy(Policies.CanViewAppointments, policy =>
            policy.Requirements.Add(new PermissionRequirement(Permissions.Programare.View)));
        
        options.AddPolicy(Policies.CanManageAppointments, policy =>
            policy.Requirements.Add(new AnyPermissionRequirement(
                Permissions.Programare.Create,
                Permissions.Programare.Edit,
                Permissions.Programare.Delete,
                Permissions.Programare.Cancel)));

        // ============================================
        // POLITICI PENTRU ADMINISTRARE
        // ============================================
        
        options.AddPolicy(Policies.CanAccessAdmin, policy =>
            policy.Requirements.Add(new PermissionRequirement(Permissions.Admin.AccessDashboard)));
        
        options.AddPolicy(Policies.CanViewAuditLog, policy =>
            policy.Requirements.Add(new PermissionRequirement(Permissions.Admin.ViewAuditLog)));
        
        options.AddPolicy(Policies.CanManageUsers, policy =>
            policy.Requirements.Add(new PermissionRequirement(Permissions.Admin.ManageUsers)));

        // ============================================
        // POLITICI BAZATE PE ROL (pentru compatibilitate)
        // ============================================
        
        // Aceasta înlocuiește [Authorize(Roles = "Doctor,Medic")]
        options.AddPolicy(Policies.RequiresDoctor, policy =>
            policy.Requirements.Add(new MedicalStaffRequirement(requirePersonalMedicalId: true)));
        
        // Pentru orice staff medical (Doctor, Asistent)
        options.AddPolicy(Policies.RequiresMedicalStaff, policy =>
            policy.Requirements.Add(new MedicalStaffRequirement(requirePersonalMedicalId: false)));
        
        // Doar Admin
        options.AddPolicy(Policies.RequiresAdmin, policy =>
            policy.Requirements.Add(new PermissionRequirement(Permissions.Special.FullAccess)));
    }
}
