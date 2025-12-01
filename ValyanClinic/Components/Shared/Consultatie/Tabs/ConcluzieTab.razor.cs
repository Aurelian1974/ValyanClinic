using Microsoft.AspNetCore.Components;
using ValyanClinic.Application.Features.ConsultatieManagement.Commands.CreateConsultatie;

namespace ValyanClinic.Components.Shared.Consultatie.Tabs;

/// <summary>
/// Tab Concluzie pentru consultație
/// Include: Prognostic, Concluzie, Observații
/// </summary>
public partial class ConcluzieTab : ComponentBase
{
    [Parameter] public CreateConsultatieCommand Model { get; set; } = new();
    [Parameter] public bool IsActive { get; set; }
    [Parameter] public EventCallback OnChanged { get; set; }
    [Parameter] public EventCallback OnSectionCompleted { get; set; }
    [Parameter] public EventCallback OnPreview { get; set; }
    [Parameter] public bool ShowValidation { get; set; } = false;

    private bool IsSectionCompleted => IsConcluzieCompleted();

    private async Task OnFieldChanged()
    {
        await OnChanged.InvokeAsync();

        // Check if section is completed
        if (IsSectionCompleted)
        {
            await OnSectionCompleted.InvokeAsync();
        }
    }

    /// <summary>
    /// Verifică dacă secțiunea Concluzie este completă
    /// Considerăm completă dacă prognosticul și concluzia sunt completate (ambele obligatorii)
    /// </summary>
    private bool IsConcluzieCompleted()
    {
        // Prognostic obligatoriu
        if (string.IsNullOrWhiteSpace(Model.Prognostic))
            return false;

        // Concluzie obligatorie
        if (string.IsNullOrWhiteSpace(Model.Concluzie))
            return false;

        return true;
    }
}
