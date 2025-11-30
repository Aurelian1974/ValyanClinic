using FluentAssertions;
using ValyanClinic.Application.Features.ConsultatieManagement.Commands.CreateConsultatie;
using ValyanClinic.Components.Shared.Consultatie.Tabs;
using Xunit;

namespace ValyanClinic.Tests.Components.Consultatie;

/// <summary>
/// Unit tests pentru AntecedenteTab component
/// Testează logica de validare și completitudine pentru 4 subsecțiuni
/// </summary>
public class AntecedenteTabTests
{
    #region Test Setup
    
    private static AntecedenteTab CreateComponent(CreateConsultatieCommand? model = null, string? pacientSex = null)
    {
        var component = new AntecedenteTab();
        
        var modelProperty = typeof(AntecedenteTab).GetProperty(nameof(AntecedenteTab.Model));
        modelProperty?.SetValue(component, model ?? new CreateConsultatieCommand());
        
        if (pacientSex != null)
        {
            var sexProperty = typeof(AntecedenteTab).GetProperty(nameof(AntecedenteTab.PacientSex));
            sexProperty?.SetValue(component, pacientSex);
        }
        
        var isActiveProperty = typeof(AntecedenteTab).GetProperty(nameof(AntecedenteTab.IsActive));
        isActiveProperty?.SetValue(component, true);
        
        return component;
    }
    
