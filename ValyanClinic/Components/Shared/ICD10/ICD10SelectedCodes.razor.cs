using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using ValyanClinic.Application.Features.ICD10Management.DTOs;

namespace ValyanClinic.Components.Shared.ICD10;

/// <summary>
/// ICD10 Selected Codes - Afișare coduri selectate (Principal + Secundare)
/// </summary>
public partial class ICD10SelectedCodes : ComponentBase
{
    [Inject] private ILogger<ICD10SelectedCodes> Logger { get; set; } = default!;

    // ==================== PARAMETERS ====================
    
    /// <summary>Cod ICD-10 Principal (two-way binding)</summary>
    [Parameter] public string? PrincipalCode { get; set; }
    
    [Parameter] public EventCallback<string?> PrincipalCodeChanged { get; set; }
    
    /// <summary>Detalii complete pentru codul principal</summary>
 [Parameter] public ICD10SearchResultDto? PrincipalCodeDetails { get; set; }
    
 /// <summary>Lista coduri secundare (two-way binding)</summary>
    [Parameter] public List<SelectedCode> SecondaryCodes { get; set; } = new();
    
    [Parameter] public EventCallback<List<SelectedCode>> SecondaryCodesChanged { get; set; }
    
    /// <summary>Event când un cod este eliminat</summary>
    [Parameter] public EventCallback<string> OnCodeRemoved { get; set; }

    // ==================== COMPUTED PROPERTIES ====================
    
    private bool HasAnyCodes => !string.IsNullOrEmpty(PrincipalCode) || SecondaryCodes.Any();
    
    private int TotalCodesCount => (string.IsNullOrEmpty(PrincipalCode) ? 0 : 1) + SecondaryCodes.Count;

    // ==================== ACTIONS ====================
    
    /// <summary>Elimină codul principal</summary>
    private async Task RemovePrincipal()
    {
        Logger.LogInformation("[ICD10Selected] Removing principal code: {Code}", PrincipalCode);
        
        await OnCodeRemoved.InvokeAsync(PrincipalCode);
 
        PrincipalCode = null;
        await PrincipalCodeChanged.InvokeAsync(null);
        
        StateHasChanged();
    }
    
    /// <summary>Elimină un cod secundar</summary>
    private async Task RemoveSecondary(string code)
    {
    Logger.LogInformation("[ICD10Selected] Removing secondary code: {Code}", code);
        
        await OnCodeRemoved.InvokeAsync(code);
        
     SecondaryCodes.RemoveAll(c => c.Code == code);
        await SecondaryCodesChanged.InvokeAsync(SecondaryCodes);
        
     StateHasChanged();
    }

    // ==================== HELPERS ====================
    
    private string GetCategoryIcon(string category)
    {
        return category switch
        {
    "Cardiovascular" => "❤️",
            "Endocrin" => "🔬",
   "Respirator" => "🫁",
            "Digestiv" => "🍽️",
            "Nervos" => "🧠",
            "Simptome" => "⚕️",
       _ => "📋"
        };
    }
}
