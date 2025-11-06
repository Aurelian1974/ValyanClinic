using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using ValyanClinic.Services.Authentication;

namespace ValyanClinic.Components.Pages.Auth;

public partial class Logout : ComponentBase
{
    [Inject] private CustomAuthenticationStateProvider AuthStateProvider { get; set; } = default!;
    [Inject] private NavigationManager NavigationManager { get; set; } = default!;
    [Inject] private ILogger<Logout> Logger { get; set; } = default!;

    protected override async Task OnInitializedAsync()
    {
        try
      {
            Logger.LogInformation("User logout initiated");

            // Marchează utilizatorul ca deconectat
  await AuthStateProvider.MarkUserAsLoggedOut();

    // Așteaptă 2 secunde pentru a afișa mesajul
            await Task.Delay(2000);

    // Redirect la login
            NavigationManager.NavigateTo("/login", forceLoad: true);
        }
        catch (Exception ex)
 {
            Logger.LogError(ex, "Error during logout");
            // În caz de eroare, tot redirecționăm la login
   NavigationManager.NavigateTo("/login", forceLoad: true);
    }
    }
}
