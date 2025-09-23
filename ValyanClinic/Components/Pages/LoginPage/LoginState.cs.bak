using ValyanClinic.Domain.Models;
using ValyanClinic.Domain.Enums;

namespace ValyanClinic.Components.Pages.LoginPage;

/// <summary>
/// State Management pentru Login page - ORGANIZAT ÎN FOLDER LoginPage
/// RESTRUCTURAT conform planului de refactorizare
/// </summary>
public class LoginState
{
    // Form State
    public LoginRequest LoginRequest { get; set; } = new();
    public bool IsLoading { get; set; } = false;
    public bool ShowPassword { get; set; } = false;
    
    // Validation State - FluentValidation în loc de Data Annotations
    public string? ErrorMessage { get; set; }
    public List<string> ValidationErrors { get; set; } = [];
    public bool HasValidationErrors => ValidationErrors.Count > 0;
    
    // UI State - calculat? în loc de hardcodat?
    public bool IsFormValid => !HasValidationErrors && 
                              !string.IsNullOrWhiteSpace(LoginRequest.Username) && 
                              !string.IsNullOrWhiteSpace(LoginRequest.Password);
                              
    public string PasswordInputType => ShowPassword ? "text" : "password";
    public string PasswordToggleIcon => ShowPassword ? "fa-eye-slash" : "fa-eye";
    public string PasswordToggleLabel => ShowPassword ? "Ascunde parola" : "Arat? parola";
    
    // Business State - cu domain logic
    public UserSession? CurrentUser { get; set; }
    public bool IsAuthenticated => CurrentUser != null && CurrentUser.IsActive;
    
    // Security State - business rules
    public int LoginAttempts { get; private set; } = 0;
    public bool IsAccountLocked => LoginAttempts >= 5;
    public TimeSpan? LockoutTimeRemaining { get; set; }
    
    // UI Preferences
    public bool ShowDemoCredentials { get; set; } = true;

    #region State Management Methods

    public void SetError(string? error)
    {
        ErrorMessage = error;
        ValidationErrors.Clear();
    }

    public void SetValidationErrors(List<string> errors)
    {
        ValidationErrors = errors;
        ErrorMessage = null;
    }

    public void ClearErrors()
    {
        ErrorMessage = null;
        ValidationErrors.Clear();
    }

    public void SetLoading(bool isLoading)
    {
        IsLoading = isLoading;
        if (isLoading)
        {
            ClearErrors(); // Clear errors when starting new request
        }
    }

    public void TogglePasswordVisibility()
    {
        ShowPassword = !ShowPassword;
    }

    public void IncrementLoginAttempts()
    {
        LoginAttempts++;
        if (LoginAttempts >= 5)
        {
            LockoutTimeRemaining = TimeSpan.FromMinutes(15); // 15 minute lockout
        }
    }

    public void ResetLoginAttempts()
    {
        LoginAttempts = 0;
        LockoutTimeRemaining = null;
    }

    public void SetUser(UserSession? user)
    {
        CurrentUser = user;
        if (user != null && user.IsActive)
        {
            ResetLoginAttempts();
            ClearErrors();
        }
    }

    public void Reset()
    {
        LoginRequest = new();
        IsLoading = false;
        ShowPassword = false;
        ClearErrors();
        CurrentUser = null;
    }

    #endregion

    #region Business Logic Methods

    public bool CanAttemptLogin()
    {
        // Business rules pentru când poate încerca login
        if (IsAccountLocked && LockoutTimeRemaining.HasValue && LockoutTimeRemaining.Value > TimeSpan.Zero)
        {
            return false;
        }

        if (IsAccountLocked && LockoutTimeRemaining.HasValue && LockoutTimeRemaining.Value <= TimeSpan.Zero)
        {
            // Reset lockout if time has expired
            ResetLoginAttempts();
        }

        return !IsLoading && IsFormValid && !IsAccountLocked;
    }

    public string GetLoginButtonText()
    {
        return IsLoading switch
        {
            true => "Se autentific?...",
            false when IsAccountLocked => "Cont blocat",
            _ => "Intr? în aplica?ie"
        };
    }

    public string? GetLockoutMessage()
    {
        if (!IsAccountLocked) return null;

        if (LockoutTimeRemaining.HasValue && LockoutTimeRemaining.Value > TimeSpan.Zero)
        {
            var minutes = Math.Ceiling(LockoutTimeRemaining.Value.TotalMinutes);
            return $"Cont temporar blocat. Încerca?i din nou în {minutes} minute.";
        }

        return "Cont blocat din cauza prea multor încerc?ri de autentificare.";
    }

    public List<DemoCredential> GetDemoCredentials() =>
    [
        new("admin", "admin123", "Administrator", "Acces complet la sistem", UserRole.Administrator),
        new("doctor1", "doctor123", "Dr. Maria Popescu", "Medic Cardiolog", UserRole.Doctor),
        new("asistent1", "asistent123", "Ana Ionescu", "Asistent Medical", UserRole.Nurse),
        new("receptioner1", "receptioner123", "Elena Vasile", "Recep?ioner", UserRole.Receptionist),
        new("manager1", "manager123", "Ion Marinescu", "Manager Medical", UserRole.Manager)
    ];

    #endregion
}

/// <summary>
/// Demo credential model cu enum în loc de magic strings
/// ORGANIZAT ÎN ACELA?I FOLDER CU LoginState
/// </summary>
public record DemoCredential(
    string Username, 
    string Password, 
    string DisplayName, 
    string Description,
    UserRole Role
)
{
    public string RoleIcon => Role switch
    {
        UserRole.Administrator => "fas fa-user-shield",
        UserRole.Doctor => "fas fa-user-md",
        UserRole.Nurse => "fas fa-user-nurse", 
        UserRole.Receptionist => "fas fa-user-tie",
        UserRole.Manager => "fas fa-user-cog",
        _ => "fas fa-user"
    };

    public string RoleColor => Role switch
    {
        UserRole.Administrator => "text-danger",
        UserRole.Doctor => "text-success",
        UserRole.Nurse => "text-info",
        UserRole.Receptionist => "text-warning",
        UserRole.Manager => "text-primary",
        _ => "text-secondary"
    };
    
    public string RoleDisplayName => ((Enum)Role).GetDisplayName();
}
