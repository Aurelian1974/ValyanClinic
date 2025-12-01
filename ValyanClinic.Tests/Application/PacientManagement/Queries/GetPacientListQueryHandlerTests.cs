using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using ValyanClinic.Application.Features.PacientManagement.Queries.GetPacientList;
using ValyanClinic.Domain.Entities;
using ValyanClinic.Domain.Interfaces.Repositories;
using Xunit;

namespace ValyanClinic.Tests.Application.PacientManagement.Queries;

/// <summary>
/// Unit tests for GetPacientListQueryHandler.
/// Tests paged results, filtering, sorting, empty results, and error handling.
/// </summary>
/// <remarks>
/// Pattern: AAA (Arrange-Act-Assert)
/// Tools: xUnit, Moq, FluentAssertions
/// Coverage: ~6 test scenarios for GetPacientListQuery handler
/// </remarks>
public class GetPacientListQueryHandlerTests
{
    private readonly Mock<IPacientRepository> _mockRepository;
    private readonly Mock<ILogger<GetPacientListQueryHandler>> _mockLogger;
    private readonly GetPacientListQueryHandler _handler;

    public GetPacientListQueryHandlerTests()
    {
        _mockRepository = new Mock<IPacientRepository>();
        _mockLogger = new Mock<ILogger<GetPacientListQueryHandler>>();
        _handler = new GetPacientListQueryHandler(_mockRepository.Object, _mockLogger.Object);
    }

    #region Valid Query Scenarios

