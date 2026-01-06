namespace ValyanClinic.Application.Features.ConsultatieManagement.DTOs;

/// <summary>
/// DTO pentru Scrisoare Medicală Anexa 43
/// Conține toate datele necesare pentru generarea documentului conform Ordin MS nr. 1411/2016
/// </summary>
public class ScrisoareMedicalaDto
{
    // ==================== HEADER - INFORMAȚII CLINICĂ ====================
    public string NumeClinica { get; set; } = "ValyanClinic";
    public string TipClinica { get; set; } = "Clinică Medicală de Specialitate";
    public string AdresaClinica { get; set; } = string.Empty;
    public string TelefonClinica { get; set; } = string.Empty;
    public string EmailClinica { get; set; } = string.Empty;
    public string CUIClinica { get; set; } = string.Empty;
    public string RegistruComertClinica { get; set; } = string.Empty;
    public string ContractCAS { get; set; } = string.Empty;
    public string CASJudet { get; set; } = string.Empty;
    public string NumarRegistruConsultatii { get; set; } = string.Empty;

    // ==================== DATE CONSULTAȚIE ====================
    public Guid ConsultatieId { get; set; }
    public DateTime DataConsultatie { get; set; }
    public string TipConsultatie { get; set; } = string.Empty;

    // ==================== DATE PACIENT ====================
    public Guid PacientId { get; set; }
    public string PacientNumeComplet { get; set; } = string.Empty;
    public string? PacientCNP { get; set; }
    public DateTime? PacientDataNasterii { get; set; }
    public int? PacientVarsta { get; set; }
    public string? PacientSex { get; set; }
    public string? PacientAdresa { get; set; }
    public string? PacientTelefon { get; set; }
    public string? PacientEmail { get; set; }

    // ==================== MOTIV PREZENTARE ====================
    public string? MotivPrezentare { get; set; }
    public string? IstoricBoalaActuala { get; set; }

    // ==================== AFECȚIUNE ONCOLOGICĂ ====================
    public bool EsteAfectiuneOncologica { get; set; } = false;
    public string? DetaliiAfectiuneOncologica { get; set; }

    // ==================== DIAGNOSTIC ====================
    public DiagnosticScrisoareDto? DiagnosticPrincipal { get; set; }
    public List<DiagnosticScrisoareDto> DiagnosticeSecundare { get; set; } = new();

    // ==================== ANAMNEZA ====================
    public string? AntecendenteHeredoColaterale { get; set; }
    public string? AntecendentePatologicePersonale { get; set; }
    public string? Alergii { get; set; }
    public string? MedicatieCronicaAnterioara { get; set; }
    public string? FactoriDeRisc { get; set; }

    // ==================== EXAMEN CLINIC GENERAL ====================
    public string? StareGenerala { get; set; }
    public string? TensiuneArteriala { get; set; }
    public int? Puls { get; set; }
    public decimal? Temperatura { get; set; }
    public int? FrecventaRespiratorie { get; set; }
    public decimal? Greutate { get; set; }
    public decimal? Inaltime { get; set; }
    public decimal? IMC { get; set; }
    public string? IMCCategorie { get; set; }
    public int? SaturatieO2 { get; set; }
    public decimal? Glicemie { get; set; }
    public string? Tegumente { get; set; }
    public string? Mucoase { get; set; }
    public string? GanglioniLimfatici { get; set; }
    public string? Edeme { get; set; }
    public string? ExamenClinicGeneral { get; set; }

    // ==================== EXAMEN CLINIC LOCAL ====================
    public string? ExamenClinicLocal { get; set; }
    public string? ExamenObiectivDetaliat { get; set; }
    public string? AlteObservatiiClinice { get; set; }

    // ==================== INVESTIGAȚII LABORATOR ====================
    public List<RezultatLaboratorDto> RezultateNormale { get; set; } = new();
    public List<RezultatLaboratorDto> RezultatePatologice { get; set; } = new();

    // ==================== INVESTIGAȚII PARACLINICE ====================
    public string? RezultatEKG { get; set; }
    public string? RezultatEcografie { get; set; }
    public string? RezultatRx { get; set; }
    public string? AlteInvestigatii { get; set; }

    // ==================== TRATAMENT EFECTUAT (ANTERIOR) ====================
    public string? TratamentAnterior { get; set; }

    // ==================== ALTE INFORMAȚII ====================
    public string? AlteInformatii { get; set; }

    // ==================== TRATAMENT RECOMANDAT ====================
    public List<MedicamentScrisoareDto> TratamentRecomandat { get; set; } = new();

    // ==================== RECOMANDĂRI ====================
    public List<string> Recomandari { get; set; } = new();

    // ==================== ANALIZE RECOMANDATE ====================
    public List<AnalizaRecomandataScrisoareDto> AnalizeRecomandate { get; set; } = new();

    // ==================== INVESTIGAȚII RECOMANDATE ====================
    public List<InvestigatieRecomandataScrisoareDto> InvestigatiiImagistice { get; set; } = new();
    public List<InvestigatieRecomandataScrisoareDto> Explorari { get; set; } = new();
    public List<InvestigatieRecomandataScrisoareDto> Endoscopii { get; set; } = new();

