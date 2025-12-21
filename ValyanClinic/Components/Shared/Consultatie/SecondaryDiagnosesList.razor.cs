using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using ValyanClinic.Application.Features.ICD10Management.DTOs;

namespace ValyanClinic.Components.Shared.Consultatie;

/// <summary>
/// Secondary Diagnoses List - Multiple diagnoses with ICD10 codes
/// </summary>
public partial class SecondaryDiagnosesList : ComponentBase
{
    [Inject] private ILogger<SecondaryDiagnosesList> Logger { get; set; } = default!;

    // ==================== PARAMETERS ====================
    
    [Parameter] public List<SecondaryDiagnosis> SecondaryDiagnoses { get; set; } = new();
    
    [Parameter] public EventCallback<List<SecondaryDiagnosis>> SecondaryDiagnosesChanged { get; set; }
    
    /// <summary>User ID for favorites access</summary>
    [Parameter] public Guid? CurrentUserId { get; set; }
    
    /// <summary>Show validation errors</summary>
    [Parameter] public bool ShowValidation { get; set; }
    
    /// <summary>Event when any change occurs</summary>
    [Parameter] public EventCallback OnChanged { get; set; }

    // ==================== COMPUTED ====================
    
    private bool IsMaxReached => SecondaryDiagnoses.Count >= 10;

    // ==================== HANDLERS ====================
    
    private async Task AddSecondaryDiagnosis()
    {
        if (IsMaxReached)
        {
            Logger.LogWarning("[SecondaryDiagnoses] Cannot add - max limit reached");
            return;
        }

        var newDiagnosis = new SecondaryDiagnosis
        {
            Id = Guid.NewGuid()
        };

        SecondaryDiagnoses.Add(newDiagnosis);
        
        Logger.LogInformation("[SecondaryDiagnoses] Added diagnosis #{Count}", SecondaryDiagnoses.Count);
        
        await SecondaryDiagnosesChanged.InvokeAsync(SecondaryDiagnoses);
        await NotifyChange();
    }

    private async Task RemoveSecondaryDiagnosis(int index)
    {
        if (index < 0 || index >= SecondaryDiagnoses.Count)
        {
            Logger.LogWarning("[SecondaryDiagnoses] Invalid index: {Index}", index);
            return;
        }

        var diagnosis = SecondaryDiagnoses[index];
        SecondaryDiagnoses.RemoveAt(index);
        
        Logger.LogInformation("[SecondaryDiagnoses] Removed diagnosis #{Index}", index + 1);
        
        await SecondaryDiagnosesChanged.InvokeAsync(SecondaryDiagnoses);
        await NotifyChange();
    }

    private async Task AddICD10Code(SecondaryDiagnosis diagnosis, ICD10SearchResultDto code)
    {
        if (diagnosis.ICD10Codes.Count >= 10)
        {
            Logger.LogWarning("[SecondaryDiagnoses] Cannot add code - max 10 codes per diagnosis");
            return;
        }

        // Check if code already exists
        if (diagnosis.ICD10Codes.Any(c => c.Code == code.Code))
        {
            Logger.LogWarning("[SecondaryDiagnoses] Code {Code} already exists", code.Code);
            return;
        }

        diagnosis.ICD10Codes.Add(code);
        
        Logger.LogInformation("[SecondaryDiagnoses] Added code {Code} to diagnosis {DiagnosisId}", 
            code.Code, diagnosis.Id);
        
        await SecondaryDiagnosesChanged.InvokeAsync(SecondaryDiagnoses);
        await NotifyChange();
    }

    private async Task RemoveICD10Code(SecondaryDiagnosis diagnosis, ICD10SearchResultDto code)
    {
        diagnosis.ICD10Codes.Remove(code);
        
        Logger.LogInformation("[SecondaryDiagnoses] Removed code {Code} from diagnosis {DiagnosisId}", 
            code.Code, diagnosis.Id);
        
        await SecondaryDiagnosesChanged.InvokeAsync(SecondaryDiagnoses);
        await NotifyChange();
    }

    /// <summary>Handle textarea input without causing re-render loops</summary>
    private void HandleDetailsInput(SecondaryDiagnosis diagnosis, ChangeEventArgs e)
    {
        diagnosis.AdditionalDetails = e.Value?.ToString();
        // Note: NOT calling NotifyChange() here to avoid re-render loops during typing
        // The list is updated by reference, so changes are automatically reflected
    }

    private async Task NotifyChange()
    {
        await OnChanged.InvokeAsync();
    }

    // ==================== VALIDATION ====================
    
    private bool ValidateDiagnosis(SecondaryDiagnosis diagnosis)
    {
        // Description is required
        return !string.IsNullOrWhiteSpace(diagnosis.Description);
    }
}

/// <summary>
/// Model for secondary diagnosis entry
/// </summary>
public class SecondaryDiagnosis
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Description { get; set; } = string.Empty;
    public List<ICD10SearchResultDto> ICD10Codes { get; set; } = new();
    public string? AdditionalDetails { get; set; }
}
