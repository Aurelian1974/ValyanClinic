using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;

namespace ValyanClinic.Components.Layout;

public partial class Header : ComponentBase, IDisposable
{
    [Inject] private NavigationManager NavigationManager { get; set; } = default!;
    [Inject] private BreadcrumbService BreadcrumbService { get; set; } = default!;
    
    private string UserName = "Dr. Admin";
    private string UserRole = "Administrator";
    private List<BreadcrumbItem> breadcrumbItems = new();
    private ElementReference avatarImageRef;
    private bool imageLoadFailed = false;
    private bool ShowUserMenu = false;

    protected override void OnInitialized()
    {
        NavigationManager.LocationChanged += OnLocationChanged;
        BreadcrumbService.OnBreadcrumbChanged += OnBreadcrumbChanged;
        UpdateBreadcrumb();
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
            return "A";

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
