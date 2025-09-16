using ValyanClinic.Domain.Models;
using ValyanClinic.Domain.Enums;
using ValyanClinic.Application.Validators;
using Microsoft.Extensions.Logging;

namespace ValyanClinic.Application.Services;

/// <summary>
/// Rich Authentication Service cu business logic
/// Înlocuiește simple pass-through cu domain operations
/// </summary>
public interface IAuthenticationService
{
    Task<LoginResult> AuthenticateAsync(LoginRequest request);
    Task<bool> ValidateSessionAsync(string sessionToken);
    Task LogoutAsync(string sessionToken);
    Task<bool> IsAccountLockedAsync(string username);
    Task<TimeSpan?> GetLockoutTimeRemainingAsync(string username);
    Task ResetLoginAttemptsAsync(string username);
}

public class AuthenticationService : IAuthenticationService
{
    private readonly IValidationService _validationService;
    private readonly IUserSessionService _userSessionService;
    private readonly ISecurityAuditService _auditService;
    private readonly ILogger<AuthenticationService> _logger;
    
    // În-memory storage pentru demo - în production ar fi database
    private static readonly Dictionary<string, int> _loginAttempts = new();
    private static readonly Dictionary<string, DateTime> _lockoutTimes = new();
    
    public AuthenticationService(
        IValidationService validationService,
        IUserSessionService userSessionService,
        ISecurityAuditService auditService,
        ILogger<AuthenticationService> logger)
    {
        _validationService = validationService;
        _userSessionService = userSessionService;
        _auditService = auditService;
        _logger = logger;
    }

    public async Task<LoginResult> AuthenticateAsync(LoginRequest request)
    {
        try
        {
            _logger.LogInformation("Authentication attempt for user: {Username}", request.Username);

            // 1. Validare FluentValidation prin ValidationService
            var validationResult = await _validationService.ValidateAsync(request);
            if (!validationResult.IsValid)
            {
                var errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
                _logger.LogWarning("Authentication validation failed for user: {Username} with {ErrorCount} errors", 
                    request.Username, errors.Count);
                await _auditService.LogValidationFailureAsync(request.Username, errors);
                return LoginResult.Failure(errors, LoginFailureReason.ValidationError);
            }

            // 2. Verificare account locked
            if (await IsAccountLockedAsync(request.Username))
            {
                var remainingTime = await GetLockoutTimeRemainingAsync(request.Username);
                var message = remainingTime.HasValue 
                    ? $"Cont blocat temporar. Încercați din nou în {Math.Ceiling(remainingTime.Value.TotalMinutes)} minute."
                    : "Cont blocat din cauza prea multor încercări.";
                
                _logger.LogWarning("Locked account attempt for user: {Username}", request.Username);
                await _auditService.LogLockedAccountAttemptAsync(request.Username);
                return LoginResult.AccountLocked(message, remainingTime ?? TimeSpan.Zero);
            }

            // 3. Autentificare business logic
            var authResult = await PerformAuthenticationAsync(request);
            
            if (authResult.IsSuccess)
            {
                // Reset login attempts on success
                await ResetLoginAttemptsAsync(request.Username);
                
                // Create session
                var sessionToken = await _userSessionService.CreateSessionAsync(authResult.UserSession!);
                
                // Audit success
                await _auditService.LogSuccessfulLoginAsync(request.Username, authResult.UserSession!.Role);
                
                _logger.LogInformation("Authentication successful for user: {Username}", request.Username);
                return authResult;
            }
            else
            {
                // Increment failed attempts
                await IncrementLoginAttemptsAsync(request.Username);
                
                // Audit failure
                await _auditService.LogFailedLoginAsync(request.Username, authResult.FailureReason ?? LoginFailureReason.InvalidCredentials);
                
                _logger.LogWarning("Authentication failed for user: {Username}, reason: {FailureReason}", 
                    request.Username, authResult.FailureReason);
                return authResult;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "System error during authentication for user: {Username}", request.Username);
            await _auditService.LogSystemErrorAsync(request.Username, ex);
            return LoginResult.Failure("A apărut o eroare la autentificare. Vă rugăm să încercați din nou.", LoginFailureReason.SystemError);
        }
    }

