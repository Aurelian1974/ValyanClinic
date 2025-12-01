using System.Text.Json.Serialization;

namespace ValyanClinic.Application.Features.AuthManagement.DTOs;

/// <summary>
/// Authenticated user data returned from successful login.
/// Contains user identity and role information.
/// </summary>
/// <remarks>
/// JSON property names are lowercase to match JavaScript API response format.
/// This DTO is used for JavaScript interop between Blazor and the authentication API.
/// </remarks>
public class LoginResponseData
{
    /// <summary>
    /// Always true for successful login (redundant with LoginResult.Success)
    /// </summary>
    [JsonPropertyName("success")]
    public bool Success { get; set; }

    /// <summary>
    /// Authenticated username
    /// </summary>
    [JsonPropertyName("username")]
    public string Username { get; set; } = string.Empty;

    /// <summary>
    /// User email address
    /// </summary>
    [JsonPropertyName("email")]
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// User role for authorization (Doctor, Admin, Receptioner, etc.)
    /// Used for role-based dashboard redirection
    /// </summary>
    [JsonPropertyName("rol")]
    public string Rol { get; set; } = string.Empty;

    /// <summary>
    /// Unique user identifier (GUID)
    /// Used for session creation and user tracking
    /// </summary>
    [JsonPropertyName("utilizatorID")]
    public Guid UtilizatorID { get; set; }

    /// <summary>
    /// Indicates if user must reset password on first login
    /// If true, user should be redirected to password reset page
    /// </summary>
    [JsonPropertyName("requiresPasswordReset")]
    public bool RequiresPasswordReset { get; set; }
}
