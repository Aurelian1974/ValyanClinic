using MediatR;
using Microsoft.Extensions.Logging;
using ValyanClinic.Application.Common.Results;
using ValyanClinic.Application.Features.AuthManagement.DTOs;
using ValyanClinic.Domain.Interfaces.Repositories;
using ValyanClinic.Domain.Interfaces.Security;

namespace ValyanClinic.Application.Features.AuthManagement.Commands.Login;

/// <summary>
/// Handler for user authentication command.
/// Validates credentials, checks account status, and returns authenticated user data.
/// </summary>
/// <remarks>
/// Authentication Flow:
/// 1. Find user by username
/// 2. Verify account is active
/// 3. Check if account is locked (5+ failed attempts)
/// 4. Verify password using BCrypt
/// 5. Update last authentication date
/// 6. Return user data for session creation
/// 
/// Security Features:
/// - Account lockout after 5 failed attempts
/// - Failed attempt tracking
/// - BCrypt password verification with salt
/// - Inactive account detection
/// - Generic error messages (prevent username enumeration)
/// 
/// Error Handling:
/// - User not found: Returns generic "invalid credentials" error
/// - Inactive account: Returns specific error for user support
/// - Locked account: Returns specific error with contact instructions
/// - Invalid password: Increments failed attempts, returns generic error
/// - Exceptions: Logs details, returns generic error to user
/// </remarks>
public class LoginCommandHandler : IRequestHandler<LoginCommand, Result<LoginResultDto>>
{
    #region Constants

    /// <summary>
    /// Maximum number of failed login attempts before account lockout
    /// </summary>
    private const int MAX_FAILED_ATTEMPTS = 5;

    /// <summary>
    /// Error message for invalid credentials (generic to prevent username enumeration)
    /// </summary>
    private const string ERROR_INVALID_CREDENTIALS = "Nume de utilizator sau parolă incorecte";

    /// <summary>
    /// Error message for inactive account
    /// </summary>
    private const string ERROR_ACCOUNT_INACTIVE = "Contul este inactiv. Contactați administratorul.";

    /// <summary>
    /// Error message for locked account
    /// </summary>
    private const string ERROR_ACCOUNT_LOCKED = "Cont blocat din cauza prea multor încercări eșuate. Contactați administratorul sau resetați parola.";

    /// <summary>
    /// Error message for generic authentication exceptions
    /// </summary>
    private const string ERROR_GENERIC = "A apărut o eroare la autentificare. Vă rugăm încercați din nou.";

    /// <summary>
    /// Success message for authentication
    /// </summary>
    private const string SUCCESS_MESSAGE = "Autentificare reușită";

    #endregion

    #region Dependencies

    private readonly IUtilizatorRepository _utilizatorRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ILogger<LoginCommandHandler> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="LoginCommandHandler"/> class.
    /// </summary>
    /// <param name="utilizatorRepository">Repository for user data access</param>
    /// <param name="passwordHasher">Service for password verification</param>
    /// <param name="logger">Logger for diagnostics and audit trail</param>
    public LoginCommandHandler(
        IUtilizatorRepository utilizatorRepository,
        IPasswordHasher passwordHasher,
        ILogger<LoginCommandHandler> logger)
    {
        _utilizatorRepository = utilizatorRepository;
        _passwordHasher = passwordHasher;
        _logger = logger;
    }

    #endregion

    #region Handler Implementation

    /// <summary>
    /// Handles the login command by validating credentials and returning user data.
    /// </summary>
    /// <param name="request">Login command with username and password</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>
    /// Result containing:
    /// - Success: LoginResultDto with user data
    /// - Failure: Error message (generic for security)
    /// </returns>
    public async Task<Result<LoginResultDto>> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Login attempt for user: {Username}", request.Username);

            // 1. Find user by username
            var utilizator = await _utilizatorRepository.GetByUsernameAsync(request.Username, cancellationToken);

            if (utilizator == null)
            {
                _logger.LogWarning("Login failed: User not found - {Username}", request.Username);
                return Result<LoginResultDto>.Failure(ERROR_INVALID_CREDENTIALS);
            }

            // 2. Check if account is active
            if (!utilizator.EsteActiv)
            {
                _logger.LogWarning("Login failed: Inactive account - {Username}", request.Username);
                return Result<LoginResultDto>.Failure(ERROR_ACCOUNT_INACTIVE);
            }

            // 3. Check if account is locked (BEFORE password verification to prevent brute force)
            if (IsAccountLocked(utilizator.NumarIncercariEsuate, utilizator.DataBlocare))
            {
                _logger.LogWarning("Login failed: Account locked - {Username}, Attempts: {Attempts}",
                    request.Username, utilizator.NumarIncercariEsuate);
                return Result<LoginResultDto>.Failure(ERROR_ACCOUNT_LOCKED);
            }

            // 4. Verify password using BCrypt (includes salt verification)
            var passwordValid = _passwordHasher.VerifyPassword(request.Password, utilizator.PasswordHash);

            if (!passwordValid)
            {
                _logger.LogWarning("Login failed: Invalid password - {Username}", request.Username);

                // Increment failed attempts counter
                await _utilizatorRepository.IncrementIncercariEsuateAsync(utilizator.UtilizatorID, cancellationToken);

                return Result<LoginResultDto>.Failure(ERROR_INVALID_CREDENTIALS);
            }

            // 5. Login SUCCESS - Update last authentication timestamp
            await _utilizatorRepository.UpdateUltimaAutentificareAsync(utilizator.UtilizatorID, cancellationToken);

            _logger.LogInformation("Login successful for user: {Username}, Role: {Role}",
                request.Username, utilizator.Rol);

            // 6. Create result DTO with user data
            var result = CreateLoginResultDto(utilizator, request.ResetPasswordOnFirstLogin);

            return Result<LoginResultDto>.Success(result, SUCCESS_MESSAGE);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Authentication exception for user: {Username}", request.Username);
            return Result<LoginResultDto>.Failure(ERROR_GENERIC);
        }
    }

    #endregion

    #region Helper Methods

    /// <summary>
    /// Checks if account is locked based on failed attempts and lock date.
    /// </summary>
    /// <param name="failedAttempts">Number of failed login attempts</param>
    /// <param name="lockDate">Date when account was locked (null if not locked)</param>
    /// <returns>True if account is locked, false otherwise</returns>
    private static bool IsAccountLocked(int failedAttempts, DateTime? lockDate)
    {
        return failedAttempts >= MAX_FAILED_ATTEMPTS || lockDate.HasValue;
    }

    /// <summary>
    /// Creates LoginResultDto from user entity.
    /// </summary>
    /// <param name="utilizator">User entity from database</param>
    /// <param name="resetPasswordOnFirstLogin">Flag indicating password reset requirement</param>
    /// <returns>LoginResultDto with user data</returns>
    private static LoginResultDto CreateLoginResultDto(
        ValyanClinic.Domain.Entities.Utilizator utilizator,
        bool resetPasswordOnFirstLogin)
    {
        return new LoginResultDto
        {
            UtilizatorID = utilizator.UtilizatorID,
            PersonalMedicalID = utilizator.PersonalMedicalID,
            Username = utilizator.Username,
            Email = utilizator.Email ?? string.Empty,
            Rol = utilizator.RolDenumire ?? "User",
            RequiresPasswordReset = resetPasswordOnFirstLogin && utilizator.DataUltimaAutentificare == null
        };
    }

    #endregion
}
