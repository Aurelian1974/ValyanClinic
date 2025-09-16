using System.ComponentModel.DataAnnotations;

namespace ValyanClinic.Domain.Enums;

/// <summary>
/// Enum pentru statusul angajatului
/// Sincronizat cu valorile din baza de date Personal
/// </summary>
public enum StatusAngajat
{
    [Display(Name = "Activ")]
    Activ = 1,
    
    [Display(Name = "Inactiv")]
    Inactiv = 2
}
