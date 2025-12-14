namespace ValyanClinic.Application.Services.Consultatii;

/// <summary>
/// Serviciu pentru calculul progresului unui formular multi-step.
/// Poate fi reutilizat în orice formular cu multiple tabs/step-uri.
/// </summary>
public interface IFormProgressService
{
    /// <summary>
    /// Calculează procentul de completare bazat pe un set de reguli
    /// </summary>
    /// <param name="fieldValues">Dicționar cu numele câmpului și valoarea sa</param>
    /// <param name="requiredFields">Lista câmpurilor obligatorii</param>
    /// <returns>Procentul de completare (0-100)</returns>
    int CalculateProgress(Dictionary<string, object?> fieldValues, IEnumerable<string>? requiredFields = null);

    /// <summary>
    /// Verifică dacă un tab/step este complet
    /// </summary>
    /// <param name="tabFields">Câmpurile din tab</param>
    /// <returns>True dacă toate câmpurile obligatorii sunt completate</returns>
    bool IsTabComplete(Dictionary<string, object?> tabFields);

    /// <summary>
    /// Calculează progresul pentru consultație specific
    /// </summary>
    ConsultationProgressResult CalculateConsultationProgress(ConsultationProgressInput input);
}

/// <summary>
/// Input pentru calculul progresului unei consultații
/// </summary>
public record ConsultationProgressInput
{
    // Tab 1: Motiv & Antecedente
    public string? MotivPrezentare { get; init; }
    public string? AntecedentePatologice { get; init; }
    public string? TratamenteActuale { get; init; }

    // Tab 2: Examen Clinic
    public int? TensiuneSistolica { get; init; }
    public int? TensiuneDiastolica { get; init; }
    public int? Puls { get; init; }
    public decimal? Temperatura { get; init; }
    public int? FreqventaRespiratorie { get; init; }
    public decimal? Greutate { get; init; }
    public decimal? Inaltime { get; init; }
    public string? ExamenObiectiv { get; init; }

    // Tab 3: Diagnostic & Tratament
    public string? DiagnosticPrincipal { get; init; }
    public string? PlanTerapeutic { get; init; }

    // Tab 4: Concluzii
    public string? Concluzii { get; init; }
    public DateTime? DataUrmatoareiVizite { get; init; }
}

/// <summary>
/// Rezultatul calculului de progres pentru consultație
/// </summary>
public record ConsultationProgressResult
{
    public int ProgressPercentage { get; init; }
    public bool IsTab1Complete { get; init; }
    public bool IsTab2Complete { get; init; }
    public bool IsTab3Complete { get; init; }
    public bool IsTab4Complete { get; init; }
    public int CompletedFieldsCount { get; init; }
    public int TotalFieldsCount { get; init; }
    public List<string> MissingRequiredFields { get; init; } = new();
}
