namespace ValyanClinic.Domain.Entities;

/// <summary>
/// Entitate pentru Antecedentele Medicale ale unei Consultații
/// Relație 1:1 cu Consultatie
/// </summary>
public class ConsultatieAntecedente
{
    // ==================== PRIMARY KEY ====================
    public Guid Id { get; set; }

    // ==================== FOREIGN KEY ====================
    public Guid ConsultatieID { get; set; }

    // ==================== ANTECEDENTE HEREDO-COLATERALE (AHC) ====================
    public string? AHC_Mama { get; set; }
    public string? AHC_Tata { get; set; }
    public string? AHC_Frati { get; set; }
    public string? AHC_Bunici { get; set; }
    public string? AHC_Altele { get; set; }

    // ==================== ANTECEDENTE FIZIOLOGICE (AF) ====================
    public string? AF_Nastere { get; set; }
    public string? AF_Dezvoltare { get; set; }
    public string? AF_Menstruatie { get; set; }
    public string? AF_Sarcini { get; set; }
    public string? AF_Alaptare { get; set; }

    // ==================== ANTECEDENTE PERSONALE PATOLOGICE (APP) ====================
    public string? APP_BoliCopilarieAdolescenta { get; set; }
    public string? APP_BoliAdult { get; set; }
    public string? APP_Interventii { get; set; }
    public string? APP_Traumatisme { get; set; }
    public string? APP_Transfuzii { get; set; }
    public string? APP_Alergii { get; set; }
    public string? APP_Medicatie { get; set; }

    // ==================== CONDITII SOCIO-ECONOMICE ====================
    public string? Profesie { get; set; }
    public string? ConditiiLocuinta { get; set; }
    public string? ConditiiMunca { get; set; }
    public string? ObiceiuriAlimentare { get; set; }
    public string? Toxice { get; set; }

    // ==================== AUDIT ====================
    public DateTime DataCreare { get; set; }
    public Guid CreatDe { get; set; }
    public DateTime? DataUltimeiModificari { get; set; }
    public Guid? ModificatDe { get; set; }

    // ==================== NAVIGATION PROPERTIES ====================
    public virtual Consultatie? Consultatie { get; set; }
}
