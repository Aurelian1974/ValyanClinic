namespace ValyanClinic.Services.DataGrid;

/// <summary>
/// Service pentru filtrare avansata pe colectii de date
/// </summary>
public interface IDataFilterService
{
    /// <summary>
    /// Aplica cautare globala pe multiple campuri
    /// </summary>
    IEnumerable<T> ApplyGlobalSearch<T>(
        IEnumerable<T> data,
        string? searchText,
        params Func<T, string?>[] fieldSelectors);

    /// <summary>
    /// Aplica filtrare pe un camp specific
    /// </summary>
    IEnumerable<T> ApplyFieldFilter<T>(
        IEnumerable<T> data,
        string? filterValue,
        Func<T, string?> fieldSelector);

    /// <summary>
    /// Builder pentru filtrare complexa
    /// </summary>
    IFilterBuilder<T> CreateFilterBuilder<T>(IEnumerable<T> data);
}

/// <summary>
/// Builder pentru construirea filtrelor in mod fluent
/// </summary>
public interface IFilterBuilder<T>
{
    /// <summary>
    /// Adauga cautare globala
    /// </summary>
    IFilterBuilder<T> WithGlobalSearch(string? searchText, params Func<T, string?>[] fieldSelectors);

    /// <summary>
    /// Adauga filtru pentru un camp
    /// </summary>
    IFilterBuilder<T> WithFieldFilter(string? filterValue, Func<T, string?> fieldSelector);

    /// <summary>
    /// Adauga filtru custom
    /// </summary>
    IFilterBuilder<T> WithCustomFilter(Func<T, bool> predicate);

    /// <summary>
    /// Construieste si aplica toate filtrele
    /// </summary>
    IEnumerable<T> Build();

    /// <summary>
    /// Obtine predicatul combinat pentru toate filtrele
    /// </summary>
    Func<T, bool> GetCombinedPredicate();
}
