using MediatR;
using ValyanClinic.Application.Common.Results;

namespace ValyanClinic.Application.Features.ConsultatieManagement.Commands.SaveConsultatieDraft;

/// <summary>
/// Command pentru salvarea draft-ului unei consultatii (auto-save complet)
/// Contine toate campurile din UI pentru sincronizare cu DB
/// Foloseste sp_Consultatie_SaveDraft care face INSERT/UPDATE inteligent
/// </summary>
public class SaveConsultatieDraftCommand : IRequest<Result<Guid>>
{
    // ==================== REQUIRED FIELDS ====================
    public Guid? ConsultatieID { get; set; } // Null pentru CREATE, valoare pentru UPDATE
    public Guid? ProgramareID { get; set; } // Nullable - consultație poate fi creată fără programare
    public Guid PacientID { get; set; }
    public Guid MedicID { get; set; }
    public DateTime DataConsultatie { get; set; } = DateTime.Today;
    public TimeSpan OraConsultatie { get; set; } = DateTime.Now.TimeOfDay;
    public string TipConsultatie { get; set; } = "Prima consultatie";

    // ==================== TAB 1: MOTIV & ANTECEDENTE ====================
    public string? MotivPrezentare { get; set; }
    public string? IstoricBoalaActuala { get; set; } // AntecedentePatologice din UI
    public string? APP_Medicatie { get; set; } // TratamenteActuale din UI

    // ==================== TAB 2: EXAMEN CLINIC ====================
    
    // Semne Vitale
    public decimal? Greutate { get; set; }
    public decimal? Inaltime { get; set; }
    public decimal? IMC { get; set; }
    public decimal? Temperatura { get; set; }
    public string? TensiuneArteriala { get; set; }
    public int? Puls { get; set; }
    public int? FreccventaRespiratorie { get; set; } // FreqventaRespiratorie din UI
    public int? SaturatieO2 { get; set; } // SpO2 din UI

    // Examen General
    public string? StareGenerala { get; set; }
    public string? Tegumente { get; set; }
    public string? Mucoase { get; set; }
    public string? Edeme { get; set; }
    public string? ExamenCardiovascular { get; set; } // ExamenObiectiv din UI

    // Investigații
    public string? InvestigatiiLaborator { get; set; } // InvestigatiiParaclinice din UI

    // ==================== TAB 3: DIAGNOSTIC & TRATAMENT ====================
    
    // Diagnostic
    public string? DiagnosticPozitiv { get; set; } // DiagnosticPrincipal din UI
    public string? DiagnosticDiferential { get; set; } // DiagnosticSecundar din UI
    public string? CoduriICD10 { get; set; }
    public string? CoduriICD10Secundare { get; set; }

    // Tratament
    public string? TratamentMedicamentos { get; set; } // PlanTerapeutic din UI
    public string? RecomandariRegimViata { get; set; } // Recomandari din UI

    // ==================== TAB 4: CONCLUZII ====================
    public string? Concluzie { get; set; } // Concluzii din UI
    public string? ObservatiiMedic { get; set; }
    public string? DataUrmatoareiProgramari { get; set; } // NoteUrmatoareaVizita din UI

    // ==================== AUDIT ====================
    public Guid CreatDeSauModificatDe { get; set; }
}
