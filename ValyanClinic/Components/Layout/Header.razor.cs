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

    protected override void OnInitialized()
    {
        NavigationManager.LocationChanged += OnLocationChanged;
        BreadcrumbService.OnBreadcrumbChanged += OnBreadcrumbChanged;
        UpdateBreadcrumb();
    }

    private void OnLocationChanged(object? sender, LocationChangedEventArgs e)
    {
        UpdateBreadcrumb();
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

    public void Dispose()
    {
        NavigationManager.LocationChanged -= OnLocationChanged;
        BreadcrumbService.OnBreadcrumbChanged -= OnBreadcrumbChanged;
    }
}