    // ==================== ANALIZE EFECTUATE (CU REZULTATE) ====================
    public List<AnalizaEfectuataScrisoareDto> AnalizeEfectuate { get; set; } = new();

    // ==================== CHECKBOX SECTIONS ====================
    // Indicație Internare
    public bool AreIndicatieInternare { get; set; } = false;
    public string? TermenInternare { get; set; }

    // Prescripție Medicală
    public bool SaEliberatPrescriptie { get; set; } = false;
    public string? SeriePrescriptie { get; set; }
    public bool NuSaEliberatPrescriptieNuAFostNecesar { get; set; } = true;
    public bool NuSaEliberatPrescriptie { get; set; } = false;

    // Concediu Medical
    public bool SaEliberatConcediuMedical { get; set; } = false;
    public string? SerieConcediuMedical { get; set; }
    public bool NuSaEliberatConcediuNuAFostNecesar { get; set; } = true;
    public bool NuSaEliberatConcediuMedical { get; set; } = false;

    // Îngrijiri la Domiciliu
    public bool SaEliberatRecomandareIngrijiriDomiciliu { get; set; } = false;
    public bool NuSaEliberatIngrijiriNuAFostNecesar { get; set; } = true;

    // Dispozitive Medicale
    public bool SaEliberatPrescriptieDispozitive { get; set; } = false;
    public bool NuSaEliberatDispozitiveNuAFostNecesar { get; set; } = true;

    // ==================== FOOTER - MEDIC ====================
    public Guid MedicId { get; set; }
    public string MedicNumeComplet { get; set; } = string.Empty;
    public string? MedicSpecializare { get; set; }
    public string? MedicCodParafa { get; set; }

    // ==================== TRANSMITERE ====================
    public bool TransmiterePrinAsigurat { get; set; } = true;
    public bool TransmiterePrinEmail { get; set; } = false;
    public string? EmailTransmitere { get; set; }

    // ==================== METADATA ====================
    public DateTime DataEmitere { get; set; } = DateTime.Now;
    
    // ==================== COMPUTED PROPERTIES ====================
    public string DataConsultatieFormatata => DataConsultatie.ToString("dd.MM.yyyy");
    public string DataEmitereFormatata => DataEmitere.ToString("dd.MM.yyyy");
    public string? DataNasteriiFormatata => PacientDataNasterii?.ToString("dd.MM.yyyy");
}

/// <summary>
/// DTO pentru un diagnostic în Scrisoarea Medicală
/// </summary>
public class DiagnosticScrisoareDto
{
    public string CodICD10 { get; set; } = string.Empty;
    public string Denumire { get; set; } = string.Empty;
    public string? Detalii { get; set; }
    public bool EstePrincipal { get; set; } = false;
}

/// <summary>
/// DTO pentru rezultat laborator
/// </summary>
public class RezultatLaboratorDto
{
    public string Denumire { get; set; } = string.Empty;
    public string Valoare { get; set; } = string.Empty;
    public string? Unitate { get; set; }
    public string? ValoareNormala { get; set; }
    public bool EstePatologic { get; set; } = false;
}

/// <summary>
/// DTO pentru medicament în tratament recomandat
/// </summary>
public class MedicamentScrisoareDto
{
    public string Denumire { get; set; } = string.Empty;
    public string Doza { get; set; } = string.Empty;
    public string Frecventa { get; set; } = string.Empty;
    public string Durata { get; set; } = string.Empty;
    public string? Observatii { get; set; }
}

/// <summary>
/// DTO pentru analiză recomandată în Scrisoarea Medicală
/// </summary>
public class AnalizaRecomandataScrisoareDto
{
    public string NumeAnaliza { get; set; } = string.Empty;
    public string? Categorie { get; set; }
    public string? Prioritate { get; set; }
    public bool EsteCito { get; set; }
    public string? IndicatiiClinice { get; set; }
}

/// <summary>
/// DTO pentru investigație recomandată în Scrisoarea Medicală
/// (folosit pentru Imagistice, Explorări funcționale și Endoscopii)
/// </summary>
public class InvestigatieRecomandataScrisoareDto
{
    public string Denumire { get; set; } = string.Empty;
    public string? Cod { get; set; }
    public string? Categorie { get; set; }
    public string? Prioritate { get; set; }
    public bool EsteCito { get; set; }
    public string? IndicatiiClinice { get; set; }
    public string? Observatii { get; set; }
}

/// <summary>
/// DTO pentru analiză efectuată (cu rezultate) în Scrisoarea Medicală
/// </summary>
public class AnalizaEfectuataScrisoareDto
{
    public string NumeAnaliza { get; set; } = string.Empty;
    public string? Categorie { get; set; }
    public DateTime? DataEfectuare { get; set; }
    public string? Laborator { get; set; }
    public string? Rezultat { get; set; }
    public string? UnitateMasura { get; set; }
    public string? ValoriReferinta { get; set; }
    public bool EsteAnormal { get; set; }
    
    // Computed
    public string DataEfectuareFormatata => DataEfectuare?.ToString("dd.MM.yyyy") ?? "-";
}
