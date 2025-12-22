using Microsoft.AspNetCore.Components;
using ValyanClinic.Application.Services.Consultatii;

namespace ValyanClinic.Components.Shared.Consultatie;

/// <summary>
/// Component izolat pentru timer consultație.
/// StateHasChanged() afectează DOAR acest component, nu întreaga pagină.
/// </summary>
public partial class ConsultationTimer : ComponentBase, IDisposable
{
    [Inject] private IConsultationTimerService TimerService { get; set; } = default!;

    private string FormattedTime => TimerService.FormattedTime;
    private string WarningClass => TimerService.WarningClass;
    private bool IsPaused => TimerService.IsPaused;

    private bool _disposed = false;

    protected override void OnInitialized()
    {
        // Subscribe la timer events - doar acest component se re-renderizează
        TimerService.OnTick += OnTimerTick;
    }

    private void OnTimerTick(object? sender, EventArgs e)
    {
        // ✅ StateHasChanged() afectează DOAR ConsultationTimer, nu întreaga pagină
        InvokeAsync(StateHasChanged);
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        TimerService.OnTick -= OnTimerTick;
    }
}
