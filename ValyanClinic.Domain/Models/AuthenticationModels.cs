using System.ComponentModel.DataAnnotations;
using ValyanClinic.Domain.Enums;

namespace ValyanClinic.Domain.Models;

/// <summary>
/// Login request model cu valid?ri FluentValidation
/// ELIMINAT magic strings - folosim enums
/// </summary>
public class LoginRequest
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public bool RememberMe { get; set; } = false;
}

/// <summary>
/// Result Pattern pentru opera?ii Login
/// Înlocuie?te simple pass-through cu rich domain logic
/// </summary>
public class LoginResult
{
    public bool IsSuccess { get; private set; }
    public string? ErrorMessage { get; private set; }
    public string? SuccessMessage { get; private set; }
    public UserSession? UserSession { get; private set; }
    public List<string> Errors { get; private set; } = [];
    public LoginFailureReason? FailureReason { get; private set; }

    private LoginResult() { }

    public static LoginResult Success(UserSession userSession, string? message = null)
        => new() { IsSuccess = true, UserSession = userSession, SuccessMessage = message };

    public static LoginResult Failure(string errorMessage, LoginFailureReason reason = LoginFailureReason.InvalidCredentials)
        => new() { IsSuccess = false, ErrorMessage = errorMessage, FailureReason = reason };

    public static LoginResult Failure(List<string> errors, LoginFailureReason reason = LoginFailureReason.ValidationError)
        => new() { IsSuccess = false, Errors = errors, FailureReason = reason };

    public static LoginResult AccountLocked(string message, TimeSpan lockoutDuration)
        => new() { IsSuccess = false, ErrorMessage = message, FailureReason = LoginFailureReason.AccountLocked };
}

/// <summary>
/// Enumeration pentru tipurile de e?ec la login
/// </summary>
public enum LoginFailureReason
{
    [Display(Name = "Date invalide")]
    InvalidCredentials,
    
    [Display(Name = "Cont blocat")]
    AccountLocked,
    
    [Display(Name = "Cont inactiv")]
    AccountDisabled,
    
    [Display(Name = "Eroare validare")]
    ValidationError,
    
    [Display(Name = "Prea multe încerc?ri")]
    TooManyAttempts,
    
    [Display(Name = "Eroare sistem")]
    SystemError
}

/// <summary>
/// User session model îmbun?t??it cu Domain Logic
/// </summary>
public class UserSession
{
    public required string Username { get; init; }
    public required string FullName { get; init; }
    public required string Email { get; init; }
    public required UserRole Role { get; init; }
    public required string Department { get; init; }
    public DateTime LoginTime { get; init; } = DateTime.Now;
    public string? AvatarUrl { get; init; }
    public UserStatus Status { get; init; } = UserStatus.Active;

    // Domain Logic Methods - în loc de simple properties
    public string GetRoleDisplayName() => Role.GetDisplayName();
    public string GetStatusDisplayName() => Status.GetDisplayName();
    
    public bool IsAdmin => Role == UserRole.Administrator;
    public bool IsDoctor => Role == UserRole.Doctor;
    public bool IsActive => Status == UserStatus.Active;
    
    public string Initials => GetInitials(FullName);
    
    public bool HasPermission(string permission)
    {
        // Rich domain logic pentru permisiuni
        return Role switch
        {
            UserRole.Administrator => true,
            UserRole.Doctor => permission.StartsWith("MEDICAL_"),
            UserRole.Manager => permission.StartsWith("MANAGE_"),
            UserRole.Nurse => permission.StartsWith("PATIENT_"),
            UserRole.Receptionist => permission.StartsWith("RECEPTION_"),
            _ => false
        };
    }

    private static string GetInitials(string fullName)
    {
        if (string.IsNullOrEmpty(fullName)) return "UN";

        var parts = fullName.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length >= 2)
            return $"{parts[0][0]}{parts[^1][0]}".ToUpper();
        
        return parts.Length == 1 && parts[0].Length >= 2 
            ? parts[0][..2].ToUpper() 
            : fullName[..Math.Min(2, fullName.Length)].ToUpper();
    }
}

/// <summary>
/// Demo users pentru testing - cu enums în loc de magic strings
/// </summary>
public static class DemoUsers
{
    public static List<DemoUser> Users { get; } =
    [
        new()
        {
            Username = "admin",
            Password = "admin123",
            FullName = "Administrator Sistem",
            Email = "admin@valyanmed.ro",
            Role = UserRole.Administrator,
            Department = "Administrare",
            Status = UserStatus.Active
        },
        new()
        {
            Username = "doctor1",
            Password = "doctor123",
            FullName = "Dr. Maria Popescu",
            Email = "maria.popescu@valyanmed.ro",
            Role = UserRole.Doctor,
            Department = "Cardiologie",
            Status = UserStatus.Active
        },
        new()
        {
            Username = "asistent1", 
            Password = "asistent123",
            FullName = "Ana Ionescu",
            Email = "ana.ionescu@valyanmed.ro",
            Role = UserRole.Nurse,
            Department = "Cardiologie",
            Status = UserStatus.Active
        },
        new()
        {
            Username = "receptioner1",
            Password = "receptioner123", 
            FullName = "Elena Vasile",
            Email = "elena.vasile@valyanmed.ro",
            Role = UserRole.Receptionist,
            Department = "Recep?ie",
            Status = UserStatus.Active
        },
        new()
        {
            Username = "manager1",
            Password = "manager123",
            FullName = "Ion Marinescu",
            Email = "ion.marinescu@valyanmed.ro",
            Role = UserRole.Manager,
            Department = "Management",
            Status = UserStatus.Active
        }
    ];

    public static DemoUser? FindUser(string username, string password) =>
        Users.FirstOrDefault(u => 
            u.Username.Equals(username, StringComparison.OrdinalIgnoreCase) &&
            u.Password == password &&
            u.Status == UserStatus.Active);
}

public class DemoUser
{
    public required string Username { get; init; }
    public required string Password { get; init; }
    public required string FullName { get; init; }
    public required string Email { get; init; }
    public required UserRole Role { get; init; }
    public required string Department { get; init; }
    public UserStatus Status { get; init; } = UserStatus.Active;

    public UserSession ToUserSession() => new()
    {
        Username = Username,
        FullName = FullName,
        Email = Email,
        Role = Role,
        Department = Department,
        LoginTime = DateTime.Now,
        Status = Status
    };
}

/// <summary>
/// Extension methods pentru Enums
/// </summary>
public static class EnumExtensions
{
    public static string GetDisplayName(this Enum enumValue)
    {
        var displayAttribute = enumValue.GetType()
            .GetMember(enumValue.ToString())
            .FirstOrDefault()
            ?.GetCustomAttributes(typeof(DisplayAttribute), false)
            .FirstOrDefault() as DisplayAttribute;

        return displayAttribute?.Name ?? enumValue.ToString();
    }
}