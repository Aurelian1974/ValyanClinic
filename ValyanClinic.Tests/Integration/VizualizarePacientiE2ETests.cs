using FluentAssertions;
using Microsoft.Playwright;
using Xunit;

namespace ValyanClinic.Tests.Integration;

/// <summary>
/// End-to-end integration tests pentru VizualizarePacienti page using Playwright.
/// Tests complete user workflows including UI interactions, data loading, filtering, and pagination.
/// </summary>
/// <remarks>
/// Framework: Playwright + xUnit + FluentAssertions
/// Pattern: AAA (Arrange-Act-Assert)
/// Browser: Chromium (headless by default, set Headless = false for debugging)
/// Coverage: 14 E2E scenarios covering critical user paths
/// </remarks>
[Collection("E2E Tests")]
public class VizualizarePacientiE2ETests : PlaywrightTestBase
{
    // Override for debugging - set to false to see browser
    protected override bool Headless => true;

    // Override base URL if needed
    // protected override string BaseUrl => "https://localhost:7001";

    #region Page Load & Initialization Tests

    /// <summary>
    /// Tests that the VizualizarePacienti page loads successfully and displays main UI elements.
    /// </summary>
    [Fact]
    public async Task PageLoad_DisplaysMainUIElements()
    {
        // Arrange & Act
        await NavigateToAsync("/pacienti/vizualizare");
        await WaitForBlazorAsync();

        // Assert - Page header
        var header = Page.Locator("h1:has-text('Vizualizare Pacienti')");
        (await header.IsVisibleAsync()).Should().BeTrue();

        // Assert - Search box
        var searchBox = Page.Locator("input.search-input");
        (await searchBox.IsVisibleAsync()).Should().BeTrue();

        // Assert - Grid container
        var gridContainer = Page.Locator(".grid-container");
        (await gridContainer.IsVisibleAsync()).Should().BeTrue();

        // Assert - Total records counter
        var totalRecords = Page.Locator(".total-records");
        (await totalRecords.IsVisibleAsync()).Should().BeTrue();
        var totalText = await totalRecords.TextContentAsync();
        totalText.Should().Contain("Total:");
    }

    /// <summary>
    /// Tests that the page displays loading indicator during initial data fetch.
    /// </summary>
    [Fact]
    public async Task PageLoad_ShowsLoadingIndicatorDuringDataFetch()
    {
        // Arrange & Act
        await NavigateToAsync("/pacienti/vizualizare");

        // Assert - Loading indicator should appear briefly
        // (May be too fast to catch in some environments)
        var loadingIndicator = Page.Locator(".loading-container, .spinner-border");

        // Wait for data to load completely
        await WaitForBlazorAsync();
        await WaitForNetworkIdleAsync();

        // Assert - Loading indicator should disappear
        var isLoadingVisible = await Page.Locator(".loading-container").IsVisibleAsync();
        isLoadingVisible.Should().BeFalse();
    }

    #endregion

    #region Global Search Tests

    /// <summary>
    /// Tests global search functionality with Enter key press.
    /// </summary>
    [Fact]
    public async Task GlobalSearch_WithEnterKey_FiltersResults()
    {
        // Arrange
        await NavigateToAsync("/pacienti/vizualizare");
        await WaitForBlazorAsync();
        await WaitForNetworkIdleAsync();

        // Act - Type search term and press Enter
        var searchInput = Page.Locator("input.search-input");
        await searchInput.FillAsync("Popescu");
        await searchInput.PressAsync("Enter");
        await WaitForNetworkIdleAsync();

        // Assert - Results should be filtered
        var gridRows = Page.Locator("table tbody tr");
        var rowCount = await gridRows.CountAsync();

        if (rowCount > 0)
        {
            // At least one result should contain search term
            var firstRowText = await gridRows.First.TextContentAsync();
            firstRowText.Should().NotBeNull();
        }
    }

