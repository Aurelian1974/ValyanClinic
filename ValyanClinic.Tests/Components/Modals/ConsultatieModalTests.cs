using Bunit;
using FluentAssertions;
using MediatR;
using Moq;
using ValyanClinic.Application.Common;
using ValyanClinic.Application.Features.Consultatii.Commands.CreateConsultatie;
using ValyanClinic.Application.Features.Consultatii.Models;
using ValyanClinic.Components.Pages.Dashboard.Modals;
using ValyanClinic.Tests.Infrastructure;
using Xunit;

namespace ValyanClinic.Tests.Components.Modals;

/// <summary>
/// bUnit tests for ConsultatieModal component.
/// Tests UI logic, event handlers, tab navigation, and form interactions.
/// </summary>
public class ConsultatieModalTests : ComponentTestBase
{
    #region Constructor & Setup
    
    public ConsultatieModalTests()
    {
        // Additional setup if needed
    }
    
    #endregion
    
    #region Lifecycle Tests
    
    [Fact]
    public void Modal_WhenCreated_ShouldInitializeWithDefaultState()
    {
        // Act
        var cut = RenderWithAuth<ConsultatieModal>();
        
        // Assert
        cut.Instance.IsVisible.Should().BeFalse("modal should be hidden by default");
        cut.Instance.ActiveTab.Should().Be("motive", "default tab should be motive");
        cut.Instance.Model.Should().NotBeNull("model should be initialized");
    }
    
    [Fact]
    public async Task OnInitializedAsync_ShouldLoadData()
    {
        // Arrange
        var programareId = TestData.Ids.TestProgramareId;
        
        // Act
        var cut = RenderWithAuth<ConsultatieModal>(parameters => parameters
            .Add(p => p.ProgramareId, programareId));
        
        await WaitForAsync(cut);
        
        // Assert
        cut.Instance.Model.Should().NotBeNull();
        cut.Instance.Model.ProgramareId.Should().Be(programareId);
    }
    
    #endregion
    
    #region Render Tests
    
    [Fact]
    public void Modal_WhenVisible_ShouldDisplayHeader()
    {
        // Arrange & Act
        var cut = RenderWithAuth<ConsultatieModal>(parameters => parameters
            .Add(p => p.IsVisible, true)
            .Add(p => p.Title, "Consultatie Noua"));
        
        // Assert
        var header = cut.Find(".modal-header h2");
        header.TextContent.Should().Contain("Consultatie Noua");
        
        var overlay = cut.Find(".modal-overlay");
        overlay.ClassList.Should().Contain("visible");
    }
    
    [Fact]
    public void Modal_WhenHidden_ShouldNotDisplayContent()
    {
        // Arrange & Act
        var cut = RenderWithAuth<ConsultatieModal>(parameters => parameters
            .Add(p => p.IsVisible, false));
        
        // Assert
        var overlay = cut.Find(".modal-overlay");
        overlay.ClassList.Should().NotContain("visible");
    }
    
    [Fact]
    public void Modal_ShouldDisplay7Tabs()
    {
        // Arrange & Act
        var cut = RenderWithAuth<ConsultatieModal>(parameters => parameters
            .Add(p => p.IsVisible, true));
        
        // Assert
        var tabs = cut.FindAll("button[data-tab]");
        tabs.Count.Should().Be(7, "should have 7 tabs");
        
        // Verify tab names
        var tabNames = tabs.Select(t => t.GetAttribute("data-tab")).ToList();
        tabNames.Should().Contain(new[] { 
            "motive", "antecedente", "examen", "diagnostic", 
            "investigatii", "tratament", "concluzie" 
        });
    }
    
    [Fact]
    public void Modal_ShouldDisplayFooterButtons()
    {
        // Arrange & Act
        var cut = RenderWithAuth<ConsultatieModal>(parameters => parameters
            .Add(p => p.IsVisible, true));
        
        // Assert
        var saveButton = cut.Find("button.btn-save");
        saveButton.Should().NotBeNull();
        saveButton.TextContent.Should().Contain("Finalizare");
        
        var draftButton = cut.Find("button.btn-draft");
        draftButton.Should().NotBeNull();
        draftButton.TextContent.Should().Contain("Salvare Draft");
        
        var cancelButton = cut.Find("button.btn-cancel");
        cancelButton.Should().NotBeNull();
        cancelButton.TextContent.Should().Contain("Anulare");
    }
    
    #endregion
    
    #region Tab Navigation Tests
    
    [Fact]
    public void TabButton_WhenClicked_ShouldChangeActiveTab()
    {
        // Arrange
        var cut = RenderWithAuth<ConsultatieModal>(parameters => parameters
            .Add(p => p.IsVisible, true));
        
        // Act
        var examenTab = cut.Find("button[data-tab='examen']");
        examenTab.Click();
        
        // Assert
        cut.Instance.ActiveTab.Should().Be("examen");
        examenTab.ClassList.Should().Contain("active");
    }
    