    private async Task<LoginResult> PerformAuthenticationAsync(LoginRequest request)
    {
        // Simulate network delay pentru UX realist
        await Task.Delay(1000);

        // Business logic pentru autentificare
        var demoUser = DemoUsers.FindUser(request.Username, request.Password);
        
        if (demoUser != null)
        {
            var userSession = demoUser.ToUserSession();
            
            // Business rules validation
            if (!userSession.IsActive)
            {
                return LoginResult.Failure("Contul este inactiv. Contactați administratorul.", LoginFailureReason.AccountDisabled);
            }
            
            return LoginResult.Success(userSession, "Autentificare reușită!");
        }

        return LoginResult.Failure("Numele de utilizator sau parola sunt incorecte.", LoginFailureReason.InvalidCredentials);
    }

    private async Task IncrementLoginAttemptsAsync(string username)
    {
        await Task.Run(() =>
        {
            var key = username.ToLowerInvariant();
            _loginAttempts[key] = _loginAttempts.GetValueOrDefault(key, 0) + 1;
            
            // Lock account after 5 failed attempts
            if (_loginAttempts[key] >= 5)
            {
                _lockoutTimes[key] = DateTime.UtcNow.AddMinutes(15); // 15 minute lockout
                _logger.LogWarning("Account locked for user: {Username} after {Attempts} failed attempts", username, _loginAttempts[key]);
            }
        });
    }

    public async Task<bool> IsAccountLockedAsync(string username)
    {
        return await Task.Run(() =>
        {
            var key = username.ToLowerInvariant();
            if (!_lockoutTimes.ContainsKey(key)) return false;
            
            var lockoutTime = _lockoutTimes[key];
            if (DateTime.UtcNow > lockoutTime)
            {
                // Lockout expired, clean up
                _lockoutTimes.Remove(key);
                _loginAttempts.Remove(key);
                _logger.LogInformation("Account lockout expired for user: {Username}", username);
                return false;
            }
            
            return true;
        });
    }

    public async Task<TimeSpan?> GetLockoutTimeRemainingAsync(string username)
    {
        return await Task.Run(() =>
        {
            var key = username.ToLowerInvariant();
            if (!_lockoutTimes.ContainsKey(key)) 
                return (TimeSpan?)null;
            
            var lockoutTime = _lockoutTimes[key];
            var remaining = lockoutTime - DateTime.UtcNow;
            
            return remaining > TimeSpan.Zero ? (TimeSpan?)remaining : null;
        });
    }

    public async Task ResetLoginAttemptsAsync(string username)
    {
        await Task.Run(() =>
        {
            var key = username.ToLowerInvariant();
            _loginAttempts.Remove(key);
            _lockoutTimes.Remove(key);
        });
    }

    public async Task<bool> ValidateSessionAsync(string sessionToken)
    {
        return await _userSessionService.ValidateSessionAsync(sessionToken);
    }

    public async Task LogoutAsync(string sessionToken)
    {
        await _userSessionService.InvalidateSessionAsync(sessionToken);
    }
}

/// <summary>
/// Service pentru gestionarea sesiunilor utilizatorilor
/// </summary>
public interface IUserSessionService
{
    Task<string> CreateSessionAsync(UserSession userSession);
    Task<bool> ValidateSessionAsync(string sessionToken);
    Task InvalidateSessionAsync(string sessionToken);
    Task<UserSession?> GetSessionAsync(string sessionToken);
}

