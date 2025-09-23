using System.ComponentModel.DataAnnotations;

namespace ValyanClinic.Domain.Enums;

/// <summary>
/// Enumerare pentru pozitiile personalului medical in clinica
/// Valorile sunt mapate pentru a corespunde cu datele din baza de date
/// </summary>
public enum PozitiePersonalMedical
{
    /// <summary>
    /// Doctor - Medic specialist sau generalist
    /// </summary>
    [Display(Name = "Doctor")]
    Doctor = 1,

    /// <summary>
    /// Asistent Medical - Asistent medical calificat
    /// </summary>
    [Display(Name = "Asistent Medical")]
    AsistentMedical = 2,

    /// <summary>
    /// Tehnician Medical - Tehnician de laborator sau imagistica
    /// </summary>
    [Display(Name = "Tehnician Medical")]
    TehnicianMedical = 3,

    /// <summary>
    /// Receptioner Medical - Personal de la receptie in zona medicala
    /// </summary>
    [Display(Name = "Receptioner Medical")]
    ReceptionerMedical = 4,

    /// <summary>
    /// Radiolog - Specialist in imagistica medicala
    /// </summary>
    [Display(Name = "Radiolog")]
    Radiolog = 5,

    /// <summary>
    /// Laborant - Tehnician de laborator specializat
    /// </summary>
    [Display(Name = "Laborant")]
    Laborant = 6
}

/// <summary>
/// Extensii pentru enum-ul PozitiePersonalMedical
/// </summary>
public static class PozitiePersonalMedicalExtensions
{
    /// <summary>
    /// Returneaza numele afisat al pozitiei
    /// </summary>
    /// <param name="pozitie">Pozitia pentru care se doreste numele</param>
    /// <returns>Numele afisat al pozitiei</returns>
    public static string GetDisplayName(this PozitiePersonalMedical pozitie)
    {
        var field = pozitie.GetType().GetField(pozitie.ToString());
        var attribute = field?.GetCustomAttributes(typeof(DisplayAttribute), false)
            .FirstOrDefault() as DisplayAttribute;
        return attribute?.Name ?? pozitie.ToString();
    }

    /// <summary>
    /// Parseaza o valoare string sau numerica catre enum PozitiePersonalMedical
    /// Aceasta metoda este robusta si poate parsa atat nume, cat si valori numerice
    /// </summary>
    /// <param name="value">Valoarea de parsare (string nume sau numar)</param>
    /// <returns>Enum-ul corespunzator sau null daca nu se poate parsa</returns>
    public static PozitiePersonalMedical? ParseFromDatabase(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        // incercam sa parsam ca numar mai intai
        if (int.TryParse(value.Trim(), out var numericValue))
        {
            if (Enum.IsDefined(typeof(PozitiePersonalMedical), numericValue))
            {
                return (PozitiePersonalMedical)numericValue;
            }
        }

        // incercam sa parsam ca string - prin numele enum-ului
        if (Enum.TryParse<PozitiePersonalMedical>(value.Trim(), true, out var enumResult))
        {
            return enumResult;
        }

        // incercam sa gasim prin display name
        foreach (PozitiePersonalMedical pozitie in Enum.GetValues<PozitiePersonalMedical>())
        {
            if (string.Equals(pozitie.GetDisplayName(), value.Trim(), StringComparison.OrdinalIgnoreCase))
            {
                return pozitie;
            }
        }

        // Mapari comune pentru compatibilitate cu datele existente din baza de date
        var mappings = new Dictionary<string, PozitiePersonalMedical>(StringComparer.OrdinalIgnoreCase)
        {
            // Variante comune pentru Doctor
            { "Medic", PozitiePersonalMedical.Doctor },
            { "Dr", PozitiePersonalMedical.Doctor },
            { "Medic Specialist", PozitiePersonalMedical.Doctor },
            { "Medic Generalist", PozitiePersonalMedical.Doctor },
            
            // Variante comune pentru Asistent Medical
            { "Asistent", PozitiePersonalMedical.AsistentMedical },
            { "Asistenta", PozitiePersonalMedical.AsistentMedical },
            { "Asistenta Medicala", PozitiePersonalMedical.AsistentMedical },
            { "Nurse", PozitiePersonalMedical.AsistentMedical },
            
            // Variante comune pentru Tehnician Medical
            { "Tehnician", PozitiePersonalMedical.TehnicianMedical },
            { "Technician", PozitiePersonalMedical.TehnicianMedical },
            { "Tehnician de Laborator", PozitiePersonalMedical.TehnicianMedical },
            
            // Variante comune pentru Receptioner Medical
            { "Receptioner", PozitiePersonalMedical.ReceptionerMedical },
            { "Receptie", PozitiePersonalMedical.ReceptionerMedical },
            { "Front Desk", PozitiePersonalMedical.ReceptionerMedical },
            
            // Variante comune pentru Radiolog
            { "Medic Radiolog", PozitiePersonalMedical.Radiolog },
            { "Specialist Radiologie", PozitiePersonalMedical.Radiolog },
            
            // Variante comune pentru Laborant
            { "Lab Technician", PozitiePersonalMedical.Laborant },
            { "Analist", PozitiePersonalMedical.Laborant },
            { "Biolog", PozitiePersonalMedical.Laborant }
        };

        if (mappings.TryGetValue(value.Trim(), out var mappedResult))
        {
            return mappedResult;
        }

        // Nu s-a putut parsa
        return null;
    }

