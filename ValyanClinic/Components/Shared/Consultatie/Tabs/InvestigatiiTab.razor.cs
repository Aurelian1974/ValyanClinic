using Microsoft.AspNetCore.Components;
using ValyanClinic.Application.Features.ConsultatieManagement.Commands.CreateConsultatie;

namespace ValyanClinic.Components.Shared.Consultatie.Tabs;

/// <summary>
/// Tab Investigații pentru consultație
/// Include: Laborator, Imagistice, EKG, Alte investigații
/// </summary>
public partial class InvestigatiiTab : ComponentBase
{
    [Parameter] public CreateConsultatieCommand Model { get; set; } = new();
    [Parameter] public bool IsActive { get; set; }
    [Parameter] public EventCallback OnChanged { get; set; }
    [Parameter] public EventCallback OnSectionCompleted { get; set; }
    [Parameter] public bool ShowValidation { get; set; } = false;

    private bool IsSectionCompleted => IsInvestigatiiCompleted();

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
    /// Verifică dacă secțiunea Investigații este completă
    /// Considerăm completă dacă cel puțin două tipuri de investigații sunt completate
    /// </summary>
    private bool IsInvestigatiiCompleted()
    {
        var completedCount = 0;

        if (!string.IsNullOrWhiteSpace(Model.InvestigatiiLaborator)) completedCount++;
        if (!string.IsNullOrWhiteSpace(Model.InvestigatiiImagistice)) completedCount++;
        if (!string.IsNullOrWhiteSpace(Model.InvestigatiiEKG)) completedCount++;
        if (!string.IsNullOrWhiteSpace(Model.AlteInvestigatii)) completedCount++;

        // Cel puțin 2 tipuri de investigații completate
        return completedCount >= 2;
    }
}
