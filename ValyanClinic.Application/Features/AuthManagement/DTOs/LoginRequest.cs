using System.ComponentModel.DataAnnotations;

namespace ValyanClinic.Application.Features.AuthManagement.DTOs;

/// <summary>
/// Request DTO for login endpoint.
/// Contains user credentials and authentication preferences.
/// </summary>
public class LoginRequest
{
    /// <summary>
    /// Username for authentication (required)
    /// </summary>
    [Required(ErrorMessage = "Username is required")]
    [StringLength(100, ErrorMessage = "Username cannot exceed 100 characters")]
    public string Username { get; set; } = string.Empty;

    /// <summary>
    /// Password for authentication (required)
    /// </summary>
    [Required(ErrorMessage = "Password is required")]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be between 6 and 100 characters")]
    public string Password { get; set; } = string.Empty;

    /// <summary>
    /// Save username in localStorage for future logins
    /// </summary>
    public bool RememberMe { get; set; }

    /// <summary>
    /// Force password reset on first login
    /// </summary>
    public bool ResetPasswordOnFirstLogin { get; set; }
}
