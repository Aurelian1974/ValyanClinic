namespace ValyanClinic.Application.Features.ConsultatieManagement.DTOs;

/// <summary>
/// DTO pentru datele antecedentelor heredo-colaterale
/// Folosit în pagina de consultații pentru secțiunea de antecedente
/// </summary>
public class AntecedenteHeredoDto
{
    // === Antecedente Heredo-Colaterale ===
    
    /// <summary>
    /// Diabet zaharat în familie
    /// </summary>
    public bool DiabetZaharat { get; set; }

    /// <summary>
    /// Hipertensiune arterială în familie
    /// </summary>
    public bool HTA { get; set; }

    /// <summary>
    /// Boli cardiace în familie
    /// </summary>
    public bool BoliCardiace { get; set; }

    /// <summary>
    /// Cancer în familie
    /// </summary>
    public bool Cancer { get; set; }

    /// <summary>
    /// Boli neurologice în familie
    /// </summary>
    public bool BoliNeurologice { get; set; }

    /// <summary>
    /// Boli psihice în familie
    /// </summary>
    public bool BoliPsihice { get; set; }

    /// <summary>
    /// Observații suplimentare antecedente heredo
    /// </summary>
    public string Observatii { get; set; } = string.Empty;

    // === Antecedente Fiziologice ===

    /// <summary>
    /// Condiții la naștere
    /// </summary>
    public string ConditiiNastere { get; set; } = string.Empty;

    /// <summary>
    /// Alimentația în primul an de viață
    /// </summary>
    public string AlimentatiePrimulAn { get; set; } = string.Empty;

    /// <summary>
    /// Menarha (pentru femei)
    /// </summary>
    public string Menarha { get; set; } = string.Empty;

    // === Antecedente Patologice ===

    /// <summary>
    /// Internări anterioare
    /// </summary>
    public string InternariAnterioare { get; set; } = string.Empty;

    /// <summary>
    /// Alergii cunoscute
    /// </summary>
    public string AlergiiCunoscute { get; set; } = string.Empty;

    // === Condiții Socio-Economice ===

    /// <summary>
    /// Ocupația pacientului
    /// </summary>
    public string Ocupatie { get; set; } = string.Empty;

    /// <summary>
    /// Mediul de proveniență (urban/rural)
    /// </summary>
    public string Mediu { get; set; } = string.Empty;

    /// <summary>
    /// Condiții de locuit
    /// </summary>
    public string ConditiiLocuit { get; set; } = string.Empty;

    /// <summary>
    /// Status fumător
    /// </summary>
    public string Fumat { get; set; } = string.Empty;

    /// <summary>
    /// Consum alcool
    /// </summary>
    public string Alcool { get; set; } = string.Empty;

    /// <summary>
    /// Nivel activitate fizică
    /// </summary>
    public string ActivitateFizica { get; set; } = string.Empty;

    /// <summary>
    /// Verifică dacă există antecedente heredo-colaterale selectate
    /// </summary>
    public bool HasHeredoColaterale => DiabetZaharat || HTA || BoliCardiace || Cancer || BoliNeurologice || BoliPsihice;

    /// <summary>
    /// Returnează lista antecedentelor heredo-colaterale pozitive
    /// </summary>
    public IEnumerable<string> GetPositiveHeredoColaterale()
    {
        if (DiabetZaharat) yield return "Diabet Zaharat";
        if (HTA) yield return "HTA";
        if (BoliCardiace) yield return "Boli Cardiace";
        if (Cancer) yield return "Cancer";
        if (BoliNeurologice) yield return "Boli Neurologice";
        if (BoliPsihice) yield return "Boli Psihice";
    }
}