    /// <summary>
    /// Tests that a valid query returns paged results.
    /// </summary>
    [Fact]
    public async Task Handle_ValidQuery_ReturnsPagedResults()
    {
        // Arrange
        var query = new GetPacientListQuery
        {
            PageNumber = 1,
            PageSize = 20,
            SearchText = null,
            Judet = null,
            Asigurat = null,
            Activ = null,
            SortColumn = "Nume",
            SortDirection = "ASC"
        };

        var mockPacienti = CreateMockPacienti(15); // Less than page size

        _mockRepository
            .Setup(r => r.GetPagedAsync(
                query.PageNumber,
                query.PageSize,
                query.SearchText,
                query.Judet,
                query.Asigurat,
                query.Activ,
                query.SortColumn,
                query.SortDirection,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((mockPacienti, 15)); // 15 total records

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Value.Should().HaveCount(15);
        result.Value.TotalCount.Should().Be(15);
        result.Value.CurrentPage.Should().Be(1);
        result.Value.PageSize.Should().Be(20);

        // Verify repository interaction
        _mockRepository.Verify(r => r.GetPagedAsync(
            query.PageNumber,
            query.PageSize,
            query.SearchText,
            query.Judet,
            query.Asigurat,
            query.Activ,
            query.SortColumn,
            query.SortDirection,
            It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Tests that query with filters returns filtered results.
    /// </summary>
    [Fact]
    public async Task Handle_WithFilters_ReturnsFilteredResults()
    {
        // Arrange
        var query = new GetPacientListQuery
        {
            PageNumber = 1,
            PageSize = 20,
            SearchText = "Popescu",
            Judet = "Bucuresti",
            Asigurat = true,
            Activ = true,
            SortColumn = "Nume",
            SortDirection = "ASC"
        };

        var mockPacienti = new List<Pacient>
        {
            new Pacient
            {
                Id = Guid.NewGuid(),
                Cod_Pacient = "PAC00001",
                Nume = "Popescu",
                Prenume = "Ion",
                Judet = "Bucuresti",
                Asigurat = true,
                Activ = true
            }
        };

        _mockRepository
            .Setup(r => r.GetPagedAsync(
                query.PageNumber,
                query.PageSize,
                query.SearchText,
                query.Judet,
                query.Asigurat,
                query.Activ,
                query.SortColumn,
                query.SortDirection,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((mockPacienti, 1)); // 1 filtered result

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.Value.Should().HaveCount(1);
        result.Value.Value.First().Nume.Should().Be("Popescu");
        result.Value.Value.First().Judet.Should().Be("Bucuresti");
        result.Value.Value.First().Asigurat.Should().BeTrue();
        result.Value.Value.First().Activ.Should().BeTrue();
    }

    /// <summary>
    /// Tests that query with sorting returns sorted results.
    /// </summary>
    [Fact]
    public async Task Handle_WithSorting_ReturnsSortedResults()
    {
        // Arrange
        var query = new GetPacientListQuery
        {
            PageNumber = 1,
            PageSize = 20,
            SearchText = null,
            Judet = null,
            Asigurat = null,
            Activ = null,
            SortColumn = "Data_Nasterii",
            SortDirection = "DESC"
        };

        var mockPacienti = new List<Pacient>
        {
            new Pacient
            {
                Id = Guid.NewGuid(),
                Cod_Pacient = "PAC00001",
                Nume = "Popescu",
                Prenume = "Ion",
                Data_Nasterii = new DateTime(1990, 1, 1)
            },
            new Pacient
            {
                Id = Guid.NewGuid(),
                Cod_Pacient = "PAC00002",
                Nume = "Ionescu",
                Prenume = "Maria",
                Data_Nasterii = new DateTime(1985, 5, 15)
            }
        };

        _mockRepository
            .Setup(r => r.GetPagedAsync(
                query.PageNumber,
                query.PageSize,
                query.SearchText,
                query.Judet,
                query.Asigurat,
                query.Activ,
                query.SortColumn,
                query.SortDirection,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((mockPacienti, 2));

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.Value.Should().HaveCount(2);

        // Verify repository was called with correct sort parameters
        _mockRepository.Verify(r => r.GetPagedAsync(
            query.PageNumber,
            query.PageSize,
            query.SearchText,
            query.Judet,
            query.Asigurat,
            query.Activ,
            "Data_Nasterii",
            "DESC",
            It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Tests that query with pagination returns correct page.
    /// </summary>
    [Fact]
    public async Task Handle_WithPagination_ReturnsCorrectPage()
    {
        // Arrange
        var query = new GetPacientListQuery
        {
            PageNumber = 2,
            PageSize = 10,
            SearchText = null,
            Judet = null,
            Asigurat = null,
            Activ = null,
            SortColumn = "Nume",
            SortDirection = "ASC"
        };

        var mockPacienti = CreateMockPacienti(10); // Page 2 has 10 records

        _mockRepository
            .Setup(r => r.GetPagedAsync(
                query.PageNumber,
                query.PageSize,
                query.SearchText,
                query.Judet,
                query.Asigurat,
                query.Activ,
                query.SortColumn,
                query.SortDirection,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((mockPacienti, 25)); // Total 25 records (3 pages)

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.Value.Should().HaveCount(10);
        result.Value.CurrentPage.Should().Be(2);
        result.Value.PageSize.Should().Be(10);
        result.Value.TotalCount.Should().Be(25);
        result.Value.TotalPages.Should().Be(3);
    }

    #endregion

    #region Empty Results

    /// <summary>
    /// Tests that query with no matching records returns empty list.
    /// </summary>
    [Fact]
    public async Task Handle_EmptyResults_ReturnsEmptyList()
    {
        // Arrange
        var query = new GetPacientListQuery
        {
            PageNumber = 1,
            PageSize = 20,
            SearchText = "NonExistentName",
            Judet = null,
            Asigurat = null,
            Activ = null,
            SortColumn = "Nume",
            SortDirection = "ASC"
        };

        var emptyList = new List<Pacient>();

        _mockRepository
            .Setup(r => r.GetPagedAsync(
                query.PageNumber,
                query.PageSize,
                query.SearchText,
                query.Judet,
                query.Asigurat,
                query.Activ,
                query.SortColumn,
                query.SortDirection,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((emptyList, 0)); // 0 total records

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.Value.Should().BeEmpty();
        result.Value.TotalCount.Should().Be(0);
        result.Value.CurrentPage.Should().Be(1);
        result.Value.TotalPages.Should().Be(0);
    }

    #endregion

    #region Repository Exception

    /// <summary>
    /// Tests that repository exception returns failure with error message.
    /// </summary>
    [Fact]
    public async Task Handle_RepositoryException_ReturnsFailure()
    {
        // Arrange
        var query = new GetPacientListQuery
        {
            PageNumber = 1,
            PageSize = 20,
            SearchText = null,
            Judet = null,
            Asigurat = null,
            Activ = null,
            SortColumn = "Nume",
            SortDirection = "ASC"
        };

        // Setup: GetPagedAsync throws exception
        _mockRepository
            .Setup(r => r.GetPagedAsync(
                query.PageNumber,
                query.PageSize,
                query.SearchText,
                query.Judet,
                query.Asigurat,
                query.Activ,
                query.SortColumn,
                query.SortDirection,
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database connection timeout"));

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.Contains("Eroare") && e.Contains("Database connection timeout"));
        result.Value.Should().BeNull();

        // Verify logger logged error
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => true),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception?, string>>((v, t) => true)),
            Times.AtLeastOnce);
    }

    #endregion

    #region Helper Methods

    /// <summary>
    /// Creates a list of mock Pacient entities for testing.
    /// </summary>
    private List<Pacient> CreateMockPacienti(int count)
    {
        var pacienti = new List<Pacient>();
        for (int i = 1; i <= count; i++)
        {
            pacienti.Add(new Pacient
            {
                Id = Guid.NewGuid(),
                Cod_Pacient = $"PAC{i:D5}",
                CNP = $"{1234567890000 + i}",
                Nume = $"Pacient{i}",
                Prenume = $"Test{i}",
                Data_Nasterii = DateTime.Now.AddYears(-30 - i),
                Sex = i % 2 == 0 ? "M" : "F",
                Telefon = $"0712345{i:D3}",
                Email = $"pacient{i}@test.com",
                Judet = $"Judet{i % 5}",
                Localitate = $"Localitate{i}",
                Asigurat = i % 2 == 0,
                Activ = true,
                Nr_Total_Vizite = i
            });
        }
        return pacienti;
    }

    #endregion
}