    /// <summary>
    /// Returneaza toate pozitiile disponibile ca lista de perechi nume-valoare
    /// </summary>
    /// <returns>Lista pozitiilor cu numele si valorile lor</returns>
    public static List<(string Name, int Value)> GetAllPositions()
    {
        return Enum.GetValues<PozitiePersonalMedical>()
            .Select(p => (p.GetDisplayName(), (int)p))
            .ToList();
    }

    /// <summary>
    /// Verifica daca pozitia necesita licenta medicala
    /// </summary>
    /// <param name="pozitie">Pozitia de verificat</param>
    /// <returns>True daca pozitia necesita licenta medicala</returns>
    public static bool RequiresLicensaMedicala(this PozitiePersonalMedical pozitie)
    {
        return pozitie == PozitiePersonalMedical.Doctor || 
               pozitie == PozitiePersonalMedical.AsistentMedical ||
               pozitie == PozitiePersonalMedical.Radiolog;
    }

    /// <summary>
    /// Returneaza culoarea badge-ului pentru pozitie (pentru UI)
    /// </summary>
    /// <param name="pozitie">Pozitia pentru care se doreste culoarea</param>
    /// <returns>Clasa CSS pentru culoarea badge-ului</returns>
    public static string GetBadgeColorClass(this PozitiePersonalMedical pozitie)
    {
        return pozitie switch
        {
            PozitiePersonalMedical.Doctor => "badge-success",
            PozitiePersonalMedical.AsistentMedical => "badge-primary",
            PozitiePersonalMedical.TehnicianMedical => "badge-info",
            PozitiePersonalMedical.ReceptionerMedical => "badge-warning",
            PozitiePersonalMedical.Radiolog => "badge-danger",
            PozitiePersonalMedical.Laborant => "badge-secondary",
            _ => "badge-light"
        };
    }

    /// <summary>
    /// Returneaza iconita Font Awesome pentru pozitie
    /// </summary>
    /// <param name="pozitie">Pozitia pentru care se doreste iconita</param>
    /// <returns>Clasa CSS pentru iconita Font Awesome</returns>
    public static string GetFontAwesomeIcon(this PozitiePersonalMedical pozitie)
    {
        return pozitie switch
        {
            PozitiePersonalMedical.Doctor => "fa-user-md",
            PozitiePersonalMedical.AsistentMedical => "fa-user-nurse",
            PozitiePersonalMedical.TehnicianMedical => "fa-microscope",
            PozitiePersonalMedical.ReceptionerMedical => "fa-desktop",
            PozitiePersonalMedical.Radiolog => "fa-x-ray",
            PozitiePersonalMedical.Laborant => "fa-vials",
            _ => "fa-user"
        };
    }

    /// <summary>
    /// Converteste enum-ul la format string pentru salvare in baza de date
    /// </summary>
    /// <param name="pozitie">Pozitia de convertit</param>
    /// <returns>Reprezentarea string pentru baza de date</returns>
    public static string ToDatabase(this PozitiePersonalMedical pozitie)
    {
        // Salvam ca numar pentru consistenta
        return ((int)pozitie).ToString();
    }

    /// <summary>
    /// Valideaza daca o pozitie este valida pentru operatiuni medicale
    /// </summary>
    /// <param name="pozitie">Pozitia de validat</param>
    /// <returns>True daca pozitia este valida</returns>
    public static bool IsValidMedicalPosition(this PozitiePersonalMedical pozitie)
    {
        return Enum.IsDefined(typeof(PozitiePersonalMedical), pozitie);
    }
}
