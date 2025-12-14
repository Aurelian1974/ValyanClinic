using MediatR;
using ValyanClinic.Application.Common.Results;

namespace ValyanClinic.Application.Features.ConsultatieManagement.Commands.SaveConsultatieDraft;

/// <summary>
/// Command pentru salvarea draft-ului unei consultatii (auto-save optimizat)
/// Contine doar campurile esentiale frecvent completate
/// Foloseste sp_Consultatie_SaveDraft care face INSERT/UPDATE inteligent
/// </summary>
public class SaveConsultatieDraftCommand : IRequest<Result<Guid>>
{
    // ==================== REQUIRED FIELDS ====================
    public Guid? ConsultatieID { get; set; } // Null pentru CREATE, valoare pentru UPDATE
    public Guid? ProgramareID { get; set; } // ✅ CHANGED: Nullable - consultație poate fi creată fără programare
    public Guid PacientID { get; set; }
    public Guid MedicID { get; set; }
    public DateTime DataConsultatie { get; set; } = DateTime.Today;
    public TimeSpan OraConsultatie { get; set; } = DateTime.Now.TimeOfDay;
    public string TipConsultatie { get; set; } = "Prima consultatie";

    // ==================== ESSENTIAL FIELDS (Most frequently filled) ====================
    public string? MotivPrezentare { get; set; }
    public string? IstoricBoalaActuala { get; set; }

    // Semne Vitale (Essential measurements)
    public decimal? Greutate { get; set; }
    public decimal? Inaltime { get; set; }
    public decimal? IMC { get; set; }
    public decimal? Temperatura { get; set; }
    public string? TensiuneArteriala { get; set; }
    public int? Puls { get; set; }

    // Diagnostic (Primary fields)
    public string? DiagnosticPozitiv { get; set; }
    public string? CoduriICD10 { get; set; }

    // Tratament (Primary field)
    public string? TratamentMedicamentos { get; set; }

    // Observatii
    public string? ObservatiiMedic { get; set; }

    // ==================== AUDIT ====================
    public Guid CreatDeSauModificatDe { get; set; }
}
