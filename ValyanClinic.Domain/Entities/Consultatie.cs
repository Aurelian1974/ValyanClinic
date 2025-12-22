namespace ValyanClinic.Domain.Entities;

/// <summary>
/// Entitate pentru Consultatie Medicala - Structura completa conform Scrisoare Medicala
/// </summary>
public class Consultatie
{
    // ==================== PRIMARY KEY ====================
    public Guid ConsultatieID { get; set; }

    // ==================== FOREIGN KEYS ====================
    public Guid? ProgramareID { get; set; } // ✅ CHANGED: Nullable - consultație poate fi creată fără programare (walk-in)
    public Guid PacientID { get; set; }
    public Guid MedicID { get; set; }

    // ==================== DATE CONSULTATIE ====================
    public DateTime DataConsultatie { get; set; }
    public TimeSpan OraConsultatie { get; set; }
    public string TipConsultatie { get; set; } = string.Empty; // Prima consultatie, Control, Urgenta

    // ==================== I. MOTIVE PREZENTARE ====================
    public string? MotivPrezentare { get; set; }
    public string? IstoricBoalaActuala { get; set; }

    // ==================== II. ANTECEDENTE ====================

    // A. Antecedente Heredo-Colaterale (AHC)
    public string? AHC_Mama { get; set; }
    public string? AHC_Tata { get; set; }
    public string? AHC_Frati { get; set; }
    public string? AHC_Bunici { get; set; }
    public string? AHC_Altele { get; set; }

    // B. Antecedente Fiziologice (AF)
    public string? AF_Nastere { get; set; }
    public string? AF_Dezvoltare { get; set; }
    public string? AF_Menstruatie { get; set; } // Pentru femei
    public string? AF_Sarcini { get; set; } // Pentru femei
    public string? AF_Alaptare { get; set; } // Pentru femei

    // C. Antecedente Personale Patologice (APP)
    public string? APP_BoliCopilarieAdolescenta { get; set; }
    public string? APP_BoliAdult { get; set; }
    public string? APP_Interventii { get; set; }
    public string? APP_Traumatisme { get; set; }
    public string? APP_Transfuzii { get; set; }
    public string? APP_Alergii { get; set; }
    public string? APP_Medicatie { get; set; }

    // D. Conditii Socio-Economice
    public string? Profesie { get; set; }
    public string? ConditiiLocuinta { get; set; }
    public string? ConditiiMunca { get; set; }
    public string? ObiceiuriAlimentare { get; set; }
    public string? Toxice { get; set; } // Tutun, Alcool, Droguri

    // ==================== III. EXAMEN OBIECTIV ====================

    // A. Examen General
    public string? StareGenerala { get; set; }
    public string? Constitutie { get; set; }
    public string? Atitudine { get; set; }
    public string? Facies { get; set; }
    public string? Tegumente { get; set; }
    public string? Mucoase { get; set; }
    public string? GangliniLimfatici { get; set; }
    public string? Edeme { get; set; } // ✅ ADDED: Edeme field from UI

    // B. Semne Vitale
    public decimal? Greutate { get; set; } // kg
    public decimal? Inaltime { get; set; } // cm
    public decimal? IMC { get; set; } // calculat automat
    public decimal? Temperatura { get; set; } // °C
    public string? TensiuneArteriala { get; set; } // ex: 120/80 mmHg
    public int? Puls { get; set; } // bpm
    public int? FreccventaRespiratorie { get; set; } // /min
    public int? SaturatieO2 { get; set; } // %
    public decimal? Glicemie { get; set; } // mg/dL

    // C. Examen pe Aparate/Sisteme
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

    // ==================== IV. INVESTIGATII EFECTUATE ====================
    public string? InvestigatiiLaborator { get; set; }
    public string? InvestigatiiImagistice { get; set; }
    public string? InvestigatiiEKG { get; set; }
    public string? AlteInvestigatii { get; set; }

    // ==================== V. DIAGNOSTIC ====================
    public string? DiagnosticPozitiv { get; set; }
    public string? DiagnosticDiferential { get; set; }
    public string? DiagnosticEtiologic { get; set; }
    public string? CoduriICD10 { get; set; } // Cod ICD-10 principal
    public string? CoduriICD10Secundare { get; set; } // Coduri ICD-10 secundare (comma-separated)

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

    // ==================== VIII. PROGNOSTIC ====================
    public string? Prognostic { get; set; } // Favorabil, Rezervat, Sever

    // ==================== IX. CONCLUZIE ====================
    public string? Concluzie { get; set; }

    // ==================== X. OBSERVATII SUPLIMENTARE ====================
    public string? ObservatiiMedic { get; set; }
    public string? NotePacient { get; set; }

    // ==================== STATUS & WORKFLOW ====================
    public string Status { get; set; } = "In desfasurare"; // In desfasurare, Finalizata, Anulata
    public DateTime? DataFinalizare { get; set; }
    public int DurataMinute { get; set; } // Durata efectiva a consultatiei

    // ==================== DOCUMENTE ANEXATE ====================
    public string? DocumenteAtatate { get; set; } // JSON array cu paths catre documente

    // ==================== AUDIT ====================
    public DateTime DataCreare { get; set; }
    public Guid CreatDe { get; set; }
    public DateTime? DataUltimeiModificari { get; set; }
    public Guid? ModificatDe { get; set; }

    // ==================== COMPUTED PROPERTIES ====================
    public DateTime DataOraConsultatie => DataConsultatie.Date + OraConsultatie;

    public decimal? IMCCalculat
    {
        get
        {
            if (Greutate.HasValue && Inaltime.HasValue && Inaltime > 0)
            {
                var inaltimeMetri = Inaltime.Value / 100;
                return Math.Round(Greutate.Value / (inaltimeMetri * inaltimeMetri), 2);
            }
            return null;
        }
    }

    public string InterpretareIMC
    {
        get
        {
            var imc = IMCCalculat ?? IMC;
            if (!imc.HasValue) return "N/A";

            return imc.Value switch
            {
                < 18.5m => "Subponderal",
                >= 18.5m and < 25m => "Normal",
                >= 25m and < 30m => "Supraponderal",
                >= 30m and < 35m => "Obezitate grad I",
                >= 35m and < 40m => "Obezitate grad II",
                >= 40m => "Obezitate morbida"
            };
        }
    }
}
