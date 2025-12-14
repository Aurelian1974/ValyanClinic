namespace ValyanClinic.Application.Features.ConsultatieManagement.DTOs;

/// <summary>
/// DTO pentru rândul de medicație în tabelul de tratament
/// Folosit pentru afișarea și editarea medicamentelor prescrise
/// </summary>
public class MedicationRowDto
{
    /// <summary>
    /// ID unic pentru tracking în UI (nu persistat)
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Numele medicamentului
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Doza medicamentului (ex: 500mg, 10ml)
    /// </summary>
    public string Dose { get; set; } = string.Empty;

    /// <summary>
    /// Frecvența administrării (ex: "1x3/zi", "la 8 ore")
    /// </summary>
    public string Frequency { get; set; } = string.Empty;

    /// <summary>
    /// Durata tratamentului (ex: "7 zile", "2 săptămâni")
    /// </summary>
    public string Duration { get; set; } = string.Empty;

    /// <summary>
    /// Cantitatea prescrisă (ex: "1 cutie", "30 comprimate")
    /// </summary>
    public string Quantity { get; set; } = string.Empty;

    /// <summary>
    /// Observații suplimentare pentru medicament
    /// </summary>
    public string? Notes { get; set; }

    /// <summary>
    /// Verifică dacă rândul are date valide
    /// </summary>
    public bool IsValid => !string.IsNullOrWhiteSpace(Name);

    /// <summary>
    /// Verifică dacă medicamentul conține un termen specific (pentru detectare alergii)
    /// </summary>
    public bool ContainsTerm(string term)
    {
        if (string.IsNullOrWhiteSpace(term)) return false;
        return Name.Contains(term, StringComparison.OrdinalIgnoreCase);
    }
}