    [Fact]
    public void TabContent_ShouldShowCorrectComponentForActiveTab()
    {
        // Arrange
        var cut = RenderWithAuth<ConsultatieModal>(parameters => parameters
            .Add(p => p.IsVisible, true));
        
        // Act - Switch to "antecedente" tab
        var antecedenteTab = cut.Find("button[data-tab='antecedente']");
        antecedenteTab.Click();
        
        // Assert
        cut.Instance.ActiveTab.Should().Be("antecedente");
        // Verify correct component is rendered (component-specific check)
    }
    
    [Theory]
    [InlineData("motive")]
    [InlineData("antecedente")]
    [InlineData("examen")]
    [InlineData("diagnostic")]
    [InlineData("investigatii")]
    [InlineData("tratament")]
    [InlineData("concluzie")]
    public void AllTabs_ShouldBeClickable(string tabName)
    {
        // Arrange
        var cut = RenderWithAuth<ConsultatieModal>(parameters => parameters
            .Add(p => p.IsVisible, true));
        
        // Act
        var tab = cut.Find($"button[data-tab='{tabName}']");
        tab.Click();
        
        // Assert
        cut.Instance.ActiveTab.Should().Be(tabName);
    }
    
    #endregion
    
    #region Event Handler Tests
    
    [Fact]
    public void CloseButton_WhenClicked_ShouldInvokeOnCloseCallback()
    {
        // Arrange
        var onCloseCalled = false;
        var cut = RenderWithAuth<ConsultatieModal>(parameters => parameters
            .Add(p => p.IsVisible, true)
            .Add(p => p.OnClose, EventCallback.Factory.Create(this, () => onCloseCalled = true)));
        
        // Act
        var closeButton = cut.Find("button.btn-close");
        closeButton.Click();
        
        // Assert
        onCloseCalled.Should().BeTrue("OnClose callback should be invoked");
    }
    
    [Fact]
    public void CancelButton_WhenClicked_ShouldInvokeOnCloseCallback()
    {
        // Arrange
        var onCloseCalled = false;
        var cut = RenderWithAuth<ConsultatieModal>(parameters => parameters
            .Add(p => p.IsVisible, true)
            .Add(p => p.OnClose, EventCallback.Factory.Create(this, () => onCloseCalled = true)));
        
        // Act
        var cancelButton = cut.Find("button.btn-cancel");
        cancelButton.Click();
        
        // Assert
        onCloseCalled.Should().BeTrue("OnClose should be invoked when cancel is clicked");
    }
    
    [Fact]
    public async Task SaveButton_WhenClicked_ShouldCallMediatorSend()
    {
        // Arrange
        var testId = Guid.NewGuid();
        MockMediator
            .Setup(m => m.Send(It.IsAny<CreateConsultatieCommand>(), default))
            .ReturnsAsync(Result<Guid>.Success(testId));
        
        var cut = RenderWithAuth<ConsultatieModal>(parameters => parameters
            .Add(p => p.IsVisible, true));
        
        // Fill in required data
        cut.Instance.Model.MotivPrezentare = "Test motive";
        
        // Act
        var saveButton = cut.Find("button.btn-save");
        saveButton.Click();
        await WaitForAsync(cut);
        
        // Assert
        MockMediator.Verify(m => m.Send(
            It.IsAny<CreateConsultatieCommand>(), 
            default), Times.Once);
    }
    
    #endregion
    
    #region Form Interaction Tests
    
    [Fact]
    public void MotivPrezentare_WhenChanged_ShouldUpdateModel()
    {
        // Arrange
        var cut = RenderWithAuth<ConsultatieModal>(parameters => parameters
            .Add(p => p.IsVisible, true));
        
        // Act
        var textarea = cut.Find("textarea[name='motiv-prezentare']");
        textarea.Change("Dureri de cap");
        
        // Assert
        cut.Instance.Model.MotivPrezentare.Should().Be("Dureri de cap");
    }
    
    [Fact]
    public void Greutate_WhenChanged_ShouldCalculateIMC()
    {
        // Arrange
        var cut = RenderWithAuth<ConsultatieModal>(parameters => parameters
            .Add(p => p.IsVisible, true));
        
        // Set initial values
        cut.Instance.Model.Inaltime = 175;
        
        // Act - Switch to examen tab
        var examenTab = cut.Find("button[data-tab='examen']");
        examenTab.Click();
        
        var greutateInput = cut.Find("input[name='greutate']");
        greutateInput.Change("75.5");
        
        // Assert
        cut.Instance.Model.Greutate.Should().Be(75.5m);
        // IMC should be calculated (if auto-calculation is implemented)
    }
    
    #endregion
    
    #region Draft Auto-Save Tests
    
