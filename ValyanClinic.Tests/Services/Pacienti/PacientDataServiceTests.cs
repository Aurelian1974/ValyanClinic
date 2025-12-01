using FluentAssertions;
using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using ValyanClinic.Application.Common.Results;
using ValyanClinic.Application.Features.PacientManagement.Queries.GetPacientList;
using ValyanClinic.Application.Services.Pacienti;
using Xunit;

namespace ValyanClinic.Tests.Services.Pacienti;

/// <summary>
/// Unit tests for PacientDataService.
/// Tests business logic extraction from UI components for improved testability.
/// </summary>
/// <remarks>
/// Framework: xUnit + FluentAssertions + Moq
/// Pattern: AAA (Arrange-Act-Assert)
/// Coverage: All public methods, success/failure scenarios, edge cases
/// </remarks>
public class PacientDataServiceTests
{
    private readonly Mock<IMediator> _mockMediator;
    private readonly Mock<ILogger<PacientDataService>> _mockLogger;
    private readonly PacientDataService _service;

    public PacientDataServiceTests()
    {
        _mockMediator = new Mock<IMediator>();
        _mockLogger = new Mock<ILogger<PacientDataService>>();
        _service = new PacientDataService(_mockMediator.Object, _mockLogger.Object);
    }

    #region Helper Methods

    /// <summary>
    /// Creates mock patient data for testing.
    /// </summary>
    private List<PacientListDto> CreateMockPacienti(int count, string? judet = null)
    {
        var pacienti = new List<PacientListDto>();
        for (int i = 1; i <= count; i++)
        {
            pacienti.Add(new PacientListDto
            {
                Id = Guid.NewGuid(),
                Cod_Pacient = $"PAC{i:D6}",
                Nume = $"Pacient{i}",
                Prenume = $"Test{i}",
                NumeComplet = $"Pacient{i} Test{i}",
                CNP = $"{1234567890000 + i}",
                Data_Nasterii = DateTime.Now.AddYears(-30),
                Varsta = 30,
                Sex = i % 2 == 0 ? "M" : "F",
                Telefon = $"0712345{i:D3}",
                Email = $"pacient{i}@test.com",
                Localitate = $"Localitate{i}",
                Judet = judet ?? $"Judet{i % 5}",
                Asigurat = i % 2 == 0,
                Ultima_Vizita = DateTime.Now.AddDays(-i),
                Nr_Total_Vizite = i * 2,
                Activ = true
            });
        }
        return pacienti;
    }

    /// <summary>
    /// Creates a mock PagedResult for testing.
    /// </summary>
    private PagedResult<PacientListDto> CreateMockPagedResult(
        List<PacientListDto> data,
        int totalCount,
        int currentPage,
        int pageSize)
    {
        return PagedResult<PacientListDto>.Success(data, currentPage, pageSize, totalCount);
    }

    #endregion

    #region LoadPagedDataAsync Tests

