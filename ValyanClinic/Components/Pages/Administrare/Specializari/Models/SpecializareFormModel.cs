using System.ComponentModel.DataAnnotations;

namespace ValyanClinic.Components.Pages.Administrare.Specializari.Models;

public class SpecializareFormModel
{
    public Guid? Id { get; set; }

    [Required(ErrorMessage = "Denumirea este obligatorie")]
    [StringLength(200, ErrorMessage = "Denumirea nu poate depasi 200 de caractere")]
    public string Denumire { get; set; } = string.Empty;

    [StringLength(100, ErrorMessage = "Categoria nu poate depasi 100 de caractere")]
    public string? Categorie { get; set; }

    [StringLength(1000, ErrorMessage = "Descrierea nu poate depasi 1000 de caractere")]
    public string? Descriere { get; set; }

    public bool EsteActiv { get; set; } = true;

    public bool IsEditMode => Id.HasValue && Id.Value != Guid.Empty;
}
