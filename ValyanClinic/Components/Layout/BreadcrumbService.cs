using Microsoft.AspNetCore.Components;

namespace ValyanClinic.Components.Layout;

public class BreadcrumbService
{
    public event Action<List<BreadcrumbItem>>? OnBreadcrumbChanged;

    private List<BreadcrumbItem> _currentBreadcrumb = new();

    public void SetBreadcrumb(List<BreadcrumbItem> breadcrumb)
    {
        _currentBreadcrumb = breadcrumb;
        OnBreadcrumbChanged?.Invoke(_currentBreadcrumb);
    }

    public void SetBreadcrumb(params BreadcrumbItem[] items)
    {
        SetBreadcrumb(items.ToList());
    }

    public List<BreadcrumbItem> GetCurrentBreadcrumb() => _currentBreadcrumb;

    // Helper method pentru a construi breadcrumb din path
    public static List<BreadcrumbItem> BuildFromPath(string path)
    {
        var items = new List<BreadcrumbItem>
        {
            new BreadcrumbItem { Text = "Acasă", Icon = "fa-home", Url = "/" }
        };

        if (path == "/" || string.IsNullOrEmpty(path))
        {
            items[0].IsActive = true;
            return items;
        }

        // Parse path și construiește breadcrumb
        var pathParts = path.Trim('/').Split('/');

        switch (pathParts[0].ToLower())
        {
            case "administrare":
                // Nivel 1: Administrare - navigare la prima pagină din secțiune
                items.Add(new BreadcrumbItem
                {
                    Text = "Administrare",
                    Icon = "fa-cog",
                    Url = "/administrare/personal" // Link către prima sub-pagină
                });

                if (pathParts.Length > 1)
                {
                    if (pathParts[1] == "personal" || pathParts[1] == "personal-medical")
                    {
                        // Nivel 2: Administrare Personal
                        items.Add(new BreadcrumbItem
                        {
                            Text = "Administrare Personal",
                            Icon = "fa-users-cog",
                            Url = "/administrare/personal" // Link către prima sub-pagină
                        });

                        if (pathParts[1] == "personal")
                        {
                            items.Add(new BreadcrumbItem { Text = "Personal", Icon = "fa-users", Url = "/administrare/personal", IsActive = true });
                        }
                        else if (pathParts[1] == "personal-medical")
                        {
                            items.Add(new BreadcrumbItem { Text = "Personal Medical", Icon = "fa-user-md", Url = "/administrare/personal-medical", IsActive = true });
                        }
                    }
                }
                else
                {
                    // Suntem pe ruta /administrare direct
                    items[^1].IsActive = true;
                }
                break;

            case "programari":
                // Nivel 1: Programări
                items.Add(new BreadcrumbItem
                {
                    Text = "Programări",
                    Icon = "fa-calendar-check",
                    Url = "/programari" // Link către calendar
                });

                if (pathParts.Length > 1)
                {
                    if (pathParts[1] == "lista")
                    {
                        items[^1].IsActive = false; // Programări nu e activ
                        items.Add(new BreadcrumbItem { Text = "Lista Programări", Icon = "fa-list", Url = "/programari/lista", IsActive = true });
                    }
                    else
                    {
                        items[^1].IsActive = true; // Calendar e activ
                    }
                }
                else
                {
                    items[^1].IsActive = true;
                }
                break;

            case "rapoarte":
                items.Add(new BreadcrumbItem
                {
                    Text = "Rapoarte",
                    Icon = "fa-chart-bar",
                    Url = "/rapoarte/statistici" // Link către prima pagină
                });

                if (pathParts.Length > 1 && pathParts[1] == "statistici")
                {
                    items[^1].IsActive = false;
                    items.Add(new BreadcrumbItem { Text = "Statistici", Icon = "fa-chart-line", Url = "/rapoarte/statistici", IsActive = true });
                }
                else
                {
                    items[^1].IsActive = true;
                }
                break;

            case "health":
                items.Add(new BreadcrumbItem
                {
                    Text = "Monitorizare",
                    Icon = "fa-heart-pulse",
                    Url = "/health" // Link direct
                });
                items.Add(new BreadcrumbItem { Text = "Health Check", Icon = "fa-heartbeat", Url = "/health", IsActive = true });
                break;
        }

        return items;
    }
}
