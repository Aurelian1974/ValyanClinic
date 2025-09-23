using System.ComponentModel.DataAnnotations;

namespace ValyanClinic.Domain.Models;

/// <summary>
/// Model pentru entitatea Judet din baza de date
/// </summary>
public class Judet
{
    public int IdJudet { get; set; }
    public Guid JudetGuid { get; set; }
    
    [Required]
    [StringLength(10)]
    public string CodJudet { get; set; } = string.Empty;
    
    [Required]
    [StringLength(100)]
    public string Nume { get; set; } = string.Empty;
    
    public int? Siruta { get; set; }
    
    [StringLength(10)]
    public string? CodAuto { get; set; }
    
    public int? Ordine { get; set; }
    
    // Navigation properties
    public virtual ICollection<Localitate> Localitati { get; set; } = new List<Localitate>();
}
