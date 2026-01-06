using MediatR;
using ValyanClinic.Application.Common.Results;
using ValyanClinic.Application.Features.ConsultatieManagement.DTOs;

namespace ValyanClinic.Application.Features.ConsultatieManagement.Commands.CreateConsultatie;

/// <summary>
/// Command pentru crearea unei consultatii medicale noi
/// Implementează pattern-ul CQRS cu MediatR
/// </summary>
public class CreateConsulatieCommand : IRequest<Result<Guid>>
{
    // ==================== REQUIRED FIELDS ====================
    public Guid ProgramareID { get; set; }
    public Guid PacientID { get; set; }
    public Guid MedicID { get; set; }
    public DateTime DataConsultatie { get; set; } = DateTime.Today;
    public TimeSpan OraConsultatie { get; set; } = DateTime.Now.TimeOfDay;
    public string TipConsultatie { get; set; } = "Prima consultatie";

    // ==================== I. MOTIVE PREZENTARE ====================
    public string? MotivPrezentare { get; set; }
    public string? IstoricBoalaActuala { get; set; }

    // ==================== II. ANTECEDENTE (SIMPLIFIED) ====================
    /// <summary>Istoric medical personal - boli anterioare, intervenții, alergii, medicație cronică</summary>
    public string? IstoricMedicalPersonal { get; set; }
    /// <summary>Istoric familial - antecedente heredocolaterale</summary>
    public string? IstoricFamilial { get; set; }

    // ==================== III.A. EXAMEN GENERAL ====================
    public string? StareGenerala { get; set; }
    public string? Tegumente { get; set; }
    public string? Mucoase { get; set; }
    public string? GanglioniLimfatici { get; set; }

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

    // ==================== IV. INVESTIGATII ====================
    public string? InvestigatiiLaborator { get; set; }
    public string? InvestigatiiImagistice { get; set; }
    public string? InvestigatiiEKG { get; set; }
    public string? AlteInvestigatii { get; set; }

    // ==================== V. DIAGNOSTIC ====================
    public string? DiagnosticPozitiv { get; set; }
    public string? CoduriICD10 { get; set; }
    // Normalized diagnostic fields
    public string? CodICD10Principal { get; set; }
    public string? NumeDiagnosticPrincipal { get; set; }
    public string? DescriereDetaliataPrincipal { get; set; }

    // ==================== VI. TRATAMENT ====================
    public string? TratamentMedicamentos { get; set; }
    public string? TratamentNemedicamentos { get; set; }
    public string? RecomandariDietetice { get; set; }
    public string? RecomandariRegimViata { get; set; }
    
    /// <summary>Lista de medicamente prescrise pentru tratament recomandat</summary>
    public List<MedicationRowDto>? MedicationList { get; set; }

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

    // ==================== AUDIT ====================
    public Guid CreatDe { get; set; }
}
