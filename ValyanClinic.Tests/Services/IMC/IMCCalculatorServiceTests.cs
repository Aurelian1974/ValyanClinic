using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using ValyanClinic.Application.Services.IMC;
using Xunit;

namespace ValyanClinic.Tests.Services.IMC;

/// <summary>
/// Unit tests pentru IMCCalculatorService
/// Validează calculele IMC conform standardelor OMS
/// </summary>
public class IMCCalculatorServiceTests
{
    private readonly IIMCCalculatorService _service;
    private readonly Mock<ILogger<IMCCalculatorService>> _loggerMock;

    public IMCCalculatorServiceTests()
    {
        _loggerMock = new Mock<ILogger<IMCCalculatorService>>();
        _service = new IMCCalculatorService(_loggerMock.Object);
    }

    #region Calculate Tests - Categorii IMC

    [Theory(DisplayName = "Calculate - Subponderal (IMC < 18.5)")]
    [InlineData(50, 170, 17.30)] // Foarte subponderal
    [InlineData(55, 175, 17.96)] // La limită
    public void Calculate_SubponderalCategory_ReturnsCorrectCategory(decimal greutate, decimal inaltime, decimal expectedIMC)
    {
        // Act
        var result = _service.Calculate(greutate, inaltime);

        // Assert
        result.Should().NotBeNull();
        result.Value.Should().Be(expectedIMC);
        result.Category.Should().Be(IMCCategory.Subponderal);
        result.Interpretation.Should().Contain("Subponderal");
        result.ColorClass.Should().Be("imc-badge-subponderal");
        result.HealthRisk.Should().Be("Medium");
    }

    [Theory(DisplayName = "Calculate - Normal (IMC 18.5-24.9)")]
    [InlineData(65, 175, 21.22)] // IMC normal jos
    [InlineData(70, 170, 24.22)] // IMC normal sus
    [InlineData(80, 180, 24.69)] // Aproape de supraponderal
    public void Calculate_NormalCategory_ReturnsCorrectCategory(decimal greutate, decimal inaltime, decimal expectedIMC)
    {
        // Act
        var result = _service.Calculate(greutate, inaltime);

        // Assert
        result.Should().NotBeNull();
        result.Value.Should().Be(expectedIMC);
        result.Category.Should().Be(IMCCategory.Normal);
        result.Interpretation.Should().Be("Greutate normală");
        result.ColorClass.Should().Be("imc-badge-normal");
        result.HealthRisk.Should().Be("Low");
    }

    [Theory(DisplayName = "Calculate - Supraponderal (IMC 25-29.9)")]
    [InlineData(85, 180, 26.23)] // Supraponderal jos
    [InlineData(90, 175, 29.39)] // Aproape obezitate
    public void Calculate_SupraponderalCategory_ReturnsCorrectCategory(decimal greutate, decimal inaltime, decimal expectedIMC)
    {
        // Act
        var result = _service.Calculate(greutate, inaltime);

        // Assert
        result.Should().NotBeNull();
        result.Value.Should().Be(expectedIMC);
        result.Category.Should().Be(IMCCategory.Supraponderal);
        result.Interpretation.Should().Contain("Supraponderal");
        result.ColorClass.Should().Be("imc-badge-supraponderal");
        result.HealthRisk.Should().Be("Medium");
    }

    [Theory(DisplayName = "Calculate - Obezitate Grad I (IMC 30-34.9)")]
    [InlineData(95, 175, 31.02)] // Obezitate grad I jos
    [InlineData(105, 175, 34.29)] // Obezitate grad I sus
    public void Calculate_Obezitate1Category_ReturnsCorrectCategory(decimal greutate, decimal inaltime, decimal expectedIMC)
    {
        // Act
        var result = _service.Calculate(greutate, inaltime);

        // Assert
        result.Should().NotBeNull();
        result.Value.Should().Be(expectedIMC);
        result.Category.Should().Be(IMCCategory.Obezitate1);
        result.Interpretation.Should().Contain("Obezitate grad I");
        result.ColorClass.Should().Be("imc-badge-obezitate1");
        result.HealthRisk.Should().Be("High");
    }

