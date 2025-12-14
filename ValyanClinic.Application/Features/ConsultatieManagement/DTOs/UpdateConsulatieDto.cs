namespace ValyanClinic.Application.Features.ConsultatieManagement.DTOs;

/// <summary>
/// DTO pentru actualizarea unei consultatii existente (toate campurile pentru UPDATE)
/// Folosit de UpdateConsulatieCommand
/// </summary>
public class UpdateConsulatieDto
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

    // ==================== II.A. ANTECEDENTE HEREDO-COLATERALE ====================
    public string? AHC_Mama { get; set; }
    public string? AHC_Tata { get; set; }
    public string? AHC_Frati { get; set; }
    public string? AHC_Bunici { get; set; }
    public string? AHC_Altele { get; set; }

    // ==================== II.B. ANTECEDENTE FIZIOLOGICE ====================
    public string? AF_Nastere { get; set; }
    public string? AF_Dezvoltare { get; set; }
    public string? AF_Menstruatie { get; set; }
    public string? AF_Sarcini { get; set; }
    public string? AF_Alaptare { get; set; }

    // ==================== II.C. ANTECEDENTE PERSONALE PATOLOGICE ====================
    public string? APP_BoliCopilarieAdolescenta { get; set; }
    public string? APP_BoliAdult { get; set; }
    public string? APP_Interventii { get; set; }
    public string? APP_Traumatisme { get; set; }
    public string? APP_Transfuzii { get; set; }
    public string? APP_Alergii { get; set; }
    public string? APP_Medicatie { get; set; }

    // ==================== II.D. CONDITII SOCIO-ECONOMICE ====================
    public string? Profesie { get; set; }
    public string? ConditiiLocuinta { get; set; }
    public string? ConditiiMunca { get; set; }
    public string? ObiceiuriAlimentare { get; set; }
    public string? Toxice { get; set; }

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
