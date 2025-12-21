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
    
    /// <summary>Event when any change occurs</summary>
    [Parameter] public EventCallback OnChanged { get; set; }

    // ==================== HANDLERS ====================
    
    private async Task HandleCodeSelected(ICD10SearchResultDto code)
    {
        Logger.LogInformation("[PrimaryDiagnosis] Code selected: {Code} - {Description}", 
            code.Code, code.ShortDescription);
        
        SelectedCode = code;
        await SelectedCodeChanged.InvokeAsync(SelectedCode);
        await NotifyChange();
    }

    /// <summary>Handle textarea input without causing re-render loops</summary>
    private async Task HandleDetailsInput(ChangeEventArgs e)
    {
        DiagnosisDetails = e.Value?.ToString();
        await DiagnosisDetailsChanged.InvokeAsync(DiagnosisDetails);
        // Note: NOT calling NotifyChange() here to avoid re-render loops during typing
        // The parent will sync on blur or when other fields change
    }

    private async Task ClearSelection()
    {
        Logger.LogInformation("[PrimaryDiagnosis] Clearing selection");
        
        SelectedCode = null;
        DiagnosisDetails = null;
        
        await SelectedCodeChanged.InvokeAsync(SelectedCode);
        await DiagnosisDetailsChanged.InvokeAsync(DiagnosisDetails);
        await NotifyChange();
    }

    private async Task NotifyChange()
    {
        await OnChanged.InvokeAsync();
    }
}