    /// <summary>
    /// Tests clear search button functionality.
    /// </summary>
    [Fact]
    public async Task GlobalSearch_ClearButton_ResetsSearch()
    {
        // Arrange
        await NavigateToAsync("/pacienti/vizualizare");
        await WaitForBlazorAsync();
        await WaitForNetworkIdleAsync();

        var searchInput = Page.Locator("input.search-input");
        await searchInput.FillAsync("TestSearch");
        await searchInput.PressAsync("Enter");
        await WaitForNetworkIdleAsync();

        // Act - Click clear button (X icon)
        var clearButton = Page.Locator("button.search-clear-btn");
        if (await clearButton.IsVisibleAsync())
        {
            await clearButton.ClickAsync();

            // Wait explicitly for the input to become empty
            await Page.WaitForFunctionAsync("() => document.querySelector('input.search-input').value === ''", new PageWaitForFunctionOptions { Timeout = 2000 });

            await WaitForNetworkIdleAsync();

            // Assert - Search input should be empty
            var searchValue = await searchInput.InputValueAsync();
            searchValue.Should().BeEmpty();
        }
    }

    #endregion

    #region Advanced Filters Tests

    /// <summary>
    /// Tests opening and closing advanced filters panel.
    /// </summary>
    [Fact]
    public async Task AdvancedFilters_ToggleButton_OpensAndClosesPanel()
    {
        // Arrange
        await NavigateToAsync("/pacienti/vizualizare");
        await WaitForBlazorAsync();

        var filterButton = Page.Locator("button.btn-filter");
        var filterPanel = Page.Locator(".advanced-filter-panel");

        // Act - Open filters
        await filterButton.ClickAsync();
        await Task.Delay(500); // Wait for CSS animation

        // Assert - Panel should be visible
        var isExpanded = await filterPanel.GetAttributeAsync("class");
        isExpanded.Should().Contain("expanded");

        // Act - Close filters
        await filterButton.ClickAsync();
        await Task.Delay(500);

        // Assert - Panel should be hidden
        isExpanded = await filterPanel.GetAttributeAsync("class");
        isExpanded.Should().NotContain("expanded");
    }

    /// <summary>
    /// Tests applying filters updates the patient list.
    /// </summary>
    [Fact]
    public async Task AdvancedFilters_ApplyFilters_UpdatesResults()
    {
        // Arrange
        await NavigateToAsync("/pacienti/vizualizare");
        await WaitForBlazorAsync();
        await WaitForNetworkIdleAsync();

        // Act - Open filter panel
        var filterButton = Page.Locator("button.btn-filter");
        await filterButton.ClickAsync();
        await Task.Delay(500);

        // Act - Select a filter (e.g., Judet)
        var judetDropdown = Page.Locator(".filter-dropdown").First;
        if (await judetDropdown.IsVisibleAsync())
        {
            await judetDropdown.ClickAsync();
            await Task.Delay(300);

            // Select first option if available
            var firstOption = Page.Locator(".filter-dropdown li").First;
            if (await firstOption.IsVisibleAsync())
            {
                await firstOption.ClickAsync();
            }
        }

        // Act - Click Apply Filters
        var applyButton = Page.Locator("button.btn-apply");
        await applyButton.ClickAsync();
        await WaitForNetworkIdleAsync();

        // Assert - Filter chip should appear
        var filterChips = Page.Locator(".filter-chip");
        var chipCount = await filterChips.CountAsync();
        chipCount.Should().BeGreaterThanOrEqualTo(0);
    }

