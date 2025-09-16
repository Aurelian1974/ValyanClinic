using ValyanClinic.Domain.Enums;

namespace ValyanClinic.Domain.Models;

// Domain Models - reprezinta concepte de business
public class User
{
    public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public UserRole Role { get; set; }
    public UserStatus Status { get; set; }
    public string? Department { get; set; }
    public string? JobTitle { get; set; }
    public DateTime CreatedDate { get; set; } = DateTime.Now; // CONSISTENT: folosește ora locală
    public DateTime? LastLoginDate { get; set; }
    
    // Domain Properties - business logic in model
    public string FullName => $"{FirstName} {LastName}";
    public bool IsActive => Status == UserStatus.Active;
    public bool IsDoctor => Role == UserRole.Doctor;
    public bool IsAdmin => Role == UserRole.Administrator;
    public int DaysSinceCreated => (DateTime.Now - CreatedDate).Days;
    public int? DaysSinceLastLogin => LastLoginDate?.Subtract(DateTime.Now).Days * -1;
    
    // Business Methods
    public bool CanAccessModule(string module)
    {
        if (!IsActive) return false;
        
        return Role switch
        {
            UserRole.Administrator => true,
            UserRole.Manager => module != "system-admin",
            UserRole.Doctor => module is "patients" or "consultations" or "prescriptions",
            UserRole.Nurse => module is "patients" or "consultations",
            UserRole.Receptionist => module is "appointments" or "patients",
            UserRole.Operator => module is "basic-operations",
            _ => false
        };
    }
    
    public void UpdateLastLogin()
    {
        LastLoginDate = DateTime.Now;
    }
    
    public bool HasRecentActivity(int days = 7)
    {
        return LastLoginDate.HasValue && 
               (DateTime.Now - LastLoginDate.Value).Days <= days;
    }
}
