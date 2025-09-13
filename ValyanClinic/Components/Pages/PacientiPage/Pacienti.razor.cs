using Microsoft.AspNetCore.Components;

namespace ValyanClinic.Components.Pages.PacientiPage;

/// <summary>
/// Business Logic pentru Pacienti.razor - ORGANIZAT ÎN FOLDER PacientiPage
/// Coming Soon page pentru func?ionalitatea Pacien?i
/// </summary>
public partial class Pacienti : ComponentBase
{
    [Inject] private NavigationManager Navigation { get; set; } = default!;
    [Inject] private ILogger<Pacienti> Logger { get; set; } = default!;
    
    protected override void OnInitialized()
    {
        Logger.LogInformation("Pacienti page initialized - Coming Soon functionality");
    }

    private void NavigateToUtilizatori()
    {
        Navigation.NavigateTo("/utilizatori");
    }
    
    private void NavigateToHome()
    {
        Navigation.NavigateTo("/");
    }
}