using System.ComponentModel.DataAnnotations;

namespace ValyanClinic.Application.Features.ConsultatieManagement.DTOs;

/// <summary>
/// DTO pentru formularul de consultație folosit în UI
/// Conține toate câmpurile editabile din pagina de consultații
/// </summary>
public class ConsultatieFormDto
{
    // === Identificare ===
    
    /// <summary>
    /// ID consultație (null pentru consultație nouă)
    /// </summary>
    public Guid? ConsultatieId { get; set; }

    /// <summary>
    /// ID pacient (obligatoriu)
    /// </summary>
    [Required(ErrorMessage = "Pacientul este obligatoriu")]
    public Guid PacientId { get; set; }

    /// <summary>
    /// ID programare (opțional - pentru walk-in patients)
    /// </summary>
    public Guid? ProgramareId { get; set; }

    // === Tab 1: Motiv Prezentare & Antecedente ===

    /// <summary>
    /// Motivul prezentării pacientului (obligatoriu pentru finalizare)
    /// </summary>
    [Required(ErrorMessage = "Motivul prezentării este obligatoriu")]
    [MaxLength(1000, ErrorMessage = "Motivul prezentării nu poate depăși 1000 caractere")]
    public string MotivPrezentare { get; set; } = string.Empty;

    /// <summary>
    /// Antecedente personale patologice
    /// </summary>
    [MaxLength(2000, ErrorMessage = "Antecedentele patologice nu pot depăși 2000 caractere")]
    public string AntecedentePatologice { get; set; } = string.Empty;

    /// <summary>
    /// Tratamente actuale ale pacientului
    /// </summary>
    [MaxLength(1000, ErrorMessage = "Tratamentele actuale nu pot depăși 1000 caractere")]
    public string TratamenteActuale { get; set; } = string.Empty;

    // === Tab 2: Examen Clinic ===

    /// <summary>
    /// Stare generală
    /// </summary>
    public string StareGenerala { get; set; } = string.Empty;

    /// <summary>
    /// Tegumente
    /// </summary>
    public string Tegumente { get; set; } = string.Empty;

    /// <summary>
    /// Mucoase
    /// </summary>
    public string Mucoase { get; set; } = string.Empty;

    /// <summary>
    /// Greutate (kg)
    /// </summary>
    [Range(0.5, 500, ErrorMessage = "Greutatea trebuie să fie între 0.5 și 500 kg")]
    public decimal? Greutate { get; set; }

    /// <summary>
    /// Înălțime (cm)
    /// </summary>
    [Range(30, 250, ErrorMessage = "Înălțimea trebuie să fie între 30 și 250 cm")]
    public decimal? Inaltime { get; set; }

    /// <summary>
    /// Tensiune arterială sistolică (mmHg)
    /// </summary>
    [Range(50, 300, ErrorMessage = "Tensiunea sistolică trebuie să fie între 50 și 300 mmHg")]
    public int? TensiuneSistolica { get; set; }

    /// <summary>
    /// Tensiune arterială diastolică (mmHg)
    /// </summary>
    [Range(30, 200, ErrorMessage = "Tensiunea diastolică trebuie să fie între 30 și 200 mmHg")]
    public int? TensiuneDiastolica { get; set; }

    /// <summary>
    /// Puls (bpm)
    /// </summary>
    [Range(30, 250, ErrorMessage = "Pulsul trebuie să fie între 30 și 250 bpm")]
    public int? Puls { get; set; }

    /// <summary>
    /// Frecvența respiratorie (resp/min)
    /// </summary>
    [Range(5, 60, ErrorMessage = "Frecvența respiratorie trebuie să fie între 5 și 60 resp/min")]
    public int? FreqventaRespiratorie { get; set; }

    /// <summary>
    /// Temperatura (°C)
    /// </summary>
    [Range(32, 45, ErrorMessage = "Temperatura trebuie să fie între 32 și 45 °C")]
    public decimal? Temperatura { get; set; }

    /// <summary>
    /// Saturația oxigen SpO2 (%)
    /// </summary>
    [Range(50, 100, ErrorMessage = "SpO2 trebuie să fie între 50 și 100%")]
    public int? SpO2 { get; set; }

    /// <summary>
    /// Edeme
    /// </summary>
    public string Edeme { get; set; } = string.Empty;

