namespace ValyanClinic.Application.Authorization;

/// <summary>
/// [DEPRECATED] - Permisiunile sunt acum gestionate în baza de date.
/// Această clasă este păstrată doar ca referință pentru structura inițială a permisiunilor.
/// 
/// ✅ Migrare completă: Handlerii de autorizare folosesc acum IRolRepository.GetPermisiuniForRolByDenumireAsync()
/// pentru a citi permisiunile din tabelele Roluri și RoluriPermisiuni.
/// 
/// @see sp_Roluri_GetByDenumire - stored procedure care returnează permisiunile pentru un rol
/// </summary>
[Obsolete("Permisiunile sunt acum gestionate în baza de date. Folosiți IRolRepository.GetPermisiuniForRolByDenumireAsync().")]
public static class RolePermissions
{
    /// <summary>
    /// Obține toate permisiunile pentru un rol dat.
    /// </summary>
    /// <param name="role">Rolul utilizatorului (Admin, Doctor, Receptioner, etc.)</param>
    /// <returns>Lista de permisiuni pentru rol</returns>
    public static IReadOnlyList<string> GetPermissionsForRole(string role)
    {
        return role?.ToLower() switch
        {
            "admin" or "administrator" => AdminPermissions,
            "doctor" or "medic" => DoctorPermissions,
            "asistent" => AsistentPermissions,
            "receptioner" => ReceptionerPermissions,
            "manager" => ManagerPermissions,
            _ => DefaultPermissions
        };
    }
    
    /// <summary>
    /// Verifică dacă un rol are o permisiune specifică.
    /// </summary>
    public static bool HasPermission(string role, string permission)
    {
        var permissions = GetPermissionsForRole(role);
        return permissions.Contains(permission);
    }

    #region Permission Sets per Role

    /// <summary>
    /// Administrator - Acces complet la toate funcționalitățile
    /// </summary>
    private static readonly IReadOnlyList<string> AdminPermissions = new[]
    {
        // Pacienți - Full Access
        Permissions.Pacient.View,
        Permissions.Pacient.Create,
        Permissions.Pacient.Edit,
        Permissions.Pacient.Delete,
        Permissions.Pacient.ViewSensitiveData,
        Permissions.Pacient.Export,
        
        // Consultații - Full Access
        Permissions.Consultatie.View,
        Permissions.Consultatie.ViewOwn,
        Permissions.Consultatie.ViewDepartment,
        Permissions.Consultatie.Create,
        Permissions.Consultatie.Edit,
        Permissions.Consultatie.EditOwn,
        Permissions.Consultatie.Delete,
        Permissions.Consultatie.Finalize,
        Permissions.Consultatie.Prescribe,
        
        // Programări - Full Access
        Permissions.Programare.View,
        Permissions.Programare.Create,
        Permissions.Programare.Edit,
        Permissions.Programare.Delete,
        Permissions.Programare.Cancel,
        Permissions.Programare.Confirm,
        
        // Personal - Full Access
        Permissions.Personal.View,
        Permissions.Personal.Create,
        Permissions.Personal.Edit,
        Permissions.Personal.Delete,
        Permissions.Personal.ManageRoles,
        
        // Admin - Full Access
        Permissions.Admin.AccessDashboard,
        Permissions.Admin.ViewAuditLog,
        Permissions.Admin.ManageSettings,
        Permissions.Admin.ManageUsers,
        Permissions.Admin.ViewReports,
        Permissions.Admin.ExportData,
        
        // Special
        Permissions.Special.FullAccess,
        Permissions.Special.EmergencyAccess
    };

    /// <summary>
    /// Doctor/Medic - Acces la pacienți, consultații proprii, prescripții
    /// </summary>
    private static readonly IReadOnlyList<string> DoctorPermissions = new[]
    {
        // Pacienți - View + Edit (fără Delete)
        Permissions.Pacient.View,
        Permissions.Pacient.Create,
        Permissions.Pacient.Edit,
        Permissions.Pacient.ViewSensitiveData,
        
        // Consultații - Full pentru proprii, View pentru departament
        Permissions.Consultatie.View,
        Permissions.Consultatie.ViewOwn,
        Permissions.Consultatie.ViewDepartment,
        Permissions.Consultatie.Create,
        Permissions.Consultatie.EditOwn,
        Permissions.Consultatie.Finalize,
        Permissions.Consultatie.Prescribe,
        
        // Programări - View + Create
        Permissions.Programare.View,
        Permissions.Programare.Create,
        Permissions.Programare.Edit,
        Permissions.Programare.Confirm,
        
        // Personal - doar View
        Permissions.Personal.View,
        
        // Special - Emergency access pentru urgențe
        Permissions.Special.EmergencyAccess
    };

    /// <summary>
    /// Asistent - Acces limitat, ajută medicul
    /// </summary>
    private static readonly IReadOnlyList<string> AsistentPermissions = new[]
    {
        // Pacienți - View + Create
        Permissions.Pacient.View,
        Permissions.Pacient.Create,
        Permissions.Pacient.Edit,
        
        // Consultații - doar View
        Permissions.Consultatie.View,
        Permissions.Consultatie.ViewDepartment,
        
        // Programări - Full pentru gestionare
        Permissions.Programare.View,
        Permissions.Programare.Create,
        Permissions.Programare.Edit,
        Permissions.Programare.Cancel,
        Permissions.Programare.Confirm,
        
        // Personal - doar View
        Permissions.Personal.View
    };

    /// <summary>
    /// Receptioner - Gestionează programări și înregistrare pacienți
    /// </summary>
    private static readonly IReadOnlyList<string> ReceptionerPermissions = new[]
    {
        // Pacienți - Create + Edit (înregistrare)
        Permissions.Pacient.View,
        Permissions.Pacient.Create,
        Permissions.Pacient.Edit,
        // NU are ViewSensitiveData - nu vede diagnostice!
        
        // Consultații - doar View limitat (fără detalii medicale)
        // NU are Consultatie.View - confidențialitate
        
        // Programări - Full Access
        Permissions.Programare.View,
        Permissions.Programare.Create,
        Permissions.Programare.Edit,
        Permissions.Programare.Delete,
        Permissions.Programare.Cancel,
        Permissions.Programare.Confirm,
        
        // Personal - doar View
        Permissions.Personal.View
    };

    /// <summary>
    /// Manager - Rapoarte și administrare, fără acces medical
    /// </summary>
    private static readonly IReadOnlyList<string> ManagerPermissions = new[]
    {
        // Pacienți - doar View
        Permissions.Pacient.View,
        Permissions.Pacient.Export,
        
        // Programări - View + Rapoarte
        Permissions.Programare.View,
        
        // Personal - Full Access
        Permissions.Personal.View,
        Permissions.Personal.Create,
        Permissions.Personal.Edit,
        
        // Admin - Rapoarte
        Permissions.Admin.AccessDashboard,
        Permissions.Admin.ViewReports,
        Permissions.Admin.ExportData
    };

    /// <summary>
    /// Default (Utilizator) - Acces minimal
    /// </summary>
    private static readonly IReadOnlyList<string> DefaultPermissions = new[]
    {
        Permissions.Pacient.View
    };

    #endregion
}
