using System.ComponentModel.DataAnnotations;

namespace ValyanClinic.Application.Features.AuthManagement.DTOs;

/// <summary>
/// Form model for login input fields.
/// Contains username, password, and authentication preferences.
/// Used by the Login component to collect user credentials.
/// </summary>
public class LoginFormModel
{
    /// <summary>
    /// Username for authentication (required, max 100 characters)
    /// </summary>
    [Required(ErrorMessage = "Numele de utilizator este obligatoriu")]
    [StringLength(100, ErrorMessage = "Numele de utilizator nu poate depăși 100 de caractere")]
    public string Username { get; set; } = string.Empty;

    /// <summary>
    /// Password for authentication (required, 6-100 characters)
    /// </summary>
    [Required(ErrorMessage = "Parola este obligatorie")]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "Parola trebuie să aibă între 6 și 100 de caractere")]
    public string Password { get; set; } = string.Empty;

    /// <summary>
    /// Save username in localStorage for future logins
    /// </summary>
    public bool RememberMe { get; set; } = false;

    /// <summary>
    /// Force password reset on first login
    /// </summary>
    public bool ResetPasswordOnFirstLogin { get; set; } = true;
}