    /// <summary>
    /// Tests successful data loading with no filters.
    /// </summary>
    [Fact]
    public async Task LoadPagedDataAsync_NoFilters_ReturnsSuccessWithData()
    {
        // Arrange
        var mockData = CreateMockPacienti(20);
        var pagedResult = CreateMockPagedResult(mockData, 100, 1, 20);

        _mockMediator
            .Setup(m => m.Send(It.IsAny<GetPacientListQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<PagedResult<PacientListDto>>.Success(pagedResult));

        var filters = new PacientFilters();
        var pagination = new PaginationOptions { PageNumber = 1, PageSize = 20 };
        var sorting = new SortOptions { Column = "Nume", Direction = "ASC" };

        // Act
        var result = await _service.LoadPagedDataAsync(filters, pagination, sorting);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().HaveCount(20);
        result.Value.TotalCount.Should().Be(100);
        result.Value.CurrentPage.Should().Be(1);
        result.Value.PageSize.Should().Be(20);
        result.Value.TotalPages.Should().Be(5);
        result.Value.HasPreviousPage.Should().BeFalse();
        result.Value.HasNextPage.Should().BeTrue();

        _mockMediator.Verify(
            m => m.Send(
                It.Is<GetPacientListQuery>(q =>
                    q.PageNumber == 1 &&
                    q.PageSize == 20 &&
                    q.SortColumn == "Nume" &&
                    q.SortDirection == "ASC"),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    /// <summary>
    /// Tests data loading with search text filter.
    /// </summary>
    [Fact]
    public async Task LoadPagedDataAsync_WithSearchText_FiltersCorrectly()
    {
        // Arrange
        var mockData = CreateMockPacienti(5);
        var pagedResult = CreateMockPagedResult(mockData, 5, 1, 20);

        _mockMediator
            .Setup(m => m.Send(It.IsAny<GetPacientListQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<PagedResult<PacientListDto>>.Success(pagedResult));

        var filters = new PacientFilters { SearchText = "Popescu" };
        var pagination = new PaginationOptions { PageNumber = 1, PageSize = 20 };
        var sorting = new SortOptions { Column = "Nume", Direction = "ASC" };

        // Act
        var result = await _service.LoadPagedDataAsync(filters, pagination, sorting);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().HaveCount(5);
        result.Value.TotalCount.Should().Be(5);

        _mockMediator.Verify(
            m => m.Send(
                It.Is<GetPacientListQuery>(q => q.SearchText == "Popescu"),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    /// <summary>
    /// Tests data loading with multiple filters (judet, asigurat, activ).
    /// </summary>
    [Fact]
    public async Task LoadPagedDataAsync_WithMultipleFilters_AppliesAllFilters()
    {
        // Arrange
        var mockData = CreateMockPacienti(10, "Bucuresti");
        var pagedResult = CreateMockPagedResult(mockData, 10, 1, 20);

        _mockMediator
            .Setup(m => m.Send(It.IsAny<GetPacientListQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<PagedResult<PacientListDto>>.Success(pagedResult));

        var filters = new PacientFilters
        {
            SearchText = "Test",
            Judet = "Bucuresti",
            Asigurat = true,
            Activ = true
        };
        var pagination = new PaginationOptions { PageNumber = 1, PageSize = 20 };
        var sorting = new SortOptions { Column = "Nume", Direction = "ASC" };

        // Act
        var result = await _service.LoadPagedDataAsync(filters, pagination, sorting);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().HaveCount(10);

        _mockMediator.Verify(
            m => m.Send(
                It.Is<GetPacientListQuery>(q =>
                    q.SearchText == "Test" &&
                    q.Judet == "Bucuresti" &&
                    q.Asigurat == true &&
                    q.Activ == true),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    /// <summary>
    /// Tests pagination with page 2 request.
    /// </summary>
    [Fact]
    public async Task LoadPagedDataAsync_Page2_ReturnsCorrectPage()
    {
        // Arrange
        var mockData = CreateMockPacienti(20);
        var pagedResult = CreateMockPagedResult(mockData, 100, 2, 20);

        _mockMediator
            .Setup(m => m.Send(It.IsAny<GetPacientListQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<PagedResult<PacientListDto>>.Success(pagedResult));

        var filters = new PacientFilters();
        var pagination = new PaginationOptions { PageNumber = 2, PageSize = 20 };
        var sorting = new SortOptions { Column = "Nume", Direction = "ASC" };

        // Act
        var result = await _service.LoadPagedDataAsync(filters, pagination, sorting);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.CurrentPage.Should().Be(2);
        result.Value.HasPreviousPage.Should().BeTrue();
        result.Value.HasNextPage.Should().BeTrue();

        _mockMediator.Verify(
            m => m.Send(
                It.Is<GetPacientListQuery>(q => q.PageNumber == 2),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    /// <summary>
    /// Tests sorting by different columns and directions.
    /// </summary>
    [Fact]
    public async Task LoadPagedDataAsync_CustomSorting_AppliesSortOptions()
    {
        // Arrange
        var mockData = CreateMockPacienti(20);
        var pagedResult = CreateMockPagedResult(mockData, 20, 1, 20);

        _mockMediator
            .Setup(m => m.Send(It.IsAny<GetPacientListQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<PagedResult<PacientListDto>>.Success(pagedResult));

        var filters = new PacientFilters();
        var pagination = new PaginationOptions { PageNumber = 1, PageSize = 20 };
        var sorting = new SortOptions { Column = "Data_Nasterii", Direction = "DESC" };

        // Act
        var result = await _service.LoadPagedDataAsync(filters, pagination, sorting);

        // Assert
        result.IsSuccess.Should().BeTrue();

        _mockMediator.Verify(
            m => m.Send(
                It.Is<GetPacientListQuery>(q =>
                    q.SortColumn == "Data_Nasterii" &&
                    q.SortDirection == "DESC"),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    /// <summary>
    /// Tests that empty search text is converted to null.
    /// </summary>
    [Fact]
    public async Task LoadPagedDataAsync_EmptySearchText_ConvertsToNull()
    {
        // Arrange
        var mockData = CreateMockPacienti(20);
        var pagedResult = CreateMockPagedResult(mockData, 20, 1, 20);

        _mockMediator
            .Setup(m => m.Send(It.IsAny<GetPacientListQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<PagedResult<PacientListDto>>.Success(pagedResult));

        var filters = new PacientFilters { SearchText = "   " }; // Whitespace only
        var pagination = new PaginationOptions { PageNumber = 1, PageSize = 20 };
        var sorting = new SortOptions { Column = "Nume", Direction = "ASC" };

        // Act
        var result = await _service.LoadPagedDataAsync(filters, pagination, sorting);

        // Assert
        result.IsSuccess.Should().BeTrue();

        _mockMediator.Verify(
            m => m.Send(
                It.Is<GetPacientListQuery>(q => q.SearchText == null),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    /// <summary>
    /// Tests handling of MediatR failure result.
    /// </summary>
    [Fact]
    public async Task LoadPagedDataAsync_MediatRFailure_ReturnsFailure()
    {
        // Arrange
        _mockMediator
            .Setup(m => m.Send(It.IsAny<GetPacientListQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<PagedResult<PacientListDto>>.Failure("Database error"));

        var filters = new PacientFilters();
        var pagination = new PaginationOptions { PageNumber = 1, PageSize = 20 };
        var sorting = new SortOptions { Column = "Nume", Direction = "ASC" };

        // Act
        var result = await _service.LoadPagedDataAsync(filters, pagination, sorting);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain("Database error");
    }

    /// <summary>
    /// Tests handling of MediatR exception.
    /// </summary>
    [Fact]
    public async Task LoadPagedDataAsync_MediatRException_ReturnsFailure()
    {
        // Arrange
        _mockMediator
            .Setup(m => m.Send(It.IsAny<GetPacientListQuery>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Connection failed"));

        var filters = new PacientFilters();
        var pagination = new PaginationOptions { PageNumber = 1, PageSize = 20 };
        var sorting = new SortOptions { Column = "Nume", Direction = "ASC" };

        // Act
        var result = await _service.LoadPagedDataAsync(filters, pagination, sorting);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.FirstError.Should().Contain("Eroare la încărcarea datelor");
        result.FirstError.Should().Contain("Connection failed");
    }

    /// <summary>
    /// Tests cancellation token handling.
    /// </summary>
    [Fact]
    public async Task LoadPagedDataAsync_OperationCancelled_ReturnsFailure()
    {
        // Arrange
        _mockMediator
            .Setup(m => m.Send(It.IsAny<GetPacientListQuery>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new OperationCanceledException());

        var filters = new PacientFilters();
        var pagination = new PaginationOptions { PageNumber = 1, PageSize = 20 };
        var sorting = new SortOptions { Column = "Nume", Direction = "ASC" };

        // Act
        var result = await _service.LoadPagedDataAsync(filters, pagination, sorting);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.FirstError.Should().Contain("anulată");
    }

    #endregion

    #region LoadFilterOptionsAsync Tests

    /// <summary>
    /// Tests successful filter options loading.
    /// </summary>
    [Fact]
    public async Task LoadFilterOptionsAsync_Success_ReturnsUniqueJudete()
    {
        // Arrange
        var mockData = new List<PacientListDto>
        {
            new() { Judet = "Bucuresti" },
            new() { Judet = "Cluj" },
            new() { Judet = "Bucuresti" }, // Duplicate
            new() { Judet = "Timis" },
            new() { Judet = null } // Null judet
        };
        var pagedResult = CreateMockPagedResult(mockData, mockData.Count, 1, int.MaxValue);

        _mockMediator
            .Setup(m => m.Send(It.IsAny<GetPacientListQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<PagedResult<PacientListDto>>.Success(pagedResult));

        // Act
        var result = await _service.LoadFilterOptionsAsync();

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Judete.Should().HaveCount(3); // Unique, non-null
        result.Value.Judete.Should().Contain("Bucuresti");
        result.Value.Judete.Should().Contain("Cluj");
        result.Value.Judete.Should().Contain("Timis");
        result.Value.Judete.Should().BeInAscendingOrder();

        // Static options should be present
        result.Value.AsiguratOptions.Should().HaveCount(2);
        result.Value.StatusOptions.Should().HaveCount(2);

        _mockMediator.Verify(
            m => m.Send(
                It.Is<GetPacientListQuery>(q => q.PageSize == int.MaxValue),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    /// <summary>
    /// Tests filter options with empty data.
    /// </summary>
    [Fact]
    public async Task LoadFilterOptionsAsync_EmptyData_ReturnsEmptyJudete()
    {
        // Arrange
        var pagedResult = CreateMockPagedResult(new List<PacientListDto>(), 0, 1, int.MaxValue);

        _mockMediator
            .Setup(m => m.Send(It.IsAny<GetPacientListQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<PagedResult<PacientListDto>>.Success(pagedResult));

        // Act
        var result = await _service.LoadFilterOptionsAsync();

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Judete.Should().BeEmpty();
        result.Value.AsiguratOptions.Should().HaveCount(2);
        result.Value.StatusOptions.Should().HaveCount(2);
    }

    /// <summary>
    /// Tests filter options loading failure.
    /// </summary>
    [Fact]
    public async Task LoadFilterOptionsAsync_MediatRFailure_ReturnsFailure()
    {
        // Arrange
        _mockMediator
            .Setup(m => m.Send(It.IsAny<GetPacientListQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<PagedResult<PacientListDto>>.Failure("Database error"));

        // Act
        var result = await _service.LoadFilterOptionsAsync();

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain("Database error");
    }

    /// <summary>
    /// Tests filter options loading with exception.
    /// </summary>
    [Fact]
    public async Task LoadFilterOptionsAsync_Exception_ReturnsFailure()
    {
        // Arrange
        _mockMediator
            .Setup(m => m.Send(It.IsAny<GetPacientListQuery>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Network error"));

        // Act
        var result = await _service.LoadFilterOptionsAsync();

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.FirstError.Should().Contain("Eroare la încărcarea opțiunilor");
        result.FirstError.Should().Contain("Network error");
    }

    /// <summary>
    /// Tests filter options loading cancellation.
    /// </summary>
    [Fact]
    public async Task LoadFilterOptionsAsync_Cancelled_ReturnsFailure()
    {
        // Arrange
        _mockMediator
            .Setup(m => m.Send(It.IsAny<GetPacientListQuery>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new OperationCanceledException());

        // Act
        var result = await _service.LoadFilterOptionsAsync();

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.FirstError.Should().Contain("anulată");
    }

    #endregion

    #region Edge Cases & Integration Tests

    /// <summary>
    /// Tests that PagedPacientData metadata is calculated correctly.
    /// </summary>
    [Fact]
    public async Task LoadPagedDataAsync_MetadataCalculation_IsCorrect()
    {
        // Arrange
        var mockData = CreateMockPacienti(20);
        var pagedResult = CreateMockPagedResult(mockData, 97, 3, 20); // 97 total, page 3

        _mockMediator
            .Setup(m => m.Send(It.IsAny<GetPacientListQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<PagedResult<PacientListDto>>.Success(pagedResult));

        var filters = new PacientFilters();
        var pagination = new PaginationOptions { PageNumber = 3, PageSize = 20 };
        var sorting = new SortOptions();

        // Act
        var result = await _service.LoadPagedDataAsync(filters, pagination, sorting);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.TotalPages.Should().Be(5); // Ceiling(97/20) = 5
        result.Value.HasPreviousPage.Should().BeTrue();
        result.Value.HasNextPage.Should().BeTrue();
        result.Value.CurrentPage.Should().Be(3);
    }

    /// <summary>
    /// Tests last page metadata.
    /// </summary>
    [Fact]
    public async Task LoadPagedDataAsync_LastPage_HasNoNextPage()
    {
        // Arrange
        var mockData = CreateMockPacienti(17); // Last page with partial data
        var pagedResult = CreateMockPagedResult(mockData, 97, 5, 20);

        _mockMediator
            .Setup(m => m.Send(It.IsAny<GetPacientListQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<PagedResult<PacientListDto>>.Success(pagedResult));

        var filters = new PacientFilters();
        var pagination = new PaginationOptions { PageNumber = 5, PageSize = 20 };
        var sorting = new SortOptions();

        // Act
        var result = await _service.LoadPagedDataAsync(filters, pagination, sorting);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().HaveCount(17);
        result.Value.HasPreviousPage.Should().BeTrue();
        result.Value.HasNextPage.Should().BeFalse();
    }

    /// <summary>
    /// Tests that logging occurs for all operations.
    /// </summary>
    [Fact]
    public async Task LoadPagedDataAsync_LogsOperations()
    {
        // Arrange
        var mockData = CreateMockPacienti(20);
        var pagedResult = CreateMockPagedResult(mockData, 20, 1, 20);

        _mockMediator
            .Setup(m => m.Send(It.IsAny<GetPacientListQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<PagedResult<PacientListDto>>.Success(pagedResult));

        var filters = new PacientFilters();
        var pagination = new PaginationOptions { PageNumber = 1, PageSize = 20 };
        var sorting = new SortOptions();

        // Act
        await _service.LoadPagedDataAsync(filters, pagination, sorting);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Loading paged data")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);

        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Data loaded successfully")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    #endregion
}
