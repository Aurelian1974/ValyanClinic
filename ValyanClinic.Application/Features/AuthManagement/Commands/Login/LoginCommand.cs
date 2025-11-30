using MediatR;
using ValyanClinic.Application.Common.Results;
using ValyanClinic.Application.Features.AuthManagement.DTOs;

namespace ValyanClinic.Application.Features.AuthManagement.Commands.Login;

/// <summary>
/// Command for user authentication.
/// Represents a request to authenticate a user with username and password.
/// </summary>
/// <remarks>
/// This command is used by:
/// - AuthenticationController for API-based login
/// - Any other component requiring user authentication
/// 
/// Validation:
/// - Handled by LoginCommandValidator using FluentValidation
/// - Ensures username and password meet minimum requirements
/// 
/// Security:
/// - Passwords are never logged
/// - Failed attempts are tracked and logged
/// - Account lockout after 5 failed attempts
/// - Password verification uses BCrypt with salt
/// 
/// Usage:
/// <code>
/// var command = new LoginCommand 
/// { 
///     Username = "john.doe", 
///     Password = "SecureP@ssw0rd",
///     RememberMe = true,
///     ResetPasswordOnFirstLogin = false
/// };
/// var result = await mediator.Send(command);
/// </code>
/// </remarks>
public record LoginCommand : IRequest<Result<LoginResultDto>>
{
    /// <summary>
    /// Username for authentication (required, 3-100 characters)
    /// </summary>
    /// <remarks>
    /// Validation:
    /// - Not empty
    /// - Length between 3 and 100 characters
    /// - Only alphanumeric and special characters: . _ @ -
    /// 
    /// Case Sensitivity:
    /// - Username comparison is case-insensitive in database
    /// </remarks>
    public string Username { get; init; } = string.Empty;

    /// <summary>
    /// Password for authentication (required, 6-100 characters)
    /// </summary>
    /// <remarks>
    /// Security:
    /// - Never logged or stored in plain text
    /// - Verified against BCrypt hashed password in database
    /// - Salt is included in BCrypt hash
    /// 
    /// Validation:
    /// - Not empty
    /// - Length between 6 and 100 characters
    /// - No specific complexity requirements (managed separately)
    /// </remarks>
    public string Password { get; init; } = string.Empty;

    /// <summary>
    /// Save username in localStorage for future logins (optional)
    /// </summary>
    /// <remarks>
    /// Note: This flag does NOT affect cookie persistence.
    /// Cookie is always session-only (expires when browser closes).
    /// This flag only controls client-side username storage in localStorage.
    /// </remarks>
    public bool RememberMe { get; init; }

    /// <summary>
    /// Force password reset on first login (optional, default: false)
    /// </summary>
    /// <remarks>
    /// Behavior:
    /// - If true and DataUltimaAutentificare is null, RequiresPasswordReset flag is set
    /// - Client should redirect user to password reset page
    /// - Used for new accounts or after admin password reset
    /// </remarks>
    public bool ResetPasswordOnFirstLogin { get; init; }
}
