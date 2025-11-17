using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using System.Globalization;
using System.Security.Claims;
using MediatR;
using ValyanClinic.Application.Features.PersonalMedicalManagement.Queries.GetPersonalMedicalById;

namespace ValyanClinic.Components.Pages;

public partial class Home : ComponentBase
{
    [Inject] private AuthenticationStateProvider AuthenticationStateProvider { get; set; } = default!;
    [Inject] private IMediator Mediator { get; set; } = default!;
    [Inject] private ILogger<Home> Logger { get; set; } = default!;
    [Inject] private NavigationManager NavigationManager { get; set; } = default!;

    // User info
    private string UserGreeting = "Bună, Utilizator!";
    
    // Dashboard statistics
    private int PatientsTodayCount { get; set; } = 42;
    private int PatientsTodayGrowth { get; set; } = 12;
    private int AppointmentsCount { get; set; } = 28;
    private int AppointmentsGrowth { get; set; } = 8;
    private int ActiveStaffCount { get; set; } = 15;
    private string MonthlyRevenue { get; set; } = "125K";
    private int RevenueGrowth { get; set; } = 18;
    private bool isLoadingUserData = false;

    protected override async Task OnInitializedAsync()
    {
        Logger.LogInformation("========== Home.razor (Dashboard General) OnInitializedAsync START ==========");

        // ✅ VERIFICARE ROL - Redirect Receptioner către dashboard dedicat
        await CheckAndRedirectReceptioner();

   // Load user data for greeting
        await LoadUserGreeting();

        // Initialize dashboard data
        // TODO: fetch from service/repository

        Logger.LogInformation("========== Home.razor OnInitializedAsync END ==========");
    }

  private async Task CheckAndRedirectReceptioner()
 {
        try
        {
            var authState = await AuthenticationStateProvider.GetAuthenticationStateAsync();
            var user = authState.User;

            if (user.Identity?.IsAuthenticated == true)
            {
                var role = user.FindFirst(ClaimTypes.Role)?.Value ?? "";
                
                Logger.LogInformation("🔍 Dashboard General - User role: {Role}", role);

                // ✅ Redirect către dashboard specific rolului
                string? redirectUrl = role switch
                {
                    "Receptioner" => "/dashboard/receptioner",
                    "Doctor" or "Medic" => "/dashboard/medic",
                    _ => null  // Permite accesul la dashboard general
                };

                if (!string.IsNullOrEmpty(redirectUrl))
                {
                    Logger.LogInformation("🔄 User role {Role} detected on General Dashboard - Redirecting to {Url}", 
                        role, redirectUrl);
                    NavigationManager.NavigateTo(redirectUrl, forceLoad: false);
                    return; // STOP aici, nu mai încărcăm restul
                }

                Logger.LogInformation("✅ User role {Role} is allowed on General Dashboard", role);
            }
     }
        catch (Exception ex)
        {
  Logger.LogError(ex, "Error checking user role for redirect");
    }
    }

    private async Task LoadUserGreeting()
    {
        try
        {
       isLoadingUserData = true;
        var authState = await AuthenticationStateProvider.GetAuthenticationStateAsync();
     var user = authState.User;

     if (user.Identity?.IsAuthenticated == true)
     {
      var username = user.Identity.Name ?? "Utilizator";

// Get PersonalMedicalID from claims
 var personalMedicalIdClaim = user.FindFirst("PersonalMedicalID")?.Value;
         
     if (!string.IsNullOrEmpty(personalMedicalIdClaim) && Guid.TryParse(personalMedicalIdClaim, out Guid personalMedicalId))
      {
        // Load PersonalMedical details for full name
       await LoadPersonalMedicalDetails(personalMedicalId);
  }
           else
         {
    // Fallback to username - mesaj compact fără titlu
     UserGreeting = $"Bună, {username}!";
     Logger.LogWarning("PersonalMedicalID not found in claims for user: {Username}", username);
  }
            }
        }
     catch (Exception ex)
     {
      Logger.LogError(ex, "Error loading user greeting");
     UserGreeting = "Bună!";
        }
        finally
        {
    isLoadingUserData = false;
  }
    }

    private async Task LoadPersonalMedicalDetails(Guid personalMedicalId)
    {
        try
        {
    var query = new GetPersonalMedicalByIdQuery(personalMedicalId);
         var result = await Mediator.Send(query);

     if (result.IsSuccess && result.Value != null)
    {
       // ✅ Mesaj COMPACT - doar nume complet, fără titlu (Dr., As.)
            UserGreeting = $"Bună, {result.Value.NumeComplet}!";
      
       Logger.LogInformation("Loaded greeting for: {NumeComplet}", result.Value.NumeComplet);
            }
   else
     {
        Logger.LogWarning("Failed to load PersonalMedical details for ID: {PersonalMedicalID}", personalMedicalId);
   }
        }
        catch (Exception ex)
        {
  Logger.LogError(ex, "Error loading PersonalMedical details for greeting");
        }
    }

    private string GetCurrentDate()
    {
 var culture = new CultureInfo("ro-RO");
      return DateTime.Now.ToString("dddd, dd MMMM yyyy", culture);
    }
}
