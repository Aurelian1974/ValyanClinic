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

        // Load primary diagnosis - use NEW fields first, fallback to legacy
        if ((!string.IsNullOrEmpty(Model.CodICD10Principal) || !string.IsNullOrEmpty(Model.CoduriICD10)) 
            && PrimaryDiagnosisCode == null)
        {
            LoadPrimaryDiagnosisFromModel();
        }

        // Load secondary diagnoses - use NEW list first, fallback to legacy
        if ((Model.DiagnosticeSecundare?.Any() == true || !string.IsNullOrEmpty(Model.CoduriICD10Secundare)) 
            && !SecondaryDiagnosesList.Any())
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
                    Id = Guid.NewGuid(),
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

        // Fallback to LEGACY (comma-separated codes)
        if (string.IsNullOrEmpty(Model.CoduriICD10Secundare))
        {
            SecondaryDiagnosesList.Clear();
            return;
        }

        var codes = Model.CoduriICD10Secundare
            .Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(c => c.Trim())
            .ToList();

        SecondaryDiagnosesList = codes.Select(code => new SecondaryDiagnosis
        {
            Id = Guid.NewGuid(),
            Description = code, // Just the code as description (legacy)
            ICD10Codes = new List<ICD10SearchResultDto>
            {
                new ICD10SearchResultDto { Code = code, ShortDescription = code }
            }
        }).ToList();
        
        Logger.LogInformation("[DiagnosticTab] Loaded {Count} secondary diagnoses (LEGACY)", 
            SecondaryDiagnosesList.Count);
    }

    // ==================== DATA SYNCING ====================

    /// <summary>Sync primary and secondary diagnoses to Model</summary>
    private void SyncToModel()
    {
        // Sync Primary Diagnosis - NEW normalized fields
        Model.CodICD10Principal = PrimaryDiagnosisCode?.Code;
        Model.NumeDiagnosticPrincipal = PrimaryDiagnosisCode?.ShortDescription;
        Model.DescriereDetaliataPrincipal = PrimaryDiagnosisDetails;
        
        // LEGACY fields for backwards compatibility
        Model.CoduriICD10 = PrimaryDiagnosisCode?.Code;
        Model.DiagnosticPozitiv = PrimaryDiagnosisDetails ?? PrimaryDiagnosisCode?.ShortDescription;

        // Sync Secondary Diagnoses - NEW normalized list
        if (SecondaryDiagnosesList.Any())
        {
            Model.DiagnosticeSecundare = SecondaryDiagnosesList
                .SelectMany((diagnosis, diagIndex) => 
                    diagnosis.ICD10Codes.Select((code, codeIndex) => new DiagnosticSecundarDto
                    {
                        OrdineAfisare = diagIndex * 10 + codeIndex + 1, // 1, 2, 3... 11, 12...
                        CodICD10 = code.Code,
                        NumeDiagnostic = code.ShortDescription,
                        Descriere = diagnosis.Description // Description is the RTE content
                    }))
                .Take(10) // Max 10 secondary diagnoses
                .ToList();

            // LEGACY: Collect all ICD10 codes for CoduriICD10Secundare
            var allSecondaryCodes = SecondaryDiagnosesList
                .SelectMany(d => d.ICD10Codes.Select(c => c.Code))
                .Distinct()
                .ToList();

            Model.CoduriICD10Secundare = string.Join(", ", allSecondaryCodes);
            
            Logger.LogInformation("[DiagnosticTab] Synced {Count} secondary diagnoses, codes: {Codes}", 
                Model.DiagnosticeSecundare.Count, Model.CoduriICD10Secundare);
        }
        else
        {
            Model.DiagnosticeSecundare = null;
            Model.CoduriICD10Secundare = null;
        }
    }
}
