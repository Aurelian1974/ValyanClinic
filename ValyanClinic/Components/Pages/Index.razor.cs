using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;

namespace ValyanClinic.Components.Pages;

/// <summary>
/// Pagina principală care redirecționează către dashboard specific rolului
/// </summary>
public partial class Index : ComponentBase
{
    [Inject] private NavigationManager NavigationManager { get; set; } = default!;
    [Inject] private AuthenticationStateProvider AuthenticationStateProvider { get; set; } = default!;

    protected override async Task OnInitializedAsync()
    {
        var authState = await AuthenticationStateProvider.GetAuthenticationStateAsync();
        var user = authState.User;

        if (user.Identity?.IsAuthenticated == true)
        {
            // Extrage rolul utilizatorului din claims
            var role = user.FindFirst(ClaimTypes.Role)?.Value;

            // Redirect către dashboard specific rolului
            var dashboardUrl = role switch
            {
                "Doctor" or "Medic" => "/dashboard/medic",
                "Receptioner" => "/dashboard/receptioner",
                "Administrator" or "Admin" => "/dashboard",
                "Asistent" or "Asistent Medical" => "/dashboard",  // TODO: Create dashboard asistent
                "Manager" => "/dashboard",  // TODO: Create dashboard manager
                _ => "/dashboard"  // Default dashboard pentru roluri necunoscute
            };

            NavigationManager.NavigateTo(dashboardUrl, forceLoad: false);
        }
        else
        {
            // Utilizator neautentificat → redirect la login
            NavigationManager.NavigateTo("/login", forceLoad: false);
        }
    }
}
