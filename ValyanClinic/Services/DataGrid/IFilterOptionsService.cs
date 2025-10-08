namespace ValyanClinic.Services.DataGrid;

/// <summary>
/// Service pentru generarea optiunilor de filtrare din colectii de date
/// </summary>
public interface IFilterOptionsService
{
    /// <summary>
    /// Genereaza optiuni de filtrare pentru un camp string
    /// </summary>
    List<FilterOption> GenerateOptions<T>(
        IEnumerable<T> data,
        Func<T, string?> selector,
        bool includeEmpty = false,
        string? emptyText = null);

    /// <summary>
    /// Genereaza optiuni de filtrare pentru multiple campuri
    /// </summary>
    Dictionary<string, List<FilterOption>> GenerateMultipleOptions<T>(
        IEnumerable<T> data,
        Dictionary<string, Func<T, string?>> selectors,
        bool includeEmpty = false);

    /// <summary>
    /// Genereaza optiuni pentru enum
    /// </summary>
    List<FilterOption> GenerateEnumOptions<TEnum>() where TEnum : struct, Enum;

    /// <summary>
    /// Genereaza optiuni pentru valori booleane
    /// </summary>
    List<FilterOption> GenerateBooleanOptions(string trueText = "Da", string falseText = "Nu");
}

/// <summary>
/// Model pentru optiunile de filtrare
/// </summary>
public class FilterOption
{
    public string Text { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    
    public FilterOption() { }
    
    public FilterOption(string text, string value)
    {
        Text = text;
        Value = value;
    }
}
