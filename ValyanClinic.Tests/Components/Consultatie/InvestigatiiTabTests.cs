using FluentAssertions;
using ValyanClinic.Application.Features.ConsultatieManagement.Commands.CreateConsultatie;
using ValyanClinic.Components.Shared.Consultatie.Tabs;
using Xunit;

namespace ValyanClinic.Tests.Components.Consultatie;

/// <summary>
/// Unit tests pentru InvestigatiiTab component
/// Testează logica de validare - necesită minim 2 tipuri de investigații
/// </summary>
public class InvestigatiiTabTests
{
    #region Test Setup
    
    private static InvestigatiiTab CreateComponent(CreateConsultatieCommand? model = null)
    {
        var component = new InvestigatiiTab();
        
        var modelProperty = typeof(InvestigatiiTab).GetProperty(nameof(InvestigatiiTab.Model));
        modelProperty?.SetValue(component, model ?? new CreateConsultatieCommand());
        
        var isActiveProperty = typeof(InvestigatiiTab).GetProperty(nameof(InvestigatiiTab.IsActive));
        isActiveProperty?.SetValue(component, true);
        
        return component;
    }
    
    private static bool GetIsSectionCompleted(InvestigatiiTab component)
    {
        var property = typeof(InvestigatiiTab).GetProperty("IsSectionCompleted", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        return (bool)(property?.GetValue(component) ?? false);
    }
    
    #endregion

    #region Validation Tests
    
    [Fact(DisplayName = "IsSectionCompleted - Returnează false când toate câmpurile sunt goale")]
    public void IsSectionCompleted_EmptyFields_ReturnsFalse()
    {
        // Arrange
        var model = new CreateConsultatieCommand();
        var component = CreateComponent(model);
        
        // Act
        var isCompleted = GetIsSectionCompleted(component);
        
        // Assert
        isCompleted.Should().BeFalse("niciun tip de investigație nu este completat");
    }
    
    [Fact(DisplayName = "IsSectionCompleted - Returnează false cu doar 1 tip completat")]
    public void IsSectionCompleted_OnlyOneType_ReturnsFalse()
    {
        // Arrange
        var model = new CreateConsultatieCommand
        {
            InvestigatiiLaborator = "Hemoleucogramă: WBC 7.5, RBC 4.8"
        };
        var component = CreateComponent(model);
        
        // Act
        var isCompleted = GetIsSectionCompleted(component);
        
        // Assert
        isCompleted.Should().BeFalse("necesită minim 2 tipuri de investigații");
    }
    
    [Fact(DisplayName = "IsSectionCompleted - Returnează true cu 2 tipuri completate")]
    public void IsSectionCompleted_TwoTypes_ReturnsTrue()
    {
        // Arrange
        var model = new CreateConsultatieCommand
        {
            InvestigatiiLaborator = "Hemoleucogramă: WBC 7.5",
            InvestigatiiImagistice = "Radiografie pulmonară: normale"
        };
        var component = CreateComponent(model);
        
        // Act
        var isCompleted = GetIsSectionCompleted(component);
        
        // Assert
        isCompleted.Should().BeTrue("2 tipuri de investigații sunt completate");
    }
    
    [Fact(DisplayName = "IsSectionCompleted - Returnează true cu toate tipurile completate")]
    public void IsSectionCompleted_AllTypes_ReturnsTrue()
    {
        // Arrange
        var model = new CreateConsultatieCommand
        {
            InvestigatiiLaborator = "Hemoleucogramă completă",
            InvestigatiiImagistice = "Radiografie torace",
            InvestigatiiEKG = "Ritm sinusal, fără modificări",
            AlteInvestigatii = "Spirometrie normală"
        };
        var component = CreateComponent(model);
        
        // Act
        var isCompleted = GetIsSectionCompleted(component);
        
        // Assert
        isCompleted.Should().BeTrue("toate cele 4 tipuri sunt completate");
    }
    
    #endregion
    
    #region Individual Field Tests
    
    [Theory(DisplayName = "Combinații valide de 2 tipuri de investigații")]
    [InlineData("Laborator", "Imagistice")]
    [InlineData("Laborator", "EKG")]
    [InlineData("Laborator", "Alte")]
    [InlineData("Imagistice", "EKG")]
    [InlineData("Imagistice", "Alte")]
    [InlineData("EKG", "Alte")]
    public void TwoTypeCombinations_AllValid(string type1, string type2)
    {
        // Arrange
        var model = new CreateConsultatieCommand();
        
        if (type1 == "Laborator" || type2 == "Laborator")
            model.InvestigatiiLaborator = "Test data";
        if (type1 == "Imagistice" || type2 == "Imagistice")
            model.InvestigatiiImagistice = "Test data";
        if (type1 == "EKG" || type2 == "EKG")
            model.InvestigatiiEKG = "Test data";
        if (type1 == "Alte" || type2 == "Alte")
            model.AlteInvestigatii = "Test data";
        
        var component = CreateComponent(model);
        
        // Act
        var isCompleted = GetIsSectionCompleted(component);
        
        // Assert
        isCompleted.Should().BeTrue($"combinația {type1}+{type2} este validă");
    }
    
    #endregion
    
    #region Edge Cases
    
    [Fact(DisplayName = "Edge Case - Whitespace-only nu sunt considerate completate")]
    public void EdgeCase_WhitespaceOnly_NotValid()
    {
        // Arrange
        var model = new CreateConsultatieCommand
        {
            InvestigatiiLaborator = "   ",
            InvestigatiiImagistice = "\t\n"
        };
        var component = CreateComponent(model);
        
        // Act
        var isCompleted = GetIsSectionCompleted(component);
        
        // Assert
        isCompleted.Should().BeFalse("whitespace-only nu contează ca date valide");
    }
    
    [Fact(DisplayName = "Edge Case - 3 tipuri completate este și mai bine")]
    public void EdgeCase_ThreeTypes_Valid()
    {
        // Arrange
        var model = new CreateConsultatieCommand
        {
            InvestigatiiLaborator = "Analize complete",
            InvestigatiiImagistice = "CT abdomen",
            InvestigatiiEKG = "Normal"
        };
        var component = CreateComponent(model);
        
        // Act
        var isCompleted = GetIsSectionCompleted(component);
        
        // Assert
        isCompleted.Should().BeTrue("3 tipuri este mai mult decât minimul necesar");
    }
    
    [Fact(DisplayName = "Edge Case - Text foarte lung este acceptat")]
    public void EdgeCase_VeryLongText_Valid()
    {
        // Arrange
        var longText = new string('A', 10000);
        var model = new CreateConsultatieCommand
        {
            InvestigatiiLaborator = longText,
            InvestigatiiImagistice = longText
        };
        var component = CreateComponent(model);
        
        // Act
        var isCompleted = GetIsSectionCompleted(component);
        
        // Assert
        isCompleted.Should().BeTrue("text lung este valid");
    }
    
    #endregion
    
    #region Real Scenarios
    
    [Fact(DisplayName = "Scenariu Real - Check-up complet cu toate investigațiile")]
    public void RealScenario_CompleteCheckup_Valid()
    {
        // Arrange
        var model = new CreateConsultatieCommand
        {
            InvestigatiiLaborator = @"
                Hemoleucogramă: WBC 7.2, RBC 4.5, Hb 14.5
                Biochimie: Glicemie 95 mg/dl, Colesterol 185 mg/dl
                VSH 12 mm/h",
            InvestigatiiImagistice = @"
                Radiografie pulmonară: Câmpuri pulmonare normale
                Ecocardiografie: Fracție ejecție 60%",
            InvestigatiiEKG = "Ritm sinusal, 72 bpm, fără modificări ST-T",
            AlteInvestigatii = "Spirometrie: FEV1 95%, normală"
        };
        var component = CreateComponent(model);
        
        // Act
        var isCompleted = GetIsSectionCompleted(component);
        
        // Assert
        isCompleted.Should().BeTrue();
        model.InvestigatiiLaborator.Should().Contain("Hemoleucogramă");
        model.InvestigatiiEKG.Should().Contain("Ritm sinusal");
    }
    
    [Fact(DisplayName = "Scenariu Real - Investigații de urgență minimale")]
    public void RealScenario_EmergencyMinimal_Valid()
    {
        // Arrange
        var model = new CreateConsultatieCommand
        {
            InvestigatiiLaborator = "Hemoleucogramă urgență, Creatinină",
            InvestigatiiEKG = "EKG 12 derivații: normal"
        };
        var component = CreateComponent(model);
        
        // Act
        var isCompleted = GetIsSectionCompleted(component);
        
        // Assert
        isCompleted.Should().BeTrue("2 investigații sunt suficiente pentru urgență");
    }
    
    [Fact(DisplayName = "Scenariu Real - Consultație fără investigații nu este validă")]
    public void RealScenario_NoInvestigations_NotValid()
    {
        // Arrange
        var model = new CreateConsultatieCommand
        {
            // Pacientul nu a făcut investigații
        };
        var component = CreateComponent(model);
        
        // Act
        var isCompleted = GetIsSectionCompleted(component);
        
        // Assert
        isCompleted.Should().BeFalse("lipsesc investigațiile");
    }
    
    [Fact(DisplayName = "Scenariu Real - Doar analize de sânge și RX sunt comune")]
    public void RealScenario_BloodTestsAndXray_Valid()
    {
        // Arrange
        var model = new CreateConsultatieCommand
        {
            InvestigatiiLaborator = "Hemoleucogramă, VSH, transaminaze",
            InvestigatiiImagistice = "Radiografie torace PA"
        };
        var component = CreateComponent(model);
        
        // Act
        var isCompleted = GetIsSectionCompleted(component);
        
        // Assert
        isCompleted.Should().BeTrue("combinația cea mai frecventă");
    }
    
    #endregion
    
    #region Negative Tests
    
    [Fact(DisplayName = "Negative Test - Null model aruncă NullReferenceException")]
    public void NegativeTest_NullModel_ThrowsNullReferenceException()
    {
        // Arrange
        var component = new InvestigatiiTab();
        var modelProperty = typeof(InvestigatiiTab).GetProperty(nameof(InvestigatiiTab.Model));
        modelProperty?.SetValue(component, null);
        
        // Act
        Action act = () => GetIsSectionCompleted(component);
        
        // Assert
        act.Should().Throw<System.Reflection.TargetInvocationException>()
            .WithInnerException<NullReferenceException>("model-ul nu poate fi null în Blazor components");
    }
    
    #endregion
}
