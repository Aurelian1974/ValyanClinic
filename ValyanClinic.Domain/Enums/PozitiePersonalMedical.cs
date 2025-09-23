using System.ComponentModel.DataAnnotations;

namespace ValyanClinic.Domain.Enums;

/// <summary>
/// Enumerare pentru pozițiile personalului medical în clinică
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
               pozitie == PozitiePersonalMedical.AsistentMedical;
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
}
