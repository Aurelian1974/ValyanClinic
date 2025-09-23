using ValyanClinic.Domain.Models;

namespace ValyanClinic.Domain.Extensions;

/// <summary>
/// Extension methods for DepartamentMedical to help with display names, formatting, and UI operations
/// </summary>
public static class DepartamentMedicalExtensions
{
    /// <summary>
    /// Gets a formatted display name for the department with optional prefix
    /// </summary>
    /// <param name="departament">The medical department</param>
    /// <param name="includeType">Whether to include the type in display name</param>
    /// <returns>Formatted display name</returns>
    public static string GetFormattedDisplayName(this DepartamentMedical departament, bool includeType = false)
    {
        if (departament == null || string.IsNullOrWhiteSpace(departament.Nume))
            return string.Empty;

        return includeType && !string.IsNullOrWhiteSpace(departament.Tip)
            ? $"{departament.Nume} ({departament.Tip})"
            : departament.Nume;
    }

    /// <summary>
    /// Gets a short display name (truncated if too long)
    /// </summary>
    /// <param name="departament">The medical department</param>
    /// <param name="maxLength">Maximum length for the display name</param>
    /// <returns>Truncated display name with ellipsis if needed</returns>
    public static string GetShortDisplayName(this DepartamentMedical departament, int maxLength = 30)
    {
        if (departament == null || string.IsNullOrWhiteSpace(departament.Nume))
            return string.Empty;

        return departament.Nume.Length <= maxLength
            ? departament.Nume
            : $"{departament.Nume[..(maxLength - 3)]}...";
    }

    /// <summary>
    /// Gets a display name with icon/emoji based on department type
    /// </summary>
    /// <param name="departament">The medical department</param>
    /// <returns>Display name with appropriate icon</returns>
    public static string GetIconDisplayName(this DepartamentMedical departament)
    {
        if (departament == null || string.IsNullOrWhiteSpace(departament.Nume))
            return string.Empty;

        var icon = departament.Tip?.ToLowerInvariant() switch
        {
            "categorie" => "🏥",
            "specializare" => "👩‍⚕️",
            "subspecializare" => "🩺",
            "medical" => "⚕️",
            _ => "📋"
        };

        return $"{icon} {departament.Nume}";
    }

    /// <summary>
    /// Creates a breadcrumb-style display name for hierarchical departments
    /// </summary>
    /// <param name="departament">The medical department</param>
    /// <param name="parentDepartament">The parent department (optional)</param>
    /// <param name="separator">Separator for breadcrumb</param>
    /// <returns>Breadcrumb display name</returns>
    public static string GetBreadcrumbDisplayName(this DepartamentMedical departament, 
        DepartamentMedical? parentDepartament = null, 
        string separator = " > ")
    {
        if (departament == null || string.IsNullOrWhiteSpace(departament.Nume))
            return string.Empty;

        if (parentDepartament != null && !string.IsNullOrWhiteSpace(parentDepartament.Nume))
        {
            return $"{parentDepartament.Nume}{separator}{departament.Nume}";
        }

        return departament.Nume;
    }

    /// <summary>
    /// Gets a CSS class name based on department type
    /// </summary>
    /// <param name="departament">The medical department</param>
    /// <param name="prefix">CSS class prefix</param>
    /// <returns>CSS class name</returns>
    public static string GetCssClass(this DepartamentMedical departament, string prefix = "dept")
    {
        if (departament == null || string.IsNullOrWhiteSpace(departament.Tip))
            return $"{prefix}-default";

        var cleanType = departament.Tip.ToLowerInvariant()
            .Replace(" ", "-")
            .Replace("a", "a")
            .Replace("i", "i")
            .Replace("s", "s")
            .Replace("t", "t");

        return $"{prefix}-{cleanType}";
    }

