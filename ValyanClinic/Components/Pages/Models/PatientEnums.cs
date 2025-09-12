using System.ComponentModel.DataAnnotations;

namespace ValyanClinic.Components.Pages.Models;

public enum PatientStatus
{
    [Display(Name = "Activ")] Active = 1,
    [Display(Name = "Inactiv")] Inactive = 2,
    [Display(Name = "Suspendat")] Suspended = 3
}

public enum Gender
{
    [Display(Name = "Masculin")] Male = 1,
    [Display(Name = "Feminin")] Female = 2,
    [Display(Name = "Altul")] Other = 3
}

public enum FilterOptions
{
    All,
    Active,
    Inactive
}