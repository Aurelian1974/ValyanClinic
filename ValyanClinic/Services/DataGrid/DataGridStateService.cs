namespace ValyanClinic.Services.DataGrid;

/// <summary>
/// Implementare service pentru gestionarea starii unui DataGrid
/// </summary>
public class DataGridStateService<T> : IDataGridStateService<T> where T : class
{
    private List<T> _allData = new();
    private List<T> _filteredData = new();
    private List<T> _pagedData = new();
    private List<Func<T, bool>> _activeFilters = new();
    private int _currentPage = 1;
    private int _pageSize = 20;

    public IReadOnlyList<T> AllData => _allData.AsReadOnly();
    public IReadOnlyList<T> FilteredData => _filteredData.AsReadOnly();
    public IReadOnlyList<T> PagedData => _pagedData.AsReadOnly();

    public int CurrentPage => _currentPage;
    public int PageSize => _pageSize;
    public int TotalPages => _filteredData.Count == 0 ? 1 : (int)Math.Ceiling((double)_filteredData.Count / _pageSize);
    
    public int DisplayedRecordsStart => _filteredData.Count == 0 ? 0 : (_currentPage - 1) * _pageSize + 1;
    public int DisplayedRecordsEnd => Math.Min(_currentPage * _pageSize, _filteredData.Count);
    
    public int TotalFilteredRecords => _filteredData.Count;
    public int TotalRecords => _allData.Count;
    
    public bool HasPreviousPage => _currentPage > 1;
    public bool HasNextPage => _currentPage < TotalPages;

    public event EventHandler? StateChanged;

    public void SetData(IEnumerable<T> data)
    {
        _allData = data?.ToList() ?? new List<T>();
        _activeFilters.Clear();
        _filteredData = new List<T>(_allData);
        _currentPage = 1;
        UpdatePagedData();
        OnStateChanged();
    }

    public void ApplyFilter(Func<T, bool> filterPredicate)
    {
        if (filterPredicate == null) return;

        _activeFilters.Clear();
        _activeFilters.Add(filterPredicate);
        ApplyAllFilters();
    }

    public void ApplyFilters(params Func<T, bool>[] filterPredicates)
    {
        if (filterPredicates == null || filterPredicates.Length == 0)
        {
            ClearFilters();
            return;
        }

        _activeFilters = filterPredicates.Where(f => f != null).ToList();
        ApplyAllFilters();
    }

    private void ApplyAllFilters()
    {
        var filtered = _allData.AsEnumerable();

        foreach (var filter in _activeFilters)
        {
            filtered = filtered.Where(filter);
        }

        _filteredData = filtered.ToList();
        _currentPage = 1; // Reset to first page when filters change
        UpdatePagedData();
        OnStateChanged();
    }

    public void ClearFilters()
    {
        _activeFilters.Clear();
        _filteredData = new List<T>(_allData);
        _currentPage = 1;
        UpdatePagedData();
        OnStateChanged();
    }

    public void ChangePageSize(int newPageSize)
    {
        if (newPageSize <= 0) return;

        _pageSize = newPageSize;
        _currentPage = 1;
        UpdatePagedData();
        OnStateChanged();
    }

    public bool GoToPage(int pageNumber)
    {
        if (pageNumber < 1 || pageNumber > TotalPages || pageNumber == _currentPage)
            return false;

        _currentPage = pageNumber;
        UpdatePagedData();
        OnStateChanged();
        return true;
    }

    public void GoToFirstPage()
    {
        if (_currentPage != 1)
        {
            _currentPage = 1;
            UpdatePagedData();
            OnStateChanged();
        }
    }

    public void GoToLastPage()
    {
        var lastPage = TotalPages;
        if (_currentPage != lastPage)
        {
            _currentPage = lastPage;
            UpdatePagedData();
            OnStateChanged();
        }
    }

    public bool GoToPreviousPage()
    {
        if (!HasPreviousPage) return false;
        
        _currentPage--;
        UpdatePagedData();
        OnStateChanged();
        return true;
    }

    public bool GoToNextPage()
    {
        if (!HasNextPage) return false;
        
        _currentPage++;
        UpdatePagedData();
        OnStateChanged();
        return true;
    }

    public (int start, int end) GetPagerRange(int visiblePages = 5)
    {
        var totalPages = TotalPages;
        
        if (totalPages <= visiblePages)
            return (1, totalPages);

        var halfVisible = visiblePages / 2;
        var start = Math.Max(1, _currentPage - halfVisible);
        var end = Math.Min(totalPages, start + visiblePages - 1);

        // Adjust start if we're near the end
        if (end == totalPages)
            start = Math.Max(1, end - visiblePages + 1);

        return (start, end);
    }

    private void UpdatePagedData()
    {
        var startIndex = (_currentPage - 1) * _pageSize;
        _pagedData = _filteredData.Skip(startIndex).Take(_pageSize).ToList();
    }

    protected virtual void OnStateChanged()
    {
        StateChanged?.Invoke(this, EventArgs.Empty);
    }
}
