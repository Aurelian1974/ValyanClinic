using Microsoft.Extensions.Logging;

namespace ValyanClinic.Application.Services.IMC;

/// <summary>
/// Implementare serviciu calcul IMC conform standardelor OMS
/// </summary>
public class IMCCalculatorService : IIMCCalculatorService
{
    private readonly ILogger<IMCCalculatorService> _logger;

    // Limite rezonabile pentru validare
    private const decimal MIN_GREUTATE = 1m;
    private const decimal MAX_GREUTATE = 500m;
    private const decimal MIN_INALTIME = 30m;
    private const decimal MAX_INALTIME = 300m;

    public IMCCalculatorService(ILogger<IMCCalculatorService> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Calculează IMC și returnează rezultat complet
    /// </summary>
    public IMCResult Calculate(decimal greutate, decimal inaltime)
    {
        // Validare intrări
        if (!AreValuesValid(greutate, inaltime))
        {
            _logger.LogWarning(
                "Calcul IMC cu valori invalide: Greutate={Greutate}kg, Inaltime={Inaltime}cm",
                greutate,
                inaltime
            );
            return IMCResult.Invalid;
        }

        // Calcul IMC: greutate (kg) / (inaltime (m))²
        var inaltimeMetri = inaltime / 100m;
        var imc = greutate / (inaltimeMetri * inaltimeMetri);
        var imcRotunjit = Math.Round(imc, 2);

        // Determinare categorie
        var category = GetCategory(imcRotunjit);

        _logger.LogInformation(
            "IMC calculat: {IMC} - Categorie: {Category} (Greutate: {Greutate}kg, Inaltime: {Inaltime}cm)",
            imcRotunjit,
            category,
            greutate,
            inaltime
        );

        return new IMCResult
        {
            Value = imcRotunjit,
            Category = category,
            Interpretation = GetInterpretation(category),
            ColorClass = GetColorClass(category),
            HealthRecommendation = GetHealthRecommendation(category),
            HealthRisk = GetHealthRisk(category)
        };
    }

    /// <summary>
    /// Validare valori pentru calcul
    /// </summary>
    public bool AreValuesValid(decimal greutate, decimal inaltime)
    {
        return greutate >= MIN_GREUTATE &&
               greutate <= MAX_GREUTATE &&
               inaltime >= MIN_INALTIME &&
               inaltime <= MAX_INALTIME;
    }

    /// <summary>
    /// Calculează greutatea ideală folosind formula Lorentz
    /// </summary>
    public decimal CalculateIdealWeight(decimal inaltime, string sex)
    {
        if (inaltime < MIN_INALTIME || inaltime > MAX_INALTIME)
            return 0;

        // Formula Lorentz
        // Bărbați: Greutate ideală = (Înălțime - 100) - ((Înălțime - 150) / 4)
        // Femei: Greutate ideală = (Înălțime - 100) - ((Înălțime - 150) / 2.5)

        var factor = sex?.ToUpperInvariant() == "M" ? 4m : 2.5m;
        var idealWeight = (inaltime - 100) - ((inaltime - 150) / factor);

        return Math.Round(idealWeight, 1);
    }

    /// <summary>
    /// Determină categoria IMC conform clasificării OMS
    /// </summary>
    private IMCCategory GetCategory(decimal imc) => imc switch
    {
        < 18.5m => IMCCategory.Subponderal,
        < 25m => IMCCategory.Normal,
        < 30m => IMCCategory.Supraponderal,
        < 35m => IMCCategory.Obezitate1,
        < 40m => IMCCategory.Obezitate2,
        _ => IMCCategory.ObezitateMorbida
    };

    /// <summary>
    /// Returnează interpretarea medicală în limba română
    /// </summary>
    private string GetInterpretation(IMCCategory category) => category switch
    {
        IMCCategory.Subponderal => "Subponderal - risc nutrițional",
        IMCCategory.Normal => "Greutate normală",
        IMCCategory.Supraponderal => "Supraponderal - atenție la alimentație",
        IMCCategory.Obezitate1 => "Obezitate grad I",
        IMCCategory.Obezitate2 => "Obezitate grad II - necesită intervenție medicală",
        IMCCategory.ObezitateMorbida => "Obezitate morbidă (grad III) - necesită tratament urgent",
        _ => "Date insuficiente"
    };

    /// <summary>
    /// Returnează clasa CSS pentru styling
    /// </summary>
    private string GetColorClass(IMCCategory category) => category switch
    {
        IMCCategory.Subponderal => "imc-badge-subponderal",
        IMCCategory.Normal => "imc-badge-normal",
        IMCCategory.Supraponderal => "imc-badge-supraponderal",
        IMCCategory.Obezitate1 => "imc-badge-obezitate1",
        IMCCategory.Obezitate2 => "imc-badge-obezitate2",
        IMCCategory.ObezitateMorbida => "imc-badge-obezitate-morbida",
        _ => "imc-badge-invalid"
    };

    /// <summary>
    /// Returnează recomandări medicale
    /// </summary>
    private string GetHealthRecommendation(IMCCategory category) => category switch
    {
        IMCCategory.Subponderal =>
            "Se recomandă: consultație nutriționist, evaluare medicală pentru identificarea cauzelor subponderalității",

        IMCCategory.Normal =>
            "Se recomandă: menținerea greutății actuale prin alimentație echilibrată și activitate fizică regulată",

        IMCCategory.Supraponderal =>
            "Se recomandă: atenție la alimentație, creșterea activității fizice, monitorizare periodică",

        IMCCategory.Obezitate1 =>
            "Se recomandă: consultație specialist nutriție, program de scădere în greutate, activitate fizică supravegheată",

        IMCCategory.Obezitate2 =>
            "Se recomandă: evaluare medicală completă, plan terapeutic personalizat, monitorizare constantă",

        IMCCategory.ObezitateMorbida =>
            "Se recomandă urgent: evaluare medicală complexă, posibilă intervenție chirurgicală bariatrică, management multidisciplinar",

        _ => "Introduceți date valide pentru recomandări"
    };

    /// <summary>
    /// Returnează nivelul de risc pentru sănătate
    /// </summary>
    private string GetHealthRisk(IMCCategory category) => category switch
    {
        IMCCategory.Subponderal => "Medium",
        IMCCategory.Normal => "Low",
        IMCCategory.Supraponderal => "Medium",
        IMCCategory.Obezitate1 => "High",
        IMCCategory.Obezitate2 => "VeryHigh",
        IMCCategory.ObezitateMorbida => "Critical",
        _ => "Unknown"
    };
}
