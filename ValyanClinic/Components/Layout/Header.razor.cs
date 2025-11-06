using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;
using MediatR;
using ValyanClinic.Application.Features.PersonalMedicalManagement.Queries.GetPersonalMedicalById;

namespace ValyanClinic.Components.Layout;

public partial class Header : ComponentBase, IDisposable
{
    [Inject] private NavigationManager NavigationManager { get; set; } = default!;
    [Inject] private BreadcrumbService BreadcrumbService { get; set; } = default!;
    [Inject] private AuthenticationStateProvider AuthenticationStateProvider { get; set; } = default!;
    [Inject] private IMediator Mediator { get; set; } = default!;
    [Inject] private ILogger<Header> Logger { get; set; } = default!;
    
    private string UserName = "Utilizator";
    private string UserRole = "User";
    private Guid? PersonalMedicalID;
    private List<BreadcrumbItem> breadcrumbItems = new();
    private ElementReference avatarImageRef;
    private bool imageLoadFailed = false;
    private bool ShowUserMenu = false;
    private bool isLoadingUserData = false;

    protected override async Task OnInitializedAsync()
    {
        NavigationManager.LocationChanged += OnLocationChanged;
        BreadcrumbService.OnBreadcrumbChanged += OnBreadcrumbChanged;
        UpdateBreadcrumb();
        
        // Load user data from authentication
        await LoadUserData();
    }

    private async Task LoadUserData()
    {
   try
        {
            isLoadingUserData = true;
            var authState = await AuthenticationStateProvider.GetAuthenticationStateAsync();
   var user = authState.User;

   if (user.Identity?.IsAuthenticated == true)
        {
       // Get basic info from claims
          UserName = user.Identity.Name ?? "Utilizator";
           UserRole = user.FindFirst(ClaimTypes.Role)?.Value ?? "User";
         
        // Get PersonalMedicalID from claims (if available)
 var personalMedicalIdClaim = user.FindFirst("PersonalMedicalID")?.Value;
       
    if (!string.IsNullOrEmpty(personalMedicalIdClaim) && Guid.TryParse(personalMedicalIdClaim, out Guid personalMedicalId))
    {
         PersonalMedicalID = personalMedicalId;
      
           // Load PersonalMedical details
          await LoadPersonalMedicalDetails(personalMedicalId);
       }
      else
       {
        Logger.LogWarning("PersonalMedicalID not found in claims for user: {Username}", UserName);
                }
            }
        }
        catch (Exception ex)
        {
    Logger.LogError(ex, "Error loading user data");
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
 // Update UserName with full name from PersonalMedical
                UserName = result.Value.NumeComplet;
      
        Logger.LogInformation("Loaded PersonalMedical data for: {NumeComplet}", UserName);
       }
        else
   {
            Logger.LogWarning("Failed to load PersonalMedical details for ID: {PersonalMedicalID}", personalMedicalId);
       }
        }
catch (Exception ex)
        {
         Logger.LogError(ex, "Error loading PersonalMedical details");
        }
    }

    private void OnLocationChanged(object? sender, LocationChangedEventArgs e)
    {
        UpdateBreadcrumb();
      ShowUserMenu = false; // Close menu on navigation
        StateHasChanged();
    }

    private void OnBreadcrumbChanged(List<BreadcrumbItem> items)
    {
        breadcrumbItems = items;
     StateHasChanged();
    }

    private void UpdateBreadcrumb()
    {
        var uri = new Uri(NavigationManager.Uri);
        var path = uri.AbsolutePath;
        breadcrumbItems = BreadcrumbService.BuildFromPath(path);
    }

    private void ToggleUserMenu()
 {
        ShowUserMenu = !ShowUserMenu;
    }

 private string GetUserAvatarUrl() => "/images/avatar-default.png";

    private string GetUserInitials()
    {
        if (string.IsNullOrEmpty(UserName))
  return "U";

   var parts = UserName.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length >= 2)
            return $"{parts[0][0]}{parts[1][0]}".ToUpper();

        return UserName[0].ToString().ToUpper();
    }

    private string GetAvatarCssClass() => imageLoadFailed ? "user-avatar hidden" : "user-avatar";

    private string GetAvatarFallbackCssClass() => imageLoadFailed ? "user-avatar-fallback visible" : "user-avatar-fallback";

    public void Dispose()
    {
        NavigationManager.LocationChanged -= OnLocationChanged;
        BreadcrumbService.OnBreadcrumbChanged -= OnBreadcrumbChanged;
    }
}
