using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using ValyanClinic.Application.Features.ConsultatieManagement.Commands.CreateConsultatie;

namespace ValyanClinic.Components.Shared.Consultatie.Tabs;

public partial class DiagnosticTab : ComponentBase
{
    [Inject] private ILogger<DiagnosticTab> Logger { get; set; } = default!;

    [Parameter] public CreateConsultatieCommand Model { get; set; } = new();
    [Parameter] public EventCallback OnChanged { get; set; }
    [Parameter] public EventCallback OnSectionCompleted { get; set; }
    [Parameter] public bool ShowValidation { get; set; } = false;

    private bool IsComplete => !string.IsNullOrWhiteSpace(Model.DiagnosticPozitiv);

    private async Task OnFieldChanged()
    {
        await OnChanged.InvokeAsync();

        if (IsComplete)
        {
            await OnSectionCompleted.InvokeAsync();
        }
    }
}
