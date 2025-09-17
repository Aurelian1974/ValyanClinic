using System.ComponentModel.DataAnnotations;

namespace ValyanClinic.Domain.Models;

/// <summary>
/// Model pentru entitatea Localitate din baza de date
/// </summary>
public class Localitate
{
    public int IdOras { get; set; }
    public Guid LocalitateGuid { get; set; }
    
    [Required]
    public int IdJudet { get; set; }
    
    [Required]
    [StringLength(100)]
    public string Nume { get; set; } = string.Empty;
    
    [Required]
    public int Siruta { get; set; }
    
    public int? IdTipLocalitate { get; set; }
    
    [Required]
    [StringLength(20)]
    public string CodLocalitate { get; set; } = string.Empty;
    
    // Navigation properties
    public virtual Judet? Judet { get; set; }
}
