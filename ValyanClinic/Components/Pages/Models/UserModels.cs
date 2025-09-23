using System.ComponentModel.DataAnnotations;

namespace ValyanClinic.Components.Pages.Models;

public class User
{
    public int Id { get; set; }
    
    [Required(ErrorMessage = "Numele este obligatoriu")]
    [StringLength(50, ErrorMessage = "Numele nu poate dep??i 50 de caractere")]
    public string FirstName { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Prenumele este obligatoriu")]
    [StringLength(50, ErrorMessage = "Prenumele nu poate dep??i 50 de caractere")]
    public string LastName { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Email-ul este obligatoriu")]
    [EmailAddress(ErrorMessage = "Format email invalid")]
    public string Email { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Numele de utilizator este obligatoriu")]
    [StringLength(30, MinimumLength = 3, ErrorMessage = "Numele de utilizator trebuie s? aib? intre 3 ?i 30 de caractere")]
    public string Username { get; set; } = string.Empty;
    
    [Phone(ErrorMessage = "Format telefon invalid")]
    public string? Phone { get; set; }
    
    public UserRole Role { get; set; } = UserRole.Operator;
    public UserStatus Status { get; set; } = UserStatus.Active;
    public DateTime CreatedDate { get; set; } = DateTime.Now;
    public DateTime? LastLoginDate { get; set; }
    public string? Department { get; set; }
    public string? JobTitle { get; set; }
    
    // Computed properties
    public string FullName => $"{FirstName} {LastName}";
    public string StatusDisplay => Status switch
    {
        UserStatus.Active => "Activ",
        UserStatus.Inactive => "Inactiv",
        UserStatus.Suspended => "Suspendat",
        UserStatus.Pending => "in ateptare",
        _ => "Necunoscut"
    };
    
    public string RoleDisplay => Role switch
    {
        UserRole.Administrator => "Administrator",
        UserRole.Doctor => "Doctor",
        UserRole.Nurse => "Asistent medical",
        UserRole.Receptionist => "Receptioner",
        UserRole.Operator => "Operator",
        UserRole.Manager => "Manager",
        _ => "Necunoscut"
    };
    
    public int DaysSinceCreated => (DateTime.Now - CreatedDate).Days;
    public int? DaysSinceLastLogin => LastLoginDate?.Subtract(DateTime.Now).Days;
}

public enum UserRole
{
    Administrator = 1,
    Doctor = 2,
    Nurse = 3,
    Receptionist = 4,
    Operator = 5,
    Manager = 6
}

public enum UserStatus
{
    Active = 1,
    Inactive = 2,
    Suspended = 3,
    Pending = 4
}

public class UserGroup
{
    public string Key { get; set; } = string.Empty;
    public List<User> Items { get; set; } = new();
    public int Count => Items.Count;
}