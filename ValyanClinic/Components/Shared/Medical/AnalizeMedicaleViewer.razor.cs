using Microsoft.AspNetCore.Components;
using ValyanClinic.Application.ViewModels;

namespace ValyanClinic.Components.Shared.Medical;

/// <summary>
/// Componentă pentru afișarea analizelor medicale parsate din PDF/OCR
/// Grupează analizele pe documente și categorii cu expand/collapse
/// </summary>
public partial class AnalizeMedicaleViewer : ComponentBase
{
    [Parameter] public List<AnalizeMedicaleGroupDto>? Groups { get; set; }
    [Parameter] public bool IsLoading { get; set; } = false;

    // State pentru expand/collapse categorii
    private HashSet<string> _expandedCategories = new();

    protected override void OnParametersSet()
    {
        // La prima încărcare, expandăm toate categoriile
        if (Groups != null && _expandedCategories.Count == 0)
        {
            foreach (var group in Groups)
            {
                foreach (var category in GetCategoriesFromGroup(group))
                {
                    _expandedCategories.Add(GetCategoryKey(group.BatchId, category));
                }
            }
        }
    }

    /// <summary>
    /// Obține categoriile unice din grup
    /// </summary>
    private IEnumerable<string> GetCategoriesFromGroup(AnalizeMedicaleGroupDto group)
    {
        return group.Analize
            .Select(a => a.Categorie ?? string.Empty)
            .Distinct()
            .OrderBy(c => string.IsNullOrEmpty(c) ? 1 : 0) // Categoriile goale la final
            .ThenBy(c => c);
    }

    /// <summary>
    /// Obține analizele dintr-o anumită categorie
    /// </summary>
    private List<AnalizaMedicalaDto> GetAnalizeByCategory(AnalizeMedicaleGroupDto group, string category)
    {
        return group.Analize
            .Where(a => (a.Categorie ?? string.Empty) == category)
            .OrderBy(a => a.NumeAnaliza)
            .ToList();
    }

    /// <summary>
    /// Generează cheie unică pentru categorie (BatchId + Category)
    /// </summary>
    private string GetCategoryKey(Guid? batchId, string category)
    {
        return $"{batchId}_{category}";
    }

    /// <summary>
    /// Verifică dacă categoria este expandată
    /// </summary>
    private bool IsCategoryExpanded(Guid? batchId, string category)
    {
        return _expandedCategories.Contains(GetCategoryKey(batchId, category));
    }

    /// <summary>
    /// Toggle expand/collapse pentru categorie
    /// </summary>
    private void ToggleCategory(Guid? batchId, string category)
    {
        var key = GetCategoryKey(batchId, category);
        if (_expandedCategories.Contains(key))
        {
            _expandedCategories.Remove(key);
        }
        else
        {
            _expandedCategories.Add(key);
        }
    }

    /// <summary>
    /// Returnează iconița pentru categorie
    /// </summary>
    private string GetCategoryIcon(string category)
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
