using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using ValyanClinic.Application.Features.ConsultatieManagement.Commands.CreateConsultatie;
using ValyanClinic.Application.Features.ConsultatieManagement.Commands.SaveConsultatieDraft;
using ValyanClinic.Application.Features.ConsultatieManagement.DTOs;
using ValyanClinic.Application.Features.ICD10Management.DTOs;

namespace ValyanClinic.Components.Shared.Consultatie.Tabs;

/// <summary>
/// Tab Diagnostic & Tratament - ✅ UPDATED: With medications list
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

    /// <summary>✅ NEW: List of prescribed medications</summary>
    private List<MedicationRowDto> MedicationsList
    {
        get => Model.MedicationList ?? new();
        set => Model.MedicationList = value;
    }

    /// <summary>Flag to track if initial data has been loaded (prevents re-loading on every render)</summary>
    private bool _isInitialDataLoaded = false;

    // ==================== COMPUTED ====================

    private bool IsComplete => PrimaryDiagnosisCode != null;

    // ==================== LIFECYCLE ====================

    protected override void OnParametersSet()
    {
        // Only load from Model on FIRST render
        // Once data is loaded, user changes take priority
        if (_isInitialDataLoaded)
            return;

        Logger.LogInformation("[DiagnosticTab] OnParametersSet - Model.DiagnosticeSecundare count: {Count}",
            Model.DiagnosticeSecundare?.Count ?? 0);

        // Load primary diagnosis - use NEW fields first, fallback to legacy
        if ((!string.IsNullOrEmpty(Model.CodICD10Principal) || !string.IsNullOrEmpty(Model.CoduriICD10)) 
            && PrimaryDiagnosisCode == null)
        {
            LoadPrimaryDiagnosisFromModel();
        }

        // Load secondary diagnoses - use NEW list
        if (Model.DiagnosticeSecundare?.Any() == true && !SecondaryDiagnosesList.Any())
        {
            LoadSecondaryDiagnosesFromModel();
        }
        
        _isInitialDataLoaded = true;
    }

    protected override void OnAfterRender(bool firstRender)
    {
        if (firstRender)
        {
            Logger.LogInformation("[DiagnosticTab] CurrentUserId: {UserId}", CurrentUserId);
        }
    }

    // ==================== EVENT HANDLERS ====================

    /// <summary>Handler pentru schimbarea codului ICD-10 principal</summary>
    private async Task HandlePrimaryCodeChanged(ICD10SearchResultDto? code)
    {
        Logger.LogInformation("[DiagnosticTab] HandlePrimaryCodeChanged: {Code}", code?.Code ?? "NULL");
        PrimaryDiagnosisCode = code;
        await OnFieldChanged();
    }

    /// <summary>Handler pentru schimbarea descrierii diagnosticului principal</summary>
    private async Task HandlePrimaryDetailsChanged(string? details)
    {
        Logger.LogInformation("[DiagnosticTab] HandlePrimaryDetailsChanged: {Details}", 
            details?.Substring(0, Math.Min(50, details?.Length ?? 0)) ?? "NULL");
        PrimaryDiagnosisDetails = details;
        await OnFieldChanged();
    }

    /// <summary>Handler când un câmp se schimbă</summary>
    private async Task OnFieldChanged()
    {
        Logger.LogInformation("[DiagnosticTab] OnFieldChanged - PrimaryDiagnosisDetails BEFORE sync: {Details}",
            PrimaryDiagnosisDetails?.Substring(0, Math.Min(50, PrimaryDiagnosisDetails?.Length ?? 0)) ?? "NULL");
        
        // Sync to Model
        SyncToModel();
        
        await OnChanged.InvokeAsync();

        if (IsComplete)
        {
            await OnSectionCompleted.InvokeAsync();
        }
    }

    // ==================== DATA LOADING ====================

    /// <summary>Load primary diagnosis from Model - uses NEW fields with legacy fallback</summary>
    private void LoadPrimaryDiagnosisFromModel()
    {
        // Try NEW fields first
        if (!string.IsNullOrEmpty(Model.CodICD10Principal))
        {
            PrimaryDiagnosisCode = new ICD10SearchResultDto
            {
                Code = Model.CodICD10Principal,
                ShortDescription = Model.NumeDiagnosticPrincipal ?? ""
            };
            PrimaryDiagnosisDetails = Model.DescriereDetaliataPrincipal;
            
            Logger.LogInformation("[DiagnosticTab] Loaded primary diagnosis (NEW): {Code} - {Name}", 
                Model.CodICD10Principal, Model.NumeDiagnosticPrincipal);
            return;
        }

        // Fallback to LEGACY fields
        if (string.IsNullOrEmpty(Model.CoduriICD10))
            return;

        PrimaryDiagnosisCode = new ICD10SearchResultDto
        {
            Code = Model.CoduriICD10,
            ShortDescription = Model.DiagnosticPozitiv ?? ""
        };
        PrimaryDiagnosisDetails = Model.DiagnosticPozitiv;
        
        Logger.LogInformation("[DiagnosticTab] Loaded primary diagnosis (LEGACY): {Code}", Model.CoduriICD10);
    }

    /// <summary>Load secondary diagnoses from Model - uses NEW list with legacy fallback</summary>
    private void LoadSecondaryDiagnosesFromModel()
    {
        // Try NEW structure first
        if (Model.DiagnosticeSecundare?.Any() == true)
        {
            SecondaryDiagnosesList = Model.DiagnosticeSecundare
                .OrderBy(d => d.OrdineAfisare)
                .Select(d => new SecondaryDiagnosis
                {
                    Id = Guid.NewGuid(),  // UI tracking
                    DatabaseId = d.Id,     // Preserve DB ID for MERGE logic
                    Description = d.Descriere ?? "", // RTE content
                    ICD10Codes = new List<ICD10SearchResultDto>
                    {
                        new ICD10SearchResultDto 
                        { 
                            Code = d.CodICD10 ?? "", 
                            ShortDescription = d.NumeDiagnostic ?? "" 
                        }
                    }
                })
                .ToList();
            
            Logger.LogInformation("[DiagnosticTab] Loaded {Count} secondary diagnoses (NEW)", 
                SecondaryDiagnosesList.Count);
            return;
        }

        SecondaryDiagnosesList.Clear();
    }

    // ==================== DATA SYNCING ====================

    /// <summary>Sync primary and secondary diagnoses to Model</summary>
    private void SyncToModel()
    {
        // Sync Primary Diagnosis - NEW normalized fields
        Model.CodICD10Principal = PrimaryDiagnosisCode?.Code;
        Model.NumeDiagnosticPrincipal = PrimaryDiagnosisCode?.ShortDescription;
        Model.DescriereDetaliataPrincipal = PrimaryDiagnosisDetails;
        
        Logger.LogInformation("[DiagnosticTab] SyncToModel - DescriereDetaliataPrincipal: {Desc}", 
            PrimaryDiagnosisDetails?.Substring(0, Math.Min(50, PrimaryDiagnosisDetails?.Length ?? 0)) ?? "NULL");
        
        // LEGACY fields for backwards compatibility
        Model.CoduriICD10 = PrimaryDiagnosisCode?.Code;
        Model.DiagnosticPozitiv = PrimaryDiagnosisDetails ?? PrimaryDiagnosisCode?.ShortDescription;

        // Sync Secondary Diagnoses - NEW normalized list
        // Each SecondaryDiagnosis has: Description (RTE content) + ICD10Codes (list of codes)
        // We create one DiagnosticSecundarDto per ICD10Code, or one per diagnosis if no codes
        if (SecondaryDiagnosesList.Any())
        {
            var diagnostice = new List<DiagnosticSecundarDto>();
            int ordine = 1;
            
            foreach (var diagnosis in SecondaryDiagnosesList)
            {
                if (diagnosis.ICD10Codes.Any())
                {
                    // Has ICD10 codes - create entry for each code
                    foreach (var code in diagnosis.ICD10Codes)
                    {
                        diagnostice.Add(new DiagnosticSecundarDto
                        {
                            // Preserve DatabaseId for MERGE logic (Guid.Empty = new, valid = existing)
                            Id = diagnosis.DatabaseId,
                            OrdineAfisare = ordine++,
                            CodICD10 = code.Code,
                            NumeDiagnostic = code.ShortDescription,
                            Descriere = diagnosis.Description
                        });
                    }
                }
                else if (!string.IsNullOrWhiteSpace(diagnosis.Description))
                {
                    // No ICD10 code but has description - save anyway
                    diagnostice.Add(new DiagnosticSecundarDto
                    {
                        // Preserve DatabaseId for MERGE logic
                        Id = diagnosis.DatabaseId,
                        OrdineAfisare = ordine++,
                        CodICD10 = null,
                        NumeDiagnostic = null,
                        Descriere = diagnosis.Description
                    });
                }
            }
            
            Model.DiagnosticeSecundare = diagnostice.Take(10).ToList();
            
            Logger.LogInformation("[DiagnosticTab] Synced {Count} secondary diagnoses", 
                Model.DiagnosticeSecundare.Count);
        }
        else
        {
            Model.DiagnosticeSecundare = null;
        }
    }
}
