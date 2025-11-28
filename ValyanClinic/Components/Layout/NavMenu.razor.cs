using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;
using System.Security.Claims;
using Microsoft.Extensions.Logging;

namespace ValyanClinic.Components.Layout;

public partial class NavMenu : ComponentBase
{
    [Inject] private IJSRuntime JSRuntime { get; set; } = default!;
    [Inject] private AuthenticationStateProvider AuthenticationStateProvider { get; set; } = default!;
    [Inject] private NavigationManager NavigationManager { get; set; } = default!;
    [Inject] private ILogger<NavMenu> Logger { get; set; } = default!;
    
    private bool isCollapsed = false;
    private int clickCount = 0;
    
    // Expand/collapse state pentru categorii
    private bool isPacientiExpanded = false;
    private bool isAdministrareExpanded = false;
    private bool isAdministrarePersonalExpanded = false;
    private bool isAdministrareClinicaExpanded = false;
    private bool isSetariGeneraleExpanded = false;
    private bool isProgramariExpanded = false;
    private bool isRapoarteExpanded = false;
    private bool isMonitorizareExpanded = false;

    // ✅ Cached user role - STATIC pentru a persista între instanțe
    private static string? _sharedUserRole = null;
    private static DateTime _lastRoleCheckTime = DateTime.MinValue;
    private static readonly TimeSpan RoleCacheExpiration = TimeSpan.FromMinutes(5);
    
    private bool _instanceRoleChecked = false;

    protected override async Task OnInitializedAsync()
    {
        // ✅ Încarcă rolul utilizatorului
        await LoadUserRole();
    }

