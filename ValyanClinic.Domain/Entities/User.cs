using ValyanClinic.Domain.Common;
using ValyanClinic.Domain.Enums;

namespace ValyanClinic.Domain.Entities;

public class User : BaseEntity
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public UserRole Role { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime? LastLoginAt { get; set; }
    public int? DoctorId { get; set; } // Link to Doctor entity if user is a doctor

    // Navigation properties
    public virtual Doctor? Doctor { get; set; }

    public string FullName => $"{FirstName} {LastName}";
}