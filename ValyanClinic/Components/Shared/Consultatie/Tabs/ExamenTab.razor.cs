using Microsoft.AspNetCore.Components;
using ValyanClinic.Application.Features.ConsultatieManagement.Commands.CreateConsultatie;

namespace ValyanClinic.Components.Shared.Consultatie.Tabs;

public partial class ExamenTab : ComponentBase
{
    [Parameter] public CreateConsultatieCommand Model { get; set; } = new();
    [Parameter] public EventCallback OnChanged { get; set; }
    [Parameter] public EventCallback OnSectionCompleted { get; set; }
    [Parameter] public bool ShowValidation { get; set; } = false;
    
    private bool IsComplete => 
        !string.IsNullOrWhiteSpace(Model.StareGenerala) ||
        !string.IsNullOrWhiteSpace(Model.TensiuneArteriala) ||
        Model.Greutate.HasValue ||
        Model.Inaltime.HasValue;
    
    private async Task OnFieldChanged()
    {
        await OnChanged.InvokeAsync();
        
        if (IsComplete)
        {
            await OnSectionCompleted.InvokeAsync();
        }
    }
}
