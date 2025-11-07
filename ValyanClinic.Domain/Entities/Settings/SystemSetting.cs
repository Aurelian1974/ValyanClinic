namespace ValyanClinic.Domain.Entities.Settings;

/// <summary>
/// Entitate pentru setari sistem (Key-Value Pattern)
/// Tabela: Setari_Sistem
/// </summary>
public class SystemSetting
{
    public Guid SetareID { get; set; }
    public string Categorie { get; set; } = string.Empty;
    public string Cheie { get; set; } = string.Empty;
    public string Valoare { get; set; } = string.Empty;
    public string TipDate { get; set; } = "String";
    public string? Descriere { get; set; }
    public string? ValoareDefault { get; set; }
    public bool EsteEditabil { get; set; } = true;
    public DateTime DataCrearii { get; set; }
    public DateTime? DataModificarii { get; set; }
    public string? ModificatDe { get; set; }
}