    [Theory(DisplayName = "Calculate - Obezitate Grad II (IMC 35-39.9)")]
    [InlineData(110, 175, 35.92)] // Obezitate grad II jos
    [InlineData(120, 175, 39.18)] // Obezitate grad II sus
    public void Calculate_Obezitate2Category_ReturnsCorrectCategory(decimal greutate, decimal inaltime, decimal expectedIMC)
    {
        // Act
        var result = _service.Calculate(greutate, inaltime);

        // Assert
        result.Should().NotBeNull();
        result.Value.Should().Be(expectedIMC);
        result.Category.Should().Be(IMCCategory.Obezitate2);
        result.Interpretation.Should().Contain("Obezitate grad II");
        result.ColorClass.Should().Be("imc-badge-obezitate2");
        result.HealthRisk.Should().Be("VeryHigh");
    }

    [Theory(DisplayName = "Calculate - Obezitate Morbidă (IMC >= 40)")]
    [InlineData(125, 175, 40.82)] // La limită
    [InlineData(150, 175, 48.98)] // Foarte obez - CORECTAT: 48.98 nu 49.00
    public void Calculate_ObezitateMorbidaCategory_ReturnsCorrectCategory(decimal greutate, decimal inaltime, decimal expectedIMC)
    {
        // Act
        var result = _service.Calculate(greutate, inaltime);

        // Assert
        result.Should().NotBeNull();
        result.Value.Should().Be(expectedIMC);
        result.Category.Should().Be(IMCCategory.ObezitateMorbida);
        result.Interpretation.Should().Contain("Obezitate morbidă");
        result.ColorClass.Should().Be("imc-badge-obezitate-morbida");
        result.HealthRisk.Should().Be("Critical");
    }

    #endregion

    #region Calculate Tests - Edge Cases

    [Fact(DisplayName = "Calculate - Valori invalide (greutate = 0) returnează Invalid")]
    public void Calculate_InvalidWeight_ReturnsInvalid()
    {
        // Act
        var result = _service.Calculate(0, 175);

        // Assert
        result.Should().NotBeNull();
        result.Category.Should().Be(IMCCategory.Invalid);
        result.Interpretation.Should().Contain("Date insuficiente");
    }

    [Fact(DisplayName = "Calculate - Valori invalide (înălțime = 0) returnează Invalid")]
    public void Calculate_InvalidHeight_ReturnsInvalid()
    {
        // Act
        var result = _service.Calculate(70, 0);

        // Assert
        result.Should().NotBeNull();
        result.Category.Should().Be(IMCCategory.Invalid);
        result.Interpretation.Should().Contain("Date insuficiente");
    }

    [Fact(DisplayName = "Calculate - Valori invalide (greutate prea mare) returnează Invalid")]
    public void Calculate_WeightTooHigh_ReturnsInvalid()
    {
        // Act
        var result = _service.Calculate(600, 175); // > 500kg

        // Assert
        result.Should().NotBeNull();
        result.Category.Should().Be(IMCCategory.Invalid);
    }

    [Fact(DisplayName = "Calculate - Valori invalide (înălțime prea mică) returnează Invalid")]
    public void Calculate_HeightTooLow_ReturnsInvalid()
    {
        // Act
        var result = _service.Calculate(70, 20); // < 30cm

        // Assert
        result.Should().NotBeNull();
        result.Category.Should().Be(IMCCategory.Invalid);
    }

    #endregion

    #region AreValuesValid Tests

    [Theory(DisplayName = "AreValuesValid - Valori valide returnează true")]
    [InlineData(50, 150)]
    [InlineData(70, 170)]
    [InlineData(100, 180)]
    [InlineData(150, 200)]
    public void AreValuesValid_ValidValues_ReturnsTrue(decimal greutate, decimal inaltime)
    {
        // Act
        var result = _service.AreValuesValid(greutate, inaltime);

        // Assert
        result.Should().BeTrue();
    }

