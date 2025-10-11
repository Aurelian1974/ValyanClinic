namespace ValyanClinic.Services.DataGrid;

/// <summary>
/// Implementare service pentru filtrare de date
/// </summary>
public class DataFilterService : IDataFilterService
{
    public IEnumerable<T> ApplyGlobalSearch<T>(
        IEnumerable<T> data,
        string? searchText,
        params Func<T, string?>[] fieldSelectors)
    {
        if (string.IsNullOrWhiteSpace(searchText) || fieldSelectors == null || fieldSelectors.Length == 0)
            return data;

        var searchLower = searchText.ToLower().Trim();

        return data.Where(item =>
            fieldSelectors.Any(selector =>
            {
                var fieldValue = selector(item);
                return fieldValue?.ToLower().Contains(searchLower) == true;
            })
        );
    }

    public IEnumerable<T> ApplyFieldFilter<T>(
        IEnumerable<T> data,
        string? filterValue,
        Func<T, string?> fieldSelector)
    {
        if (string.IsNullOrWhiteSpace(filterValue) || fieldSelector == null)
            return data;

        return data.Where(item =>
        {
            var fieldValue = fieldSelector(item);
            return string.Equals(fieldValue, filterValue, StringComparison.OrdinalIgnoreCase);
        });
    }

    public IFilterBuilder<T> CreateFilterBuilder<T>(IEnumerable<T> data)
    {
        return new FilterBuilder<T>(data, this);
    }

    private class FilterBuilder<T> : IFilterBuilder<T>
    {
        private readonly IEnumerable<T> _data;
        private readonly DataFilterService _service;
        private readonly List<Func<T, bool>> _predicates = new();

        public FilterBuilder(IEnumerable<T> data, DataFilterService service)
        {
            _data = data ?? Enumerable.Empty<T>();
            _service = service;
        }

        public IFilterBuilder<T> WithGlobalSearch(string? searchText, params Func<T, string?>[] fieldSelectors)
        {
            if (!string.IsNullOrWhiteSpace(searchText) && fieldSelectors != null && fieldSelectors.Length > 0)
            {
                var searchLower = searchText.ToLower().Trim();
                _predicates.Add(item =>
                    fieldSelectors.Any(selector =>
                    {
                        var fieldValue = selector(item);
                        return fieldValue?.ToLower().Contains(searchLower) == true;
                    })
                );
            }
            return this;
        }

        public IFilterBuilder<T> WithFieldFilter(string? filterValue, Func<T, string?> fieldSelector)
        {
            if (!string.IsNullOrWhiteSpace(filterValue) && fieldSelector != null)
            {
                _predicates.Add(item =>
                {
                    var fieldValue = fieldSelector(item);
                    return string.Equals(fieldValue, filterValue, StringComparison.OrdinalIgnoreCase);
                });
            }
            return this;
        }

        public IFilterBuilder<T> WithCustomFilter(Func<T, bool> predicate)
        {
            if (predicate != null)
            {
                _predicates.Add(predicate);
            }
            return this;
        }

        public IEnumerable<T> Build()
        {
            var result = _data;
            foreach (var predicate in _predicates)
            {
                result = result.Where(predicate);
            }
            return result;
        }

        public Func<T, bool> GetCombinedPredicate()
        {
            if (_predicates.Count == 0)
                return _ => true;

            return item => _predicates.All(predicate => predicate(item));
        }
    }
}