/// <summary>
/// Service pentru audit și logging securitate
/// </summary>
public interface ISecurityAuditService
{
    Task LogSuccessfulLoginAsync(string username, UserRole role);
    Task LogFailedLoginAsync(string username, LoginFailureReason reason);
    Task LogValidationFailureAsync(string username, List<string> errors);
    Task LogLockedAccountAttemptAsync(string username);
    Task LogSystemErrorAsync(string username, Exception exception);
}

// Implementări simple pentru demo
public class UserSessionService : IUserSessionService
{
    private static readonly Dictionary<string, UserSession> _activeSessions = new();
    private readonly ILogger<UserSessionService> _logger;

    public UserSessionService(ILogger<UserSessionService> logger)
    {
        _logger = logger;
    }

    public async Task<string> CreateSessionAsync(UserSession userSession)
    {
        await Task.CompletedTask;
        var sessionToken = Guid.NewGuid().ToString();
        _activeSessions[sessionToken] = userSession;
        _logger.LogInformation("Session created for user: {Username}", userSession.Username);
        return sessionToken;
    }

    public async Task<bool> ValidateSessionAsync(string sessionToken)
    {
        await Task.CompletedTask;
        
        // Pentru testare: acceptă și token-ul de test
        if (sessionToken == "test-session-token")
        {
            _logger.LogInformation("Test session token accepted");
            return true;
        }
        
        var isValid = _activeSessions.ContainsKey(sessionToken);
        _logger.LogInformation("Session validation result: {IsValid}", isValid);
        return isValid;
    }

    public async Task InvalidateSessionAsync(string sessionToken)
    {
        await Task.CompletedTask;
        var removed = _activeSessions.Remove(sessionToken);
        _logger.LogInformation("Session invalidated, removed: {Removed}", removed);
    }

    public async Task<UserSession?> GetSessionAsync(string sessionToken)
    {
        await Task.CompletedTask;
        
        // Pentru testare: returnez un utilizator de test pentru token-ul de test
        if (sessionToken == "test-session-token")
        {
            _logger.LogInformation("Returning test user session");
            return new UserSession
            {
                Username = "test-user",
                FullName = "Test User",
                Email = "test@valyanmed.ro",
                Role = UserRole.Administrator,
                Department = "Testing",
                LoginTime = DateTime.Now,
                Status = UserStatus.Active
            };
        }
        
        var session = _activeSessions.GetValueOrDefault(sessionToken);
        _logger.LogInformation("Getting session, user: {Username}", session?.Username ?? "null");
        return session;
    }
}

public class SecurityAuditService : ISecurityAuditService
{
    private readonly ILogger<SecurityAuditService> _logger;

    public SecurityAuditService(ILogger<SecurityAuditService> logger)
    {
        _logger = logger;
    }

    public async Task LogSuccessfulLoginAsync(string username, UserRole role)
    {
        await Task.CompletedTask;
        _logger.LogInformation("Successful login: {Username} ({Role}) at {Timestamp}", 
            username, role.GetDisplayName(), DateTime.Now);
    }

    public async Task LogFailedLoginAsync(string username, LoginFailureReason reason)
    {
        await Task.CompletedTask;
        _logger.LogWarning("Failed login: {Username} - {Reason} at {Timestamp}", 
            username, reason.GetDisplayName(), DateTime.Now);
    }

    public async Task LogValidationFailureAsync(string username, List<string> errors)
    {
        await Task.CompletedTask;
        _logger.LogWarning("Validation failure: {Username} - {Errors} at {Timestamp}", 
            username, string.Join(", ", errors), DateTime.Now);
    }

    public async Task LogLockedAccountAttemptAsync(string username)
    {
        await Task.CompletedTask;
        _logger.LogWarning("Locked account attempt: {Username} at {Timestamp}", username, DateTime.Now);
    }

    public async Task LogSystemErrorAsync(string username, Exception exception)
    {
        await Task.CompletedTask;
        _logger.LogError(exception, "System error during login: {Username} at {Timestamp}", username, DateTime.Now);
    }
}
