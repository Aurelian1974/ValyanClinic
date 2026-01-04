using MediatR;
using ValyanClinic.Application.Common.Results;
using ValyanClinic.Application.Features.ConsultatieManagement.DTOs;

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
    
    // Antecedente (SIMPLIFIED)
    /// <summary>Istoric medical personal - boli anterioare, interventii, alergii, medicatie cronica</summary>
    public string? IstoricMedicalPersonal { get; set; }
    /// <summary>Istoric familial - antecedente heredocolaterale</summary>
    public string? IstoricFamilial { get; set; }
    /// <summary>Tratament urmat anterior (medicație, proceduri, intervenții) - Anexa 43</summary>
    public string? TratamentAnterior { get; set; }
    /// <summary>Factori de risc identificați (HTA, diabet, fumat, etc.) - Anexa 43</summary>
    public string? FactoriDeRisc { get; set; }
    /// <summary>Alergii cunoscute (medicamente, alimente, substanțe) - Anexa 43</summary>
    public string? Alergii { get; set; }

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
    
    // Diagnostic Principal (structură normalizată)
    /// <summary>Codul ICD-10 pentru diagnosticul principal (ex: "I10")</summary>
    public string? CodICD10Principal { get; set; }
    /// <summary>Numele diagnosticului principal din catalog ICD-10</summary>
    public string? NumeDiagnosticPrincipal { get; set; }
    /// <summary>Descriere detaliată diagnostic principal (HTML din RTE)</summary>
    public string? DescriereDetaliataPrincipal { get; set; }
    
    // Diagnostice Secundare (listă normalizată, max 10)
    /// <summary>Lista diagnosticelor secundare cu cod ICD10 + descriere</summary>
    public List<DiagnosticSecundarDto>? DiagnosticeSecundare { get; set; }
    
    // LEGACY (kept for backwards compatibility)
    public string? DiagnosticPozitiv { get; set; }
    public string? CoduriICD10 { get; set; }

    // Tratament
    public string? TratamentMedicamentos { get; set; } // PlanTerapeutic din UI
    public string? RecomandariRegimViata { get; set; } // Recomandari din UI
    
    /// <summary>Lista de medicamente prescrise pentru tratament recomandat</summary>
    public List<MedicationRowDto>? MedicationList { get; set; }

    // ==================== TAB 4: CONCLUZII ====================
    public string? Concluzie { get; set; } // Concluzii din UI
    public string? ObservatiiMedic { get; set; }
    public string? DataUrmatoareiProgramari { get; set; } // NoteUrmatoareaVizita din UI
    
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
    public Guid CreatDeSauModificatDe { get; set; }
}
