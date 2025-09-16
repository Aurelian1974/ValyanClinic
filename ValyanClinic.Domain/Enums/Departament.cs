using System.ComponentModel.DataAnnotations;

namespace ValyanClinic.Domain.Enums;

/// <summary>
/// Enum pentru departamentele din clinica ValyanMed
/// Sincronizat cu valorile din baza de date Personal
/// </summary>
public enum Departament
{
    [Display(Name = "Administratie")]
    Administratie = 1,
    
    [Display(Name = "Financiar")]
    Financiar = 2,
    
    [Display(Name = "IT")]
    IT = 3,
    
    [Display(Name = "Intretinere")]
    Intretinere = 4,
    
    [Display(Name = "Logistica")]
    Logistica = 5,
    
    [Display(Name = "Marketing")]
    Marketing = 6,
    
    [Display(Name = "Receptie")]
    Receptie = 7,
    
    [Display(Name = "Resurse Umane")]
    ResurseUmane = 8,
    
    [Display(Name = "Securitate")]
    Securitate = 9,
    
    [Display(Name = "Transport")]
    Transport = 10,
    
    [Display(Name = "Juridic")]
    Juridic = 11,
    
    [Display(Name = "Relatii Clienti")]  // Corectat: spațiu în loc de underscore
    RelatiiClienti = 12,
    
    [Display(Name = "Calitate")]
    Calitate = 13,
    
    [Display(Name = "Call Center")]  // Corectat: spațiu în loc de CamelCase
    CallCenter = 14
}