    /// <summary>
    /// Checks if the department matches any of the given types
    /// </summary>
    /// <param name="departament">The medical department</param>
    /// <param name="types">Types to check against</param>
    /// <returns>True if department matches any of the types</returns>
    public static bool IsOfType(this DepartamentMedical departament, params string[] types)
    {
        if (departament == null || string.IsNullOrWhiteSpace(departament.Tip))
            return false;

        return types.Any(type => string.Equals(departament.Tip, type, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Checks if the department is a main category
    /// </summary>
    /// <param name="departament">The medical department</param>
    /// <returns>True if it's a main category</returns>
    public static bool IsCategorie(this DepartamentMedical departament)
    {
        return departament.IsOfType("Categorie", "Category");
    }

    /// <summary>
    /// Checks if the department is a specialization
    /// </summary>
    /// <param name="departament">The medical department</param>
    /// <returns>True if it's a specialization</returns>
    public static bool IsSpecializare(this DepartamentMedical departament)
    {
        return departament.IsOfType("Specializare", "Specialization");
    }

    /// <summary>
    /// Checks if the department is a subspecialization
    /// </summary>
    /// <param name="departament">The medical department</param>
    /// <returns>True if it's a subspecialization</returns>
    public static bool IsSubspecializare(this DepartamentMedical departament)
    {
        return departament.IsOfType("Subspecializare", "Subspecialization");
    }

    /// <summary>
    /// Gets a tooltip text for the department with additional information
    /// </summary>
    /// <param name="departament">The medical department</param>
    /// <returns>Tooltip text</returns>
    public static string GetTooltipText(this DepartamentMedical departament)
    {
        if (departament == null)
            return string.Empty;

        var tooltip = departament.Nume;
        
        if (!string.IsNullOrWhiteSpace(departament.Tip))
        {
            tooltip += $"\nTip: {departament.Tip}";
        }

        tooltip += $"\nID: {departament.DepartamentID:D}";

        return tooltip;
    }

    /// <summary>
    /// Creates a search-friendly text representation
    /// </summary>
    /// <param name="departament">The medical department</param>
    /// <returns>Search text with all relevant fields</returns>
    public static string GetSearchableText(this DepartamentMedical departament)
    {
        if (departament == null)
            return string.Empty;

        var searchText = departament.Nume ?? string.Empty;
        
        if (!string.IsNullOrWhiteSpace(departament.Tip))
        {
            searchText += $" {departament.Tip}";
        }

        return searchText.ToLowerInvariant();
    }

    /// <summary>
    /// Checks if the department has valid data for display
    /// </summary>
    /// <param name="departament">The medical department</param>
    /// <returns>True if department has valid display data</returns>
    public static bool HasValidDisplayData(this DepartamentMedical departament)
    {
        return departament != null &&
               departament.DepartamentID != Guid.Empty &&
               !string.IsNullOrWhiteSpace(departament.Nume);
    }

    /// <summary>
    /// Creates a JSON-friendly representation for client-side operations
    /// </summary>
    /// <param name="departament">The medical department</param>
    /// <returns>Anonymous object with key properties</returns>
    public static object ToClientObject(this DepartamentMedical departament)
    {
        if (departament == null)
            return new { };

        return new
        {
            id = departament.DepartamentID,
            value = departament.Value,
            text = departament.Text,
            displayName = departament.DisplayName,
            tip = departament.Tip,
            isMedical = departament.IsMedical,
            cssClass = departament.GetCssClass(),
            tooltip = departament.GetTooltipText()
        };
    }
}

/// <summary>
/// Extension methods for collections of DepartamentMedical
/// </summary>
public static class DepartamentMedicalCollectionExtensions
{
    /// <summary>
    /// Filters departments by type
    /// </summary>
    /// <param name="departamente">Collection of departments</param>
    /// <param name="tip">Type to filter by</param>
    /// <returns>Filtered collection</returns>
    public static IEnumerable<DepartamentMedical> ByType(this IEnumerable<DepartamentMedical> departamente, string tip)
    {
        return departamente?.Where(d => string.Equals(d.Tip, tip, StringComparison.OrdinalIgnoreCase)) ?? Enumerable.Empty<DepartamentMedical>();
    }

    /// <summary>
    /// Sorts departments by name
    /// </summary>
    /// <param name="departamente">Collection of departments</param>
    /// <param name="ascending">Sort order</param>
    /// <returns>Sorted collection</returns>
    public static IEnumerable<DepartamentMedical> SortByName(this IEnumerable<DepartamentMedical> departamente, bool ascending = true)
    {
        if (departamente == null) return Enumerable.Empty<DepartamentMedical>();

        return ascending
            ? departamente.OrderBy(d => d.Nume)
            : departamente.OrderByDescending(d => d.Nume);
    }

    /// <summary>
    /// Groups departments by type
    /// </summary>
    /// <param name="departamente">Collection of departments</param>
    /// <returns>Grouped collection</returns>
    public static IEnumerable<IGrouping<string, DepartamentMedical>> GroupByType(this IEnumerable<DepartamentMedical> departamente)
    {
        return departamente?.GroupBy(d => d.Tip ?? "Unknown") ?? Enumerable.Empty<IGrouping<string, DepartamentMedical>>();
    }

    /// <summary>
    /// Converts collection to dropdown options
    /// </summary>
    /// <param name="departamente">Collection of departments</param>
    /// <returns>Collection of dropdown options</returns>
    public static IEnumerable<DepartamentMedicalOption> ToDropdownOptions(this IEnumerable<DepartamentMedical> departamente)
    {
        return departamente?.Select(DepartamentMedicalOption.FromDepartamentMedical) ?? Enumerable.Empty<DepartamentMedicalOption>();
    }

    /// <summary>
    /// Searches departments by text in name or type
    /// </summary>
    /// <param name="departamente">Collection of departments</param>
    /// <param name="searchText">Text to search for</param>
    /// <returns>Filtered collection matching search text</returns>
    public static IEnumerable<DepartamentMedical> Search(this IEnumerable<DepartamentMedical> departamente, string? searchText)
    {
        if (departamente == null) return Enumerable.Empty<DepartamentMedical>();
        if (string.IsNullOrWhiteSpace(searchText)) return departamente;

        return departamente.Where(d => d.MatchesSearchText(searchText));
    }

    /// <summary>
    /// Gets only departments with valid display data
    /// </summary>
    /// <param name="departamente">Collection of departments</param>
    /// <returns>Filtered collection with valid data</returns>
    public static IEnumerable<DepartamentMedical> WithValidDisplayData(this IEnumerable<DepartamentMedical> departamente)
    {
        return departamente?.Where(d => d.HasValidDisplayData()) ?? Enumerable.Empty<DepartamentMedical>();
    }
}
