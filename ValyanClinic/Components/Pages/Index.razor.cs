using Microsoft.AspNetCore.Components;

namespace ValyanClinic.Components.Pages;

/// <summary>
/// Pagina principală care redirecționează către dashboard
/// Autentificarea este verificată la nivel de router prin AuthorizeRouteView
/// </summary>
public partial class Index : ComponentBase
{
    [Inject] private NavigationManager NavigationManager { get; set; } = default!;

    protected override void OnInitialized()
    {
        // Redirect direct la dashboard - autentificarea e verificată la nivel de router
        NavigationManager.NavigateTo("/dashboard", forceLoad: false);
    }
}
