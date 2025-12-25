using System.ComponentModel.DataAnnotations;

namespace ValyanClinic.Components.Pages.Administrare.Roluri.Models;

/// <summary>
/// Model pentru formularul de creare/editare rol.
/// </summary>
public class RolFormModel
{
    public Guid? Id { get; set; }
    
    [Required(ErrorMessage = "Denumirea este obligatorie.")]
    [StringLength(100, ErrorMessage = "Denumirea nu poate depăși 100 caractere.")]
    public string Denumire { get; set; } = string.Empty;
    
    [StringLength(500, ErrorMessage = "Descrierea nu poate depăși 500 caractere.")]
    public string? Descriere { get; set; }
    
    public bool EsteActiv { get; set; } = true;
    
    [Range(0, 999, ErrorMessage = "Ordinea de afișare trebuie să fie între 0 și 999.")]
    public int OrdineAfisare { get; set; }
    
    /// <summary>
    /// Lista de coduri permisiuni selectate pentru acest rol.
    /// </summary>
    public List<string> Permisiuni { get; set; } = new();
    
    public bool IsEditMode => Id.HasValue;
}
