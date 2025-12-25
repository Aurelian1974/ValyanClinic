namespace ValyanClinic.Application.Features.AuthManagement.DTOs;

/// <summary>
/// Result DTO returned after successful authentication.
/// Contains authenticated user information and session details.
/// </summary>
public class LoginResultDto
{
    /// <summary>
    /// Unique user identifier (GUID)
    /// </summary>
    public Guid UtilizatorID { get; set; }

    /// <summary>
    /// Personal medical identifier (for medical staff)
    /// Used for linking to PersonalMedical table
    /// Nullable for superadmin users without PersonalMedical attachment
    /// </summary>
    public Guid? PersonalMedicalID { get; set; }

    /// <summary>
    /// Username used for authentication
    /// </summary>
    public string Username { get; set; } = string.Empty;

    /// <summary>
    /// User email address
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// User role for authorization (Doctor, Admin, Receptioner, etc.)
    /// Used for role-based access control and dashboard routing
    /// </summary>
    public string Rol { get; set; } = string.Empty;

    /// <summary>
    /// Indicates if user must reset password on first login
    /// Typically true when DataUltimaAutentificare is null
    /// </summary>
    public bool RequiresPasswordReset { get; set; }

    /// <summary>
    /// JWT token for stateless authentication (for future use)
    /// Currently null as we use cookie-based authentication
    /// </summary>
    public string? Token { get; set; }
}
