using FluentAssertions;
using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using ValyanClinic.Application.Common.Results;
using ValyanClinic.Application.Features.ConsultatieManagement.Commands.CreateConsultatie;
using ValyanClinic.Application.Services.IMC;
using ValyanClinic.Application.ViewModels;
using ValyanClinic.Infrastructure.Services.DraftStorage;
using Xunit;

namespace ValyanClinic.Tests.ViewModels;

/// <summary>
/// Unit tests pentru ConsultatieViewModel
/// Testează orchestrarea state management și business logic
/// </summary>
public class ConsultatieViewModelTests
{
    private readonly Mock<IMediator> _mediatorMock;
    private readonly Mock<IDraftStorageService<CreateConsultatieCommand>> _draftServiceMock;
    private readonly Mock<IIMCCalculatorService> _imcCalculatorMock;
    private readonly Mock<ILogger<ConsultatieViewModel>> _loggerMock;
    private readonly ConsultatieViewModel _viewModel;

    public ConsultatieViewModelTests()
    {
        _mediatorMock = new Mock<IMediator>();
        _draftServiceMock = new Mock<IDraftStorageService<CreateConsultatieCommand>>();
        _imcCalculatorMock = new Mock<IIMCCalculatorService>();
        _loggerMock = new Mock<ILogger<ConsultatieViewModel>>();

        _viewModel = new ConsultatieViewModel(
            _mediatorMock.Object,
            _draftServiceMock.Object,
            _imcCalculatorMock.Object,
            _loggerMock.Object
        );
    }

    #region InitializeAsync Tests

    [Fact(DisplayName = "InitializeAsync - Inițializează fără draft existent")]
    public async Task InitializeAsync_NoDraft_InitializesNewCommand()
    {
        // Arrange
        var programareId = Guid.NewGuid();
        var pacientId = Guid.NewGuid();
        var medicId = Guid.NewGuid();
        var userId = "doctor123";

        _draftServiceMock
            .Setup(x => x.LoadDraftAsync(programareId))
            .ReturnsAsync(DraftResult<CreateConsultatieCommand>.NotFound);

        // Act
        await _viewModel.InitializeAsync(programareId, pacientId, medicId, userId);

        // Assert
        _viewModel.Command.Should().NotBeNull();
        _viewModel.Command.ProgramareID.Should().Be(programareId);
        _viewModel.Command.PacientID.Should().Be(pacientId);
        _viewModel.Command.MedicID.Should().Be(medicId);
        _viewModel.Command.CreatDe.Should().Be(userId);
        _viewModel.HasDraftLoaded.Should().BeFalse();
        _viewModel.IsInitializing.Should().BeFalse();
    }

    [Fact(DisplayName = "InitializeAsync - Încarcă draft existent cu succes")]
    public async Task InitializeAsync_WithDraft_LoadsDraftData()
    {
        // Arrange
        var programareId = Guid.NewGuid();
        var savedTime = DateTime.Now.AddMinutes(-10);

        var draftCommand = new CreateConsultatieCommand
        {
            ProgramareID = programareId,
            MotivPrezentare = "Dureri de cap",
            DiagnosticPozitiv = "Migrena"
        };

        _draftServiceMock
            .Setup(x => x.LoadDraftAsync(programareId))
            .ReturnsAsync(DraftResult<CreateConsultatieCommand>.Success(draftCommand, savedTime));

        // Act
        await _viewModel.InitializeAsync(programareId, Guid.NewGuid(), Guid.NewGuid(), "user123");

        // Assert
        _viewModel.Command.Should().NotBeNull();
        _viewModel.Command.MotivPrezentare.Should().Be("Dureri de cap");
        _viewModel.Command.DiagnosticPozitiv.Should().Be("Migrena");
        _viewModel.HasDraftLoaded.Should().BeTrue();
        _viewModel.LastSaveTime.Should().Be(savedTime);
        _viewModel.HasUnsavedChanges.Should().BeFalse();
    }

    [Fact(DisplayName = "InitializeAsync - Draft expirat continuă cu command nou")]
    public async Task InitializeAsync_ExpiredDraft_ContinuesWithNewCommand()
    {
        // Arrange
        var programareId = Guid.NewGuid();

        _draftServiceMock
            .Setup(x => x.LoadDraftAsync(programareId))
            .ReturnsAsync(DraftResult<CreateConsultatieCommand>.Expired);

        // Act
        await _viewModel.InitializeAsync(programareId, Guid.NewGuid(), Guid.NewGuid(), "user123");

        // Assert
        _viewModel.Command.Should().NotBeNull();
        _viewModel.HasDraftLoaded.Should().BeFalse();
        _viewModel.Command.ProgramareID.Should().Be(programareId);
    }