    /// <summary>
    /// Examen obiectiv detaliat
    /// </summary>
    [MaxLength(2000, ErrorMessage = "Examenul obiectiv nu poate depăși 2000 caractere")]
    public string ExamenObiectiv { get; set; } = string.Empty;

    /// <summary>
    /// Investigații paraclinice
    /// </summary>
    [MaxLength(1500, ErrorMessage = "Investigațiile paraclinice nu pot depăși 1500 caractere")]
    public string InvestigatiiParaclinice { get; set; } = string.Empty;

    // === Tab 3: Diagnostic & Tratament ===

    /// <summary>
    /// Diagnostic principal (obligatoriu pentru finalizare)
    /// </summary>
    [Required(ErrorMessage = "Diagnosticul principal este obligatoriu")]
    [MaxLength(500, ErrorMessage = "Diagnosticul principal nu poate depăși 500 caractere")]
    public string DiagnosticPrincipal { get; set; } = string.Empty;

    /// <summary>
    /// Diagnostic secundar
    /// </summary>
    [MaxLength(500, ErrorMessage = "Diagnosticul secundar nu poate depăși 500 caractere")]
    public string DiagnosticSecundar { get; set; } = string.Empty;

    /// <summary>
    /// Plan terapeutic / Tratament prescris
    /// </summary>
    [MaxLength(2000, ErrorMessage = "Planul terapeutic nu poate depăși 2000 caractere")]
    public string PlanTerapeutic { get; set; } = string.Empty;

    /// <summary>
    /// Recomandări
    /// </summary>
    [MaxLength(1500, ErrorMessage = "Recomandările nu pot depăși 1500 caractere")]
    public string Recomandari { get; set; } = string.Empty;

    // === Tab 4: Concluzii ===

    /// <summary>
    /// Concluzii / Rezumat consultație
    /// </summary>
    [MaxLength(2000, ErrorMessage = "Concluziile nu pot depăși 2000 caractere")]
    public string Concluzii { get; set; } = string.Empty;

    /// <summary>
    /// Data următoarei vizite
    /// </summary>
    public DateTime? DataUrmatoareiVizite { get; set; }

    /// <summary>
    /// Note pentru vizita următoare
    /// </summary>
    [MaxLength(1000, ErrorMessage = "Notele pentru vizita următoare nu pot depăși 1000 caractere")]
    public string NoteUrmatoareaVizita { get; set; } = string.Empty;

    // === Colecții ===

    /// <summary>
    /// Lista diagnosticelor (ICD-10)
    /// </summary>
    public List<DiagnosisCardDto> DiagnosisList { get; set; } = new();

    /// <summary>
    /// Lista medicamentelor prescrise
    /// </summary>
    public List<MedicationRowDto> MedicationList { get; set; } = new();

    /// <summary>
    /// Antecedente heredo-colaterale
    /// </summary>
    public AntecedenteHeredoDto AntecedenteHeredo { get; set; } = new();

    // === Computed Properties ===

    /// <summary>
    /// Calculează IMC dacă greutatea și înălțimea sunt disponibile
    /// </summary>
    public decimal? IMC
    {
        get
        {
            if (Greutate.HasValue && Inaltime.HasValue && Inaltime.Value > 0)
            {
                var inaltimeMetri = Inaltime.Value / 100m;
                return Math.Round(Greutate.Value / (inaltimeMetri * inaltimeMetri), 1);
            }
            return null;
        }
    }

    /// <summary>
    /// Tensiune arterială formatată (ex: "120/80")
    /// </summary>
    public string? TensiuneArterialaFormatata =>
        TensiuneSistolica.HasValue && TensiuneDiastolica.HasValue
            ? $"{TensiuneSistolica}/{TensiuneDiastolica}"
            : null;

    /// <summary>
    /// Verifică dacă formularul este valid pentru salvare draft
    /// </summary>
    public bool IsValidForDraft => PacientId != Guid.Empty;

    /// <summary>
    /// Verifică dacă formularul este valid pentru finalizare
    /// </summary>
    public bool IsValidForFinalize =>
        PacientId != Guid.Empty &&
        !string.IsNullOrWhiteSpace(MotivPrezentare) &&
        !string.IsNullOrWhiteSpace(DiagnosticPrincipal);
}
