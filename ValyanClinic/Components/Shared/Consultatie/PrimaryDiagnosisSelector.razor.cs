using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using ValyanClinic.Application.Features.ICD10Management.DTOs;

namespace ValyanClinic.Components.Shared.Consultatie;

/// <summary>
/// Primary Diagnosis Selector - Single ICD10 code selection
/// </summary>
public partial class PrimaryDiagnosisSelector : ComponentBase
{
    [Inject] private ILogger<PrimaryDiagnosisSelector> Logger { get; set; } = default!;

    // ==================== PARAMETERS ====================
    
    /// <summary>Currently selected ICD10 code</summary>
    [Parameter] public ICD10SearchResultDto? SelectedCode { get; set; }
    
    [Parameter] public EventCallback<ICD10SearchResultDto?> SelectedCodeChanged { get; set; }
    
    /// <summary>Optional details/description text</summary>
    [Parameter] public string? DiagnosisDetails { get; set; }
    
    [Parameter] public EventCallback<string?> DiagnosisDetailsChanged { get; set; }
    
    /// <summary>User ID for favorites access</summary>
    [Parameter] public Guid? CurrentUserId { get; set; }
    
    /// <summary>Show validation errors</summary>
    [Parameter] public bool ShowValidation { get; set; }

    // ==================== HANDLERS ====================
    
    private async Task HandleCodeSelected(ICD10SearchResultDto code)
    {
        Logger.LogInformation("[PrimaryDiagnosis] Code selected: {Code} - {Description}", 
            code.Code, code.ShortDescription);
        
        SelectedCode = code;
        await SelectedCodeChanged.InvokeAsync(SelectedCode);
    }

    /// <summary>Handle RTE value change and propagate to parent</summary>
    private async Task HandleDetailsChanged(string? value)
    {
        Logger.LogInformation("[PrimaryDiagnosis] HandleDetailsChanged: {Value}", 
            value?.Substring(0, Math.Min(50, value?.Length ?? 0)) ?? "NULL");
        
        DiagnosisDetails = value;
        await DiagnosisDetailsChanged.InvokeAsync(DiagnosisDetails);
    }

    private async Task ClearSelection()
    {
        Logger.LogInformation("[PrimaryDiagnosis] Clearing selection");
        
        SelectedCode = null;
        DiagnosisDetails = null;
        
        await SelectedCodeChanged.InvokeAsync(SelectedCode);
        await DiagnosisDetailsChanged.InvokeAsync(DiagnosisDetails);
    }
}
