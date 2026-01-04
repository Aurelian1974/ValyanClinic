using MediatR;
using ValyanClinic.Application.Common.Results;
using ValyanClinic.Application.Features.ConsultatieManagement.DTOs;

namespace ValyanClinic.Application.Features.ConsultatieManagement.Commands.CreateConsultatie;

/// <summary>
/// Command pentru crearea unei consultatii medicale complete
/// </summary>
public class CreateConsultatieCommand : IRequest<Result<Guid>>
{
    // ==================== IDENTIFICARE ====================
    public Guid ProgramareID { get; set; }
    public Guid PacientID { get; set; }
    public Guid MedicID { get; set; }
    public string TipConsultatie { get; set; } = "Prima consultatie";

    // ==================== MOTIVE PREZENTARE ====================
    public string? MotivPrezentare { get; set; }
    public string? IstoricBoalaActuala { get; set; }

    // ==================== ANTECEDENTE (SIMPLIFICAT) ====================
    /// <summary>Istoric medical personal (boli anterioare, intervenții, alergii, tratamente cronice)</summary>
    public string? IstoricMedicalPersonal { get; set; }
    
    /// <summary>Istoric familial (boli ereditare, antecedente în familie)</summary>
    public string? IstoricFamilial { get; set; }

    // ==================== EXAMEN OBIECTIV ====================
    public string? StareGenerala { get; set; }
    public string? Constitutie { get; set; }
    public string? Atitudine { get; set; }
    public string? Facies { get; set; }
    public string? Tegumente { get; set; }
    public string? Mucoase { get; set; }
    public string? GangliniLimfatici { get; set; }

    public decimal? Greutate { get; set; }
    public decimal? Inaltime { get; set; }
    public decimal? Temperatura { get; set; }
    public string? TensiuneArteriala { get; set; }
    public int? Puls { get; set; }
    public int? FreccventaRespiratorie { get; set; }
    public int? SaturatieO2 { get; set; }
    public decimal? Glicemie { get; set; }

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

    // ==================== INVESTIGATII ====================
    public string? InvestigatiiLaborator { get; set; }
    public string? InvestigatiiImagistice { get; set; }
    public string? InvestigatiiEKG { get; set; }
    public string? AlteInvestigatii { get; set; }

    // ==================== DIAGNOSTIC ====================
    public string? DiagnosticPozitiv { get; set; }
    public string? DiagnosticDiferential { get; set; }
    public string? DiagnosticEtiologic { get; set; }
    public string? CoduriICD10 { get; set; } // Cod principal
    public string? CoduriICD10Secundare { get; set; } // Coduri secundare (comma-separated)

    // ==================== TRATAMENT ====================
    
    /// <summary>Tratament efectuat anterior consultației</summary>
    public string? TratamentAnterior { get; set; }
    
    public string? TratamentMedicamentos { get; set; }
    public string? TratamentNemedicamentos { get; set; }
    public string? RecomandariDietetice { get; set; }
    public string? RecomandariRegimViata { get; set; }

    /// <summary>Lista medicamentelor prescrise</summary>
    public List<MedicationRowDto>? MedicationList { get; set; }

    // ==================== RECOMANDARI ====================
    public string? InvestigatiiRecomandate { get; set; }
    public string? ConsulturiSpecialitate { get; set; }
    public string? DataUrmatoareiProgramari { get; set; }
    public string? RecomandariSupraveghere { get; set; }

    // ==================== PROGNOSTIC & CONCLUZIE ====================
    public string? Prognostic { get; set; }
    public string? Concluzie { get; set; }

    // ==================== OBSERVATII ====================
    public string? ObservatiiMedic { get; set; }
    public string? NotePacient { get; set; }

    // ==================== SCRISOARE MEDICALĂ - ANEXA 43 ====================
    
    /// <summary>Pacient diagnosticat cu afecțiune oncologică</summary>
    public bool EsteAfectiuneOncologica { get; set; }
    public string? DetaliiAfectiuneOncologica { get; set; }
    
    /// <summary>Indicație de revenire pentru internare</summary>
    public bool AreIndicatieInternare { get; set; }
    public string? TermenInternare { get; set; }
    
    /// <summary>Prescripție medicală</summary>
    public bool? SaEliberatPrescriptie { get; set; }
    public string? SeriePrescriptie { get; set; }
    
    /// <summary>Concediu medical</summary>
    public bool? SaEliberatConcediuMedical { get; set; }
    public string? SerieConcediuMedical { get; set; }
    
    /// <summary>Îngrijiri medicale la domiciliu</summary>
    public bool? SaEliberatIngrijiriDomiciliu { get; set; }
    
    /// <summary>Dispozitive medicale</summary>
    public bool? SaEliberatDispozitiveMedicale { get; set; }
    
    /// <summary>Calea de transmitere</summary>
    public bool TransmiterePrinEmail { get; set; }
    public string? EmailTransmitere { get; set; }

    // ==================== AUDIT ====================
    public string CreatDe { get; set; } = string.Empty;
}
