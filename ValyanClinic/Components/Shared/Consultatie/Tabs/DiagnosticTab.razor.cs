using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using ValyanClinic.Application.Features.ConsultatieManagement.Commands.CreateConsultatie;
using ValyanClinic.Application.Features.ICD10Management.DTOs;
using ValyanClinic.Components.Shared.ICD10;

namespace ValyanClinic.Components.Shared.Consultatie.Tabs;

/// <summary>
/// Tab Diagnostic & Tratament - Cu integrare ICD10
/// </summary>
public partial class DiagnosticTab : ComponentBase
{
    [Inject] private ILogger<DiagnosticTab> Logger { get; set; } = default!;

    // ==================== PARAMETERS ====================
    
    [Parameter] public CreateConsultatieCommand Model { get; set; } = new();
    
    [Parameter] public EventCallback OnChanged { get; set; }
    
    [Parameter] public EventCallback OnSectionCompleted { get; set; }
    
    [Parameter] public bool ShowValidation { get; set; } = false;

    /// <summary>ID utilizator curent (pentru favorites)</summary>
    [Parameter] public Guid? CurrentUserId { get; set; }

    // ==================== STATE - ICD10 ====================
    
    /// <summary>Detalii complete pentru codul principal selectat</summary>
    private ICD10SearchResultDto? PrincipalCodeDetails { get; set; }
    
    /// <summary>Lista coduri secundare cu detalii</summary>
    private List<ICD10SelectedCodes.SelectedCode> SecondaryCodes { get; set; } = new();

    // ==================== COMPUTED ====================
    
    private bool IsComplete => !string.IsNullOrWhiteSpace(Model.DiagnosticPozitiv);

    // ==================== LIFECYCLE ====================

    protected override void OnParametersSet()
    {
   // Load secondary codes from Model if exists
      if (!string.IsNullOrEmpty(Model.CoduriICD10Secundare))
        {
          LoadSecondaryCodesFromModel();
        }
    }

    // ==================== EVENT HANDLERS ====================

    /// <summary>Handler când un câmp se schimbă</summary>
    private async Task OnFieldChanged()
    {
        await OnChanged.InvokeAsync();

     if (IsComplete)
      {
          await OnSectionCompleted.InvokeAsync();
        }
    }

    /// <summary>Handler când un cod ICD10 este selectat din search</summary>
    private async Task HandleCodeSelected(ICD10SearchResultDto selectedCode)
    {
        Logger.LogInformation("[DiagnosticTab] Code selected: {Code} - {Description}", 
          selectedCode.Code, selectedCode.ShortDescription);

        // Dacă nu există cod principal, setează ca principal
     if (string.IsNullOrEmpty(Model.CoduriICD10))
        {
    Model.CoduriICD10 = selectedCode.Code;
        PrincipalCodeDetails = selectedCode;
      
            Logger.LogInformation("[DiagnosticTab] Set as principal code: {Code}", selectedCode.Code);
   }
        else
   {
            // Altfel, adaugă la secundare
       if (!SecondaryCodes.Any(c => c.Code == selectedCode.Code))
        {
           SecondaryCodes.Add(new ICD10SelectedCodes.SelectedCode
              {
        Code = selectedCode.Code,
     Description = selectedCode.ShortDescription,
          Category = selectedCode.Category
          });

                UpdateSecondaryCodesInModel();

     Logger.LogInformation("[DiagnosticTab] Added secondary code: {Code}", selectedCode.Code);
            }
            else
    {
   Logger.LogWarning("[DiagnosticTab] Code {Code} already exists in secondary", selectedCode.Code);
   }
        }

        await OnFieldChanged();
      StateHasChanged();
    }

    /// <summary>Handler când un cod este eliminat</summary>
    private async Task HandleCodeRemoved(string code)
    {
        Logger.LogInformation("[DiagnosticTab] Removing code: {Code}", code);

        // Verifică dacă este codul principal
        if (Model.CoduriICD10 == code)
        {
      Model.CoduriICD10 = null;
        PrincipalCodeDetails = null;
     
        Logger.LogInformation("[DiagnosticTab] Removed principal code");
        }
        else
        {
        // Elimină din secundare
            SecondaryCodes.RemoveAll(c => c.Code == code);
 UpdateSecondaryCodesInModel();
            
    Logger.LogInformation("[DiagnosticTab] Removed secondary code");
 }

        await OnFieldChanged();
        StateHasChanged();
    }

    // ==================== HELPER METHODS ====================

    /// <summary>Încarcă codurile secundare din Model.CoduriICD10Secundare</summary>
    private void LoadSecondaryCodesFromModel()
    {
        if (string.IsNullOrEmpty(Model.CoduriICD10Secundare))
{
            SecondaryCodes.Clear();
            return;
    }

  var codes = Model.CoduriICD10Secundare
       .Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(c => c.Trim())
        .ToList();

        SecondaryCodes = codes.Select(code => new ICD10SelectedCodes.SelectedCode
        {
 Code = code,
   Description = "", // TODO: Load from database if needed
  Category = null
        }).ToList();
    }

    /// <summary>Actualizează Model.CoduriICD10Secundare din lista SecondaryCodes</summary>
    private void UpdateSecondaryCodesInModel()
    {
        if (!SecondaryCodes.Any())
        {
            Model.CoduriICD10Secundare = null;
      }
        else
        {
            Model.CoduriICD10Secundare = string.Join(", ", SecondaryCodes.Select(c => c.Code));
 }
    }
}
