using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using ValyanClinic.Services.Authentication;

namespace ValyanClinic.Components.Pages.Auth;

public partial class Logout : ComponentBase
{
    [Inject] private CustomAuthenticationStateProvider AuthStateProvider { get; set; } = default!;
    [Inject] private NavigationManager NavigationManager { get; set; } = default!;
    [Inject] private ILogger<Logout> Logger { get; set; } = default!;
    [Inject] private IJSRuntime JSRuntime { get; set; } = default!;

    protected override async Task OnInitializedAsync()
    {
        try
        {
            Logger.LogInformation("User logout initiated");

            // Call logout API through JavaScript
            await JSRuntime.InvokeVoidAsync("ValyanAuth.logout");

            // Notify Blazor about authentication change
            AuthStateProvider.NotifyAuthenticationChanged();

            // Wait 2 seconds to show message
            await Task.Delay(2000);

            // Redirect to login
            NavigationManager.NavigateTo("/login", forceLoad: true);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error during logout");
            // In case of error, still redirect to login
            NavigationManager.NavigateTo("/login", forceLoad: true);
        }
    }
}