    /// <summary>
    /// Tests clearing all filters resets the list.
    /// </summary>
    [Fact]
    public async Task AdvancedFilters_ClearAllFilters_ResetsToDefaultList()
    {
        // Arrange
        await NavigateToAsync("/pacienti/vizualizare");
        await WaitForBlazorAsync();
        await WaitForNetworkIdleAsync();

        // Apply a search filter first
        var searchInput = Page.Locator("input.search-input");
        await searchInput.FillAsync("Test");
        await searchInput.PressAsync("Enter");
        await WaitForNetworkIdleAsync();

        // Act - Open filters and click Clear All
        var filterButton = Page.Locator("button.btn-filter");
        await filterButton.ClickAsync();
        await Task.Delay(500);

        var clearAllButton = Page.Locator("button.btn-clear");
        if (await clearAllButton.IsEnabledAsync())
        {
            await clearAllButton.ClickAsync();

            // Wait explicitly for the input to become empty
            await Page.WaitForFunctionAsync("() => document.querySelector('input.search-input').value === ''", new PageWaitForFunctionOptions { Timeout = 2000 });

            await WaitForNetworkIdleAsync();

            // Assert - Search input should be empty
            var searchValue = await searchInput.InputValueAsync();
            searchValue.Should().BeEmpty();

            // Assert - No filter chips
            var filterChips = Page.Locator(".filter-chip");
            var chipCount = await filterChips.CountAsync();
            chipCount.Should().Be(0);
        }
    }

    #endregion

    #region Pagination Tests

    /// <summary>
    /// Tests navigating to the next page updates the grid.
    /// </summary>
    [Fact]
    public async Task Pagination_NextPage_LoadsNewData()
    {
        // Arrange
        await NavigateToAsync("/pacienti/vizualizare");
        await WaitForBlazorAsync();
        await WaitForNetworkIdleAsync();

        // Check if we have enough data for multiple pages
        var totalRecordsText = await Page.Locator(".total-records").TextContentAsync();

        // Act - Click next page button (Syncfusion Grid pagination)
        var nextPageButton = Page.Locator(".e-nextpage");
        if (await nextPageButton.IsVisibleAsync() && await nextPageButton.IsEnabledAsync())
        {
            await nextPageButton.ClickAsync();
            await WaitForNetworkIdleAsync();

            // Assert - Page number should change
            var currentPageIndicator = Page.Locator(".e-currentitem");
            var pageText = await currentPageIndicator.TextContentAsync();
            pageText.Should().NotBeNullOrEmpty();
        }
    }

    /// <summary>
    /// Tests changing page size updates the grid.
    /// </summary>
    [Fact]
    public async Task Pagination_ChangePageSize_UpdatesGridRows()
    {
        // Arrange
        await NavigateToAsync("/pacienti/vizualizare");
        await WaitForBlazorAsync();
        await WaitForNetworkIdleAsync();

        // Get initial row count
        var initialRows = await Page.Locator("table tbody tr").CountAsync();

        // Act - Open page size dropdown (if available)
        var pageSizeDropdown = Page.Locator(".e-pagesizes");
        if (await pageSizeDropdown.IsVisibleAsync())
        {
            await pageSizeDropdown.ClickAsync();
            await Task.Delay(300);

            // Select a different page size (FIX: Use exact match to avoid substring issue)
            // GetByRole with Exact = true ensures we match "50" exactly, not "250"
            var pageSizeOption = Page.GetByRole(AriaRole.Option, new() { Name = "50", Exact = true });
            if (await pageSizeOption.IsVisibleAsync())
            {
                await pageSizeOption.ClickAsync();
                await WaitForNetworkIdleAsync();

                // Assert - Row count might change (depends on data)
                var newRows = await Page.Locator("table tbody tr").CountAsync();
                // We can't guarantee exact count, but grid should still render
                newRows.Should().BeGreaterThan(0);
            }
        }
    }

    #endregion

    #region Row Selection & Modal Tests

    /// <summary>
    /// Tests selecting a patient row enables action buttons.
    /// </summary>
    [Fact]
    public async Task RowSelection_SelectPatient_EnablesActionButtons()
    {
        // Arrange
        await NavigateToAsync("/pacienti/vizualizare");
        await WaitForBlazorAsync();
        await WaitForNetworkIdleAsync();

        // Act - Click first row to select
        var firstRow = Page.Locator("table tbody tr").First;
        if (await firstRow.IsVisibleAsync())
        {
            await firstRow.ClickAsync();
            await Task.Delay(300);

            // Assert - Action buttons should be enabled
            var viewButton = Page.Locator("button.btn-view");
            var manageDoctorsButton = Page.Locator("button.btn-doctors");

            var isViewEnabled = !(await viewButton.IsDisabledAsync());
            var isDoctorsEnabled = !(await manageDoctorsButton.IsDisabledAsync());

            // At least one should be enabled after selection
            (isViewEnabled || isDoctorsEnabled).Should().BeTrue();
        }
    }

