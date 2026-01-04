using MediatR;
using ValyanClinic.Application.Common.Results;

namespace ValyanClinic.Application.Features.ConsultatieManagement.Commands.UpdateConsultatie;

/// <summary>
/// Command pentru actualizarea unei consultatii medicale existente
/// Implementează pattern-ul CQRS cu MediatR
/// </summary>
public class UpdateConsulatieCommand : IRequest<Result<bool>>
{
    // ==================== REQUIRED FIELDS ====================
    public Guid ConsultatieID { get; set; }
    public Guid ProgramareID { get; set; }
    public Guid PacientID { get; set; }
    public Guid MedicID { get; set; }
    public DateTime DataConsultatie { get; set; }
    public TimeSpan OraConsultatie { get; set; }
    public string TipConsultatie { get; set; } = string.Empty;

    // ==================== I. MOTIVE PREZENTARE ====================
    public string? MotivPrezentare { get; set; }
    public string? IstoricBoalaActuala { get; set; }

    // ==================== II. ANTECEDENTE (SIMPLIFIED + Anexa 43) ====================
    /// <summary>Istoric medical personal - boli anterioare, intervenții, alergii, medicație cronică</summary>
    public string? IstoricMedicalPersonal { get; set; }
    /// <summary>Istoric familial - antecedente heredocolaterale</summary>
    public string? IstoricFamilial { get; set; }
    /// <summary>Tratament urmat anterior (medicație, proceduri, intervenții) - Anexa 43</summary>
    public string? TratamentAnterior { get; set; }
    /// <summary>Factori de risc identificați (HTA, diabet, fumat, etc.) - Anexa 43</summary>
    public string? FactoriDeRisc { get; set; }

    // ==================== III.A. EXAMEN GENERAL ====================
    public string? StareGenerala { get; set; }
    public string? Constitutie { get; set; }
    public string? Atitudine { get; set; }
    public string? Facies { get; set; }
    public string? Tegumente { get; set; }
    public string? Mucoase { get; set; }
    public string? GangliniLimfatici { get; set; }

    // ==================== III.B. SEMNE VITALE ====================
    public decimal? Greutate { get; set; }
    public decimal? Inaltime { get; set; }
    public decimal? IMC { get; set; }
    public decimal? Temperatura { get; set; }
    public string? TensiuneArteriala { get; set; }
    public int? Puls { get; set; }
    public int? FreccventaRespiratorie { get; set; }
    public int? SaturatieO2 { get; set; }
    public decimal? Glicemie { get; set; }

    // ==================== III.C. EXAMEN PE APARATE ====================
    public string? ExamenCardiovascular { get; set; }
    public string? ExamenRespiratoriu { get; set; }
    public string? ExamenDigestiv { get; set; }
    public string? ExamenUrinar { get; set; }
    public string? ExamenNervos { get; set; }
    public string? ExamenLocomotor { get; set; }
    public string? ExamenEndocrin { get; set; }
    public string? ExamenORL { get; set; }
    public string? ExamenOftalmologic { get; set; }
    public string? ExamenDermatologic { get; set; }

    // ==================== IV. INVESTIGATII ====================
    public string? InvestigatiiLaborator { get; set; }
    public string? InvestigatiiImagistice { get; set; }
    public string? InvestigatiiEKG { get; set; }
    public string? AlteInvestigatii { get; set; }

    // ==================== V. DIAGNOSTIC ====================
    public string? DiagnosticPozitiv { get; set; }
    public string? DiagnosticDiferential { get; set; }
    public string? DiagnosticEtiologic { get; set; }
    public string? CoduriICD10 { get; set; }
    public string? CoduriICD10Secundare { get; set; }

    // ==================== VI. TRATAMENT ====================
    public string? TratamentMedicamentos { get; set; }
    public string? TratamentNemedicamentos { get; set; }
    public string? RecomandariDietetice { get; set; }
    public string? RecomandariRegimViata { get; set; }

    // ==================== VII. RECOMANDARI ====================
    public string? InvestigatiiRecomandate { get; set; }
    public string? ConsulturiSpecialitate { get; set; }
    public string? DataUrmatoareiProgramari { get; set; }
    public string? RecomandariSupraveghere { get; set; }

    // ==================== VIII. PROGNOSTIC & CONCLUZIE ====================
    public string? Prognostic { get; set; }
    public string? Concluzie { get; set; }
    public string? ObservatiiMedic { get; set; }
    public string? NotePacient { get; set; }

    // ==================== STATUS ====================
    public string Status { get; set; } = "In desfasurare";
    public DateTime? DataFinalizare { get; set; }
    public int DurataMinute { get; set; }

    // ==================== AUDIT ====================
    public Guid ModificatDe { get; set; }
}
