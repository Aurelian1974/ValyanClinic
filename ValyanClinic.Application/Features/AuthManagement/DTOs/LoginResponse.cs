namespace ValyanClinic.Application.Features.AuthManagement.DTOs;

/// <summary>
/// Response DTO for successful login.
/// Contains authenticated user information and session details.
/// </summary>
public class LoginResponse
{
    /// <summary>
    /// Indicates if login was successful
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Authenticated username
    /// </summary>
    public string Username { get; set; } = string.Empty;

    /// <summary>
    /// User email address
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// User role for authorization (Doctor, Admin, etc.)
    /// </summary>
    public string Rol { get; set; } = string.Empty;

    /// <summary>
    /// Unique user identifier
    /// </summary>
    public Guid UtilizatorID { get; set; }

    /// <summary>
    /// Personal medical identifier (for medical staff)
    /// </summary>
    public Guid PersonalMedicalID { get; set; }

    /// <summary>
    /// Indicates if user must reset password on first login
    /// </summary>
    public bool RequiresPasswordReset { get; set; }
}
