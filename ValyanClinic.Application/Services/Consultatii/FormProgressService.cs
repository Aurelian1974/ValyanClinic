using Microsoft.Extensions.Logging;

namespace ValyanClinic.Application.Services.Consultatii;

/// <summary>
/// Implementare serviciu pentru calculul progresului formularelor.
/// Centralizează logica de validare completare câmpuri.
/// </summary>
public class FormProgressService : IFormProgressService
{
    private readonly ILogger<FormProgressService> _logger;

    // Câmpuri obligatorii pentru consultație
    private static readonly HashSet<string> RequiredConsultationFields = new()
    {
        "MotivPrezentare",
        "DiagnosticPrincipal"
    };

    public FormProgressService(ILogger<FormProgressService> logger)
    {
        _logger = logger;
    }

    public int CalculateProgress(Dictionary<string, object?> fieldValues, IEnumerable<string>? requiredFields = null)
    {
        if (fieldValues.Count == 0) return 0;

        var completedCount = fieldValues.Count(kvp => IsFieldComplete(kvp.Value));
        var percentage = (int)((completedCount / (double)fieldValues.Count) * 100);

        _logger.LogDebug(
            "Progress calculated: {Completed}/{Total} = {Percentage}%",
            completedCount,
            fieldValues.Count,
            percentage
        );

        return percentage;
    }

    public bool IsTabComplete(Dictionary<string, object?> tabFields)
    {
        return tabFields.All(kvp => IsFieldComplete(kvp.Value));
    }

    public ConsultationProgressResult CalculateConsultationProgress(ConsultationProgressInput input)
    {
        var completedFields = 0;
        const int totalFields = 14;
        var missingFields = new List<string>();

        // Tab 1: Anamneză (4 câmpuri)
        if (!string.IsNullOrWhiteSpace(input.MotivPrezentare)) completedFields++;
        else missingFields.Add("Motiv Prezentare");

        if (!string.IsNullOrWhiteSpace(input.IstoricBoalaActuala)) completedFields++;
        if (!string.IsNullOrWhiteSpace(input.IstoricMedicalPersonal)) completedFields++;
        if (!string.IsNullOrWhiteSpace(input.IstoricFamilial)) completedFields++;

        // Tab 2: Examen Clinic (6 câmpuri)
        if (input.TensiuneSistolica.HasValue || input.TensiuneDiastolica.HasValue) completedFields++;
        if (input.Puls.HasValue) completedFields++;
        if (input.Temperatura.HasValue) completedFields++;
        if (input.FreqventaRespiratorie.HasValue) completedFields++;
        if (input.Greutate.HasValue || input.Inaltime.HasValue) completedFields++;
        if (!string.IsNullOrWhiteSpace(input.ExamenObiectiv)) completedFields++;

        // Tab 3: Diagnostic & Tratament (2 câmpuri)
        if (!string.IsNullOrWhiteSpace(input.DiagnosticPrincipal)) completedFields++;
        else missingFields.Add("Diagnostic Principal");

        if (!string.IsNullOrWhiteSpace(input.PlanTerapeutic)) completedFields++;

        // Tab 4: Concluzii (2 câmpuri)
        if (!string.IsNullOrWhiteSpace(input.Concluzii)) completedFields++;
        if (input.DataUrmatoareiVizite.HasValue) completedFields++;

        // Calcul tab-uri complete
        var isTab1Complete = !string.IsNullOrWhiteSpace(input.MotivPrezentare) &&
                             !string.IsNullOrWhiteSpace(input.IstoricBoalaActuala) &&
                             !string.IsNullOrWhiteSpace(input.IstoricMedicalPersonal) &&
                             !string.IsNullOrWhiteSpace(input.IstoricFamilial);

        var isTab2Complete = (input.TensiuneSistolica.HasValue || input.TensiuneDiastolica.HasValue) &&
                             input.Puls.HasValue &&
                             input.Temperatura.HasValue &&
                             input.FreqventaRespiratorie.HasValue &&
                             (input.Greutate.HasValue || input.Inaltime.HasValue) &&
                             !string.IsNullOrWhiteSpace(input.ExamenObiectiv);

        // Tab Diagnostic & Tratament: doar diagnosticul principal e obligatoriu
        var isTab3Complete = !string.IsNullOrWhiteSpace(input.DiagnosticPrincipal);

        var isTab4Complete = !string.IsNullOrWhiteSpace(input.Concluzii);

        var percentage = (int)((completedFields / (double)totalFields) * 100);

        _logger.LogDebug(
            "Consultation progress: {Completed}/{Total} = {Percentage}%, Tabs: [{T1},{T2},{T3},{T4}]",
            completedFields,
            totalFields,
            percentage,
            isTab1Complete,
            isTab2Complete,
            isTab3Complete,
            isTab4Complete
        );

        return new ConsultationProgressResult
        {
            ProgressPercentage = percentage,
            IsTab1Complete = isTab1Complete,
            IsTab2Complete = isTab2Complete,
            IsTab3Complete = isTab3Complete,
            IsTab4Complete = isTab4Complete,
            CompletedFieldsCount = completedFields,
            TotalFieldsCount = totalFields,
            MissingRequiredFields = missingFields
        };
    }

    private static bool IsFieldComplete(object? value)
    {
        return value switch
        {
            null => false,
            string s => !string.IsNullOrWhiteSpace(s),
            int i => i != 0,
            decimal d => d != 0,
            DateTime dt => dt != DateTime.MinValue,
            Guid g => g != Guid.Empty,
            bool => true, // booleans are always "set"
            _ => true
        };
    }
}
