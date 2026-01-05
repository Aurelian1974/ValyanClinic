using Microsoft.AspNetCore.Components;
using ValyanClinic.Application.ViewModels;

namespace ValyanClinic.Components.Shared.Medical;

/// <summary>
/// Componentă pentru compararea side-by-side a două seturi de analize medicale
/// Afișează evoluția valorilor și identifică îmbunătățiri/înrăutățiri
/// </summary>
public partial class AnalizeMedicaleComparator : ComponentBase
{
    [Parameter] public ComparatieAnalizeMedicaleDto? Comparatie { get; set; }
    [Parameter] public bool IsLoading { get; set; } = false;

    /// <summary>
    /// Returnează clasa CSS pentru rândul de analiză
    /// </summary>
    private string GetRowClass(AnalizaComparatieDto item)
    {
        var classes = new List<string>();

        if (item.EsteNou)
            classes.Add("row-new");
        else if (item.ADisparut)
            classes.Add("row-missing");
        else if (item.Trend == TrendComparatie.Imbunatatit)
            classes.Add("row-improved");
        else if (item.Trend == TrendComparatie.Inrautatit)
            classes.Add("row-worsened");

        return string.Join(" ", classes);
    }

    /// <summary>
    /// Returnează clasa CSS pentru celula de trend
    /// </summary>
    private string GetTrendClass(TrendComparatie trend)
    {
        return trend switch
        {
            TrendComparatie.Crescut => "trend-up",
            TrendComparatie.Scazut => "trend-down",
            TrendComparatie.Stabil => "trend-stable",
            TrendComparatie.Imbunatatit => "trend-improved",
            TrendComparatie.Inrautatit => "trend-worsened",
            _ => "trend-unknown"
        };
    }

    /// <summary>
    /// Returnează iconița pentru trend
    /// </summary>
    private string GetTrendIcon(TrendComparatie trend)
    {
        return trend switch
        {
            TrendComparatie.Crescut => "↑",
            TrendComparatie.Scazut => "↓",
            TrendComparatie.Stabil => "≈",
            TrendComparatie.Imbunatatit => "✓",
            TrendComparatie.Inrautatit => "⚠",
            _ => "?"
        };
    }

    /// <summary>
    /// Returnează iconița pentru categorie
    /// </summary>
    private string GetCategoryIcon(string? category)
    {
        return category?.ToUpperInvariant() switch
        {
            "HEMATOLOGIE" => "fa-tint",
            "IMUNOLOGIE" => "fa-shield-virus",
            "BIOCHIMIE" => "fa-flask",
            "ENDOCRINOLOGIE" => "fa-brain",
            "URINA" or "URINĂ" => "fa-toilet",
            "COAGULARE" => "fa-droplet",
            "MARKERI TUMORALI" => "fa-dna",
            "SEROLOGIE" => "fa-syringe",
            "VITAMINE" => "fa-capsules",
            "MINERALE" => "fa-gem",
            "LIPIDE" => "fa-heart",
            "FICAT" or "HEPATIC" => "fa-liver",
            "RINICHI" or "RENAL" => "fa-kidneys",
            "TIROIDĂ" or "TIROIDA" => "fa-lungs",
            _ => "fa-vial"
        };
    }
}
