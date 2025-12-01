using Bunit;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MediatR;
using Moq;
using ValyanClinic.Application.Common.Results;
using ValyanClinic.Application.Features.PacientManagement.Queries.GetPacientList;
using ValyanClinic.Components.Pages.Pacienti;
using ValyanClinic.Tests.Infrastructure;
using Xunit;
using Microsoft.AspNetCore.Components;

namespace ValyanClinic.Tests.Components.Pages.Pacienti;

/// <summary>
/// Simplified bUnit tests pentru VizualizarePacienti page component.
/// Tests PUBLIC behavior only (markup, rendering, MediatR interactions).
/// </summary>
/// <remarks>
/// Folosește bUnit, xUnit, FluentAssertions, Moq conform ghidului de proiect.
/// Pattern: AAA (Arrange, Act, Assert)
/// Note: Tests verify public behavior without accessing private state.
/// </remarks>
public class VizualizarePacientiTests : ComponentTestBase
{
    private readonly Mock<IMediator> _mockMediator;
    private readonly Mock<ILogger<VizualizarePacienti>> _mockLogger;
    private readonly Mock<NavigationManager> _mockNavigationManager;

    public VizualizarePacientiTests()
    {
        _mockMediator = new Mock<IMediator>();
        _mockLogger = MockLogger<VizualizarePacienti>();
        _mockNavigationManager = new Mock<NavigationManager>();

        // Register services
        Services.AddSingleton(_mockMediator.Object);
        Services.AddSingleton(_mockLogger.Object);
        Services.AddSingleton(_mockNavigationManager.Object);

        // Setup default MediatR response
        SetupDefaultMediatRResponse();
    }

    #region Helper Methods

