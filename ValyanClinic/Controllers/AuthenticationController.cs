using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using MediatR;
using ValyanClinic.Application.Features.AuthManagement.Commands.Login;
using ValyanClinic.Application.Features.AuthManagement.DTOs;

namespace ValyanClinic.Controllers;

/// <summary>
/// API Controller for authentication operations.
/// Handles login, logout, and authentication state management.
/// Sets HTTP-only cookies BEFORE Blazor rendering for secure session management.
/// </summary>
/// <remarks>
/// This controller is critical for security:
/// - Sets HTTP-only, secure cookies (not accessible via JavaScript)
/// - Manages authentication claims and principal
/// - Coordinates with MediatR for business logic
/// - Provides session-only cookies (expire on browser close)
/// 
/// All endpoints return standardized JSON responses.
/// </remarks>
[ApiController]
[Route("api/[controller]")]
public class AuthenticationController : ControllerBase
{
    #region Constants

    /// <summary>
    /// Error message for generic authentication failures
    /// </summary>
    private const string ERROR_AUTHENTICATION_FAILED = "Autentificare esuata";

    /// <summary>
    /// Error message for generic authentication exceptions
    /// </summary>
    private const string ERROR_AUTHENTICATION_EXCEPTION = "A aparut o eroare la autentificare";

    /// <summary>
    /// Error message for logout exceptions
    /// </summary>
    private const string ERROR_LOGOUT_EXCEPTION = "A aparut o eroare la deconectare";

    /// <summary>
    /// Custom claim type for PersonalMedicalID
    /// </summary>
    private const string CLAIM_PERSONAL_MEDICAL_ID = "PersonalMedicalID";

    #endregion

    #region Dependencies

