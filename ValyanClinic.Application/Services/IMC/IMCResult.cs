namespace ValyanClinic.Application.Services.IMC;

/// <summary>
/// Rezultatul calculului Indicelui de Masă Corporală (IMC/BMI)
/// </summary>
public class IMCResult
{
    /// <summary>
    /// Valoarea calculată a IMC (kg/m²)
    /// </summary>
    public decimal Value { get; set; }

    /// <summary>
    /// Categoria IMC conform clasificării OMS
    /// </summary>
    public IMCCategory Category { get; set; }

    /// <summary>
    /// Interpretare medicală în limba română
    /// </summary>
    public string Interpretation { get; set; } = string.Empty;

    /// <summary>
    /// Clasă CSS pentru badge-ul de culoare
    /// </summary>
    public string ColorClass { get; set; } = string.Empty;

    /// <summary>
    /// Recomandări medicale bazate pe categoria IMC
    /// </summary>
    public string HealthRecommendation { get; set; } = string.Empty;

    /// <summary>
    /// Nivel de risc pentru sănătate (Low, Medium, High, VeryHigh)
    /// </summary>
    public string HealthRisk { get; set; } = string.Empty;

    /// <summary>
    /// Rezultat invalid - folosit când datele sunt incomplete
    /// </summary>
    public static IMCResult Invalid => new()
    {
        Value = 0,
        Category = IMCCategory.Invalid,
        Interpretation = "Date insuficiente pentru calcul",
        ColorClass = "imc-badge-invalid",
        HealthRecommendation = "Introduceți greutatea și înălțimea pentru calcul",
        HealthRisk = "Unknown"
    };
}
