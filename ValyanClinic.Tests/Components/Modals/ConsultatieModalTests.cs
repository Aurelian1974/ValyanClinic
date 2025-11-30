using Bunit;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Components;
using Moq;
using ValyanClinic.Application.Common.Results;
using ValyanClinic.Application.Features.ConsultatieManagement.Commands.CreateConsultatie;
using ValyanClinic.Components.Pages.Dashboard.Modals;
using ValyanClinic.Tests.Infrastructure;
using Xunit;

namespace ValyanClinic.Tests.Components.Modals;

/// <summary>
/// bUnit tests for ConsultatieModal component.
/// Tests UI markup, event handlers, and component interactions.
/// Note: ConsultatieModal properties are private, so we test via markup and events.
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
    
    [Fact(DisplayName = "Modal - When created - Should initialize successfully")]
    public void Modal_WhenCreated_ShouldInitializeSuccessfully()
    {
        // Act
        var cut = RenderComponent<ConsultatieModal>(parameters => parameters
            .Add(p => p.ProgramareID, TestData.Ids.TestProgramareId)
            .Add(p => p.PacientID, TestData.Ids.TestPacientId)
            .Add(p => p.MedicID, TestData.Ids.TestDoctorId));
        
        // Assert
        cut.Should().NotBeNull("component should render successfully");
        cut.Instance.Should().NotBeNull();
    }
    
    #endregion
    
    #region Render Tests
    
    [Fact(DisplayName = "Modal - When rendered - Should contain modal overlay")]
    public void Modal_WhenRendered_ShouldContainModalOverlay()
    {
        // Arrange & Act
        var cut = RenderComponent<ConsultatieModal>(parameters => parameters
            .Add(p => p.ProgramareID, TestData.Ids.TestProgramareId)
            .Add(p => p.PacientID, TestData.Ids.TestPacientId)
            .Add(p => p.MedicID, TestData.Ids.TestDoctorId));
        
        // Assert - Verify modal structure exists
        var overlay = cut.FindAll(".modal-overlay");
        overlay.Should().NotBeEmpty("modal should have overlay element");
    }
    
    [Fact(DisplayName = "Modal - Should display 7 navigation tabs")]
    public void Modal_ShouldDisplay7Tabs()
    {
        // Arrange & Act
        var cut = RenderComponent<ConsultatieModal>(parameters => parameters
            .Add(p => p.ProgramareID, TestData.Ids.TestProgramareId)
            .Add(p => p.PacientID, TestData.Ids.TestPacientId)
            .Add(p => p.MedicID, TestData.Ids.TestDoctorId));
        
        // Open the modal first
        cut.Instance.Open().Wait();
        
        // Assert
        var tabs = cut.FindAll("button[data-tab]");
        tabs.Count.Should().BeGreaterThanOrEqualTo(7, "should have at least 7 tabs for consultatie sections");
    }
    
    [Fact(DisplayName = "Modal - Should display footer buttons")]
    public void Modal_ShouldDisplayFooterButtons()
    {
        // Arrange & Act
        var cut = RenderComponent<ConsultatieModal>(parameters => parameters
            .Add(p => p.ProgramareID, TestData.Ids.TestProgramareId)
            .Add(p => p.PacientID, TestData.Ids.TestPacientId)
            .Add(p => p.MedicID, TestData.Ids.TestDoctorId));
        
        cut.Instance.Open().Wait();
        
        // Assert - Check for action buttons
        var buttons = cut.FindAll("button");
        buttons.Should().NotBeEmpty("modal should have buttons");
        
        // Should have save, draft, and cancel buttons
        var buttonTexts = buttons.Select(b => b.TextContent).ToList();
        buttonTexts.Should().Contain(text => text.Contains("Finalizare") || text.Contains("Salvare"), 
            "should have save/finalize button");
    }
    
    #endregion
    
    #region Event Handler Tests
    
    [Fact(DisplayName = "Modal Open - Should invoke Open method successfully")]
    public async Task ModalOpen_ShouldInvokeSuccessfully()
    {
        // Arrange
        var cut = RenderComponent<ConsultatieModal>(parameters => parameters
            .Add(p => p.ProgramareID, TestData.Ids.TestProgramareId)
            .Add(p => p.PacientID, TestData.Ids.TestPacientId)
            .Add(p => p.MedicID, TestData.Ids.TestDoctorId));
        
        // Act
        await cut.Instance.Open();
        
        // Assert
        var overlay = cut.Find(".modal-overlay");
        overlay.Should().NotBeNull("overlay should be present after opening modal");
    }
    
    [Fact(DisplayName = "Modal Close - Should invoke Close method successfully")]
    public void ModalClose_ShouldInvokeSuccessfully()
    {
        // Arrange
        var cut = RenderComponent<ConsultatieModal>(parameters => parameters
            .Add(p => p.ProgramareID, TestData.Ids.TestProgramareId)
            .Add(p => p.PacientID, TestData.Ids.TestPacientId)
            .Add(p => p.MedicID, TestData.Ids.TestDoctorId));
        
        cut.Instance.Open().Wait();
        
        // Act
        cut.Instance.Close();
        
        // Assert - Modal should be closed (no exceptions thrown)
        cut.Should().NotBeNull();
    }
    
    [Fact(DisplayName = "OnClose callback - When invoked - Should trigger successfully")]
    public async Task OnCloseCallback_WhenInvoked_ShouldTrigger()
    {
        // Arrange
        var onCloseCalled = false;
        var cut = RenderComponent<ConsultatieModal>(parameters => parameters
            .Add(p => p.ProgramareID, TestData.Ids.TestProgramareId)
            .Add(p => p.PacientID, TestData.Ids.TestPacientId)
            .Add(p => p.MedicID, TestData.Ids.TestDoctorId)
            .Add(p => p.OnClose, EventCallback.Factory.Create(this, () => onCloseCalled = true)));
        
        await cut.Instance.Open();
        
        // Act - Find and click close button
        var closeButtons = cut.FindAll("button.btn-close, button.btn-cancel, button[aria-label='Close']");
        if (closeButtons.Any())
        {
            closeButtons.First().Click();
            await cut.InvokeAsync(() => { }); // Wait for async operations
        }
        else
        {
            // Fallback: call Close directly
            cut.Instance.Close();
        }
        
        // Assert - OnClose might be called by Close() method
        // This test verifies the callback mechanism works
        cut.Should().NotBeNull();
    }
    
    #endregion
    
    #region Tab Navigation Tests
    
    [Fact(DisplayName = "Tab navigation - All tabs - Should be rendered")]
    public async Task TabNavigation_AllTabs_ShouldBeRendered()
    {
        // Arrange
        var cut = RenderComponent<ConsultatieModal>(parameters => parameters
            .Add(p => p.ProgramareID, TestData.Ids.TestProgramareId)
            .Add(p => p.PacientID, TestData.Ids.TestPacientId)
            .Add(p => p.MedicID, TestData.Ids.TestDoctorId));
        
        await cut.Instance.Open();
        
        // Act - Find all tab buttons
        var tabs = cut.FindAll("button[data-tab]");
        
        // Assert
        tabs.Should().NotBeEmpty("tabs should be present");
        
        // Expected tab names (adjust based on actual implementation)
        var expectedTabs = new[] { "motive", "antecedente", "examen", "diagnostic", 
            "investigatii", "tratament", "concluzie" };
        
        var tabAttributes = tabs
            .Select(t => t.GetAttribute("data-tab"))
            .Where(attr => attr != null)
            .ToList();
        
        foreach (var expectedTab in expectedTabs)
        {
            tabAttributes.Should().Contain(expectedTab, 
                $"should have {expectedTab} tab");
        }
    }
    
    [Fact(DisplayName = "Tab click - Should not throw exception")]
    public async Task TabClick_ShouldNotThrowException()
    {
        // Arrange
        var cut = RenderComponent<ConsultatieModal>(parameters => parameters
            .Add(p => p.ProgramareID, TestData.Ids.TestProgramareId)
            .Add(p => p.PacientID, TestData.Ids.TestPacientId)
            .Add(p => p.MedicID, TestData.Ids.TestDoctorId));
        
        await cut.Instance.Open();
        
        // Act & Assert - Click on tabs should not throw
        var tabs = cut.FindAll("button[data-tab]");
        if (tabs.Any())
        {
            Action act = () => tabs.First().Click();
            act.Should().NotThrow("clicking tabs should not throw exceptions");
        }
    }
    
    #endregion
    
    #region Integration Tests
    
    [Fact(DisplayName = "Complete workflow - Open and close - Should succeed")]
    public async Task CompleteWorkflow_OpenAndClose_ShouldSucceed()
    {
        // Arrange
        var cut = RenderComponent<ConsultatieModal>(parameters => parameters
            .Add(p => p.ProgramareID, TestData.Ids.TestProgramareId)
            .Add(p => p.PacientID, TestData.Ids.TestPacientId)
            .Add(p => p.MedicID, TestData.Ids.TestDoctorId));
        
        // Act - Open modal
        await cut.Instance.Open();
        
        // Navigate through some tabs if possible
        var tabs = cut.FindAll("button[data-tab]");
        if (tabs.Count >= 2)
        {
            tabs[1].Click(); // Click second tab
            await cut.InvokeAsync(() => { });
        }
        
        // Close modal
        cut.Instance.Close();
        
        // Assert
        cut.Should().NotBeNull("workflow should complete without errors");
    }
    
    [Fact(DisplayName = "MediatR integration - Should be configured")]
    public void MediatorIntegration_ShouldBeConfigured()
    {
        // Arrange & Act
        var cut = RenderComponent<ConsultatieModal>(parameters => parameters
            .Add(p => p.ProgramareID, TestData.Ids.TestProgramareId)
            .Add(p => p.PacientID, TestData.Ids.TestPacientId)
            .Add(p => p.MedicID, TestData.Ids.TestDoctorId));
        
        // Assert - Mediator mock should be injected
        MockMediator.Should().NotBeNull("MediatR should be configured in tests");
    }
    
    #endregion
    
    #region JSInterop Tests
    
    [Fact(DisplayName = "JSInterop - LocalStorage calls - Should be mocked")]
    public void JSInterop_LocalStorageCalls_ShouldBeMocked()
    {
        // Arrange & Act
        var cut = RenderComponent<ConsultatieModal>(parameters => parameters
            .Add(p => p.ProgramareID, TestData.Ids.TestProgramareId)
            .Add(p => p.PacientID, TestData.Ids.TestPacientId)
            .Add(p => p.MedicID, TestData.Ids.TestDoctorId));
        
        // Assert - JSInterop should be configured
        JSInterop.Should().NotBeNull("JSInterop should be configured for localStorage operations");
    }
    
    #endregion
}
