namespace ValyanClinic.Domain.Enums;

/// <summary>
/// Tipurile posibile de programări medicale.
/// </summary>
public enum TipProgramare
{
    /// <summary>
    /// Consultație inițială (prima vizită a pacientului).
    /// </summary>
    ConsultatieInitiala = 0,

    /// <summary>
    /// Control periodic (pacient existent, consultație de follow-up).
    /// </summary>
    ControlPeriodic = 1,

    /// <summary>
    /// Consultație standard (consultație generală).
    /// </summary>
    Consultatie = 2,

    /// <summary>
    /// Investigație medicală (analize, ecografie, radiografie, etc.).
    /// </summary>
    Investigatie = 3,

    /// <summary>
    /// Procedură medicală (tratament, intervenție minoră, etc.).
    /// </summary>
    Procedura = 4,

    /// <summary>
    /// Urgență medicală (necesită prioritizare).
    /// </summary>
    Urgenta = 5,

    /// <summary>
    /// Telemedicină (consultație online/la distanță).
    /// </summary>
    Telemedicina = 6,

    /// <summary>
    /// Consultație la domiciliu.
    /// </summary>
    LaDeomiciliu = 7,

    /// <summary>
    /// Slot blocat (pauză de masă, întâlnire internă, timp rezervat).
    /// Nu permite programări de pacienți în acest interval.
    /// </summary>
    SlotBlocat = 8
}

/// <summary>
/// Extensii pentru enum-ul TipProgramare.
/// </summary>
public static class TipProgramareExtensions
{
    /// <summary>
    /// Convertește enum-ul în string pentru stocare în baza de date.
    /// </summary>
    public static string ToDbString(this TipProgramare tip)
    {
        return tip switch
        {
            TipProgramare.ConsultatieInitiala => "ConsultatieInitiala",
            TipProgramare.ControlPeriodic => "ControlPeriodic",
            TipProgramare.Consultatie => "Consultatie",
            TipProgramare.Investigatie => "Investigatie",
            TipProgramare.Procedura => "Procedura",
            TipProgramare.Urgenta => "Urgenta",
            TipProgramare.Telemedicina => "Telemedicina",
            TipProgramare.LaDeomiciliu => "LaDomiciliu",
            TipProgramare.SlotBlocat => "SlotBlocat",
            _ => "Consultatie"
        };
    }

    /// <summary>
    /// Convertește string-ul din baza de date în enum.
    /// </summary>
    public static TipProgramare FromDbString(string? tip)
    {
        return tip?.ToLower() switch
        {
            "consultatieinitial" or "consultatieinitială" => TipProgramare.ConsultatieInitiala,
            "controlperiodic" => TipProgramare.ControlPeriodic,
            "consultatie" or "consultație" => TipProgramare.Consultatie,
            "investigatie" or "investigație" => TipProgramare.Investigatie,
            "procedura" or "procedură" => TipProgramare.Procedura,
            "urgenta" or "urgență" => TipProgramare.Urgenta,
            "telemedicina" or "telemedicină" => TipProgramare.Telemedicina,
            "ladomiciliu" => TipProgramare.LaDeomiciliu,
            "slotblocat" or "blocat" => TipProgramare.SlotBlocat,
            _ => TipProgramare.Consultatie
        };
    }

    /// <summary>
    /// Returnează culoarea Bootstrap pentru afișare în UI.
    /// </summary>
    public static string GetBootstrapColor(this TipProgramare tip)
    {
        return tip switch
        {
            TipProgramare.ConsultatieInitiala => "primary",    // Blue
            TipProgramare.ControlPeriodic => "info",           // Light Blue
            TipProgramare.Consultatie => "secondary",    // Gray
            TipProgramare.Investigatie => "warning",           // Orange
            TipProgramare.Procedura => "success",  // Green
            TipProgramare.Urgenta => "danger",    // Red
            TipProgramare.Telemedicina => "dark",       // Dark
            TipProgramare.LaDeomiciliu => "purple",  // Purple (custom)
            TipProgramare.SlotBlocat => "dark",          // Black/Dark Gray
            _ => "secondary"
        };
    }

    /// <summary>
    /// Returnează descrierea user-friendly a tipului de programare.
    /// </summary>
    public static string GetDisplayName(this TipProgramare tip)
    {
        return tip switch
        {
            TipProgramare.ConsultatieInitiala => "Consultație Inițială",
            TipProgramare.ControlPeriodic => "Control Periodic",
            TipProgramare.Consultatie => "Consultație",
            TipProgramare.Investigatie => "Investigație",
            TipProgramare.Procedura => "Procedură",
            TipProgramare.Urgenta => "Urgență",
            TipProgramare.Telemedicina => "Telemedicină",
            TipProgramare.LaDeomiciliu => "La Domiciliu",
            TipProgramare.SlotBlocat => "🚫 Blocat",
            _ => "Consultație"
        };
    }

    /// <summary>
    /// Returnează durata implicită în minute pentru fiecare tip de programare.
    /// </summary>
    public static int GetDurataImplicitaMinute(this TipProgramare tip)
    {
        return tip switch
        {
            TipProgramare.ConsultatieInitiala => 45,  // 45 min
            TipProgramare.ControlPeriodic => 30,      // 30 min
            TipProgramare.Consultatie => 30,          // 30 min
            TipProgramare.Investigatie => 20,         // 20 min
            TipProgramare.Procedura => 60,     // 60 min
            TipProgramare.Urgenta => 15,  // 15 min
            TipProgramare.Telemedicina => 20,         // 20 min
            TipProgramare.LaDeomiciliu => 60,         // 60 min
            TipProgramare.SlotBlocat => 60,           // 60 min (1 oră default pentru blocare)
            _ => 30
        };
    }

    /// <summary>
    /// Returnează iconul Bootstrap pentru afișare în UI.
    /// </summary>
    public static string GetBootstrapIcon(this TipProgramare tip)
    {
        return tip switch
        {
            TipProgramare.ConsultatieInitiala => "bi-person-plus-fill",
            TipProgramare.ControlPeriodic => "bi-arrow-repeat",
            TipProgramare.Consultatie => "bi-clipboard2-pulse",
            TipProgramare.Investigatie => "bi-eyeglasses",
            TipProgramare.Procedura => "bi-scissors",
            TipProgramare.Urgenta => "bi-exclamation-triangle-fill",
            TipProgramare.Telemedicina => "bi-camera-video",
            TipProgramare.LaDeomiciliu => "bi-house-fill",
            TipProgramare.SlotBlocat => "bi-lock-fill",
            _ => "bi-calendar-check"
        };
    }

    /// <summary>
    /// Verifică dacă tipul de programare permite confirmare automată.
    /// </summary>
    public static bool PermiteConfirmareAutomata(this TipProgramare tip)
    {
        // Urgențele și telemedicina pot fi confirmate automat
        return tip is TipProgramare.Urgenta or TipProgramare.Telemedicina;
    }

    /// <summary>
    /// Verifică dacă tipul de programare necesită pregătire specială.
    /// </summary>
    public static bool NecesitaPregatire(this TipProgramare tip)
    {
        // Investigațiile și procedurile necesită pregătire
        return tip is TipProgramare.Investigatie or TipProgramare.Procedura;
    }

    /// <summary>
    /// Verifică dacă tipul de programare este un slot blocat (nu necesită pacient).
    /// </summary>
    public static bool EsteSlotBlocat(this TipProgramare tip)
    {
        return tip == TipProgramare.SlotBlocat;
    }
}
