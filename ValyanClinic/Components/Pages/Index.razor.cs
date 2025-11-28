using Microsoft.AspNetCore.Components;

namespace ValyanClinic.Components.Pages;

/// <summary>
/// Pagina principală care redirecționează instant către /dashboard
/// NavMenu se ocupă de navigarea către dashboard-ul specific rolului
/// </summary>
public partial class Index : ComponentBase
{
    [Inject] private NavigationManager NavigationManager { get; set; } = default!;

    protected override void OnInitialized()
    {
        // ✅ Redirect INSTANT la /dashboard fără verificări
        // NavMenu va gestiona navigarea către dashboard-ul specific rolului
        NavigationManager.NavigateTo("/dashboard", forceLoad: false);
    }
}
