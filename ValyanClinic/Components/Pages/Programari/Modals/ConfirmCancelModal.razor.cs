using Microsoft.AspNetCore.Components;

namespace ValyanClinic.Components.Pages.Programari.Modals;

public partial class ConfirmCancelModal : ComponentBase
{
[Parameter] public bool IsVisible { get; set; }
    [Parameter] public EventCallback<bool> IsVisibleChanged { get; set; }
    [Parameter] public string Message { get; set; } = string.Empty;
    [Parameter] public EventCallback OnConfirmed { get; set; }

  private async Task Confirm()
    {
  await OnConfirmed.InvokeAsync();
        await IsVisibleChanged.InvokeAsync(false);
    }

    private async Task Cancel()
    {
   await IsVisibleChanged.InvokeAsync(false);
    }
}
