using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;

namespace ValyanClinic.Components.Pages;

/// <summary>
/// Pagina principală care redirecționează către login dacă utilizatorul nu este autentificat
/// </summary>
public partial class Index : ComponentBase
{
    [Inject] private NavigationManager NavigationManager { get; set; } = default!;
    [Inject] private AuthenticationStateProvider AuthenticationStateProvider { get; set; } = default!;

    private bool _hasCheckedAuth = false;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender && !_hasCheckedAuth)
        {
            _hasCheckedAuth = true;

            try
            {
                // Verifică starea de autentificare
                var authState = await AuthenticationStateProvider.GetAuthenticationStateAsync();
                var user = authState.User;

                if (!user.Identity?.IsAuthenticated ?? true)
                {
                    // Utilizatorul nu este autentificat → redirect la login
                    NavigationManager.NavigateTo("/login", forceLoad: true);
                }
                else
                {
                    // Utilizatorul este autentificat → redirect la dashboard
                    NavigationManager.NavigateTo("/dashboard", forceLoad: true);
                }
            }
            catch (Exception)
            {
                // În caz de eroare, redirect la login
                NavigationManager.NavigateTo("/login", forceLoad: true);
            }
        }
    }
}
