using Bunit;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using MediatR;
using Moq;
using ValyanClinic.Application.Common.Results;
using ValyanClinic.Application.Features.PacientPersonalMedicalManagement.DTOs;
using ValyanClinic.Application.Features.PacientPersonalMedicalManagement.Queries.GetDoctoriByPacient;
using ValyanClinic.Application.Features.PacientPersonalMedicalManagement.Commands.RemoveRelatie;
using ValyanClinic.Components.Pages.Pacienti.Modals;
using ValyanClinic.Tests.Infrastructure;
using Xunit;

namespace ValyanClinic.Tests.Components.Pages.Pacienti.Modals;

/// <summary>
/// bUnit tests pentru PacientDoctoriModal component.
/// Testează rendering, parameter binding, state management, user interactions.
/// </summary>
/// <remarks>
/// Folosește bUnit, xUnit, FluentAssertions, Moq conform ghidului de proiect.
/// Pattern: AAA (Arrange, Act, Assert)
/// </remarks>
public class PacientDoctoriModalTests : ComponentTestBase
{
    private readonly Mock<IMediator> _mockMediator;
    private readonly Mock<IJSRuntime> _mockJSRuntime;
    private readonly Mock<ILogger<PacientDoctoriModal>> _mockLogger;

    public PacientDoctoriModalTests()
    {
        _mockMediator = new Mock<IMediator>();
        _mockJSRuntime = new Mock<IJSRuntime>();
        _mockLogger = MockLogger<PacientDoctoriModal>();

        // Register services
        Services.AddSingleton(_mockMediator.Object);
        Services.AddSingleton(_mockJSRuntime.Object);
        Services.AddSingleton(_mockLogger.Object);
    }

    /// <summary>
    /// Testează că modalul se renderează corect când IsVisible = false.
    /// </summary>
    [Fact]
    public void Component_WhenIsVisibleFalse_ShouldNotRenderContent()
    {
        // Arrange & Act
        var cut = RenderComponent<PacientDoctoriModal>(parameters => parameters
            .Add(p => p.IsVisible, false)
            .Add(p => p.PacientID, Guid.NewGuid())
            .Add(p => p.PacientNume, "Popescu Ion"));

        // Assert
        cut.Markup.Should().NotBeEmpty();
        // Modal should not be visible (CSS class check)
        cut.Find(".modal-overlay").ClassList.Should().NotContain("visible");
    }

