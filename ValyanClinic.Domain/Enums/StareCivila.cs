using System.ComponentModel.DataAnnotations;

namespace ValyanClinic.Domain.Enums;

/// <summary>
/// Enum pentru starea civila a personalului
/// Sincronizat cu valorile din baza de date Personal
/// </summary>
public enum StareCivila
{
    [Display(Name = "Necasatorit/a")]
    Necasatorit = 1,
    
    [Display(Name = "Casatorit/a")]
    Casatorit = 2,
    
    [Display(Name = "Divortat/a")]
    Divortat = 3,
    
    [Display(Name = "Vaduv/a")]
    Vaduv = 4,
    
    [Display(Name = "Uniune Consensuala")]
    UniuneConsensuala = 5
}
