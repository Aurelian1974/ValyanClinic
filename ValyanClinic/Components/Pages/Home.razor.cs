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

    // User info
    private string UserGreeting = "Bună, Utilizator!";
  private string UserTitle = "";
  
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
        // Load user data for greeting
        await LoadUserGreeting();
        
      // Initialize dashboard data
    // TODO: fetch from service/repository
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
      var role = user.FindFirst(ClaimTypes.Role)?.Value ?? "";
         
                // Get PersonalMedicalID from claims
        var personalMedicalIdClaim = user.FindFirst("PersonalMedicalID")?.Value;
       
      if (!string.IsNullOrEmpty(personalMedicalIdClaim) && Guid.TryParse(personalMedicalIdClaim, out Guid personalMedicalId))
    {
    // Load PersonalMedical details for full name
      await LoadPersonalMedicalDetails(personalMedicalId, role);
       }
       else
   {
      // Fallback to username
       UserTitle = GetUserTitle(role);
   UserGreeting = $"Bună, {UserTitle} {username}!";
       Logger.LogWarning("PersonalMedicalID not found in claims for user: {Username}", username);
         }
      }
        }
     catch (Exception ex)
   {
      Logger.LogError(ex, "Error loading user greeting");
  UserGreeting = "Bună, Utilizator!";
        }
     finally
        {
      isLoadingUserData = false;
    }
    }

    private async Task LoadPersonalMedicalDetails(Guid personalMedicalId, string role)
  {
     try
        {
var query = new GetPersonalMedicalByIdQuery(personalMedicalId);
       var result = await Mediator.Send(query);

      if (result.IsSuccess && result.Value != null)
     {
     // Update greeting with PersonalMedical full name
      UserTitle = GetUserTitle(role);
       UserGreeting = $"Bună, {UserTitle} {result.Value.NumeComplet}!";
       
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

    private string GetUserTitle(string role)
    {
        return role switch
        {
            "Doctor" or "Medic" => "Dr.",
            "Asistent" or "Asistent Medical" => "As.",
            "Administrator" => "",
     _ => ""
   };
    }

    private string GetCurrentDate()
    {
        var culture = new CultureInfo("ro-RO");
    return DateTime.Now.ToString("dddd, dd MMMM yyyy", culture);
    }
}
