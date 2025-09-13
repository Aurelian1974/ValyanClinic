using ValyanClinic.Domain.Models;
using ValyanClinic.Domain.Enums;
using FluentValidation;

namespace ValyanClinic.Application.Services;

/// <summary>
/// Rich Authentication Service cu business logic
/// Înlocuie?te simple pass-through cu domain operations
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
    private readonly IValidator<LoginRequest> _validator;
    private readonly IUserSessionService _userSessionService;
    private readonly ISecurityAuditService _auditService;
    
    // În-memory storage pentru demo - în production ar fi database
    private static readonly Dictionary<string, int> _loginAttempts = new();
    private static readonly Dictionary<string, DateTime> _lockoutTimes = new();
    
    public AuthenticationService(
        IValidator<LoginRequest> validator,
        IUserSessionService userSessionService,
        ISecurityAuditService auditService)
    {
        _validator = validator;
        _userSessionService = userSessionService;
        _auditService = auditService;
    }

    public async Task<LoginResult> AuthenticateAsync(LoginRequest request)
    {
        try
        {
            // 1. Validare FluentValidation
            var validationResult = await _validator.ValidateAsync(request);
            if (!validationResult.IsValid)
            {
                var errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
                await _auditService.LogValidationFailureAsync(request.Username, errors);
                return LoginResult.Failure(errors, LoginFailureReason.ValidationError);
            }

            // 2. Verificare account locked
            if (await IsAccountLockedAsync(request.Username))
            {
                var remainingTime = await GetLockoutTimeRemainingAsync(request.Username);
                var message = remainingTime.HasValue 
                    ? $"Cont blocat temporar. Încerca?i din nou în {Math.Ceiling(remainingTime.Value.TotalMinutes)} minute."
                    : "Cont blocat din cauza prea multor încerc?ri.";
                
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
                
                return authResult;
            }
            else
            {
                // Increment failed attempts
                await IncrementLoginAttemptsAsync(request.Username);
                
                // Audit failure
                await _auditService.LogFailedLoginAsync(request.Username, authResult.FailureReason ?? LoginFailureReason.InvalidCredentials);
                
                return authResult;
            }
        }
        catch (Exception ex)
        {
            await _auditService.LogSystemErrorAsync(request.Username, ex);
            return LoginResult.Failure("A ap?rut o eroare la autentificare. V? rug?m s? încerca?i din nou.", LoginFailureReason.SystemError);
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
                return LoginResult.Failure("Contul este inactiv. Contacta?i administratorul.", LoginFailureReason.AccountDisabled);
            }
            
            return LoginResult.Success(userSession, "Autentificare reu?it?!");
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
/// Service pentru audit ?i logging securitate
/// </summary>
public interface ISecurityAuditService
{
    Task LogSuccessfulLoginAsync(string username, UserRole role);
    Task LogFailedLoginAsync(string username, LoginFailureReason reason);
    Task LogValidationFailureAsync(string username, List<string> errors);
    Task LogLockedAccountAttemptAsync(string username);
    Task LogSystemErrorAsync(string username, Exception exception);
}

// Implement?ri simple pentru demo
public class UserSessionService : IUserSessionService
{
    private static readonly Dictionary<string, UserSession> _activeSessions = new();

    public async Task<string> CreateSessionAsync(UserSession userSession)
    {
        await Task.CompletedTask;
        var sessionToken = Guid.NewGuid().ToString();
        _activeSessions[sessionToken] = userSession;
        Console.WriteLine($"DEBUG: Session created with token: {sessionToken.Substring(0, 8)}... for user: {userSession.Username}");
        return sessionToken;
    }

    public async Task<bool> ValidateSessionAsync(string sessionToken)
    {
        await Task.CompletedTask;
        
        // Pentru testare: accept? ?i token-ul de test
        if (sessionToken == "test-session-token")
        {
            Console.WriteLine("DEBUG: Test session token accepted");
            return true;
        }
        
        var isValid = _activeSessions.ContainsKey(sessionToken);
        Console.WriteLine($"DEBUG: Session validation for {sessionToken.Substring(0, 8)}...: {isValid}");
        return isValid;
    }

    public async Task InvalidateSessionAsync(string sessionToken)
    {
        await Task.CompletedTask;
        var removed = _activeSessions.Remove(sessionToken);
        Console.WriteLine($"DEBUG: Session invalidated: {sessionToken.Substring(0, 8)}... (removed: {removed})");
    }

    public async Task<UserSession?> GetSessionAsync(string sessionToken)
    {
        await Task.CompletedTask;
        
        // Pentru testare: returnez un utilizator de test pentru token-ul de test
        if (sessionToken == "test-session-token")
        {
            Console.WriteLine("DEBUG: Returning test user session");
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
        Console.WriteLine($"DEBUG: Getting session for {sessionToken.Substring(0, 8)}...: {session?.Username ?? "null"}");
        return session;
    }
}

public class SecurityAuditService : ISecurityAuditService
{
    public async Task LogSuccessfulLoginAsync(string username, UserRole role)
    {
        await Task.CompletedTask;
        Console.WriteLine($"[AUDIT] Successful login: {username} ({role.GetDisplayName()}) at {DateTime.Now}");
    }

    public async Task LogFailedLoginAsync(string username, LoginFailureReason reason)
    {
        await Task.CompletedTask;
        Console.WriteLine($"[AUDIT] Failed login: {username} - {reason.GetDisplayName()} at {DateTime.Now}");
    }

    public async Task LogValidationFailureAsync(string username, List<string> errors)
    {
        await Task.CompletedTask;
        Console.WriteLine($"[AUDIT] Validation failure: {username} - {string.Join(", ", errors)} at {DateTime.Now}");
    }

    public async Task LogLockedAccountAttemptAsync(string username)
    {
        await Task.CompletedTask;
        Console.WriteLine($"[AUDIT] Locked account attempt: {username} at {DateTime.Now}");
    }

    public async Task LogSystemErrorAsync(string username, Exception exception)
    {
        await Task.CompletedTask;
        Console.WriteLine($"[AUDIT] System error during login: {username} - {exception.Message} at {DateTime.Now}");
    }
}