    [Theory(DisplayName = "AreValuesValid - Valori invalide returnează false")]
    [InlineData(0, 170)]        // Greutate 0
    [InlineData(70, 0)]         // Înălțime 0
    [InlineData(-10, 170)]      // Greutate negativă
    [InlineData(70, -10)]       // Înălțime negativă
    [InlineData(600, 170)]      // Greutate prea mare
    [InlineData(70, 400)]       // Înălțime prea mare
    public void AreValuesValid_InvalidValues_ReturnsFalse(decimal greutate, decimal inaltime)
    {
        // Act
        var result = _service.AreValuesValid(greutate, inaltime);

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region CalculateIdealWeight Tests

    [Theory(DisplayName = "CalculateIdealWeight - Bărbați (Formula Lorentz)")]
    [InlineData(170, "M", 65.0)]   // (170-100) - ((170-150)/4) = 70 - 5 = 65
    [InlineData(180, "M", 72.5)]   // (180-100) - ((180-150)/4) = 80 - 7.5 = 72.5
    [InlineData(190, "M", 80.0)]   // (190-100) - ((190-150)/4) = 90 - 10 = 80
    public void CalculateIdealWeight_Male_ReturnsCorrectWeight(decimal inaltime, string sex, decimal expected)
    {
        // Act
        var result = _service.CalculateIdealWeight(inaltime, sex);

        // Assert
        result.Should().Be(expected);
    }

    [Theory(DisplayName = "CalculateIdealWeight - Femei (Formula Lorentz)")]
    [InlineData(160, "F", 56.0)]   // (160-100) - ((160-150)/2.5) = 60 - 4 = 56
    [InlineData(170, "F", 62.0)]   // (170-100) - ((170-150)/2.5) = 70 - 8 = 62
    [InlineData(180, "F", 68.0)]   // (180-100) - ((180-150)/2.5) = 80 - 12 = 68
    public void CalculateIdealWeight_Female_ReturnsCorrectWeight(decimal inaltime, string sex, decimal expected)
    {
        // Act
        var result = _service.CalculateIdealWeight(inaltime, sex);

        // Assert
        result.Should().Be(expected);
    }

    [Fact(DisplayName = "CalculateIdealWeight - Înălțime invalidă returnează 0")]
    public void CalculateIdealWeight_InvalidHeight_ReturnsZero()
    {
        // Act
        var result = _service.CalculateIdealWeight(20, "M"); // < 30cm

        // Assert
        result.Should().Be(0);
    }

    #endregion

    #region Real World Scenarios

    [Fact(DisplayName = "Scenariu Real - Adult normal (75kg, 175cm)")]
    public void RealScenario_NormalAdult_ReturnsExpectedResult()
    {
        // Arrange
        var greutate = 75m;
        var inaltime = 175m;

        // Act
        var result = _service.Calculate(greutate, inaltime);

        // Assert
        result.Should().NotBeNull();
        result.Value.Should().Be(24.49m);
        result.Category.Should().Be(IMCCategory.Normal);
        result.HealthRisk.Should().Be("Low");
        result.HealthRecommendation.Should().Contain("menținerea greutății actuale");
    }

    [Fact(DisplayName = "Scenariu Real - Pacient cu risc (BMI 35.5)")]
    public void RealScenario_HighRiskPatient_ReturnsWarningRecommendation()
    {
        // Arrange
        var greutate = 110m;
        var inaltime = 175m;

        // Act
        var result = _service.Calculate(greutate, inaltime);

        // Assert
        result.Should().NotBeNull();
        result.Value.Should().Be(35.92m);
        result.Category.Should().Be(IMCCategory.Obezitate2);
        result.HealthRisk.Should().Be("VeryHigh");
        result.HealthRecommendation.Should().Contain("evaluare medicală completă");
    }

    [Fact(DisplayName = "Scenariu Real - Copil subponderal (30kg, 140cm)")]
    public void RealScenario_UnderweightChild_ReturnsSubponderalWithRecommendation()
    {
        // Arrange
        var greutate = 30m;
        var inaltime = 140m;

        // Act
        var result = _service.Calculate(greutate, inaltime);

        // Assert
        result.Should().NotBeNull();
        result.Value.Should().Be(15.31m);
        result.Category.Should().Be(IMCCategory.Subponderal);
        result.HealthRisk.Should().Be("Medium");
        result.HealthRecommendation.Should().Contain("nutriționist");
    }

    #endregion

    #region Performance Tests

    [Fact(DisplayName = "Performance - 1000 calcule se execută rapid")]
    public void Performance_ThousandCalculations_ExecutesQuickly()
    {
        // Arrange
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        // Act
        for (int i = 0; i < 1000; i++)
        {
            _service.Calculate(70 + i % 50, 170 + i % 30);
        }
        stopwatch.Stop();

        // Assert
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(100, 
            "1000 calcule ar trebui să se execute în mai puțin de 100ms");
    }

    #endregion
}