    private readonly IMediator _mediator;
    private readonly ILogger<AuthenticationController> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="AuthenticationController"/> class.
    /// </summary>
    /// <param name="mediator">MediatR instance for command/query handling</param>
    /// <param name="logger">Logger instance for structured logging</param>
    public AuthenticationController(
        IMediator mediator,
        ILogger<AuthenticationController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    #endregion

    #region Authentication Endpoints

    /// <summary>
    /// Authenticates a user and creates a session cookie.
    /// </summary>
    /// <param name="request">Login credentials and preferences</param>
    /// <returns>
    /// - 200 OK with user data if authentication successful
    /// - 401 Unauthorized if credentials are invalid
    /// - 500 Internal Server Error if an exception occurs
    /// </returns>
    /// <remarks>
    /// This endpoint:
    /// 1. Validates credentials via MediatR (LoginCommand)
    /// 2. Creates claims principal with user identity
    /// 3. Sets HTTP-only, secure, session-only cookie
    /// 4. Returns user data for client-side state management
    /// 
    /// Security:
    /// - Cookie is HTTP-only (not accessible via JavaScript)
    /// - Cookie is session-only (expires when browser closes)
    /// - Sliding expiration enabled (resets on activity)
    /// - All sensitive operations logged for audit trail
    /// </remarks>
    [HttpPost("login")]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        try
        {
            _logger.LogInformation("Login attempt for user: {Username}", request.Username);

            // Send command to business layer
            var command = new LoginCommand
            {
                Username = request.Username,
                Password = request.Password,
                RememberMe = request.RememberMe,
                ResetPasswordOnFirstLogin = request.ResetPasswordOnFirstLogin
            };

            var result = await _mediator.Send(command);

            // Check if authentication succeeded
            if (!result.IsSuccess || result.Value == null)
            {
                _logger.LogWarning("Login failed for user: {Username}", request.Username);
                return Unauthorized(new { message = result.FirstError ?? ERROR_AUTHENTICATION_FAILED });
            }

            // Create claims for authenticated user
            var claims = CreateUserClaims(result.Value);
            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            // Sign in user with session-only cookie
            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                principal,
                CreateAuthenticationProperties());

            _logger.LogInformation("User authenticated successfully: {Username} (Role: {Role})",
                request.Username, result.Value.Rol);

            // Return user data for client-side state
            return Ok(new LoginResponse
            {
                Success = true,
                Username = result.Value.Username,
                Email = result.Value.Email,
                Rol = result.Value.Rol,
                UtilizatorID = result.Value.UtilizatorID,
                PersonalMedicalID = result.Value.PersonalMedicalID,
                RequiresPasswordReset = result.Value.RequiresPasswordReset
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Authentication exception for user: {Username}", request.Username);
            return StatusCode(500, new { message = ERROR_AUTHENTICATION_EXCEPTION });
        }
    }

    /// <summary>
    /// Signs out the current user and clears the authentication cookie.
    /// </summary>
    /// <returns>
    /// - 200 OK with success flag
    /// - 500 Internal Server Error if an exception occurs
    /// </returns>
    /// <remarks>
    /// This endpoint:
    /// 1. Signs out the user from cookie authentication
    /// 2. Clears the authentication cookie
    /// 3. Logs the logout event for audit trail
    /// 
    /// Client should redirect to login page after successful logout.
    /// </remarks>
    [HttpPost("logout")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Logout()
    {
        try
        {
            var username = User.Identity?.Name ?? "Unknown";

            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            _logger.LogInformation("User logged out successfully: {Username}", username);

            return Ok(new { success = true });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Logout exception");
            return StatusCode(500, new { message = ERROR_LOGOUT_EXCEPTION });
        }
    }

    /// <summary>
    /// Checks the current authentication state.
    /// </summary>
    /// <returns>
    /// - 200 OK with authentication status and user data if authenticated
    /// - 200 OK with authenticated: false if not authenticated
    /// </returns>
    /// <remarks>
    /// This endpoint is useful for:
    /// - Client-side authentication state checks
    /// - Health checks / monitoring
    /// - Development/debugging
    /// 
    /// Does not modify authentication state.
    /// </remarks>
    [HttpGet("check")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public IActionResult CheckAuthentication()
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return Ok(new
            {
                authenticated = true,
                username = User.Identity.Name,
                role = User.FindFirst(ClaimTypes.Role)?.Value
            });
        }

        return Ok(new { authenticated = false });
    }

    #endregion

    #region Helper Methods

    /// <summary>
    /// Creates claims array from authenticated user data.
    /// </summary>
    /// <param name="userData">User data from successful authentication</param>
    /// <returns>Array of claims for claims principal</returns>
    private Claim[] CreateUserClaims(LoginResultDto userData)
    {
        return new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userData.UtilizatorID.ToString()),
            new Claim(ClaimTypes.Name, userData.Username),
            new Claim(ClaimTypes.Email, userData.Email),
            new Claim(ClaimTypes.Role, userData.Rol),
            new Claim(CLAIM_PERSONAL_MEDICAL_ID, userData.PersonalMedicalID.ToString())
        };
    }

    /// <summary>
    /// Creates authentication properties for cookie-based authentication.
    /// </summary>
    /// <returns>Authentication properties configured for session-only cookies</returns>
    /// <remarks>
    /// Configuration:
    /// - IsPersistent = false: Cookie expires when browser closes (all windows)
    /// - ExpiresUtc = null: No explicit expiration (rely on browser session)
    /// - AllowRefresh = true: Sliding expiration (resets timeout on activity)
    /// - IssuedUtc = now: Timestamp for tracking
    /// </remarks>
    private AuthenticationProperties CreateAuthenticationProperties()
    {
        return new AuthenticationProperties
        {
            IsPersistent = false,      // Session-only cookie
            ExpiresUtc = null,          // No explicit expiration
            AllowRefresh = true,        // Sliding expiration enabled
            IssuedUtc = DateTimeOffset.Now
        };
    }

    #endregion

    #region Debug Endpoints (Development Only)