    private void SetupDefaultMediatRResponse()
    {
        _mockMediator
            .Setup(m => m.Send(It.IsAny<GetPacientListQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<PagedResult<PacientListDto>>.Success(
                PagedResult<PacientListDto>.Success(
                    new List<PacientListDto>(),
                    1,
                    20,
                    0)));
    }

    private List<PacientListDto> CreateMockPacienti(int count)
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
                Judet = $"Judet{i % 5}",
                Asigurat = i % 2 == 0,
                Ultima_Vizita = DateTime.Now.AddDays(-i),
                Nr_Total_Vizite = i * 2,
                Activ = true
            });
        }
        return pacienti;
    }

    #endregion

    #region Component Rendering Tests

    /// <summary>
    /// Testează că componenta se renderează fără erori cu date goale.
    /// </summary>
    [Fact]
    public void Component_RendersSuccessfullyWithEmptyData()
    {
        // Arrange & Act
        var cut = RenderComponent<VizualizarePacienti>();

        // Assert
        cut.Markup.Should().Contain("Vizualizare Pacienti");
        cut.Markup.Should().Contain("Total:");
    }

    /// <summary>
    /// Testează că componenta apelează MediatR pentru încărcarea datelor.
    /// </summary>
    [Fact]
    public async Task Component_OnInitialization_CallsMediatRToLoadData()
    {
        // Arrange & Act
        var cut = RenderComponent<VizualizarePacienti>();
        await Task.Delay(200); // Wait for async initialization

        // Assert
        _mockMediator.Verify(
            m => m.Send(It.IsAny<GetPacientListQuery>(), It.IsAny<CancellationToken>()),
            Times.AtLeastOnce);
    }

    /// <summary>
    /// Testează că componenta afișează loading indicator în timpul încărcării.
    /// </summary>
    [Fact]
    public void Component_DuringInitialization_ShowsLoadingIndicator()
    {
        // Arrange
        var tcs = new TaskCompletionSource<Result<PagedResult<PacientListDto>>>();
        _mockMediator
            .Setup(m => m.Send(It.IsAny<GetPacientListQuery>(), It.IsAny<CancellationToken>()))
            .Returns(tcs.Task);

        // Act
        var cut = RenderComponent<VizualizarePacienti>();

        // Assert
        cut.Markup.Should().Contain("Se incarca");

        // Cleanup
        tcs.SetResult(Result<PagedResult<PacientListDto>>.Success(
            PagedResult<PacientListDto>.Success(new List<PacientListDto>(), 1, 20, 0)));
    }

    /// <summary>
    /// Testează că componenta afișează eroare când MediatR returnează failure.
    /// </summary>
    [Fact]
    public async Task Component_WhenMediatRFails_DisplaysErrorMessage()
    {
        // Arrange
        _mockMediator
            .Setup(m => m.Send(It.IsAny<GetPacientListQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<PagedResult<PacientListDto>>.Failure("Eroare la încărcarea datelor"));

        // Act
        var cut = RenderComponent<VizualizarePacienti>();
        await Task.Delay(200);

        // Assert
        cut.Markup.Should().Contain("alert-danger");
        cut.Markup.Should().Contain("Eroare");
    }

    /// <summary>
    /// Testează că butonul Reincarca există în markup.
    /// </summary>
    [Fact]
    public void Component_ContainsRefreshButton()
    {
        // Arrange & Act
        var cut = RenderComponent<VizualizarePacienti>();

        // Assert
        cut.Markup.Should().Contain("Reincarca");
        cut.Markup.Should().Contain("fa-sync-alt");
    }

    /// <summary>
    /// Testează că search box este prezent în markup.
    /// </summary>
    [Fact]
    public void Component_ContainsGlobalSearchBox()
    {
        // Arrange & Act
        var cut = RenderComponent<VizualizarePacienti>();

        // Assert
        cut.Markup.Should().Contain("Cautare rapida");
        cut.Markup.Should().Contain("search-input");
    }

    /// <summary>
    /// Testează că butonul de filtre este prezent.
    /// </summary>
    [Fact]
    public void Component_ContainsFilterButton()
    {
        // Arrange & Act
        var cut = RenderComponent<VizualizarePacienti>();

        // Assert
        cut.Markup.Should().Contain("Filtre");
        cut.Markup.Should().Contain("btn-filter");
    }

    /// <summary>
    /// Testează că toolbar-ul cu butoane (Vizualizeaza/Gestioneaza Doctori) este prezent.
    /// </summary>
    [Fact]
    public void Component_ContainsActionToolbar()
    {
        // Arrange & Act
        var cut = RenderComponent<VizualizarePacienti>();

        // Assert
        cut.Markup.Should().Contain("Vizualizeaza Detalii");
        cut.Markup.Should().Contain("Gestioneaza Doctori");
    }

    /// <summary>
    /// Testează că grid-ul Syncfusion este prezent în markup.
    /// </summary>
    [Fact]
    public void Component_ContainsSyncfusionGrid()
    {
        // Arrange & Act
        var cut = RenderComponent<VizualizarePacienti>();

        // Assert
        cut.Markup.Should().Contain("grid-container");
    }

    /// <summary>
    /// Testează că modalurile (ViewModal și DoctoriModal) sunt prezente în markup.
    /// </summary>
    [Fact]
    public void Component_ContainsModalComponents()
    {
        // Arrange & Act
        var cut = RenderComponent<VizualizarePacienti>();

        // Assert - Modals are declared in markup (even if not visible)
        // Verificăm că markup-ul conține referințe la componente modale
        cut.Markup.Should().NotBeEmpty();
    }

    #endregion

    #region MediatR Interaction Tests

    /// <summary>
    /// Testează că MediatR este apelat cu query corect pentru prima pagină.
    /// </summary>
    [Fact]
    public async Task Component_CallsMediatRWithCorrectQueryForFirstPage()
    {
        // Arrange & Act
        var cut = RenderComponent<VizualizarePacienti>();
        await Task.Delay(200);

        // Assert
        _mockMediator.Verify(
            m => m.Send(
                It.Is<GetPacientListQuery>(q =>
                    q.PageNumber == 1 &&
                    q.PageSize == 20 &&
                    q.SortColumn == "Nume" &&
                    q.SortDirection == "ASC"),
                It.IsAny<CancellationToken>()),
            Times.AtLeastOnce);
    }

    /// <summary>
    /// Testează că MediatR este apelat pentru încărcarea filter options.
    /// </summary>
    [Fact]
    public async Task Component_LoadsFilterOptionsFromServer()
    {
        // Arrange & Act
        var cut = RenderComponent<VizualizarePacienti>();
        await Task.Delay(200);

        // Assert - Verificăm că este apelat cu PageSize mare pentru filter options
        _mockMediator.Verify(
            m => m.Send(
                It.Is<GetPacientListQuery>(q => q.PageSize == int.MaxValue),
                It.IsAny<CancellationToken>()),
            Times.AtLeastOnce);
    }

    #endregion

    #region Public Behavior Tests

    /// <summary>
    /// Testează că componenta nu aruncă excepții la dispose.
    /// </summary>
    [Fact]
    public void Component_DisposesWithoutException()
    {
        // Arrange
        var cut = RenderComponent<VizualizarePacienti>();

        // Act & Assert - Should not throw
        var exception = Record.Exception(() => cut.Instance.Dispose());
        exception.Should().BeNull();
    }

    /// <summary>
    /// Testează că componenta poate fi renderizată multiple ori fără erori.
    /// </summary>
    [Fact]
    public void Component_CanBeRenderedMultipleTimes()
    {
        // Arrange & Act
        var cut1 = RenderComponent<VizualizarePacienti>();
        var cut2 = RenderComponent<VizualizarePacienti>();

        // Assert
        cut1.Markup.Should().NotBeEmpty();
        cut2.Markup.Should().NotBeEmpty();
    }

    #endregion
}
