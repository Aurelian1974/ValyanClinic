using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ValyanClinic.Domain.Entities;

/// <summary>
/// Entity pentru clasificarea ISCO-08 (International Standard Classification of Occupations)
/// Reprezentă structura ierarhică a ocupațiilor conform standardului internațional
/// </summary>
[Table("Ocupatii_ISCO08")]
public class OcupatieISCO
{
    /// <summary>
    /// Identificator unic al înregistrării (UNIQUEIDENTIFIER cu NEWSEQUENTIALID)
    /// </summary>
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Id { get; set; }

    /// <summary>
    /// Codul ISCO unic (1-4 cifre conform nivelului ierarhic)
    /// </summary>
    [Required]
    [StringLength(10)]
    [Column("Cod_ISCO")]
    public string CodISCO { get; set; } = string.Empty;

    /// <summary>
    /// Denumirea ocupației în limba română
    /// </summary>
    [Required]
    [StringLength(500)]
    [Column("Denumire_Ocupatie")]
    public string DenumireOcupatie { get; set; } = string.Empty;

    /// <summary>
    /// Denumirea ocupației în limba engleză
    /// </summary>
    [StringLength(500)]
    [Column("Denumire_Ocupatie_EN")]
    public string? DenumireOcupatieEN { get; set; }

    /// <summary>
    /// Nivelul în ierarhia ISCO: 1=Grupa Major, 2=Subgrupa, 3=Grupa Minor, 4=Ocupatie
    /// </summary>
    [Required]
    [Range(1, 4)]
    [Column("Nivel_Ierarhic")]
    public byte NivelIerarhic { get; set; }

    /// <summary>
    /// Referința la codul ISCO părinte în ierarhie
    /// </summary>
    [StringLength(10)]
    [Column("Cod_Parinte")]
    public string? CodParinte { get; set; }

    /// <summary>
    /// Codul grupei majore (1 cifră)
    /// </summary>
    [StringLength(10)]
    [Column("Grupa_Majora")]
    public string? GrupaMajora { get; set; }

    /// <summary>
    /// Denumirea grupei majore
    /// </summary>
    [StringLength(300)]
    [Column("Grupa_Majora_Denumire")]
    public string? GrupaMajoraDenumire { get; set; }

    /// <summary>
    /// Codul subgrupei (2 cifre)
    /// </summary>
    [StringLength(10)]
    [Column("Subgrupa")]
    public string? Subgrupa { get; set; }

    /// <summary>
    /// Denumirea subgrupei
    /// </summary>
    [StringLength(300)]
    [Column("Subgrupa_Denumire")]
    public string? SubgrupaDenumire { get; set; }

    /// <summary>
    /// Codul grupei minore (3 cifre)
    /// </summary>
    [StringLength(10)]
    [Column("Grupa_Minora")]
    public string? GrupaMinora { get; set; }

    /// <summary>
    /// Denumirea grupei minore
    /// </summary>
    [StringLength(300)]
    [Column("Grupa_Minora_Denumire")]
    public string? GrupaMinoraDenumire { get; set; }

    /// <summary>
    /// Descrierea detaliată a ocupației
    /// </summary>
    [Column("Descriere")]
    public string? Descriere { get; set; }

    /// <summary>
    /// Observații suplimentare
    /// </summary>
    [StringLength(1000)]
    [Column("Observatii")]
    public string? Observatii { get; set; }

    /// <summary>
    /// Indică dacă ocupația este activă în sistem
    /// </summary>
    [Required]
    [Column("Este_Activ")]
    public bool EsteActiv { get; set; } = true;

    /// <summary>
    /// Data și ora creării înregistrării
    /// </summary>
    [Required]
    [Column("Data_Crearii")]
    public DateTime DataCrearii { get; set; } = DateTime.Now;

    /// <summary>
    /// Data și ora ultimei modificări
    /// </summary>
    [Required]
    [Column("Data_Ultimei_Modificari")]
    public DateTime DataUltimeiModificari { get; set; } = DateTime.Now;

    /// <summary>
    /// Utilizatorul care a creat înregistrarea
    /// </summary>
    [StringLength(100)]
    [Column("Creat_De")]
    public string? CreatDe { get; set; }

    /// <summary>
    /// Utilizatorul care a modificat ultima dată înregistrarea
    /// </summary>
    [StringLength(100)]
    [Column("Modificat_De")]
    public string? ModificatDe { get; set; }

    // Navigation properties pentru relația ierarhică
    
    /// <summary>
    /// Referința către ocupația părinte în ierarhie
    /// </summary>
    [ForeignKey(nameof(CodParinte))]
    public virtual OcupatieISCO? Parinte { get; set; }

    /// <summary>
    /// Lista ocupațiilor copil în ierarhie
    /// </summary>
    [InverseProperty(nameof(Parinte))]
    public virtual ICollection<OcupatieISCO> Copii { get; set; } = new List<OcupatieISCO>();

    // Computed properties pentru utilizare în UI

    /// <summary>
    /// Codul și denumirea concatenate pentru afișare
    /// </summary>
    [NotMapped]
    public string CodSiDenumire => $"{CodISCO} - {DenumireOcupatie}";

    /// <summary>
    /// Numele nivelului ierarhic pentru afișare
    /// </summary>
    [NotMapped]
    public string NumeNivelIerarhic => NivelIerarhic switch
    {
        1 => "Grupa Majoră",
        2 => "Subgrupa", 
        3 => "Grupa Minoră",
        4 => "Ocupație",
        _ => "Necunoscut"
    };

    /// <summary>
    /// Indentarea pentru afișarea ierarhică în UI
    /// </summary>
    [NotMapped]
    public string IndentareIerarhica => new string(' ', (NivelIerarhic - 1) * 4);

    /// <summary>
    /// Verifică dacă ocupația este o grupă (nu o ocupație finală)
    /// </summary>
    [NotMapped]
    public bool EsteGrupa => NivelIerarhic < 4;

    /// <summary>
    /// Verifică dacă ocupația este la nivelul cel mai detaliat
    /// </summary>
    [NotMapped]
    public bool EsteOcupatieFinal => NivelIerarhic == 4;

    /// <summary>
    /// String format scurt pentru ID-ul GUID (primele 8 caractere)
    /// </summary>
    [NotMapped]
    public string IdScurt => Id.ToString("N")[..8].ToUpper();

    /// <summary>
    /// Override pentru afișarea string-ului reprezentativ
    /// </summary>
    public override string ToString()
    {
        return CodSiDenumire;
    }

    /// <summary>
    /// Verifică dacă două ocupații sunt echivalente
    /// </summary>
    public override bool Equals(object? obj)
    {
        return obj is OcupatieISCO other && (Id == other.Id || CodISCO == other.CodISCO);
    }

    /// <summary>
    /// Calculează hash code-ul bazat pe ID-ul GUID
    /// </summary>
    public override int GetHashCode()
    {
        return Id != Guid.Empty ? Id.GetHashCode() : CodISCO.GetHashCode();
    }
}
