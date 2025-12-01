using Microsoft.AspNetCore.Components;
using ValyanClinic.Application.Services.IMC;

namespace ValyanClinic.Components.Shared.Medical;

public partial class IMCCalculator : ComponentBase
{
    [Inject] private IIMCCalculatorService IMCService { get; set; } = default!;

    [Parameter] public decimal? Greutate { get; set; }
    [Parameter] public decimal? Inaltime { get; set; }
    [Parameter] public EventCallback<decimal?> GreutateChanged { get; set; }
    [Parameter] public EventCallback<decimal?> InaltimeChanged { get; set; }
    [Parameter] public bool ShowDetails { get; set; } = true;

    private IMCResult? IMCResult { get; set; }

    protected override void OnParametersSet()
    {
        CalculateIMC();
    }

    private async Task OnWeightChanged(Microsoft.AspNetCore.Components.ChangeEventArgs e)
    {
        if (decimal.TryParse(e.Value?.ToString(), out var weight))
        {
            Greutate = weight;
            await GreutateChanged.InvokeAsync(weight);
        }
        else
        {
            Greutate = null;
            await GreutateChanged.InvokeAsync(null);
        }

        CalculateIMC();
    }

    private async Task OnHeightChanged(Microsoft.AspNetCore.Components.ChangeEventArgs e)
    {
        if (decimal.TryParse(e.Value?.ToString(), out var height))
        {
            Inaltime = height;
            await InaltimeChanged.InvokeAsync(height);
        }
        else
        {
            Inaltime = null;
            await InaltimeChanged.InvokeAsync(null);
        }

        CalculateIMC();
    }

    private void CalculateIMC()
    {
        if (Greutate.HasValue && Inaltime.HasValue)
        {
            IMCResult = IMCService.Calculate(Greutate.Value, Inaltime.Value);
        }
        else
        {
            IMCResult = null;
        }
    }

    private string GetCategoryIcon()
    {
        if (IMCResult == null) return "fas fa-calculator";

        return IMCResult.Category switch
        {
            IMCCategory.Subponderal => "fas fa-arrow-down",
            IMCCategory.Normal => "fas fa-check-circle",
            IMCCategory.Supraponderal => "fas fa-exclamation-triangle",
            IMCCategory.Obezitate1 => "fas fa-exclamation-circle",
            IMCCategory.Obezitate2 => "fas fa-times-circle",
            IMCCategory.ObezitateMorbida => "fas fa-skull-crossbones",
            _ => "fas fa-calculator"
        };
    }
}
