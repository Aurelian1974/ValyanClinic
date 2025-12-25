using Microsoft.AspNetCore.Authorization;

namespace ValyanClinic.Application.Authorization.Requirements;

/// <summary>
/// Requirement care verifică dacă utilizatorul are o permisiune specifică.
/// Folosit pentru autorizare granulară bazată pe permisiuni.
/// </summary>
/// <remarks>
/// Exemplu utilizare:
/// [Authorize(Policy = "CanViewPatients")] - verifică Permissions.Pacient.View
/// </remarks>
public class PermissionRequirement : IAuthorizationRequirement
{
    /// <summary>
    /// Permisiunea necesară (ex: "Pacient.View", "Consultatie.Create")
    /// </summary>
    public string Permission { get; }
    
    public PermissionRequirement(string permission)
    {
        Permission = permission ?? throw new ArgumentNullException(nameof(permission));
    }
}

/// <summary>
/// Requirement care verifică dacă utilizatorul are cel puțin una din mai multe permisiuni.
/// </summary>
public class AnyPermissionRequirement : IAuthorizationRequirement
{
    /// <summary>
    /// Lista de permisiuni - utilizatorul trebuie să aibă cel puțin una
    /// </summary>
    public IReadOnlyList<string> Permissions { get; }
    
    public AnyPermissionRequirement(params string[] permissions)
    {
        if (permissions == null || permissions.Length == 0)
            throw new ArgumentException("At least one permission is required", nameof(permissions));
            
        Permissions = permissions;
    }
}

/// <summary>
/// Requirement care verifică dacă utilizatorul face parte din staff-ul medical.
/// </summary>
public class MedicalStaffRequirement : IAuthorizationRequirement
{
    /// <summary>
    /// Dacă true, necesită PersonalMedicalID valid
    /// </summary>
    public bool RequirePersonalMedicalId { get; }
    
    public MedicalStaffRequirement(bool requirePersonalMedicalId = true)
    {
        RequirePersonalMedicalId = requirePersonalMedicalId;
    }
}

/// <summary>
/// Requirement pentru acces la resurse proprii (ex: consultațiile medicului curent).
/// </summary>
public class ResourceOwnerRequirement : IAuthorizationRequirement
{
    /// <summary>
    /// Tipul resursei (ex: "Consultatie", "Programare")
    /// </summary>
    public string ResourceType { get; }
    
    public ResourceOwnerRequirement(string resourceType)
    {
        ResourceType = resourceType ?? throw new ArgumentNullException(nameof(resourceType));
    }
}

/// <summary>
/// Requirement pentru acces în situații de urgență (break-glass).
/// Toate accesările sunt logate pentru audit.
/// </summary>
public class EmergencyAccessRequirement : IAuthorizationRequirement
{
    /// <summary>
    /// Motivul pentru care se solicită acces de urgență
    /// </summary>
    public string? Reason { get; }
    
    public EmergencyAccessRequirement(string? reason = null)
    {
        Reason = reason;
    }
}
