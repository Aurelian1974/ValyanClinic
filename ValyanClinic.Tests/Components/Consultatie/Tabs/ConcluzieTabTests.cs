using FluentAssertions;
using ValyanClinic.Application.Features.ConsultatieManagement.Commands.CreateConsultatie;
using ValyanClinic.Components.Shared.Consultatie.Tabs;
using Xunit;

namespace ValyanClinic.Tests.Components.Consultatie;

/// <summary>
/// Unit tests pentru ConcluzieTab component
/// Testează validare: Prognostic și Concluzie sunt OBLIGATORII
/// </summary>
public class ConcluzieTabTests
{
    #region Test Setup

    private static ConcluzieTab CreateComponent(CreateConsultatieCommand? model = null)
    {
        var component = new ConcluzieTab();

        var modelProperty = typeof(ConcluzieTab).GetProperty(nameof(ConcluzieTab.Model));
        modelProperty?.SetValue(component, model ?? new CreateConsultatieCommand());

        var isActiveProperty = typeof(ConcluzieTab).GetProperty(nameof(ConcluzieTab.IsActive));
        isActiveProperty?.SetValue(component, true);

        return component;
    }

    private static bool GetIsSectionCompleted(ConcluzieTab component)
    {
        var property = typeof(ConcluzieTab).GetProperty("IsSectionCompleted",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        return (bool)(property?.GetValue(component) ?? false);
    }

    #endregion

    #region Validation Tests

    [Fact(DisplayName = "IsSectionCompleted - Câmpuri goale returnează false")]
    public void IsSectionCompleted_EmptyFields_ReturnsFalse()
    {
        // Arrange
        var model = new CreateConsultatieCommand();
        var component = CreateComponent(model);

        // Act
        var isCompleted = GetIsSectionCompleted(component);

        // Assert
        isCompleted.Should().BeFalse("niciun câmp obligatoriu nu este completat");
    }

    [Fact(DisplayName = "IsSectionCompleted - Doar Prognostic nu e suficient")]
    public void IsSectionCompleted_OnlyPrognostic_ReturnsFalse()
    {
        // Arrange
        var model = new CreateConsultatieCommand
        {
            Prognostic = "Favorabil"
        };
        var component = CreateComponent(model);

        // Act
        var isCompleted = GetIsSectionCompleted(component);

        // Assert
        isCompleted.Should().BeFalse("concluzia lipsește");
    }

    [Fact(DisplayName = "IsSectionCompleted - Doar Concluzie nu e suficient")]
    public void IsSectionCompleted_OnlyConcluzie_ReturnsFalse()
    {
        // Arrange
        var model = new CreateConsultatieCommand
        {
            Concluzie = "Evoluție favorabilă"
        };
        var component = CreateComponent(model);

        // Act
        var isCompleted = GetIsSectionCompleted(component);

        // Assert
        isCompleted.Should().BeFalse("prognosticul lipsește");
    }

    [Fact(DisplayName = "IsSectionCompleted - Ambele obligatorii completate returnează true")]
    public void IsSectionCompleted_BothRequired_ReturnsTrue()
    {
        // Arrange
        var model = new CreateConsultatieCommand
        {
            Prognostic = "Favorabil",
            Concluzie = "Evoluție favorabilă"
        };
        var component = CreateComponent(model);

        // Act
        var isCompleted = GetIsSectionCompleted(component);

        // Assert
        isCompleted.Should().BeTrue("ambele câmpuri obligatorii sunt completate");
    }

    [Fact(DisplayName = "IsSectionCompleted - Toate câmpurile completate")]
    public void IsSectionCompleted_AllFields_ReturnsTrue()
    {
        // Arrange
        var model = new CreateConsultatieCommand
        {
            Prognostic = "Favorabil",
            Concluzie = "Evoluție favorabilă",
            ObservatiiMedic = "Pacient compliant",
            NotePacient = "Mulțumit"
        };
        var component = CreateComponent(model);

        // Act
        var isCompleted = GetIsSectionCompleted(component);

        // Assert
        isCompleted.Should().BeTrue("toate câmpurile sunt completate");
    }

    #endregion

    #region Prognostic Tests

    [Theory(DisplayName = "Prognostic - Valorile valide sunt acceptate")]
    [InlineData("Favorabil")]
    [InlineData("Rezervat")]
    [InlineData("Sever")]
    public void Prognostic_ValidValues_Accepted(string prognostic)
    {
        // Arrange
        var model = new CreateConsultatieCommand
        {
            Prognostic = prognostic,
            Concluzie = "Test"
        };
        var component = CreateComponent(model);

        // Act
        var isCompleted = GetIsSectionCompleted(component);

        // Assert
        isCompleted.Should().BeTrue($"prognostic '{prognostic}' e valid");
        model.Prognostic.Should().Be(prognostic);
    }

    [Fact(DisplayName = "Prognostic - String gol nu e valid")]
    public void Prognostic_EmptyString_NotValid()
    {
        // Arrange
        var model = new CreateConsultatieCommand
        {
            Prognostic = "",
            Concluzie = "Test"
        };
        var component = CreateComponent(model);

        // Act
        var isCompleted = GetIsSectionCompleted(component);

        // Assert
        isCompleted.Should().BeFalse("prognostic gol nu e valid");
    }

    [Fact(DisplayName = "Prognostic - Whitespace nu e valid")]
    public void Prognostic_Whitespace_NotValid()
    {
        // Arrange
        var model = new CreateConsultatieCommand
        {
            Prognostic = "   \t\n",
            Concluzie = "Test"
        };
        var component = CreateComponent(model);

        // Act
        var isCompleted = GetIsSectionCompleted(component);

        // Assert
        isCompleted.Should().BeFalse("whitespace nu e valid");
    }

    #endregion

    #region Concluzie Tests

    [Fact(DisplayName = "Concluzie - Text scurt e valid")]
    public void Concluzie_ShortText_Valid()
    {
        // Arrange
        var model = new CreateConsultatieCommand
        {
            Prognostic = "Favorabil",
            Concluzie = "Evoluție favorabilă"
        };
        var component = CreateComponent(model);

        // Act
        var isCompleted = GetIsSectionCompleted(component);

        // Assert
        isCompleted.Should().BeTrue();
    }

    [Fact(DisplayName = "Concluzie - Text lung e valid")]
    public void Concluzie_LongText_Valid()
    {
        // Arrange
        var model = new CreateConsultatieCommand
        {
            Prognostic = "Favorabil",
            Concluzie = @"Pacient diagnosticat cu infecție respiratorie. 
                          Tratament cu antibiotic. 
                          Evoluție favorabilă așteptată în 7-10 zile."
        };
        var component = CreateComponent(model);

        // Act
        var isCompleted = GetIsSectionCompleted(component);

        // Assert
        isCompleted.Should().BeTrue();
    }

    [Fact(DisplayName = "Concluzie - String gol nu e valid")]
    public void Concluzie_EmptyString_NotValid()
    {
        // Arrange
        var model = new CreateConsultatieCommand
        {
            Prognostic = "Favorabil",
            Concluzie = ""
        };
        var component = CreateComponent(model);

        // Act
        var isCompleted = GetIsSectionCompleted(component);

        // Assert
        isCompleted.Should().BeFalse("concluzie goală nu e validă");
    }

    #endregion

    #region Optional Fields Tests

    [Fact(DisplayName = "ObservatiiMedic - Opțional, nu afectează validarea")]
    public void ObservatiiMedic_Optional_Valid()
    {
        // Arrange
        var model = new CreateConsultatieCommand
        {
            Prognostic = "Favorabil",
            Concluzie = "Test",
            ObservatiiMedic = "Note interne"
        };
        var component = CreateComponent(model);

        // Act
        var isCompleted = GetIsSectionCompleted(component);

        // Assert
        isCompleted.Should().BeTrue();
        model.ObservatiiMedic.Should().NotBeNullOrWhiteSpace();
    }

    [Fact(DisplayName = "NotePacient - Opțional, nu afectează validarea")]
    public void NotePacient_Optional_Valid()
    {
        // Arrange
        var model = new CreateConsultatieCommand
        {
            Prognostic = "Favorabil",
            Concluzie = "Test",
            NotePacient = "Pacient mulțumit"
        };
        var component = CreateComponent(model);

        // Act
        var isCompleted = GetIsSectionCompleted(component);

        // Assert
        isCompleted.Should().BeTrue();
        model.NotePacient.Should().NotBeNullOrWhiteSpace();
    }

    #endregion

    #region Edge Cases

    [Fact(DisplayName = "Edge Case - Text foarte lung")]
    public void EdgeCase_VeryLongText_Valid()
    {
        // Arrange
        var longText = new string('A', 10000);
        var model = new CreateConsultatieCommand
        {
            Prognostic = "Favorabil",
            Concluzie = longText
        };
        var component = CreateComponent(model);

        // Act
        var isCompleted = GetIsSectionCompleted(component);

        // Assert
        isCompleted.Should().BeTrue();
    }

    [Fact(DisplayName = "Edge Case - Caractere speciale")]
    public void EdgeCase_SpecialCharacters_Valid()
    {
        // Arrange
        var model = new CreateConsultatieCommand
        {
            Prognostic = "Favorabil",
            Concluzie = "Evoluție <>&\"' 中文 🎉"
        };
        var component = CreateComponent(model);

        // Act
        var isCompleted = GetIsSectionCompleted(component);

        // Assert
        isCompleted.Should().BeTrue();
    }

    #endregion

    #region Real Scenarios

    [Fact(DisplayName = "Scenariu Real - Prognostic favorabil standard")]
    public void RealScenario_StandardFavorable_Valid()
    {
        // Arrange
        var model = new CreateConsultatieCommand
        {
            Prognostic = "Favorabil",
            Concluzie = @"Pacient diagnosticat cu infecție respiratorie. 
                          Tratament cu antibioterapie. 
                          Evoluție favorabilă în 7-10 zile.",
            ObservatiiMedic = "Pacient compliant",
            NotePacient = "Alergie peniciline"
        };
        var component = CreateComponent(model);

        // Act
        var isCompleted = GetIsSectionCompleted(component);

        // Assert
        isCompleted.Should().BeTrue();
        model.Prognostic.Should().Be("Favorabil");
    }

    [Fact(DisplayName = "Scenariu Real - Caz sever cu prognostic rezervat")]
    public void RealScenario_SevereReserved_Valid()
    {
        // Arrange
        var model = new CreateConsultatieCommand
        {
            Prognostic = "Rezervat",
            Concluzie = @"Simptomatologie complexă. 
                          Trimitere urgentă pentru internare. 
                          Prognostic rezervat.",
            ObservatiiMedic = "Urgență",
            NotePacient = "Familie informată"
        };
        var component = CreateComponent(model);

        // Act
        var isCompleted = GetIsSectionCompleted(component);

        // Assert
        isCompleted.Should().BeTrue();
        model.Prognostic.Should().Be("Rezervat");
    }

    [Fact(DisplayName = "Scenariu Real - Control periodic")]
    public void RealScenario_RoutineCheckup_Valid()
    {
        // Arrange
        var model = new CreateConsultatieCommand
        {
            Prognostic = "Favorabil",
            Concluzie = "Control periodic. Pacient stabil. Următorul control la 3 luni.",
            ObservatiiMedic = "Complianță bună"
        };
        var component = CreateComponent(model);

        // Act
        var isCompleted = GetIsSectionCompleted(component);

        // Assert
        isCompleted.Should().BeTrue();
    }

    [Fact(DisplayName = "Scenariu Real - Caz complex cu multiple comorbidități")]
    public void RealScenario_ComplexCase_Valid()
    {
        // Arrange
        var model = new CreateConsultatieCommand
        {
            Prognostic = "Rezervat",
            Concluzie = @"Multiple comorbidități (diabet, HTA). 
                          Evaluare cardiologică urgentă. 
                          Monitorizare atentă.",
            ObservatiiMedic = "Caz complex, colaborare interdisciplinară",
            NotePacient = "Familie îngrijorată"
        };
        var component = CreateComponent(model);

        // Act
        var isCompleted = GetIsSectionCompleted(component);

        // Assert
        isCompleted.Should().BeTrue();
        model.Concluzie.Should().Contain("comorbidități");
    }

    #endregion
}