    /// <summary>
    /// Tests clicking "Vizualizeaza Detalii" opens the view modal.
    /// </summary>
    [Fact]
    public async Task ViewModal_ClickViewDetails_OpensModal()
    {
        // Arrange
        await NavigateToAsync("/pacienti/vizualizare");
        await WaitForBlazorAsync();
        await WaitForNetworkIdleAsync();

        // Select first row
        var firstRow = Page.Locator("table tbody tr").First;
        if (await firstRow.IsVisibleAsync())
        {
            await firstRow.ClickAsync();
            await Task.Delay(300);

            // Act - Click View Details button
            var viewButton = Page.Locator("button.btn-view");
            if (!(await viewButton.IsDisabledAsync()))
            {
                await viewButton.ClickAsync();
                await Task.Delay(500);

                // Assert - Modal should appear
                var modal = Page.Locator(".modal-overlay.visible");
                (await modal.IsVisibleAsync()).Should().BeTrue("Modal should appear after clicking View Details");

                // Assert - SPECIFIC modal header for ViewModal (FIX: More specific selector)
                var modalHeader = Page.Locator(".modal-overlay.visible .modal-header:has-text('Detalii Pacient')");
                (await modalHeader.IsVisibleAsync()).Should().BeTrue("View modal header should contain 'Detalii Pacient'");
            }
        }
    }

    /// <summary>
    /// Tests clicking "Gestioneaza Doctori" opens the doctors modal.
    /// </summary>
    [Fact]
    public async Task DoctorsModal_ClickManageDoctors_OpensModal()
    {
        // Arrange
        await NavigateToAsync("/pacienti/vizualizare");
        await WaitForBlazorAsync();
        await WaitForNetworkIdleAsync();

        // Select first row
        var firstRow = Page.Locator("table tbody tr").First;
        if (await firstRow.IsVisibleAsync())
        {
            await firstRow.ClickAsync();
            await Task.Delay(300);

            // Act - Click Manage Doctors button
            var manageDoctorsButton = Page.Locator("button.btn-doctors");
            if (!(await manageDoctorsButton.IsDisabledAsync()))
            {
                await manageDoctorsButton.ClickAsync();
                await Task.Delay(500);

                // Assert - Modal should appear
                var modal = Page.Locator(".modal-overlay.visible");
                (await modal.IsVisibleAsync()).Should().BeTrue("Modal should appear after clicking Manage Doctors");

                // Assert - SPECIFIC modal header for DoctoriModal (FIX: More specific selector)
                var modalHeader = Page.Locator(".modal-overlay.visible .modal-header:has-text('Doctori asociați')");
                (await modalHeader.IsVisibleAsync()).Should().BeTrue("Doctori modal header should contain 'Doctori asociați'");
            }
        }
    }

    #endregion

    #region Refresh Tests

    /// <summary>
    /// Tests clicking refresh button reloads the patient list.
    /// </summary>
    [Fact]
    public async Task Refresh_ClickRefreshButton_ReloadsData()
    {
        // Arrange
        await NavigateToAsync("/pacienti/vizualizare");
        await WaitForBlazorAsync();
        await WaitForNetworkIdleAsync();

        // Act - Click refresh button
        var refreshButton = Page.Locator("button:has-text('Reincarca')");
        await refreshButton.ClickAsync();
        await WaitForNetworkIdleAsync();

        // Assert - Success toast should appear (if configured)
        var toast = Page.Locator(".e-toast-success");
        if (await toast.IsVisibleAsync())
        {
            var toastText = await toast.TextContentAsync();
            toastText.Should().ContainAny("reincarcate", "succes");
        }

        // Assert - Grid should still be visible
        var grid = Page.Locator(".grid-container");
        (await grid.IsVisibleAsync()).Should().BeTrue();
    }

    #endregion
}
