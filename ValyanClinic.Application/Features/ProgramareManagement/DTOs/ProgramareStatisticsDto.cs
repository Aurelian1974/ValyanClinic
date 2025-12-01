namespace ValyanClinic.Application.Features.ProgramareManagement.DTOs;

/// <summary>
/// DTO pentru statistici și raportare programări.
/// </summary>
public class ProgramareStatisticsDto
{
    /// <summary>
    /// Perioada pentru care sunt calculate statisticile.
    /// </summary>
    public DateTime? DataStart { get; set; }

    /// <summary>
    /// Data de sfârșit a perioadei.
    /// </summary>
    public DateTime? DataEnd { get; set; }

    /// <summary>
    /// Număr total de programări în perioada specificată.
    /// </summary>
    public int TotalProgramari { get; set; }

    // ==================== STATISTICI PE STATUS ====================

    /// <summary>
    /// Număr programări cu status "Programata".
    /// </summary>
    public int Programate { get; set; }

    /// <summary>
    /// Număr programări cu status "Confirmata".
    /// </summary>
    public int Confirmate { get; set; }

    /// <summary>
    /// Număr programări cu status "CheckedIn".
    /// </summary>
    public int CheckedIn { get; set; }

    /// <summary>
    /// Număr programări cu status "InConsultatie".
    /// </summary>
    public int InConsultatie { get; set; }

    /// <summary>
    /// Număr programări cu status "Finalizata".
    /// </summary>
    public int Finalizate { get; set; }

    /// <summary>
    /// Număr programări cu status "Anulata".
    /// </summary>
    public int Anulate { get; set; }

    /// <summary>
    /// Număr programări cu status "NoShow" (pacienți care nu s-au prezentat).
    /// </summary>
    public int NoShow { get; set; }

    // ==================== STATISTICI PE TIP PROGRAMARE ====================

    /// <summary>
    /// Număr consultații inițiale.
    /// </summary>
    public int ConsultatiiInitiale { get; set; }

    /// <summary>
    /// Număr controale periodice.
    /// </summary>
    public int ControalePeriodice { get; set; }

    /// <summary>
    /// Număr consultații standard.
    /// </summary>
    public int Consultatii { get; set; }

    /// <summary>
    /// Număr investigații.
    /// </summary>
    public int Investigatii { get; set; }

    /// <summary>
    /// Număr proceduri.
    /// </summary>
    public int Proceduri { get; set; }

    /// <summary>
    /// Număr urgențe.
    /// </summary>
    public int Urgente { get; set; }

    /// <summary>
    /// Număr consultații telemedicină.
    /// </summary>
    public int Telemedicina { get; set; }

    /// <summary>
    /// Număr consultații la domiciliu.
    /// </summary>
    public int LaDomiciliu { get; set; }

    // ==================== STATISTICI AVANSATE ====================

    /// <summary>
    /// Număr medici activi (cu cel puțin o programare).
    /// </summary>
    public int MediciActivi { get; set; }

    /// <summary>
    /// Număr pacienți unici.
    /// </summary>
    public int PacientiUnici { get; set; }

    /// <summary>
    /// Durata medie a programărilor (în minute).
    /// </summary>
    public double DurataMedieMinute { get; set; }

    /// <summary>
    /// Rata de prezentare (% din programări care NU sunt NoShow sau Anulate).
    /// </summary>
    public double RataPrezentare
    {
        get
        {
            if (TotalProgramari == 0) return 0;
            var prezenti = TotalProgramari - NoShow - Anulate;
            return Math.Round((double)prezenti / TotalProgramari * 100, 2);
        }
    }

    /// <summary>
    /// Rata de anulare (% din total programări).
    /// </summary>
    public double RataAnulare
    {
        get
        {
            if (TotalProgramari == 0) return 0;
            return Math.Round((double)Anulate / TotalProgramari * 100, 2);
        }
    }

    /// <summary>
    /// Rata No-Show (% din total programări).
    /// </summary>
    public double RataNoShow
    {
        get
        {
            if (TotalProgramari == 0) return 0;
            return Math.Round((double)NoShow / TotalProgramari * 100, 2);
        }
    }

    /// <summary>
    /// Rata de finalizare (% programări finalizate din total).
    /// </summary>
    public double RataFinalizare
    {
        get
        {
            if (TotalProgramari == 0) return 0;
            return Math.Round((double)Finalizate / TotalProgramari * 100, 2);
        }
    }

    // ==================== TOP 5 ====================

    /// <summary>
    /// Top 5 medici după număr de programări (pentru grafice).
    /// </summary>
    public List<TopMedicDto> TopMedici { get; set; } = new();

    /// <summary>
    /// Top 5 zile cu cele mai multe programări.
    /// </summary>
    public List<TopZiDto> TopZile { get; set; } = new();

    // ==================== HELPER PROPERTIES ====================

    /// <summary>
    /// Perioada formatată pentru afișare (ex: "01.01.2025 - 31.01.2025").
    /// </summary>
    public string PerioadaFormatata
    {
        get
        {
            if (DataStart.HasValue && DataEnd.HasValue)
                return $"{DataStart.Value:dd.MM.yyyy} - {DataEnd.Value:dd.MM.yyyy}";
            if (DataStart.HasValue)
                return $"De la {DataStart.Value:dd.MM.yyyy}";
            if (DataEnd.HasValue)
                return $"Până la {DataEnd.Value:dd.MM.yyyy}";
            return "Toate perioadele";
        }
    }
}

/// <summary>
/// DTO pentru top medici (nested DTO).
/// </summary>
public class TopMedicDto
{
    public Guid DoctorID { get; set; }
    public string NumeComplet { get; set; } = string.Empty;
    public string? Specializare { get; set; }
    public int NumarProgramari { get; set; }
}

/// <summary>
/// DTO pentru top zile (nested DTO).
/// </summary>
public class TopZiDto
{
    public DateTime Data { get; set; }
    public int NumarProgramari { get; set; }
    public string DataFormatata => Data.ToString("dd.MM.yyyy (dddd)");
}