    #endregion

    #region Tab Navigation Tests

    [Fact(DisplayName = "SetActiveTab - Schimbă tab-ul activ")]
    public void SetActiveTab_ChangesActiveTab()
    {
        // Arrange
        var stateChangedCalled = false;
        _viewModel.StateChanged += (sender, args) => stateChangedCalled = true;

        // Act
        _viewModel.SetActiveTab("antecedente");

        // Assert
        _viewModel.ActiveTab.Should().Be("antecedente");
        _viewModel.CurrentSection.Should().Be("antecedente");
        stateChangedCalled.Should().BeTrue();
    }

    [Fact(DisplayName = "MarkSectionAsCompleted - Marchează secțiune completă")]
    public void MarkSectionAsCompleted_AddsToCompletedSections()
    {
        // Act
        _viewModel.MarkSectionAsCompleted("motive");
        _viewModel.MarkSectionAsCompleted("antecedente");

        // Assert
        _viewModel.CompletedSections.Should().Contain("motive");
        _viewModel.CompletedSections.Should().Contain("antecedente");
        _viewModel.CompletedSections.Should().HaveCount(2);
    }

    [Fact(DisplayName = "GetCompletionProgress - Calculează progresul corect")]
    public void GetCompletionProgress_CalculatesCorrectly()
    {
        // Arrange
        _viewModel.MarkSectionAsCompleted("motive");
        _viewModel.MarkSectionAsCompleted("antecedente");
        _viewModel.MarkSectionAsCompleted("examen");

        // Act
        var progress = _viewModel.GetCompletionProgress();

        // Assert - 3 din 7 secțiuni = ~42%
        progress.Should().BeInRange(40, 45);
    }

    #endregion

    #region Draft Management Tests

    [Fact(DisplayName = "SaveDraftAsync - Salvează draft cu succes")]
    public async Task SaveDraftAsync_Success_UpdatesState()
    {
        // Arrange
        var programareId = Guid.NewGuid();
        await _viewModel.InitializeAsync(programareId, Guid.NewGuid(), Guid.NewGuid(), "user123");

        var draftSavedCalled = false;
        _viewModel.DraftSaved += (sender, args) => draftSavedCalled = true;

        _draftServiceMock
            .Setup(x => x.SaveDraftAsync(programareId, It.IsAny<CreateConsultatieCommand>(), "user123"))
            .Returns(Task.CompletedTask);

        // Act
        await _viewModel.SaveDraftAsync();

        // Assert
        _viewModel.IsSavingDraft.Should().BeFalse();
        _viewModel.HasUnsavedChanges.Should().BeFalse();
        _viewModel.LastSaveTime.Should().NotBeNull();
        _viewModel.LastSaveTime.Should().BeCloseTo(DateTime.Now, TimeSpan.FromSeconds(2));
        draftSavedCalled.Should().BeTrue();
    }

    [Fact(DisplayName = "SaveDraftAsync - Nu salvează dacă deja e în progress")]
    public async Task SaveDraftAsync_AlreadyInProgress_Skips()
    {
        // Arrange
        var programareId = Guid.NewGuid();
        await _viewModel.InitializeAsync(programareId, Guid.NewGuid(), Guid.NewGuid(), "user123");

        // Simulăm că e deja în progress
        typeof(ConsultatieViewModel)
            .GetProperty("IsSavingDraft")!
            .SetValue(_viewModel, true);

        // Act
        await _viewModel.SaveDraftAsync();

        // Assert
        _draftServiceMock.Verify(
            x => x.SaveDraftAsync(It.IsAny<Guid>(), It.IsAny<CreateConsultatieCommand>(), It.IsAny<string>()),
            Times.Never
        );
    }

    [Fact(DisplayName = "MarkAsChanged - Marchează modificări nesalvate")]
    public void MarkAsChanged_SetsFlag()
    {
        // Arrange
        var stateChangedCalled = false;
        _viewModel.StateChanged += (sender, args) => stateChangedCalled = true;

        // Act
        _viewModel.MarkAsChanged();

        // Assert
        _viewModel.HasUnsavedChanges.Should().BeTrue();
        stateChangedCalled.Should().BeTrue();
    }

    #endregion

    #region Submit Tests

