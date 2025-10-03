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
    
    protected PagedResult(List<string> errors)
        : base(false, Enumerable.Empty<T>(), errors)
    {
        CurrentPage = 1;
        PageSize = 10;
        TotalCount = 0;
        TotalPages = 0;
    }
    
    public static PagedResult<T> Success(
        IEnumerable<T> items,
        int currentPage,
        int pageSize,
        int totalCount,
        string? message = null)
    {
        return new PagedResult<T>(items, currentPage, pageSize, totalCount, message);
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
