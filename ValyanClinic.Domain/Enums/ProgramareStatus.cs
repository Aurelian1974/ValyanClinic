namespace ValyanClinic.Domain.Enums;

/// <summary>
/// Statusurile posibile ale unei programări medicale.
/// </summary>
public enum ProgramareStatus
{
    /// <summary>
    /// Programarea a fost creată dar nu este confirmată (status implicit).
    /// </summary>
    Programata = 0,

    /// <summary>
    /// Pacientul a confirmat prezența (telefonic, email sau SMS).
    /// </summary>
    Confirmata = 1,

    /// <summary>
    /// Pacientul s-a prezentat la recepție și a fost înregistrat (check-in efectuat).
    /// </summary>
    CheckedIn = 2,

    /// <summary>
    /// Consultația este în desfășurare (pacientul este la medic).
    /// </summary>
    InConsultatie = 3,

    /// <summary>
    /// Consultația s-a finalizat cu succes.
    /// </summary>
    Finalizata = 4,

    /// <summary>
    /// Programarea a fost anulată (de pacient sau de clinică).
    /// </summary>
    Anulata = 5,

    /// <summary>
    /// Pacientul nu s-a prezentat la programare (No-Show).
    /// </summary>
    NoShow = 6
}

/// <summary>
/// Extensii pentru enum-ul ProgramareStatus.
/// </summary>
public static class ProgramareStatusExtensions
{
    /// <summary>
    /// Convertește enum-ul în string pentru stocare în baza de date.
    /// </summary>
    public static string ToDbString(this ProgramareStatus status)
    {
        return status switch
        {
            ProgramareStatus.Programata => "Programata",
            ProgramareStatus.Confirmata => "Confirmata",
            ProgramareStatus.CheckedIn => "CheckedIn",
            ProgramareStatus.InConsultatie => "InConsultatie",
            ProgramareStatus.Finalizata => "Finalizata",
            ProgramareStatus.Anulata => "Anulata",
            ProgramareStatus.NoShow => "NoShow",
            _ => "Programata"
        };
    }

    /// <summary>
    /// Convertește string-ul din baza de date în enum.
    /// </summary>
    public static ProgramareStatus FromDbString(string status)
    {
        return status?.ToLower() switch
        {
            "programata" => ProgramareStatus.Programata,
            "confirmata" => ProgramareStatus.Confirmata,
            "checkedin" => ProgramareStatus.CheckedIn,
            "inconsultatie" => ProgramareStatus.InConsultatie,
            "finalizata" => ProgramareStatus.Finalizata,
            "anulata" => ProgramareStatus.Anulata,
            "noshow" => ProgramareStatus.NoShow,
            _ => ProgramareStatus.Programata
        };
    }

    /// <summary>
    /// Returnează culoarea Bootstrap pentru afișare în UI (badge color).
    /// </summary>
    public static string GetBootstrapColor(this ProgramareStatus status)
    {
        return status switch
        {
            ProgramareStatus.Programata => "secondary",     // Gray
            ProgramareStatus.Confirmata => "info",          // Blue
            ProgramareStatus.CheckedIn => "primary",        // Dark Blue
            ProgramareStatus.InConsultatie => "warning",    // Orange
            ProgramareStatus.Finalizata => "success",       // Green
            ProgramareStatus.Anulata => "danger",           // Red
            ProgramareStatus.NoShow => "dark",           // Black
            _ => "secondary"
        };
    }

    /// <summary>
    /// Returnează descrierea user-friendly a statusului.
    /// </summary>
    public static string GetDisplayName(this ProgramareStatus status)
    {
        return status switch
        {
            ProgramareStatus.Programata => "Programată",
            ProgramareStatus.Confirmata => "Confirmată",
            ProgramareStatus.CheckedIn => "Check-in efectuat",
            ProgramareStatus.InConsultatie => "În consultație",
            ProgramareStatus.Finalizata => "Finalizată",
            ProgramareStatus.Anulata => "Anulată",
            ProgramareStatus.NoShow => "Nu s-a prezentat",
            _ => "Programată"
        };
    }

    /// <summary>
    /// Verifică dacă statusul permite editarea programării.
    /// </summary>
    public static bool PesteEditat(this ProgramareStatus status)
    {
        return status is ProgramareStatus.Programata or
        ProgramareStatus.Confirmata;
    }

    /// <summary>
    /// Verifică dacă statusul permite anularea programării.
    /// </summary>
    public static bool PoateAnulat(this ProgramareStatus status)
    {
        return status is ProgramareStatus.Programata or
       ProgramareStatus.Confirmata or
         ProgramareStatus.CheckedIn;
    }
}