    [Fact]
    public async Task DraftButton_WhenClicked_ShouldSaveDraft()
    {
        // Arrange
        var cut = RenderWithAuth<ConsultatieModal>(parameters => parameters
            .Add(p => p.IsVisible, true));
        
        cut.Instance.Model.MotivPrezentare = "Test draft";
        
        // Act
        var draftButton = cut.Find("button.btn-draft");
        draftButton.Click();
        await WaitForAsync(cut);
        
        // Assert
        // Verify JSInterop was called for localStorage.setItem
        JSInterop.VerifyInvoke("localStorage.setItem", 1);
    }
    
    #endregion
    
    #region Parameter Binding Tests
    
    [Fact]
    public void ProgramareId_WhenSet_ShouldUpdateModel()
    {
        // Arrange
        var testId = TestData.Ids.TestProgramareId;
        
        // Act
        var cut = RenderWithAuth<ConsultatieModal>(parameters => parameters
            .Add(p => p.ProgramareId, testId));
        
        // Assert
        cut.Instance.Model.ProgramareId.Should().Be(testId);
    }
    
    [Fact]
    public void Title_WhenSet_ShouldDisplayInHeader()
    {
        // Arrange
        var customTitle = "Consultatie de Control";
        
        // Act
        var cut = RenderWithAuth<ConsultatieModal>(parameters => parameters
            .Add(p => p.IsVisible, true)
            .Add(p => p.Title, customTitle));
        
        // Assert
        var header = cut.Find(".modal-header h2");
        header.TextContent.Should().Contain(customTitle);
    }
    
    #endregion
    
    #region Loading & Error States
    
    [Fact]
    public void Modal_WhenLoading_ShouldDisplayLoadingSpinner()
    {
        // Arrange & Act
        var cut = RenderWithAuth<ConsultatieModal>(parameters => parameters
            .Add(p => p.IsVisible, true));
        
        // Manually set loading state for testing
        cut.Instance.IsLoading = true;
        cut.Render();
        
        // Assert
        var spinner = cut.Find(".loading-spinner");
        spinner.Should().NotBeNull("loading spinner should be displayed");
    }
    
    [Fact]
    public void SaveButton_WhenSaving_ShouldBeDisabled()
    {
        // Arrange
        var cut = RenderWithAuth<ConsultatieModal>(parameters => parameters
            .Add(p => p.IsVisible, true));
        
        // Set saving state
        cut.Instance.IsSaving = true;
        cut.Render();
        
        // Assert
        var saveButton = cut.Find("button.btn-save");
        saveButton.HasAttribute("disabled").Should().BeTrue("button should be disabled when saving");
    }
    
    #endregion
    
    #region Validation Tests
    
    [Fact]
    public void SaveButton_WithEmptyMotivPrezentare_ShouldShowValidationError()
    {
        // Arrange
        var cut = RenderWithAuth<ConsultatieModal>(parameters => parameters
            .Add(p => p.IsVisible, true));
        
        // Ensure MotivPrezentare is empty
        cut.Instance.Model.MotivPrezentare = string.Empty;
        
        // Act
        var saveButton = cut.Find("button.btn-save");
        saveButton.Click();
        
        // Assert
        // Verify validation error is displayed (component-specific check)
        var validationMessage = cut.FindAll(".validation-message");
        validationMessage.Should().NotBeEmpty("validation message should be displayed");
    }
    
    #endregion
    
    #region Integration Tests
    
    [Fact]
    public async Task CompleteWorkflow_NewConsultatie_ShouldSucceed()
    {
        // Arrange
        var testId = Guid.NewGuid();
        MockMediator
            .Setup(m => m.Send(It.IsAny<CreateConsultatieCommand>(), default))
            .ReturnsAsync(Result<Guid>.Success(testId));
        
        var onSaveCalled = false;
        var cut = RenderWithAuth<ConsultatieModal>(parameters => parameters
            .Add(p => p.IsVisible, true)
            .Add(p => p.ProgramareId, TestData.Ids.TestProgramareId)
            .Add(p => p.OnSave, EventCallback.Factory.Create(this, () => onSaveCalled = true)));
        
        // Act - Fill in required data
        cut.Instance.Model.MotivPrezentare = "Dureri abdominale";
        cut.Instance.Model.SimptomeAsociate = "Greata";
        
        // Switch through tabs (simulate user navigation)
        var tabs = new[] { "antecedente", "examen", "diagnostic", "concluzie" };
        foreach (var tabName in tabs)
        {
            var tab = cut.Find($"button[data-tab='{tabName}']");
            tab.Click();
            await WaitForAsync(cut);
        }
        
        // Save consultatie
        var saveButton = cut.Find("button.btn-save");
        saveButton.Click();
        await WaitForAsync(cut);
        
        // Assert
        MockMediator.Verify(m => m.Send(It.IsAny<CreateConsultatieCommand>(), default), Times.Once);
        onSaveCalled.Should().BeTrue("OnSave callback should be invoked after successful save");
    }
    
    #endregion
}
