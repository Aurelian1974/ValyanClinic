using Microsoft.AspNetCore.Components;

namespace ValyanClinic.Components.Shared;

/// <summary>
/// Reusable pagination control component for server-side pagination.
/// Provides navigation buttons, page info, and page size selector.
/// </summary>
public partial class PaginationControl : ComponentBase
{
    /// <summary>
    /// Current page number (1-based)
    /// </summary>
    [Parameter]
    public int CurrentPage { get; set; } = 1;

    /// <summary>
    /// Number of records per page
    /// </summary>
    [Parameter]
    public int PageSize { get; set; } = 20;

    /// <summary>
    /// Total number of records across all pages
    /// </summary>
    [Parameter]
    public int TotalRecords { get; set; } = 0;

    /// <summary>
    /// Available page size options
    /// </summary>
    [Parameter]
    public int[] PageSizeOptions { get; set; } = new int[] { 10, 20, 50, 100, 250 };

    /// <summary>
    /// Callback when user navigates to first page
    /// </summary>
    [Parameter]
    public EventCallback OnFirstPage { get; set; }

    /// <summary>
    /// Callback when user navigates to previous page
    /// </summary>
    [Parameter]
    public EventCallback OnPreviousPage { get; set; }

    /// <summary>
    /// Callback when user navigates to next page
    /// </summary>
    [Parameter]
    public EventCallback OnNextPage { get; set; }

    /// <summary>
    /// Callback when user navigates to last page
    /// </summary>
    [Parameter]
    public EventCallback OnLastPage { get; set; }

    /// <summary>
    /// Callback when user changes page size
    /// </summary>
    [Parameter]
    public EventCallback<int> OnPageSizeChanged { get; set; }

    /// <summary>
    /// Total number of pages
    /// </summary>
    private int TotalPages => TotalRecords > 0 ? (int)Math.Ceiling((double)TotalRecords / PageSize) : 0;

    /// <summary>
    /// Internal property for page size selection binding
    /// </summary>
    private int SelectedPageSize
    {
        get => PageSize;
        set => PageSize = value;
    }

    /// <summary>
    /// Handle page size change event
    /// </summary>
    private async Task HandlePageSizeChanged()
    {
        if (OnPageSizeChanged.HasDelegate)
        {
            await OnPageSizeChanged.InvokeAsync(PageSize);
        }
    }
}
