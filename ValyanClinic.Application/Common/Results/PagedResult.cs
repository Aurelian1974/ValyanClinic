namespace ValyanClinic.Application.Common.Results;

/// <summary>
/// Paged result pentru liste cu paginare
/// </summary>
public class PagedResult<T> : Result<IEnumerable<T>>
{
    public int CurrentPage { get; private set; }
    public int PageSize { get; private set; }
    public int TotalCount { get; private set; }
    public int TotalPages { get; private set; }
    public bool HasPreviousPage => CurrentPage > 1;
    public bool HasNextPage => CurrentPage < TotalPages;

        // Additional optional metadata container (e.g., filter options, statistics)
        public object? MetaData { get; set; }

        protected PagedResult(
            IEnumerable<T> items,
            int currentPage,
            int pageSize,
            int totalCount,
            string? successMessage = null)
            : base(true, items, new List<string>(), successMessage)
        {
            CurrentPage = currentPage;
            PageSize = pageSize;
            TotalCount = totalCount;
            TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
        }

    // Original signature retained for backward compatibility
    public static PagedResult<T> Success(
        IEnumerable<T> items,
        int currentPage,
        int pageSize,
        int totalCount,
        string? message = null)
    {
        return new PagedResult<T>(items, currentPage, pageSize, totalCount, message);
    }

    // NEW: explicit method to return metadata along with paged result
    public static PagedResult<T> SuccessWithMeta(
        IEnumerable<T> items,
        int currentPage,
        int pageSize,
        int totalCount,
        object? metaData,
        string? message = null)
    {
        var result = new PagedResult<T>(items, currentPage, pageSize, totalCount, message);
        result.MetaData = metaData;
        return result;
    }

    protected PagedResult(List<string> errors)
        : base(false, Enumerable.Empty<T>(), errors)
    {
        CurrentPage = 1;
        PageSize = 10;
        TotalCount = 0;
        TotalPages = 0;
    }

    public new static PagedResult<T> Failure(string error)
    {
        return new PagedResult<T>(new List<string> { error });
    }

    public new static PagedResult<T> Failure(List<string> errors)
    {
        return new PagedResult<T>(errors);
    }
}
