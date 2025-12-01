using FluentAssertions;
using ValyanClinic.Application.Features.ConsultatieManagement.Commands.CreateConsultatie;
using ValyanClinic.Components.Shared.Consultatie.Tabs;
using Xunit;

namespace ValyanClinic.Tests.Components.Consultatie;

/// <summary>
/// Unit tests pentru TratamentTab component
/// Testează validare: TratamentMedicamentos este OBLIGATORIU + cel puțin o recomandare
/// </summary>
public class TratamentTabTests
{
    #region Test Setup

    private static TratamentTab CreateComponent(CreateConsultatieCommand? model = null)
    {
        var component = new TratamentTab();

        var modelProperty = typeof(TratamentTab).GetProperty(nameof(TratamentTab.Model));
        modelProperty?.SetValue(component, model ?? new CreateConsultatieCommand());

        var isActiveProperty = typeof(TratamentTab).GetProperty(nameof(TratamentTab.IsActive));
        isActiveProperty?.SetValue(component, true);

        return component;
    }

    private static bool GetIsSectionCompleted(TratamentTab component)
    {
        var property = typeof(TratamentTab).GetProperty("IsSectionCompleted",
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
        isCompleted.Should().BeFalse("niciun câmp nu este completat");
    }

    [Fact(DisplayName = "IsSectionCompleted - Returnează false când lipsește tratamentul medicamentos")]
    public void IsSectionCompleted_NoTratamentMedicamentos_ReturnsFalse()
    {
        // Arrange
        var model = new CreateConsultatieCommand
        {
            // Doar recomandări, fără tratament medicamentos
            RecomandariDietetice = "Dietă echilibrată",
            RecomandariRegimViata = "Activitate fizică regulată"
        };
        var component = CreateComponent(model);

        // Act
        var isCompleted = GetIsSectionCompleted(component);

        // Assert
        isCompleted.Should().BeFalse("tratamentul medicamentos este OBLIGATORIU");
    }

    [Fact(DisplayName = "IsSectionCompleted - Returnează false când TratamentMedicamentos există dar fără recomandări")]
    public void IsSectionCompleted_OnlyTratamentMedicamentos_ReturnsFalse()
    {
        // Arrange
        var model = new CreateConsultatieCommand
        {
            TratamentMedicamentos = "Paracetamol 500mg x3/zi"
        };
        var component = CreateComponent(model);

        // Act
        var isCompleted = GetIsSectionCompleted(component);

        // Assert
        isCompleted.Should().BeFalse("necesită și cel puțin o recomandare");
    }

    [Fact(DisplayName = "IsSectionCompleted - Returnează true cu tratament + 1 recomandare")]
    public void IsSectionCompleted_TratamentPlusOneRecommendation_ReturnsTrue()
    {
        // Arrange
        var model = new CreateConsultatieCommand
        {
            TratamentMedicamentos = "Paracetamol 500mg x3/zi",
            RecomandariDietetice = "Dietă bogată în fructe și legume"
        };
        var component = CreateComponent(model);

        // Act
        var isCompleted = GetIsSectionCompleted(component);

        // Assert
        isCompleted.Should().BeTrue("tratament obligatoriu + 1 recomandare sunt suficiente");
    }

    [Fact(DisplayName = "IsSectionCompleted - Returnează true cu toate câmpurile completate")]
    public void IsSectionCompleted_AllFields_ReturnsTrue()
    {
        // Arrange
        var model = new CreateConsultatieCommand
        {
            TratamentMedicamentos = "Ibuprofen 400mg x3/zi, 7 zile",
            TratamentNemedicamentos = "Fizioterapie 3 ședințe/săptămână",
            RecomandariDietetice = "Dietă hipocalorică",
            RecomandariRegimViata = "Activitate fizică zilnic 30 min",
            InvestigatiiRecomandate = "Repetare analize la 3 luni",
            ConsulturiSpecialitate = "Trimitere cardiologie",
            DataUrmatoareiProgramari = "3 luni",
            RecomandariSupraveghere = "Monitorizare tensiune arterială"
        };
        var component = CreateComponent(model);

        // Act
        var isCompleted = GetIsSectionCompleted(component);

        // Assert
        isCompleted.Should().BeTrue("toate câmpurile sunt completate");
    }

    #endregion

    #region Tratament Medicamentos Tests (Obligatoriu)

    [Theory(DisplayName = "TratamentMedicamentos - Diverse formate valide")]
    [InlineData("Paracetamol 500mg x3/zi")]
    [InlineData("Ibuprofen 400mg dimineața și seara, 5 zile")]
    [InlineData("Antibioterapie: Amoxicilină 1g x2/zi, 10 zile")]
    [InlineData("Polipragmazia: medicamentA, medicamentB, medicamentC")]
    public void TratamentMedicamentos_ValidFormats_Accepted(string tratament)
    {
        // Arrange
        var model = new CreateConsultatieCommand
        {
            TratamentMedicamentos = tratament,
            RecomandariDietetice = "Test"
        };
        var component = CreateComponent(model);

        // Act
        var isCompleted = GetIsSectionCompleted(component);

        // Assert
        isCompleted.Should().BeTrue($"format '{tratament}' este valid");
        model.TratamentMedicamentos.Should().Be(tratament);
    }

    [Fact(DisplayName = "TratamentMedicamentos - 'Fără tratament medicamentos' nu este suficient")]
    public void TratamentMedicamentos_NoTreatmentNeeded_StillRequiresText()
    {
        // Arrange - Chiar dacă pacientul nu necesită tratament, trebuie specificat
        var model = new CreateConsultatieCommand
        {
            TratamentMedicamentos = "Fără tratament medicamentos necesar",
            RecomandariDietetice = "Hidratare adecvată"
        };
        var component = CreateComponent(model);

        // Act
        var isCompleted = GetIsSectionCompleted(component);

        // Assert
        isCompleted.Should().BeTrue("specificarea 'fără tratament' este validă");
    }

    #endregion

    #region Recomandări Tests

    [Theory(DisplayName = "Orice tip de recomandare face secțiunea validă")]
    [InlineData(nameof(CreateConsultatieCommand.TratamentNemedicamentos), "Fizioterapie")]
    [InlineData(nameof(CreateConsultatieCommand.RecomandariDietetice), "Dietă hipocalorică")]
    [InlineData(nameof(CreateConsultatieCommand.RecomandariRegimViata), "Activitate fizică")]
    [InlineData(nameof(CreateConsultatieCommand.InvestigatiiRecomandate), "RMN creier")]
    [InlineData(nameof(CreateConsultatieCommand.ConsulturiSpecialitate), "Trimitere neurologie")]
    [InlineData(nameof(CreateConsultatieCommand.DataUrmatoareiProgramari), "1 lună")]
    [InlineData(nameof(CreateConsultatieCommand.RecomandariSupraveghere), "Control glicemie")]
    public void Recommendations_AnyType_MakesSectionValid(string propertyName, string value)
    {
        // Arrange
        var model = new CreateConsultatieCommand
        {
            TratamentMedicamentos = "Paracetamol 500mg"
        };

        var property = typeof(CreateConsultatieCommand).GetProperty(propertyName);
        property?.SetValue(model, value);

        var component = CreateComponent(model);

        // Act
        var isCompleted = GetIsSectionCompleted(component);

        // Assert
        isCompleted.Should().BeTrue($"recomandarea {propertyName} face secțiunea completă");
    }

    [Fact(DisplayName = "Multiple recomandări sunt și mai bine")]
    public void MultipleRecommendations_EvenBetter()
    {
        // Arrange
        var model = new CreateConsultatieCommand
        {
            TratamentMedicamentos = "Ibuprofen 400mg",
            RecomandariDietetice = "Dietă",
            RecomandariRegimViata = "Sport",
            RecomandariSupraveghere = "Control periodic"
        };
        var component = CreateComponent(model);

        // Act
        var isCompleted = GetIsSectionCompleted(component);

        // Assert
        isCompleted.Should().BeTrue("multiple recomandări sunt foarte bine");
    }

    #endregion

    #region Edge Cases

    [Fact(DisplayName = "Edge Case - Whitespace în TratamentMedicamentos nu este valid")]
    public void EdgeCase_WhitespaceTratament_NotValid()
    {
        // Arrange
        var model = new CreateConsultatieCommand
        {
            TratamentMedicamentos = "   \t\n   ",
            RecomandariDietetice = "Test"
        };
        var component = CreateComponent(model);

        // Act
        var isCompleted = GetIsSectionCompleted(component);

        // Assert
        isCompleted.Should().BeFalse("whitespace-only nu este tratament valid");
    }

    [Fact(DisplayName = "Edge Case - Text foarte lung este acceptat")]
    public void EdgeCase_VeryLongTreatment_Valid()
    {
        // Arrange
        var longText = new string('A', 10000);
        var model = new CreateConsultatieCommand
        {
            TratamentMedicamentos = longText,
            RecomandariDietetice = "Test"
        };
        var component = CreateComponent(model);

        // Act
        var isCompleted = GetIsSectionCompleted(component);

        // Assert
        isCompleted.Should().BeTrue("text lung este acceptat");
    }

    [Fact(DisplayName = "Edge Case - Caractere speciale în nume medicament")]
    public void EdgeCase_SpecialCharactersInDrugNames_Valid()
    {
        // Arrange
        var model = new CreateConsultatieCommand
        {
            TratamentMedicamentos = "Co-Amoxiclav 1000mg, Omega-3 2x/zi",
            RecomandariDietetice = "Test"
        };
        var component = CreateComponent(model);

        // Act
        var isCompleted = GetIsSectionCompleted(component);

        // Assert
        isCompleted.Should().BeTrue("caractere speciale în nume medicamente sunt normale");
    }

    #endregion

    #region Real Scenarios

    [Fact(DisplayName = "Scenariu Real - Tratament complet cu toate recomandările")]
    public void RealScenario_CompleteTreatment_Valid()
    {
        // Arrange
        var model = new CreateConsultatieCommand
        {
            TratamentMedicamentos = @"
                1. Ibuprofen 400mg x3/zi după masă, 7 zile
                2. Omeprazol 20mg x1/zi dimineața, pe stomacul gol
                3. Cetirizină 10mg x1/zi seara",
            TratamentNemedicamentos = "Fizioterapie - 10 ședințe",
            RecomandariDietetice = "Evitare alimente picante, consum moderat de cafea",
            RecomandariRegimViata = "Activitate fizică moderată 30 min/zi, evitare stres",
            InvestigatiiRecomandate = "Endoscopie digestivă superioară la 6 luni",
            ConsulturiSpecialitate = "Control gastroenterologie la 1 lună",
            DataUrmatoareiProgramari = "1 lună",
            RecomandariSupraveghere = "Monitorizare simptome, prezentare la reapariție dureri"
        };
        var component = CreateComponent(model);

        // Act
        var isCompleted = GetIsSectionCompleted(component);

        // Assert
        isCompleted.Should().BeTrue();
        model.TratamentMedicamentos.Should().Contain("Ibuprofen");
        model.RecomandariDietetice.Should().Contain("picante");
    }

    [Fact(DisplayName = "Scenariu Real - Tratament simplu cu o singură recomandare")]
    public void RealScenario_SimpleTreatment_Valid()
    {
        // Arrange
        var model = new CreateConsultatieCommand
        {
            TratamentMedicamentos = "Paracetamol 500mg dacă este necesar",
            RecomandariRegimViata = "Repaus la pat, hidratare"
        };
        var component = CreateComponent(model);

        // Act
        var isCompleted = GetIsSectionCompleted(component);

        // Assert
        isCompleted.Should().BeTrue("tratament simplu + 1 recomandare este suficient");
    }

    [Fact(DisplayName = "Scenariu Real - Tratament cronic cu multiple medicamente")]
    public void RealScenario_ChronicTreatment_Valid()
    {
        // Arrange
        var model = new CreateConsultatieCommand
        {
            TratamentMedicamentos = @"
                Tratament cronic:
                - Metformin 500mg x2/zi
                - Enalapril 10mg x1/zi dimineața
                - Atorvastatină 20mg x1/zi seara
                - Aspirină 75mg x1/zi",
            DataUrmatoareiProgramari = "3 luni",
            RecomandariSupraveghere = "Control glicemie zilnic, tensiune 2x/săptămână"
        };
        var component = CreateComponent(model);

        // Act
        var isCompleted = GetIsSectionCompleted(component);

        // Assert
        isCompleted.Should().BeTrue();
        model.TratamentMedicamentos.Should().Contain("Metformin");
    }

    [Fact(DisplayName = "Scenariu Real - Fără medicație dar cu recomandări lifestyle")]
    public void RealScenario_NoMedication_OnlyLifestyle_Valid()
    {
        // Arrange
        var model = new CreateConsultatieCommand
        {
            TratamentMedicamentos = "Fără tratament medicamentos necesar momentan",
            RecomandariDietetice = "Dietă mediteraneeană",
            RecomandariRegimViata = "Activitate fizică aerobă 150 min/săptămână",
            DataUrmatoareiProgramari = "6 luni"
        };
        var component = CreateComponent(model);

        // Act
        var isCompleted = GetIsSectionCompleted(component);

        // Assert
        isCompleted.Should().BeTrue("chiar fără medicamente, recomandările lifestyle sunt valide");
    }

    [Fact(DisplayName = "Scenariu Real - Urgență cu trimitere la spital")]
    public void RealScenario_Emergency_ReferralToHospital_Valid()
    {
        // Arrange
        var model = new CreateConsultatieCommand
        {
            TratamentMedicamentos = "Tratament inițiat în UPU",
            ConsulturiSpecialitate = "Internare urgentă chirurgie generală",
            RecomandariSupraveghere = "Prezentare imediată la UPU dacă se agravează"
        };
        var component = CreateComponent(model);

        // Act
        var isCompleted = GetIsSectionCompleted(component);

        // Assert
        isCompleted.Should().BeTrue("trimitere la spital este o recomandare validă");
    }

    #endregion

    #region Negative Tests

    [Fact(DisplayName = "Negative Test - Doar recomandări fără tratament NU este suficient")]
    public void NegativeTest_OnlyRecommendations_NotSufficient()
    {
        // Arrange
        var model = new CreateConsultatieCommand
        {
            // Lipsește TratamentMedicamentos
            RecomandariDietetice = "Dietă",
            RecomandariRegimViata = "Sport",
            InvestigatiiRecomandate = "Analize"
        };
        var component = CreateComponent(model);

        // Act
        var isCompleted = GetIsSectionCompleted(component);

        // Assert
        isCompleted.Should().BeFalse("tratamentul medicamentos este OBLIGATORIU");
    }

    [Fact(DisplayName = "Negative Test - Null model aruncă NullReferenceException")]
    public void NegativeTest_NullModel_ThrowsNullReferenceException()
    {
        // Arrange
        var component = new TratamentTab();
        var modelProperty = typeof(TratamentTab).GetProperty(nameof(TratamentTab.Model));
        modelProperty?.SetValue(component, null);

        // Act
        Action act = () => GetIsSectionCompleted(component);

        // Assert
        act.Should().Throw<System.Reflection.TargetInvocationException>()
            .WithInnerException<NullReferenceException>("model-ul nu poate fi null în Blazor components");
    }

    #endregion
}