    /// <summary>
    /// Testează că modalul se renderează corect când IsVisible = true.
    /// </summary>
    [Fact]
    public void Component_WhenIsVisibleTrue_ShouldRenderModalContent()
    {
        // Arrange
        var pacientId = Guid.NewGuid();
        var pacientNume = "Popescu Ion";

        _mockMediator
            .Setup(m => m.Send(It.IsAny<GetDoctoriByPacientQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<List<DoctorAsociatDto>>.Success(new List<DoctorAsociatDto>()));

        // Act
        var cut = RenderComponent<PacientDoctoriModal>(parameters => parameters
            .Add(p => p.IsVisible, true)
            .Add(p => p.PacientID, pacientId)
            .Add(p => p.PacientNume, pacientNume));

        // Assert
        cut.Markup.Should().Contain("modal-overlay");
        cut.Markup.Should().Contain("Doctori asociați");
        cut.Markup.Should().Contain(pacientNume);
    }

    /// <summary>
    /// Testează că loading state se afișează corect în timpul încărcării datelor.
    /// </summary>
    [Fact]
    public void Component_WhenLoadingDoctori_ShouldDisplayLoadingIndicator()
    {
        // Arrange
        var tcs = new TaskCompletionSource<Result<List<DoctorAsociatDto>>>();

        _mockMediator
            .Setup(m => m.Send(It.IsAny<GetDoctoriByPacientQuery>(), It.IsAny<CancellationToken>()))
            .Returns(tcs.Task);

        // Act
        var cut = RenderComponent<PacientDoctoriModal>(parameters => parameters
            .Add(p => p.IsVisible, true)
            .Add(p => p.PacientID, Guid.NewGuid())
            .Add(p => p.PacientNume, "Test Pacient"));

        // Assert
        cut.Markup.Should().Contain("Se încarcă");
        cut.Markup.Should().Contain("loading-container");

        // Cleanup
        tcs.SetResult(Result<List<DoctorAsociatDto>>.Success(new List<DoctorAsociatDto>()));
    }

    /// <summary>
    /// Testează că eroarea se afișează corect când query-ul eșuează.
    /// </summary>
    [Fact]
    public async Task Component_WhenQueryFails_ShouldDisplayErrorMessage()
    {
        // Arrange
        var errorMessage = "Nu s-au putut încărca doctorii";

        _mockMediator
            .Setup(m => m.Send(It.IsAny<GetDoctoriByPacientQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<List<DoctorAsociatDto>>.Failure(errorMessage));

        // Act
        var cut = RenderComponent<PacientDoctoriModal>(parameters => parameters
            .Add(p => p.IsVisible, true)
            .Add(p => p.PacientID, Guid.NewGuid())
            .Add(p => p.PacientNume, "Test Pacient"));

        await cut.InvokeAsync(() => { }); // Wait for async operations

        // Assert
        cut.Markup.Should().Contain("alert-danger");
        cut.Markup.Should().Contain(errorMessage);
    }

    /// <summary>
    /// Testează că empty state se afișează corect când nu există doctori.
    /// </summary>
    [Fact]
    public async Task Component_WhenNoDoctors_ShouldDisplayEmptyState()
    {
        // Arrange
        _mockMediator
            .Setup(m => m.Send(It.IsAny<GetDoctoriByPacientQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<List<DoctorAsociatDto>>.Success(new List<DoctorAsociatDto>()));

        // Act
        var cut = RenderComponent<PacientDoctoriModal>(parameters => parameters
            .Add(p => p.IsVisible, true)
            .Add(p => p.PacientID, Guid.NewGuid())
            .Add(p => p.PacientNume, "Test Pacient"));

        await cut.InvokeAsync(() => { }); // Wait for async operations

        // Assert
        cut.Markup.Should().Contain("empty-state");
        cut.Markup.Should().Contain("Nu există doctori activi asociați");
        cut.Markup.Should().Contain("Adaugă primul doctor");
    }

    /// <summary>
    /// Testează că lista de doctori se afișează corect.
    /// </summary>
    [Fact]
    public async Task Component_WithDoctors_ShouldDisplayDoctorList()
    {
        // Arrange
        var doctori = new List<DoctorAsociatDto>
        {
            new()
            {
                RelatieID = Guid.NewGuid(),
                PersonalMedicalID = Guid.NewGuid(),
                DoctorNumeComplet = "Dr. Popescu Ion",
                DoctorSpecializare = "Cardiologie",
                TipRelatie = "MedicPrimar",
                DataAsocierii = DateTime.Now.AddMonths(-6),
                EsteActiv = true,
                ZileDeAsociere = 180
            },
            new()
            {
                RelatieID = Guid.NewGuid(),
                PersonalMedicalID = Guid.NewGuid(),
                DoctorNumeComplet = "Dr. Ionescu Maria",
                DoctorSpecializare = "Pneumologie",
                TipRelatie = "Specialist",
                DataAsocierii = DateTime.Now.AddMonths(-3),
                EsteActiv = true,
                ZileDeAsociere = 90
            }
        };

        _mockMediator
            .Setup(m => m.Send(It.IsAny<GetDoctoriByPacientQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<List<DoctorAsociatDto>>.Success(doctori));

        // Act
        var cut = RenderComponent<PacientDoctoriModal>(parameters => parameters
            .Add(p => p.IsVisible, true)
            .Add(p => p.PacientID, Guid.NewGuid())
            .Add(p => p.PacientNume, "Test Pacient"));

        await cut.InvokeAsync(() => { }); // Wait for async operations

        // Assert
        cut.Markup.Should().Contain("Dr. Popescu Ion");
        cut.Markup.Should().Contain("Cardiologie");
        cut.Markup.Should().Contain("Dr. Ionescu Maria");
        cut.Markup.Should().Contain("Pneumologie");
        cut.Markup.Should().Contain("MedicPrimar");
        cut.Markup.Should().Contain("Specialist");
        cut.Markup.Should().Contain("Doctori activi (2)");
    }

    /// <summary>
    /// Testează că doctorii inactivi se afișează în secțiunea de istoric.
    /// </summary>
    [Fact]
    public async Task Component_WithInactiveDoctors_ShouldDisplayHistoricSection()
    {
        // Arrange
        var doctori = new List<DoctorAsociatDto>
        {
            new()
            {
                RelatieID = Guid.NewGuid(),
                PersonalMedicalID = Guid.NewGuid(),
                DoctorNumeComplet = "Dr. Activ",
                EsteActiv = true,
                ZileDeAsociere = 100
            },
            new()
            {
                RelatieID = Guid.NewGuid(),
                PersonalMedicalID = Guid.NewGuid(),
                DoctorNumeComplet = "Dr. Inactiv",
                EsteActiv = false,
                DataDezactivarii = DateTime.Now.AddDays(-10),
                ZileDeAsociere = 50
            }
        };

        _mockMediator
            .Setup(m => m.Send(It.IsAny<GetDoctoriByPacientQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<List<DoctorAsociatDto>>.Success(doctori));

        // Act
        var cut = RenderComponent<PacientDoctoriModal>(parameters => parameters
            .Add(p => p.IsVisible, true)
            .Add(p => p.PacientID, Guid.NewGuid())
            .Add(p => p.PacientNume, "Test Pacient"));

        await cut.InvokeAsync(() => { }); // Wait for async operations

        // Assert
        cut.Markup.Should().Contain("Dr. Activ");
        cut.Markup.Should().Contain("Dr. Inactiv");
        cut.Markup.Should().Contain("Istoric relații inactive (1)");
        cut.Markup.Should().Contain("INACTIV");
    }

    /// <summary>
    /// Testează că parametrii sunt bindați corect.
    /// </summary>
    [Fact]
    public void Component_ParameterBinding_ShouldWorkCorrectly()
    {
        // Arrange
        var pacientId = Guid.NewGuid();
        var pacientNume = "Popescu Ion";

        _mockMediator
            .Setup(m => m.Send(It.Is<GetDoctoriByPacientQuery>(q =>
                q.PacientID == pacientId && q.ApenumereActivi == false),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<List<DoctorAsociatDto>>.Success(new List<DoctorAsociatDto>()));

        // Act
        var cut = RenderComponent<PacientDoctoriModal>(parameters => parameters
            .Add(p => p.IsVisible, true)
            .Add(p => p.PacientID, pacientId)
            .Add(p => p.PacientNume, pacientNume));

        // Assert
        _mockMediator.Verify(m => m.Send(
            It.Is<GetDoctoriByPacientQuery>(q => q.PacientID == pacientId),
            It.IsAny<CancellationToken>()),
            Times.Once);

        cut.Markup.Should().Contain(pacientNume);
    }

    /// <summary>
    /// Testează că MediatR query nu este apelat când IsVisible = false.
    /// </summary>
    [Fact]
    public void Component_WhenIsVisibleFalse_ShouldNotCallMediatR()
    {
        // Arrange & Act
        var cut = RenderComponent<PacientDoctoriModal>(parameters => parameters
            .Add(p => p.IsVisible, false)
            .Add(p => p.PacientID, Guid.NewGuid())
            .Add(p => p.PacientNume, "Test Pacient"));

        // Assert
        _mockMediator.Verify(m => m.Send(
            It.IsAny<GetDoctoriByPacientQuery>(),
            It.IsAny<CancellationToken>()),
            Times.Never);
    }

    /// <summary>
    /// Testează că MediatR query nu este apelat când PacientID este null.
    /// </summary>
    [Fact]
    public void Component_WhenPacientIDIsNull_ShouldNotCallMediatR()
    {
        // Arrange & Act
        var cut = RenderComponent<PacientDoctoriModal>(parameters => parameters
            .Add(p => p.IsVisible, true)
            .Add(p => p.PacientID, (Guid?)null)
            .Add(p => p.PacientNume, "Test Pacient"));

        // Assert
        _mockMediator.Verify(m => m.Send(
            It.IsAny<GetDoctoriByPacientQuery>(),
            It.IsAny<CancellationToken>()),
            Times.Never);
    }
}