    private static bool GetIsSectionCompleted(AntecedenteTab component)
    {
        var property = typeof(AntecedenteTab).GetProperty("IsSectionCompleted", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        return (bool)(property?.GetValue(component) ?? false);
    }
    
    #endregion

    #region Validation Tests
    
    [Fact(DisplayName = "IsSectionCompleted - Toate câmpurile goale returnează false")]
    public void IsSectionCompleted_EmptyFields_ReturnsFalse()
    {
        // Arrange
        var model = new CreateConsultatieCommand();
        var component = CreateComponent(model);
        
        // Act
        var isCompleted = GetIsSectionCompleted(component);
        
        // Assert
        isCompleted.Should().BeFalse("niciun câmp nu este completat");
    }
    
    [Fact(DisplayName = "IsSectionCompleted - Doar AHC completat nu e suficient")]
    public void IsSectionCompleted_OnlyAHC_ReturnsFalse()
    {
        // Arrange
        var model = new CreateConsultatieCommand
        {
            AHC_Mama = "Diabet tip 2",
            AHC_Tata = "HTA"
        };
        var component = CreateComponent(model);
        
        // Act
        var isCompleted = GetIsSectionCompleted(component);
        
        // Assert
        isCompleted.Should().BeFalse("nu sunt completate toate subsecțiunile");
    }
    
    [Fact(DisplayName = "IsSectionCompleted - Toate subsecțiunile completate returnează true")]
    public void IsSectionCompleted_AllSubsections_ReturnsTrue()
    {
        // Arrange
        var model = new CreateConsultatieCommand
        {
            AHC_Mama = "Diabet tip 2",
            AF_Nastere = "La termen",
            APP_BoliAdult = "Astm",
            APP_Alergii = "Polen",
            Profesie = "Inginer",
            ConditiiMunca = "Birou"
        };
        var component = CreateComponent(model);
        
        // Act
        var isCompleted = GetIsSectionCompleted(component);
        
        // Assert
        isCompleted.Should().BeTrue("toate subsecțiunile au câmpuri completate");
    }
    
    #endregion
    
    #region AHC Tests
    
    [Theory(DisplayName = "AHC - Orice câmp completat face subsecțiunea validă")]
    [InlineData(nameof(CreateConsultatieCommand.AHC_Mama), "Diabet")]
    [InlineData(nameof(CreateConsultatieCommand.AHC_Tata), "HTA")]
    [InlineData(nameof(CreateConsultatieCommand.AHC_Frati), "Sănătoși")]
    [InlineData(nameof(CreateConsultatieCommand.AHC_Bunici), "Cancer")]
    [InlineData(nameof(CreateConsultatieCommand.AHC_Altele), "Nimic")]
    public void AHC_SingleFieldFilled_Valid(string propertyName, string value)
    {
        // Arrange
        var model = new CreateConsultatieCommand
        {
            AF_Nastere = "Test",
            APP_BoliAdult = "Test",
            APP_Alergii = "Test",
            Profesie = "Test",
            ConditiiMunca = "Test"
        };
        
        var property = typeof(CreateConsultatieCommand).GetProperty(propertyName);
        property?.SetValue(model, value);
        
        var component = CreateComponent(model);
        
        // Act
        var isCompleted = GetIsSectionCompleted(component);
        
        // Assert
        isCompleted.Should().BeTrue($"câmpul {propertyName} este completat");
    }
    
    #endregion
    
    #region AF Tests
    
    [Fact(DisplayName = "AF - Câmpuri specifice femei sunt vizibile pentru PacientSex=F")]
    public void AF_FemaleFields_VisibleForFemale()
    {
        // Arrange
        var model = new CreateConsultatieCommand
        {
            AHC_Mama = "Test",
            AF_Menstruatie = "12 ani",
            APP_BoliAdult = "Test",
            APP_Alergii = "Test",
            Profesie = "Test",
            ConditiiMunca = "Test"
        };
        var component = CreateComponent(model, pacientSex: "F");
        
        // Act
        var isCompleted = GetIsSectionCompleted(component);
        
        // Assert
        isCompleted.Should().BeTrue();
        model.AF_Menstruatie.Should().Be("12 ani");
    }
    
    #endregion
    
    #region APP Tests
    
    [Fact(DisplayName = "APP - Un singur câmp nu e suficient")]
    public void APP_OneField_NotSufficient()
    {
        // Arrange
        var model = new CreateConsultatieCommand
        {
            AHC_Mama = "Test",
            AF_Nastere = "Test",
            APP_BoliAdult = "Astm",
            Profesie = "Test",
            ConditiiMunca = "Test"
        };
        var component = CreateComponent(model);
        
        // Act
        var isCompleted = GetIsSectionCompleted(component);
        
        // Assert
        isCompleted.Should().BeFalse("APP necesită minim 2 câmpuri");
    }
    
    [Fact(DisplayName = "APP - Două câmpuri sunt suficiente")]
    public void APP_TwoFields_Sufficient()
    {
        // Arrange
        var model = new CreateConsultatieCommand
        {
            AHC_Mama = "Test",
            AF_Nastere = "Test",
            APP_BoliAdult = "Astm",
            APP_Alergii = "Polen",
            Profesie = "Test",
            ConditiiMunca = "Test"
        };
        var component = CreateComponent(model);
        
        // Act
        var isCompleted = GetIsSectionCompleted(component);
        
        // Assert
        isCompleted.Should().BeTrue("APP are 2 câmpuri completate");
    }
    
    #endregion
    
    #region Socio Tests
    
    [Fact(DisplayName = "Socio - Un câmp nu e suficient")]
    public void Socio_OneField_NotSufficient()
    {
        // Arrange
        var model = new CreateConsultatieCommand
        {
            AHC_Mama = "Test",
            AF_Nastere = "Test",
            APP_BoliAdult = "Test",
            APP_Alergii = "Test",
            Profesie = "Inginer"
        };
        var component = CreateComponent(model);
        
        // Act
        var isCompleted = GetIsSectionCompleted(component);
        
        // Assert
        isCompleted.Should().BeFalse("Socio necesită minim 2 câmpuri");
    }
    
    [Fact(DisplayName = "Socio - Două câmpuri sunt suficiente")]
    public void Socio_TwoFields_Sufficient()
    {
        // Arrange
        var model = new CreateConsultatieCommand
        {
            AHC_Mama = "Test",
            AF_Nastere = "Test",
            APP_BoliAdult = "Test",
            APP_Alergii = "Test",
            Profesie = "Inginer",
            ConditiiMunca = "Birou"
        };
        var component = CreateComponent(model);
        
        // Act
        var isCompleted = GetIsSectionCompleted(component);
        
        // Assert
        isCompleted.Should().BeTrue("Socio are 2 câmpuri completate");
    }
    
    #endregion
    
    #region Edge Cases
    
    [Fact(DisplayName = "Edge Case - Whitespace nu e considerat completat")]
    public void EdgeCase_Whitespace_NotValid()
    {
        // Arrange
        var model = new CreateConsultatieCommand
        {
            AHC_Mama = "   ",
            AF_Nastere = "\t\n",
            APP_BoliAdult = "  ",
            Profesie = "    "
        };
        var component = CreateComponent(model);
        
        // Act
        var isCompleted = GetIsSectionCompleted(component);
        
        // Assert
        isCompleted.Should().BeFalse("whitespace-only nu e valid");
    }
    
    [Fact(DisplayName = "Edge Case - Text lung e acceptat")]
    public void EdgeCase_LongText_Valid()
    {
        // Arrange
        var longText = new string('A', 5000);
        var model = new CreateConsultatieCommand
        {
            AHC_Mama = longText,
            AF_Nastere = longText,
            APP_BoliAdult = longText,
            APP_Alergii = longText,
            Profesie = longText,
            ConditiiMunca = longText
        };
        var component = CreateComponent(model);
        
        // Act
        var isCompleted = GetIsSectionCompleted(component);
        
        // Assert
        isCompleted.Should().BeTrue("text lung e valid");
    }
    
    #endregion
    
    #region Real Scenarios
    
    [Fact(DisplayName = "Scenariu Real - Pacient adult cu istoric complet")]
    public void RealScenario_AdultComplete_Valid()
    {
        // Arrange
        var model = new CreateConsultatieCommand
        {
            AHC_Mama = "Diabet tip 2, HTA",
            AHC_Tata = "Infarct la 55 ani",
            AF_Nastere = "La termen",
            AF_Dezvoltare = "Normală",
            APP_BoliAdult = "Astm de 5 ani",
            APP_Alergii = "Polen, praf",
            APP_Medicatie = "Ventolin",
            Profesie = "Inginer software",
            ConditiiMunca = "Birou, 8h/zi",
            Toxice = "Nefumător"
        };
        var component = CreateComponent(model);
        
        // Act
        var isCompleted = GetIsSectionCompleted(component);
        
        // Assert
        isCompleted.Should().BeTrue();
        model.APP_BoliAdult.Should().Contain("Astm");
    }
    
    [Fact(DisplayName = "Scenariu Real - Pacient fără istoric semnificativ")]
    public void RealScenario_MinimalHistory_Valid()
    {
        // Arrange
        var model = new CreateConsultatieCommand
        {
            AHC_Mama = "Fără antecedente",
            AF_Nastere = "La termen",
            APP_BoliAdult = "Fără boli cronice",
            APP_Alergii = "Fără alergii",
            Profesie = "Student",
            ConditiiMunca = "N/A"
        };
        var component = CreateComponent(model);
        
        // Act
        var isCompleted = GetIsSectionCompleted(component);
        
        // Assert
        isCompleted.Should().BeTrue();
    }
    
    #endregion
}
