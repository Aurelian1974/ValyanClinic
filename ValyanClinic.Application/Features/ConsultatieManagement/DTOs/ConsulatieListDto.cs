namespace ValyanClinic.Application.Features.ConsultatieManagement.DTOs;

/// <summary>
/// DTO pentru lista de consultatii (view simplificat)
/// </summary>
public class ConsulatieListDto
{
    public Guid ConsultatieID { get; set; }
    public Guid? ProgramareID { get; set; } // ✅ CHANGED: Nullable for walk-in consultations
    public Guid PacientID { get; set; }
    public Guid MedicID { get; set; }

    // Date consultatie
    public DateTime DataConsultatie { get; set; }
    public TimeSpan OraConsultatie { get; set; }
    public string TipConsultatie { get; set; } = string.Empty;

    // Informatii pacient (JOIN)
    public string PacientNumeComplet { get; set; } = string.Empty;
    public string? PacientCNP { get; set; }

    // Informatii medic (JOIN)
    public string MedicNumeComplet { get; set; } = string.Empty;
    public string? MedicSpecializare { get; set; }

    // Motive principale
    public string? MotivPrezentare { get; set; }
    public string? DiagnosticPozitiv { get; set; }

    // Status & Workflow
    public string Status { get; set; } = "In desfasurare";
    public DateTime? DataFinalizare { get; set; }
    public int DurataMinute { get; set; }

    // Audit
    public DateTime DataCreare { get; set; }

    // Computed Properties
    public string DataFormatata => DataConsultatie.ToString("dd.MM.yyyy");
    public string OraFormatata => OraConsultatie.ToString(@"hh\:mm");
    public string StatusDisplay => Status switch
    {
        "In desfasurare" => "În Desfășurare",
        "Finalizata" => "Finalizată",
        "Anulata" => "Anulată",
        _ => Status
    };
}
