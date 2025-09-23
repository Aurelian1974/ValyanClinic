using System.ComponentModel.DataAnnotations;

namespace ValyanClinic.Domain.Enums;

/// <summary>
/// Enumerare pentru pozițiile personalului medical în clinică
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
    /// Tehnician Medical - Tehnician de laborator sau imagistică
    /// </summary>
    [Display(Name = "Tehnician Medical")]
    TehnicianMedical = 3,

    /// <summary>
    /// Recepționer Medical - Personal de la recepție în zona medicală
    /// </summary>
    [Display(Name = "Recepționer Medical")]
    ReceptionerMedical = 4,

    /// <summary>
    /// Radiolog - Specialist în imagistică medicală
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
    /// Returnează numele afișat al poziției
    /// </summary>
    /// <param name="pozitie">Poziția pentru care se dorește numele</param>
    /// <returns>Numele afișat al poziției</returns>
    public static string GetDisplayName(this PozitiePersonalMedical pozitie)
    {
        var field = pozitie.GetType().GetField(pozitie.ToString());
        var attribute = field?.GetCustomAttributes(typeof(DisplayAttribute), false)
            .FirstOrDefault() as DisplayAttribute;
        return attribute?.Name ?? pozitie.ToString();
    }

    /// <summary>
    /// Parsează o valoare string sau numerică către enum PozitiePersonalMedical
    /// Această metodă este robustă și poate parsa atât nume, cât și valori numerice
    /// </summary>
    /// <param name="value">Valoarea de parsare (string nume sau număr)</param>
    /// <returns>Enum-ul corespunzător sau null dacă nu se poate parsa</returns>
    public static PozitiePersonalMedical? ParseFromDatabase(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        // Încercăm să parsăm ca număr mai întâi
        if (int.TryParse(value.Trim(), out var numericValue))
        {
            if (Enum.IsDefined(typeof(PozitiePersonalMedical), numericValue))
            {
                return (PozitiePersonalMedical)numericValue;
            }
        }

        // Încercăm să parsăm ca string - prin numele enum-ului
        if (Enum.TryParse<PozitiePersonalMedical>(value.Trim(), true, out var enumResult))
        {
            return enumResult;
        }

        // Încercăm să găsim prin display name
        foreach (PozitiePersonalMedical pozitie in Enum.GetValues<PozitiePersonalMedical>())
        {
            if (string.Equals(pozitie.GetDisplayName(), value.Trim(), StringComparison.OrdinalIgnoreCase))
            {
                return pozitie;
            }
        }

        // Mapări comune pentru compatibilitate cu datele existente din baza de date
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
            
            // Variante comune pentru Recepționer Medical
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
    /// Returnează toate pozițiile disponibile ca listă de perechi nume-valoare
    /// </summary>
    /// <returns>Lista pozițiilor cu numele și valorile lor</returns>
    public static List<(string Name, int Value)> GetAllPositions()
    {
        return Enum.GetValues<PozitiePersonalMedical>()
            .Select(p => (p.GetDisplayName(), (int)p))
            .ToList();
    }

    /// <summary>
    /// Verifică dacă poziția necesită licență medicală
    /// </summary>
    /// <param name="pozitie">Poziția de verificat</param>
    /// <returns>True dacă poziția necesită licență medicală</returns>
    public static bool RequiresLicensaMedicala(this PozitiePersonalMedical pozitie)
    {
        return pozitie == PozitiePersonalMedical.Doctor || 
               pozitie == PozitiePersonalMedical.AsistentMedical ||
               pozitie == PozitiePersonalMedical.Radiolog;
    }

    /// <summary>
    /// Returnează culoarea badge-ului pentru poziție (pentru UI)
    /// </summary>
    /// <param name="pozitie">Poziția pentru care se dorește culoarea</param>
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
    /// Returnează iconița Font Awesome pentru poziție
    /// </summary>
    /// <param name="pozitie">Poziția pentru care se dorește iconița</param>
    /// <returns>Clasa CSS pentru iconița Font Awesome</returns>
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
    /// Convertește enum-ul la format string pentru salvare în baza de date
    /// </summary>
    /// <param name="pozitie">Poziția de convertit</param>
    /// <returns>Reprezentarea string pentru baza de date</returns>
    public static string ToDatabase(this PozitiePersonalMedical pozitie)
    {
        // Salvăm ca număr pentru consistență
        return ((int)pozitie).ToString();
    }

    /// <summary>
    /// Validează dacă o poziție este validă pentru operațiuni medicale
    /// </summary>
    /// <param name="pozitie">Poziția de validat</param>
    /// <returns>True dacă poziția este validă</returns>
    public static bool IsValidMedicalPosition(this PozitiePersonalMedical pozitie)
    {
        return Enum.IsDefined(typeof(PozitiePersonalMedical), pozitie);
    }
}
