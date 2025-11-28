using MediatR;
using ValyanClinic.Application.Common.Results;

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

    // ==================== ANTECEDENTE ====================
    public string? AHC_Mama { get; set; }
    public string? AHC_Tata { get; set; }
    public string? AHC_Frati { get; set; }
    public string? AHC_Bunici { get; set; }
    public string? AHC_Altele { get; set; }

    public string? AF_Nastere { get; set; }
    public string? AF_Dezvoltare { get; set; }
    public string? AF_Menstruatie { get; set; }
    public string? AF_Sarcini { get; set; }
    public string? AF_Alaptare { get; set; }

    public string? APP_BoliCopilarieAdolescenta { get; set; }
    public string? APP_BoliAdult { get; set; }
    public string? APP_Interventii { get; set; }
    public string? APP_Traumatisme { get; set; }
    public string? APP_Transfuzii { get; set; }
    public string? APP_Alergii { get; set; }
    public string? APP_Medicatie { get; set; }

    public string? Profesie { get; set; }
    public string? ConditiiLocuinta { get; set; }
    public string? ConditiiMunca { get; set; }
    public string? ObiceiuriAlimentare { get; set; }
    public string? Toxice { get; set; }

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
    public string? TratamentMedicamentos { get; set; }
    public string? TratamentNemedicamentos { get; set; }
    public string? RecomandariDietetice { get; set; }
    public string? RecomandariRegimViata { get; set; }

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

    // ==================== AUDIT ====================
    public string CreatDe { get; set; } = string.Empty;
}
