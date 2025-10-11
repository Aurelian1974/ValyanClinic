using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.Logging;

namespace ValyanClinic.Components.Shared.Modals;

public partial class ConfirmDeleteModal : ComponentBase, IDisposable
{
    [Inject] private ILogger<ConfirmDeleteModal> Logger { get; set; } = default!;

    [Parameter] public EventCallback<Guid> OnConfirmed { get; set; }
    [Parameter] public EventCallback OnClosed { get; set; }

    private bool IsVisible { get; set; }
    private bool IsLoading { get; set; }
    private bool HasError { get; set; }
    private string ErrorMessage { get; set; } = string.Empty;

    private Guid ItemId { get; set; }
    private string ItemName { get; set; } = string.Empty;
    private string ConfirmationText { get; set; } = string.Empty;

    private int CountdownSeconds { get; set; }
    private CancellationTokenSource? _countdownTokenSource;
    private const int CountdownDuration = 3;

    private bool CanConfirm => 
        !string.IsNullOrWhiteSpace(ConfirmationText) && 
        ConfirmationText.Trim().Equals("STERGE", StringComparison.OrdinalIgnoreCase) &&
        CountdownSeconds == 0 &&
        !IsLoading;

    public async Task Open(Guid id, string itemName)
    {
        Logger.LogInformation("Deschidere confirmare stergere: {ItemName} ({ItemId})", itemName, id);

        ItemId = id;
        ItemName = itemName;
        ConfirmationText = string.Empty;
        HasError = false;
        ErrorMessage = string.Empty;
        IsLoading = false;

        IsVisible = true;
        StateHasChanged();

        await StartCountdown();
    }

    public async Task Close()
    {
        Logger.LogInformation("Inchidere confirmare stergere");

        StopCountdown();
        IsVisible = false;
        StateHasChanged();

        await Task.Delay(300);

        ItemId = Guid.Empty;
        ItemName = string.Empty;
        ConfirmationText = string.Empty;
        HasError = false;
        ErrorMessage = string.Empty;
        CountdownSeconds = 0;

        if (OnClosed.HasDelegate)
        {
            await OnClosed.InvokeAsync();
        }
    }

    private async Task StartCountdown()
    {
        StopCountdown();

        CountdownSeconds = CountdownDuration;
        StateHasChanged();

        _countdownTokenSource = new CancellationTokenSource();
        var token = _countdownTokenSource.Token;

        try
        {
            using var timer = new PeriodicTimer(TimeSpan.FromSeconds(1));
            
            while (CountdownSeconds > 0 && await timer.WaitForNextTickAsync(token))
            {
                CountdownSeconds--;
                await InvokeAsync(StateHasChanged);
            }
        }
        catch (OperationCanceledException)
        {
            Logger.LogDebug("Countdown anulat");
        }
    }

    private void StopCountdown()
    {
        _countdownTokenSource?.Cancel();
        _countdownTokenSource?.Dispose();
        _countdownTokenSource = null;
    }

    private async Task HandleConfirm()
    {
        if (!CanConfirm) return;

        Logger.LogInformation("Confirmare stergere: {ItemName} ({ItemId})", ItemName, ItemId);

        IsLoading = true;
        HasError = false;
        StateHasChanged();

        try
        {
            if (OnConfirmed.HasDelegate)
            {
                await OnConfirmed.InvokeAsync(ItemId);
            }
            
            await Close();
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Eroare la confirmarea stergerii");
            HasError = true;
            ErrorMessage = $"Eroare: {ex.Message}";
            IsLoading = false;
            StateHasChanged();
        }
    }

    private async Task RetryDelete()
    {
        Logger.LogInformation("Retry stergere: {ItemName} ({ItemId})", ItemName, ItemId);
        
        HasError = false;
        ErrorMessage = string.Empty;
        ConfirmationText = string.Empty;
        StateHasChanged();
        
        await StartCountdown();
    }

    private async Task HandleOverlayClick()
    {
        if (!IsLoading)
        {
            await Close();
        }
    }

    private async Task HandleKeyDown(KeyboardEventArgs e)
    {
        if (e.Key == "Enter" && CanConfirm)
        {
            await HandleConfirm();
        }
        else if (e.Key == "Escape" && !IsLoading)
        {
            await Close();
        }
    }

    public void Dispose()
    {
        StopCountdown();
    }
}
