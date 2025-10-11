using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace ValyanClinic.Components.Layout;

public partial class NavMenu : ComponentBase
{
    [Inject] private IJSRuntime JSRuntime { get; set; } = default!;
    
    private bool isCollapsed = false;
    private int clickCount = 0;
    
    // Expand/collapse state pentru categorii
    private bool isAdministrareExpanded = false;
    private bool isAdministrarePersonalExpanded = false;
    private bool isProgramariExpanded = false;
    private bool isRapoarteExpanded = false;
    private bool isMonitorizareExpanded = false;

    private async void HandleToggle()
    {
        clickCount++;
        isCollapsed = !isCollapsed;
        Console.WriteLine($"NavMenu: Button clicked! Click: {clickCount}, Collapsed: {isCollapsed}");
        
        // When collapsing sidebar, collapse all expanded sections
        if (isCollapsed)
        {
            isAdministrareExpanded = false;
            isAdministrarePersonalExpanded = false;
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

    private void ToggleAdministrare()
    {
        if (!isCollapsed)
        {
            isAdministrareExpanded = !isAdministrareExpanded;
            if (!isAdministrareExpanded)
            {
                isAdministrarePersonalExpanded = false;
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
            Console.WriteLine($"NavMenu: Updated sidebar width via JS function. Collapsed: {isCollapsed}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error updating sidebar width: {ex.Message}");
        }
    }

    private async Task<bool> LoadCollapsedState()
    {
        try
        {
            var savedState = await JSRuntime.InvokeAsync<string>("localStorage.getItem", "sidebar-collapsed");
            Console.WriteLine($"NavMenu: Raw localStorage value: '{savedState}'");
            
            // Compare case-insensitive and handle both "true"/"True" and boolean
            if (string.IsNullOrEmpty(savedState))
            {
                return false;
            }
            
            var result = savedState.Equals("true", StringComparison.OrdinalIgnoreCase);
            Console.WriteLine($"NavMenu: Parsed collapsed state: {result}");
            return result;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading collapsed state: {ex.Message}");
            return false;
        }
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            // Load saved state from localStorage
            isCollapsed = await LoadCollapsedState();
            Console.WriteLine($"NavMenu: OnAfterRenderAsync - Setting collapsed to: {isCollapsed}");
            
            // Force update sidebar width to match loaded state
            await UpdateSidebarWidth();
            StateHasChanged();
        }
    }

    public bool IsCollapsed => isCollapsed;
}