    [Fact(DisplayName = "SubmitAsync - Validare eșuată returnează eroare")]
    public async Task SubmitAsync_ValidationFails_ReturnsFailure()
    {
        // Arrange
        var programareId = Guid.NewGuid();
        await _viewModel.InitializeAsync(programareId, Guid.NewGuid(), Guid.NewGuid(), "user123");

        // Command fără câmpuri obligatorii
        _viewModel.Command.MotivPrezentare = null;
        _viewModel.Command.DiagnosticPozitiv = null;

        // Act
        var result = await _viewModel.SubmitAsync();

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.FirstError.Should().Contain("Validare");
        _viewModel.HasValidationErrors.Should().BeTrue();
        _viewModel.ValidationErrors.Should().ContainKey("MotivPrezentare");
        _viewModel.ValidationErrors.Should().ContainKey("DiagnosticPozitiv");
    }

    [Fact(DisplayName = "SubmitAsync - Success șterge draft-ul")]
    public async Task SubmitAsync_Success_ClearsDraft()
    {
        // Arrange
        var programareId = Guid.NewGuid();
        var consultatieId = Guid.NewGuid();

        await _viewModel.InitializeAsync(programareId, Guid.NewGuid(), Guid.NewGuid(), "user123");

        _viewModel.Command.MotivPrezentare = "Dureri abdominale";
        _viewModel.Command.DiagnosticPozitiv = "Gastrită";

        _mediatorMock
            .Setup(x => x.Send(It.IsAny<CreateConsultatieCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<Guid>.Success(consultatieId));

        var consultatieSubmittedCalled = false;
        _viewModel.ConsultatieSubmitted += (sender, args) => consultatieSubmittedCalled = true;

        // Act
        var result = await _viewModel.SubmitAsync();

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(consultatieId);
        _viewModel.HasUnsavedChanges.Should().BeFalse();
        consultatieSubmittedCalled.Should().BeTrue();

        _draftServiceMock.Verify(
            x => x.ClearDraftAsync(programareId),
            Times.Once
        );
    }

    [Fact(DisplayName = "SubmitAsync - Failure păstrează draft-ul")]
    public async Task SubmitAsync_Failure_KeepsDraft()
    {
        // Arrange
        var programareId = Guid.NewGuid();
        await _viewModel.InitializeAsync(programareId, Guid.NewGuid(), Guid.NewGuid(), "user123");

        _viewModel.Command.MotivPrezentare = "Test";
        _viewModel.Command.DiagnosticPozitiv = "Test";

        var errorOccurredCalled = false;
        string? errorMessage = null;
        _viewModel.ErrorOccurred += (sender, msg) =>
        {
            errorOccurredCalled = true;
            errorMessage = msg;
        };

        _mediatorMock
            .Setup(x => x.Send(It.IsAny<CreateConsultatieCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<Guid>.Failure("Database error"));

        // Act
        var result = await _viewModel.SubmitAsync();

        // Assert
        result.IsSuccess.Should().BeFalse();
        errorOccurredCalled.Should().BeTrue();
        errorMessage.Should().Contain("Database error");

        _draftServiceMock.Verify(
            x => x.ClearDraftAsync(It.IsAny<Guid>()),
            Times.Never
        );
    }

    #endregion

    #region IMC Tests

    [Fact(DisplayName = "IMCResult - Calculează când sunt completate ambele valori")]
    public void IMCResult_ValidValues_CalculatesIMC()
    {
        // Arrange
        _viewModel.Command.Greutate = 75;
        _viewModel.Command.Inaltime = 175;

        var expectedResult = new IMCResult
        {
            Value = 24.49m,
            Category = IMCCategory.Normal,
            Interpretation = "Normal"
        };

        _imcCalculatorMock
            .Setup(x => x.Calculate(75, 175))
            .Returns(expectedResult);

        // Act
        var result = _viewModel.IMCResult;

        // Assert
        result.Should().NotBeNull();
        result!.Value.Should().Be(24.49m);
        result.Category.Should().Be(IMCCategory.Normal);
    }

    [Fact(DisplayName = "IMCResult - Returnează null dacă lipsesc valori")]
    public void IMCResult_MissingValues_ReturnsNull()
    {
        // Arrange
        _viewModel.Command.Greutate = null;
        _viewModel.Command.Inaltime = 175;

        // Act
        var result = _viewModel.IMCResult;

        // Assert
        result.Should().BeNull();
    }

    [Fact(DisplayName = "CalculateIdealWeight - Calculează greutatea ideală")]
    public void CalculateIdealWeight_ValidHeight_ReturnsIdealWeight()
    {
        // Arrange
        _viewModel.Command.Inaltime = 170;

        _imcCalculatorMock
            .Setup(x => x.CalculateIdealWeight(170, "M"))
            .Returns(65m);

        // Act
        var result = _viewModel.CalculateIdealWeight("M");

        // Assert
        result.Should().Be(65m);
    }

    #endregion

    #region Validation Tests

    [Fact(DisplayName = "Validation - Detectează motiv prezentare lipsă")]
    public async Task Validation_MissingMotivPrezentare_AddsError()
    {
        // Arrange
        await _viewModel.InitializeAsync(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "user123");
        _viewModel.Command.DiagnosticPozitiv = "Test";

        // Act
        await _viewModel.SubmitAsync();

        // Assert
        var errors = _viewModel.GetValidationErrors("MotivPrezentare");
        errors.Should().NotBeNull();
        errors!.Should().Contain(e => e.Contains("obligatoriu"));
    }

    [Fact(DisplayName = "Validation - Detectează diagnostic lipsă")]
    public async Task Validation_MissingDiagnostic_AddsError()
    {
        // Arrange
        await _viewModel.InitializeAsync(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "user123");
        _viewModel.Command.MotivPrezentare = "Test";

        // Act
        await _viewModel.SubmitAsync();

        // Assert
        var errors = _viewModel.GetValidationErrors("DiagnosticPozitiv");
        errors.Should().NotBeNull();
        errors!.Should().Contain(e => e.Contains("obligatoriu"));
    }

    [Fact(DisplayName = "Validation - Detectează IMC invalid")]
    public async Task Validation_InvalidIMC_AddsError()
    {
        // Arrange
        await _viewModel.InitializeAsync(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "user123");
        _viewModel.Command.MotivPrezentare = "Test";
        _viewModel.Command.DiagnosticPozitiv = "Test";
        _viewModel.Command.Greutate = 600; // Invalid
        _viewModel.Command.Inaltime = 170;

        _imcCalculatorMock
            .Setup(x => x.AreValuesValid(600, 170))
            .Returns(false);

        // Act
        await _viewModel.SubmitAsync();

        // Assert
        var errors = _viewModel.GetValidationErrors("IMC");
        errors.Should().NotBeNull();
        errors!.Should().Contain(e => e.Contains("invalide"));
    }

    #endregion

    #region Reset Tests

    [Fact(DisplayName = "Reset - Resetează toate proprietățile")]
    public async Task Reset_ClearsAllState()
    {
        // Arrange
        await _viewModel.InitializeAsync(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "user123");
        _viewModel.MarkSectionAsCompleted("motive");
        _viewModel.SetActiveTab("antecedente");
        _viewModel.MarkAsChanged();

        // Act
        _viewModel.Reset();

        // Assert
        _viewModel.ActiveTab.Should().Be("motive");
        _viewModel.CompletedSections.Should().BeEmpty();
        _viewModel.HasUnsavedChanges.Should().BeFalse();
        _viewModel.IsSaving.Should().BeFalse();
        _viewModel.LastSaveTime.Should().BeNull();
    }

    #endregion

    #region Integration Scenarios

    [Fact(DisplayName = "Scenariu Real - Workflow complet cu draft")]
    public async Task IntegrationScenario_CompleteWorkflowWithDraft_Success()
    {
        // Arrange
        var programareId = Guid.NewGuid();
        var pacientId = Guid.NewGuid();
        var medicId = Guid.NewGuid();
        var consultatieId = Guid.NewGuid();

        _draftServiceMock
            .Setup(x => x.LoadDraftAsync(programareId))
            .ReturnsAsync(DraftResult<CreateConsultatieCommand>.NotFound);

        _mediatorMock
            .Setup(x => x.Send(It.IsAny<CreateConsultatieCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<Guid>.Success(consultatieId));

        // Step 1: Initialize
        await _viewModel.InitializeAsync(programareId, pacientId, medicId, "doctor123");
        _viewModel.Command.Should().NotBeNull();

        // Step 2: Fill data
        _viewModel.Command.MotivPrezentare = "Dureri de cap persistente";
        _viewModel.MarkAsChanged();

        // Step 3: Save draft
        await _viewModel.SaveDraftAsync();
        _viewModel.HasUnsavedChanges.Should().BeFalse();

        // Step 4: Navigate tabs
        _viewModel.SetActiveTab("antecedente");
        _viewModel.MarkSectionAsCompleted("motive");

        // Step 5: Complete form
        _viewModel.Command.DiagnosticPozitiv = "Migrena cronică";
        _viewModel.MarkAsChanged();

        // Step 6: Submit
        var result = await _viewModel.SubmitAsync();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(consultatieId);

        // Verify draft cleared
        _draftServiceMock.Verify(x => x.ClearDraftAsync(programareId), Times.Once);
    }

    #endregion
}
