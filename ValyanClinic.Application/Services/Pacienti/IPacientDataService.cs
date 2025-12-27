using ValyanClinic.Application.Common.Results;
using ValyanClinic.Application.Features.PacientManagement.Queries.GetPacientList;

namespace ValyanClinic.Application.Services.Pacienti;

/// <summary>
/// Service for managing patient data operations (filtering, sorting, pagination).
/// Encapsulates business logic for patient list management, separating it from UI concerns.
/// </summary>
/// <remarks>
/// <b>Purpose:</b> Extract complex business logic from Blazor components to improve testability.
/// <b>Pattern:</b> Service layer between UI and MediatR handlers.
/// <b>Benefits:</b>
/// - Testable without UI dependencies (no Syncfusion, no JSInterop mocking)
/// - Reusable across multiple components
/// - Centralized business logic for patient data operations
/// - Easy to mock in component tests
/// </remarks>
public interface IPacientDataService
{
    /// <summary>
    /// Loads paged patient data with filters and sorting options.
    /// </summary>
    /// <param name="filters">Filter criteria (search text, judet, asigurat, activ)</param>
    /// <param name="pagination">Pagination options (page number, page size)</param>
    /// <param name="sorting">Sort options (column, direction)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Result containing paged patient data or errors</returns>
    Task<Result<PagedPacientData>> LoadPagedDataAsync(
        PacientFilters filters,
        PaginationOptions pagination,
        SortOptions sorting,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Loads filter options (unique judete, etc.) from server for dropdown population.
    /// Results are cached for performance.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Result containing filter options or errors</returns>
    Task<Result<PacientFilterOptions>> LoadFilterOptionsAsync(
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Invalidates the filter options cache (call when patient data changes).
    /// </summary>
    void InvalidateFilterOptionsCache();
}

/// <summary>
/// Filter criteria for patient list queries.
/// </summary>
public record PacientFilters
{
    /// <summary>
    /// Global search text (searches across nume, prenume, CNP, telefon, email).
    /// </summary>
    public string? SearchText { get; init; }

    /// <summary>
    /// Filter by judet.
    /// </summary>
    public string? Judet { get; init; }

    /// <summary>
    /// Filter by asigurat status (true = asigurat, false = neasigurat, null = all).
    /// </summary>
    public bool? Asigurat { get; init; }

    /// <summary>
    /// Filter by active status (true = activ, false = inactiv, null = all).
    /// </summary>
    public bool? Activ { get; init; }
}

/// <summary>
/// Pagination options for server-side paging.
/// </summary>
public record PaginationOptions
{
    /// <summary>
    /// Current page number (1-based).
    /// </summary>
    public int PageNumber { get; init; } = 1;

    /// <summary>
    /// Number of items per page.
    /// </summary>
    public int PageSize { get; init; } = 20;
}

/// <summary>
/// Sort options for data ordering.
/// </summary>
public record SortOptions
{
    /// <summary>
    /// Column name to sort by (e.g., "Nume", "Prenume", "Data_Nasterii").
    /// </summary>
    public string Column { get; init; } = "Nume";

    /// <summary>
    /// Sort direction ("ASC" or "DESC").
    /// </summary>
    public string Direction { get; init; } = "ASC";
}

/// <summary>
/// Result containing paged patient data with metadata.
/// </summary>
public record PagedPacientData
{
    /// <summary>
    /// List of patients for the current page.
    /// </summary>
    public List<PacientListDto> Items { get; init; } = new();

    /// <summary>
    /// Total count of records matching the filters (across all pages).
    /// </summary>
    public int TotalCount { get; init; }

    /// <summary>
    /// Current page number.
    /// </summary>
    public int CurrentPage { get; init; }

    /// <summary>
    /// Page size used for this query.
    /// </summary>
    public int PageSize { get; init; }

    /// <summary>
    /// Total number of pages.
    /// </summary>
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);

    /// <summary>
    /// Indicates if there is a previous page.
    /// </summary>
    public bool HasPreviousPage => CurrentPage > 1;

    /// <summary>
    /// Indicates if there is a next page.
    /// </summary>
    public bool HasNextPage => CurrentPage < TotalPages;
}

/// <summary>
/// Filter options loaded from server for dropdown population.
/// </summary>
public record PacientFilterOptions
{
    /// <summary>
    /// List of unique judete available in the system.
    /// </summary>
    public List<string> Judete { get; init; } = new();

    /// <summary>
    /// Asigurat options (static: "Da", "Nu").
    /// </summary>
    public List<FilterOption> AsiguratOptions { get; init; } = new()
    {
        new FilterOption { Value = "true", Text = "Da" },
        new FilterOption { Value = "false", Text = "Nu" }
    };

    /// <summary>
    /// Status options (static: "Activ", "Inactiv").
    /// </summary>
    public List<FilterOption> StatusOptions { get; init; } = new()
    {
        new FilterOption { Value = "true", Text = "Activ" },
        new FilterOption { Value = "false", Text = "Inactiv" }
    };
}

/// <summary>
/// Filter option for dropdowns.
/// </summary>
public record FilterOption
{
    public string Value { get; init; } = string.Empty;
    public string Text { get; init; } = string.Empty;
}
