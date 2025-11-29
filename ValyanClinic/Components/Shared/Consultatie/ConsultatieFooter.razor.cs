using Microsoft.AspNetCore.Components;

namespace ValyanClinic.Components.Shared.Consultatie;

public partial class ConsultatieFooter : ComponentBase
{
    [Parameter] public bool IsSaving { get; set; }
    [Parameter] public bool IsSavingDraft { get; set; }
    [Parameter] public bool ShowDraftButton { get; set; } = true;
    [Parameter] public bool ShowPreviewButton { get; set; } = true;
    [Parameter] public string SaveButtonText { get; set; } = "Salvează Consultație";
    
    [Parameter] public EventCallback OnSaveDraft { get; set; }
    [Parameter] public EventCallback OnPreview { get; set; }
    [Parameter] public EventCallback OnCancel { get; set; }
    
    private async Task OnSaveDraftClicked()
    {
        await OnSaveDraft.InvokeAsync();
    }
    
    private async Task OnPreviewClicked()
    {
        await OnPreview.InvokeAsync();
    }
    
    private async Task OnCancelClicked()
    {
        await OnCancel.InvokeAsync();
    }
}
