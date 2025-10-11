using System.ComponentModel.DataAnnotations;

namespace ValyanClinic.Services.DataGrid;

/// <summary>
/// Implementare service pentru generarea optiunilor de filtrare
/// </summary>
public class FilterOptionsService : IFilterOptionsService
{
    public List<FilterOption> GenerateOptions<T>(
        IEnumerable<T> data,
        Func<T, string?> selector,
        bool includeEmpty = false,
        string? emptyText = null)
    {
        if (data == null || selector == null)
            return new List<FilterOption>();

        var options = data
            .Select(selector)
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Distinct()
            .OrderBy(value => value)
            .Select(value => new FilterOption(value!, value!))
            .ToList();

        if (includeEmpty)
        {
            options.Insert(0, new FilterOption(emptyText ?? "(Gol)", string.Empty));
        }

        return options;
    }

    public Dictionary<string, List<FilterOption>> GenerateMultipleOptions<T>(
        IEnumerable<T> data,
        Dictionary<string, Func<T, string?>> selectors,
        bool includeEmpty = false)
    {
        if (data == null || selectors == null)
            return new Dictionary<string, List<FilterOption>>();

        var result = new Dictionary<string, List<FilterOption>>();

        foreach (var kvp in selectors)
        {
            result[kvp.Key] = GenerateOptions(data, kvp.Value, includeEmpty);
        }

        return result;
    }

    public List<FilterOption> GenerateEnumOptions<TEnum>() where TEnum : struct, Enum
    {
        var options = new List<FilterOption>();

        foreach (TEnum value in Enum.GetValues<TEnum>())
        {
            var field = value.GetType().GetField(value.ToString());
            var displayAttribute = field?.GetCustomAttributes(typeof(DisplayAttribute), false)
                .FirstOrDefault() as DisplayAttribute;

            var displayName = displayAttribute?.Name ?? value.ToString();
            
            options.Add(new FilterOption(displayName, value.ToString()));
        }

        return options;
    }

    public List<FilterOption> GenerateBooleanOptions(string trueText = "Da", string falseText = "Nu")
    {
        return new List<FilterOption>
        {
            new FilterOption(trueText, "true"),
            new FilterOption(falseText, "false")
        };
    }
}