    private async Task LoadUserRole()
    {
        Logger.LogInformation("[NavMenu] LoadUserRole() START - _sharedUserRole: {UserRole}, Age: {Age}", 
            _sharedUserRole ?? "null", DateTime.Now - _lastRoleCheckTime);
        
        // ✅ Dacă avem deja rolul în cache și nu a expirat, folosește-l
        if (!string.IsNullOrEmpty(_sharedUserRole) && 
            (DateTime.Now - _lastRoleCheckTime) < RoleCacheExpiration)
        {
            Logger.LogInformation("[NavMenu] LoadUserRole() - Using cached role: {Role}", _sharedUserRole);
            _instanceRoleChecked = true;
            return;
        }
        
        try
        {
            Logger.LogInformation("[NavMenu] LoadUserRole() - Getting authentication state...");
            var authState = await AuthenticationStateProvider.GetAuthenticationStateAsync();
            var user = authState.User;
            
            Logger.LogInformation("[NavMenu] LoadUserRole() - IsAuthenticated: {IsAuth}", user.Identity?.IsAuthenticated);
            
            if (user.Identity?.IsAuthenticated == true)
            {
                var roleClaim = user.FindFirst(ClaimTypes.Role);
                _sharedUserRole = roleClaim?.Value;
                _lastRoleCheckTime = DateTime.Now;
                
                Logger.LogInformation("[NavMenu] LoadUserRole() - Role claim found: {HasClaim}, Value: {Role}", 
                    roleClaim != null, _sharedUserRole ?? "null");
            }
            else
            {
                Logger.LogWarning("[NavMenu] LoadUserRole() - User not authenticated, keeping existing cached role");
                // ❌ NU resetăm _sharedUserRole la null când autentificarea temporar nu e disponibilă
            }
            
            _instanceRoleChecked = true;
            Logger.LogInformation("[NavMenu] LoadUserRole() END - _sharedUserRole: {Role}", _sharedUserRole ?? "null");
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "[NavMenu] LoadUserRole() - Error loading user role");
            _instanceRoleChecked = true;
            // ❌ NU resetăm _sharedUserRole la null în caz de eroare
        }
    }

    // ✅ FIX: Navighează la dashboard când user-ul apasă "Acasă"
    private async Task NavigateToHome()
    {
        Logger.LogInformation("[NavMenu] Home button clicked");
        
        // ✅ Re-verifică rolul (dar nu resetează cache-ul static)
        await LoadUserRole();
        
        var dashboardUrl = "/dashboard"; // Default
        
        if (!string.IsNullOrEmpty(_sharedUserRole))
        {
            dashboardUrl = _sharedUserRole switch
            {
                "Doctor" or "Medic" => "/dashboard/medic",
                "Receptioner" => "/dashboard/receptioner",
                "Administrator" or "Admin" => "/dashboard",
                "Asistent" or "Asistent Medical" => "/dashboard",
                "Manager" => "/dashboard",
                _ => "/dashboard"
            };
        }
        else
        {
            Logger.LogWarning("[NavMenu] Role is still null after LoadUserRole, using default dashboard");
        }
        
        Logger.LogInformation("[NavMenu] Navigating to {DashboardUrl} (role: {Role})", dashboardUrl, _sharedUserRole ?? "null");
        NavigationManager.NavigateTo(dashboardUrl, forceLoad: false);
    }

    private async void HandleToggle()
    {
        clickCount++;
        isCollapsed = !isCollapsed;
        Logger.LogDebug("[NavMenu] Sidebar toggle - Click: {ClickCount}, Collapsed: {IsCollapsed}", clickCount, isCollapsed);
        
        // When collapsing sidebar, collapse all expanded sections
        if (isCollapsed)
        {
            isPacientiExpanded = false;
            isAdministrareExpanded = false;
            isAdministrarePersonalExpanded = false;
            isAdministrareClinicaExpanded = false;
            isSetariGeneraleExpanded = false;
            isProgramariExpanded = false;
            isRapoarteExpanded = false;
            isMonitorizareExpanded = false;
        }
        
        // Update sidebar width using global JS function
        await UpdateSidebarWidth();
        
        StateHasChanged();
    }

    private string GetCollapseButtonTitle() => isCollapsed ? "Extinde" : "Restrante";

    private string GetCollapseButtonIcon() => isCollapsed ? "fa-chevron-right" : "fa-chevron-left";

    private void TogglePacienti()
    {
        if (!isCollapsed)
        {
            isPacientiExpanded = !isPacientiExpanded;
        }
    }

    private void ToggleAdministrare()
    {
        if (!isCollapsed)
        {
            isAdministrareExpanded = !isAdministrareExpanded;
            if (!isAdministrareExpanded)
            {
                isAdministrarePersonalExpanded = false;
                isAdministrareClinicaExpanded = false;
                isSetariGeneraleExpanded = false;
            }
        }
    }

    private void ToggleAdministrarePersonal()
    {
        if (!isCollapsed && isAdministrareExpanded)
        {
            isAdministrarePersonalExpanded = !isAdministrarePersonalExpanded;
        }
    }

    private void ToggleAdministrareClinica()
    {
        if (!isCollapsed && isAdministrareExpanded)
        {
            isAdministrareClinicaExpanded = !isAdministrareClinicaExpanded;
        }
    }

    private void ToggleSetariGenerale()
    {
        if (!isCollapsed && isAdministrareExpanded)
        {
            isSetariGeneraleExpanded = !isSetariGeneraleExpanded;
        }
    }

    private void ToggleProgramari()
    {
        if (!isCollapsed)
        {
            isProgramariExpanded = !isProgramariExpanded;
        }
    }

    private void ToggleRapoarte()
    {
        if (!isCollapsed)
        {
            isRapoarteExpanded = !isRapoarteExpanded;
        }
    }

    private void ToggleMonitorizare()
    {
        if (!isCollapsed)
        {
            isMonitorizareExpanded = !isMonitorizareExpanded;
        }
    }

    private async Task UpdateSidebarWidth()
    {
        try
        {
            // Use global JavaScript function
            await JSRuntime.InvokeVoidAsync("updateSidebarWidth", isCollapsed);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "[NavMenu] Error updating sidebar width");
        }
    }

    private async Task<bool> LoadCollapsedState()
    {
        try
        {
            var savedState = await JSRuntime.InvokeAsync<string>("localStorage.getItem", "sidebar-collapsed");
            
            if (string.IsNullOrEmpty(savedState))
            {
                return false;
            }
            
            var result = savedState.Equals("true", StringComparison.OrdinalIgnoreCase);
            Logger.LogDebug("[NavMenu] Loaded collapsed state: {IsCollapsed}", result);
            return result;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "[NavMenu] Error loading collapsed state");
            return false;
        }
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            // Load saved state from localStorage
            isCollapsed = await LoadCollapsedState();
            
            // Force update sidebar width to match loaded state
            await UpdateSidebarWidth();
            StateHasChanged();
        }
    }

    public bool IsCollapsed => isCollapsed;
}
