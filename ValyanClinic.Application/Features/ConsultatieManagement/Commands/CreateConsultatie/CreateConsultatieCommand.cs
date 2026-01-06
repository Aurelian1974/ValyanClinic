using MediatR;
using ValyanClinic.Application.Common.Results;
using ValyanClinic.Application.Features.ConsultatieManagement.Commands.SaveConsultatieDraft;
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

    // ==================== ANTECEDENTE (SIMPLIFICAT + Anexa 43) ====================
    /// <summary>Istoric medical personal (boli anterioare, intervenții, alergii, tratamente cronice)</summary>
    public string? IstoricMedicalPersonal { get; set; }
    
    /// <summary>Istoric familial (boli ereditare, antecedente în familie)</summary>
    public string? IstoricFamilial { get; set; }

    /// <summary>Tratament urmat anterior (medicație, proceduri, intervenții) - Anexa 43</summary>
    public string? TratamentAnterior { get; set; }

    /// <summary>Factori de risc identificați (HTA, diabet, fumat, sedentarism, etc.) - Anexa 43</summary>
    public string? FactoriDeRisc { get; set; }

    // ==================== EXAMEN OBIECTIV ====================
    public string? StareGenerala { get; set; }
    public string? Tegumente { get; set; }
    public string? Mucoase { get; set; }
    public string? GanglioniLimfatici { get; set; }

    public decimal? Greutate { get; set; }
    public decimal? Inaltime { get; set; }
    public decimal? Temperatura { get; set; }
    public string? TensiuneArteriala { get; set; }
    public int? Puls { get; set; }
    public int? FreccventaRespiratorie { get; set; }
    public int? SaturatieO2 { get; set; }
    public decimal? Glicemie { get; set; }

    // ==================== INVESTIGATII ====================
    public string? InvestigatiiLaborator { get; set; }
    public string? InvestigatiiImagistice { get; set; }
    public string? InvestigatiiEKG { get; set; }
    public string? AlteInvestigatii { get; set; }

    // ==================== DIAGNOSTIC ====================
    
    // Normalized diagnostic structure for Scrisoare Medicală
    /// <summary>Codul ICD-10 pentru diagnosticul principal (ex: "I10")</summary>
    public string? CodICD10Principal { get; set; }
    /// <summary>Numele diagnosticului principal din catalog ICD-10</summary>
    public string? NumeDiagnosticPrincipal { get; set; }
    /// <summary>Descriere detaliată diagnostic principal (HTML din RTE)</summary>
    public string? DescriereDetaliataPrincipal { get; set; }
    
    /// <summary>Lista diagnosticelor secundare cu cod ICD10 + descriere (max 10)</summary>
    public List<DiagnosticSecundarDto>? DiagnosticeSecundare { get; set; }
    
    // LEGACY: Kept for backwards compatibility
    public string? DiagnosticPozitiv { get; set; }
    public string? CoduriICD10 { get; set; }

    // ==================== TRATAMENT ====================
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
