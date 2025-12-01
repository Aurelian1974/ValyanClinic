using System.ComponentModel.DataAnnotations;

namespace ValyanClinic.Components.Pages.Administrare.Departamente.Models;

public class DepartamentFormModel
{
    public Guid? IdDepartament { get; set; }

    public Guid? IdTipDepartament { get; set; }

    [Required(ErrorMessage = "Denumirea departamentului este obligatorie")]
    [StringLength(200, ErrorMessage = "Denumirea nu poate depasi 200 de caractere")]
    public string DenumireDepartament { get; set; } = string.Empty;

    [StringLength(500, ErrorMessage = "Descrierea nu poate depasi 500 de caractere")]
    public string? DescriereDepartament { get; set; }

    public string? DenumireTipDepartament { get; set; }

    public bool IsEditMode => IdDepartament.HasValue && IdDepartament.Value != Guid.Empty;

    public string ModalTitle => IsEditMode ? "Editare Departament" : "Adaugare Departament Nou";
}
