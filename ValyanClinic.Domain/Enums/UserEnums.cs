using System.ComponentModel.DataAnnotations;

namespace ValyanClinic.Domain.Enums;

/// <summary>
/// Roluri utilizator in sistemul ValyanMed
/// FR? magic strings - folosim enum cu Display attributes
/// </summary>
public enum UserRole
{
    [Display(Name = "Administrator")]
    Administrator = 1,
    
    [Display(Name = "Doctor")]
    Doctor = 2,
    
    [Display(Name = "Asistent Medical")]
    Nurse = 3,
    
    [Display(Name = "Receptioner")]
    Receptionist = 4,
    
    [Display(Name = "Operator")]
    Operator = 5,
    
    [Display(Name = "Manager")]
    Manager = 6
}

/// <summary>
/// Status utilizator in sistem
/// </summary>
public enum UserStatus
{
    [Display(Name = "Activ")]
    Active = 1,
    
    [Display(Name = "Inactiv")]
    Inactive = 2,
    
    [Display(Name = "Suspendat")]
    Suspended = 3,
    
    [Display(Name = "Blocat")]
    Locked = 4
}

/// <summary>
/// Tipuri de autentificare
/// </summary>
public enum AuthenticationType
{
    [Display(Name = "Standard")]
    Standard = 1,
    
    [Display(Name = "Active Directory")]
    ActiveDirectory = 2,
    
    [Display(Name = "Single Sign-On")]
    SingleSignOn = 3
}