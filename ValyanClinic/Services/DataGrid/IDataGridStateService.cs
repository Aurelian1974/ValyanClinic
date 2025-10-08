namespace ValyanClinic.Services.DataGrid;

/// <summary>
/// Service pentru gestionarea starii unui DataGrid (paginare, filtrare, sortare)
/// </summary>
public interface IDataGridStateService<T> where T : class
{
    /// <summary>
    /// Date originale complete
    /// </summary>
    IReadOnlyList<T> AllData { get; }

    /// <summary>
    /// Date filtrate (dupa aplicarea filtrelor)
    /// </summary>
    IReadOnlyList<T> FilteredData { get; }

    /// <summary>
    /// Date pentru pagina curenta
    /// </summary>
    IReadOnlyList<T> PagedData { get; }

    /// <summary>
    /// Numarul paginii curente (1-indexed)
    /// </summary>
    int CurrentPage { get; }

    /// <summary>
    /// Dimensiunea paginii
    /// </summary>
    int PageSize { get; }

    /// <summary>
    /// Numar total de pagini
    /// </summary>
    int TotalPages { get; }

    /// <summary>
    /// Index-ul primului rand afisat (1-indexed)
    /// </summary>
    int DisplayedRecordsStart { get; }

    /// <summary>
    /// Index-ul ultimului rand afisat (1-indexed)
    /// </summary>
    int DisplayedRecordsEnd { get; }

    /// <summary>
    /// Total inregistrari filtrate
    /// </summary>
    int TotalFilteredRecords { get; }

    /// <summary>
    /// Total inregistrari originale
    /// </summary>
    int TotalRecords { get; }

    /// <summary>
    /// Exista pagina anterioara?
    /// </summary>
    bool HasPreviousPage { get; }

    /// <summary>
    /// Exista pagina urmatoare?
    /// </summary>
    bool HasNextPage { get; }

    /// <summary>
    /// Incarca datele initiale
    /// </summary>
    void SetData(IEnumerable<T> data);

    /// <summary>
    /// Aplica filtrare pe date
    /// </summary>
    void ApplyFilter(Func<T, bool> filterPredicate);

    /// <summary>
    /// Aplica filtre multiple
    /// </summary>
    void ApplyFilters(params Func<T, bool>[] filterPredicates);

    /// <summary>
    /// Sterge toate filtrele
    /// </summary>
    void ClearFilters();

    /// <summary>
    /// Schimba dimensiunea paginii si reseteaza la prima pagina
    /// </summary>
    void ChangePageSize(int newPageSize);

    /// <summary>
    /// Navigheaza la o pagina specifica
    /// </summary>
    bool GoToPage(int pageNumber);

    /// <summary>
    /// Navigheaza la prima pagina
    /// </summary>
    void GoToFirstPage();

    /// <summary>
    /// Navigheaza la ultima pagina
    /// </summary>
    void GoToLastPage();

    /// <summary>
    /// Navigheaza la pagina anterioara
    /// </summary>
    bool GoToPreviousPage();

    /// <summary>
    /// Navigheaza la pagina urmatoare
    /// </summary>
    bool GoToNextPage();

    /// <summary>
    /// Obtine range-ul de pagini pentru pager (ex: 1,2,3,4,5)
    /// </summary>
    (int start, int end) GetPagerRange(int visiblePages = 5);

    /// <summary>
    /// Event declansat cand starea se schimba
    /// </summary>
    event EventHandler? StateChanged;
}
