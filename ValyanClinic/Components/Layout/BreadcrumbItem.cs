namespace ValyanClinic.Components.Layout;

public class BreadcrumbItem
{
    public string Text { get; set; } = string.Empty;
    public string? Icon { get; set; }
    public string? Url { get; set; }
    public bool IsActive { get; set; }
}