#if DEBUG
    /// <summary>
    /// Tests BCrypt password hashing and verification (DEBUG ONLY).
    /// </summary>
    /// <param name="request">Password and hash to test</param>
    /// <returns>Hash verification result and new generated hash</returns>
    /// <remarks>
    /// ⚠️ THIS ENDPOINT IS ONLY AVAILABLE IN DEBUG BUILDS.
    /// Used for testing password hash generation and verification.
    /// Should NOT be available in production.
    /// </remarks>
    [HttpPost("test-hash")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public IActionResult TestHash([FromBody] TestHashRequest request)
    {
        try
        {
            _logger.LogInformation("Testing hash verification (DEBUG)");

            // Direct instantiation for debug only
            var hasher = new ValyanClinic.Infrastructure.Security.BCryptPasswordHasher(
                _logger as ILogger<ValyanClinic.Infrastructure.Security.BCryptPasswordHasher>);

            bool isValid = hasher.VerifyPassword(request.Password, request.Hash);
            string newHash = hasher.HashPassword(request.Password);

            _logger.LogInformation("Hash verification result: {Result}", isValid);

            return Ok(new
            {
                inputPassword = request.Password,
                inputHash = request.Hash,
                verificationResult = isValid,
                newHashGenerated = newHash,
                message = isValid ? "Hash is VALID" : "Hash is INVALID - use newHashGenerated"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error testing hash");
            return StatusCode(500, new { error = ex.Message });
        }
    }

    /// <summary>
    /// Fixes/resets a user password (DEBUG ONLY).
    /// </summary>
    /// <param name="request">User ID, username, and new password</param>
    /// <returns>Success status and new credentials</returns>
    /// <remarks>
    /// ⚠️ THIS ENDPOINT IS ONLY AVAILABLE IN DEBUG BUILDS.
    /// Used for development/testing to reset passwords.
    /// Should NOT be available in production.
    /// </remarks>
    [HttpPost("fix-password")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> FixPassword([FromBody] FixPasswordRequest request)
    {
        try
        {
            _logger.LogInformation("Password reset request (DEBUG) for user: {Username}", request.Username);

            // Generate and verify new hash
            var hasher = new ValyanClinic.Infrastructure.Security.BCryptPasswordHasher(
                _logger as ILogger<ValyanClinic.Infrastructure.Security.BCryptPasswordHasher>);

            string newHash = hasher.HashPassword(request.NewPassword);
            bool testVerify = hasher.VerifyPassword(request.NewPassword, newHash);

            if (!testVerify)
            {
                _logger.LogError("Generated hash failed verification!");
                return BadRequest(new { error = "Generated hash failed verification" });
            }

            // Update password using command
            var changePasswordCommand = new ValyanClinic.Application.Features.UtilizatorManagement.Commands.ChangePassword.ChangePasswordCommand
            {
                UtilizatorID = request.UtilizatorID,
                NewPassword = request.NewPassword,
                ModificatDe = "DevSupport_PasswordFix"
            };

            var result = await _mediator.Send(changePasswordCommand);

            if (result.IsSuccess)
            {
                _logger.LogInformation("Password reset successful (DEBUG) for user: {Username}", request.Username);

                return Ok(new
                {
                    success = true,
                    message = "Password updated successfully",
                    username = request.Username,
                    newHashPreview = newHash.Substring(0, 30) + "...",
                    credentials = new
                    {
                        username = request.Username,
                        password = request.NewPassword
                    }
                });
            }
            else
            {
                _logger.LogError("Password reset failed (DEBUG): {Errors}", string.Join(", ", result.Errors));
                return BadRequest(new
                {
                    success = false,
                    error = string.Join(", ", result.Errors)
                });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Password reset exception (DEBUG)");
            return StatusCode(500, new { error = ex.Message });
        }
    }

    #region Debug DTOs

    /// <summary>
    /// Request DTO for test-hash endpoint (DEBUG ONLY)
    /// </summary>
    public class TestHashRequest
    {
        public string Password { get; set; } = string.Empty;
        public string Hash { get; set; } = string.Empty;
    }

    /// <summary>
    /// Request DTO for fix-password endpoint (DEBUG ONLY)
    /// </summary>
    public class FixPasswordRequest
    {
        public Guid UtilizatorID { get; set; }
        public string Username { get; set; } = string.Empty;
        public string NewPassword { get; set; } = string.Empty;
    }

    #endregion

#endif

    #endregion
}
