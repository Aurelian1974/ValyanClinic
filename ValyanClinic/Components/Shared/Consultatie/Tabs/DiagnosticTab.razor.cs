using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using ValyanClinic.Application.Features.ConsultatieManagement.Commands.CreateConsultatie;
using ValyanClinic.Application.Features.ICD10Management.DTOs;

namespace ValyanClinic.Components.Shared.Consultatie.Tabs;

/// <summary>
/// Tab Diagnostic & Tratament - ✅ UPDATED: With primary and secondary diagnoses split
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

    // ==================== STATE - NEW DIAGNOSIS STRUCTURE ====================
    
    /// <summary>✅ NEW: Primary diagnosis ICD10 code</summary>
    private ICD10SearchResultDto? PrimaryDiagnosisCode { get; set; }
    
    /// <summary>✅ NEW: Primary diagnosis details text</summary>
    private string? PrimaryDiagnosisDetails { get; set; }
    
    /// <summary>✅ NEW: List of secondary diagnoses</summary>
    private List<SecondaryDiagnosis> SecondaryDiagnosesList { get; set; } = new();

    // ==================== COMPUTED ====================
    
    private bool IsComplete => PrimaryDiagnosisCode != null;

    // ==================== LIFECYCLE ====================

    protected override void OnParametersSet()
    {
        // Load primary diagnosis from Model if exists
        if (!string.IsNullOrEmpty(Model.CoduriICD10))
        {
            LoadPrimaryDiagnosisFromModel();
        }

        // Load secondary diagnoses from Model if exists
        if (!string.IsNullOrEmpty(Model.CoduriICD10Secundare))
        {
            LoadSecondaryDiagnosesFromModel();
        }
    }

    protected override void OnAfterRender(bool firstRender)
    {
        if (firstRender)
        {
            Logger.LogInformation("[DiagnosticTab] CurrentUserId: {UserId}", CurrentUserId);
        }
    }

    // ==================== EVENT HANDLERS ====================

    /// <summary>Handler când un câmp se schimbă</summary>
    private async Task OnFieldChanged()
    {
        // Sync to Model
        SyncToModel();
        
        await OnChanged.InvokeAsync();

        if (IsComplete)
        {
            await OnSectionCompleted.InvokeAsync();
        }
    }

    // ==================== DATA LOADING ====================

    /// <summary>Load primary diagnosis from Model.CoduriICD10</summary>
    private void LoadPrimaryDiagnosisFromModel()
    {
        if (string.IsNullOrEmpty(Model.CoduriICD10))
            return;

        // TODO: Load full ICD10SearchResultDto from repository
        // For now, create a minimal DTO
        PrimaryDiagnosisCode = new ICD10SearchResultDto
        {
            Code = Model.CoduriICD10,
            ShortDescription = Model.DiagnosticPozitiv ?? ""
        };

        PrimaryDiagnosisDetails = Model.DiagnosticPozitiv;
        
        Logger.LogInformation("[DiagnosticTab] Loaded primary diagnosis: {Code}", Model.CoduriICD10);
    }

    /// <summary>Load secondary diagnoses from Model.CoduriICD10Secundare</summary>
    private void LoadSecondaryDiagnosesFromModel()
    {
        if (string.IsNullOrEmpty(Model.CoduriICD10Secundare))
        {
            SecondaryDiagnosesList.Clear();
            return;
        }

        // Parse comma-separated codes
        var codes = Model.CoduriICD10Secundare
            .Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(c => c.Trim())
            .ToList();

        // Create secondary diagnosis entries
        // TODO: Load full ICD10SearchResultDto from repository
        SecondaryDiagnosesList = codes.Select(code => new SecondaryDiagnosis
        {
            Id = Guid.NewGuid(),
            Description = code, // Temporary - should load actual description
            ICD10Codes = new List<ICD10SearchResultDto>
            {
                new ICD10SearchResultDto { Code = code, ShortDescription = code }
            }
        }).ToList();
        
        Logger.LogInformation("[DiagnosticTab] Loaded {Count} secondary diagnoses", SecondaryDiagnosesList.Count);
    }

    // ==================== DATA SYNCING ====================

    /// <summary>Sync primary and secondary diagnoses to Model</summary>
    private void SyncToModel()
    {
        // Sync Primary Diagnosis
        Model.CoduriICD10 = PrimaryDiagnosisCode?.Code;
        Model.DiagnosticPozitiv = PrimaryDiagnosisDetails ?? PrimaryDiagnosisCode?.ShortDescription;

        // Sync Secondary Diagnoses
        if (SecondaryDiagnosesList.Any())
        {
            // Collect all ICD10 codes from all secondary diagnoses
            var allSecondaryCodes = SecondaryDiagnosesList
                .SelectMany(d => d.ICD10Codes.Select(c => c.Code))
                .Distinct()
                .ToList();

            Model.CoduriICD10Secundare = string.Join(", ", allSecondaryCodes);
            
            Logger.LogInformation("[DiagnosticTab] Synced {Count} secondary codes: {Codes}", 
                allSecondaryCodes.Count, Model.CoduriICD10Secundare);
        }
        else
        {
            Model.CoduriICD10Secundare = null;
        }
    }
}
