using System.ComponentModel.DataAnnotations;
using System.Reflection;
using ValyanClinic.Domain.Enums;

namespace ValyanClinic.Domain.Extensions;

/// <summary>
/// Extension methods pentru enum-ul PozitiePersonalMedical
/// Oferă funcționalități pentru afișare și validare specifice pozițiilor medicale
/// </summary>
public static class PozitiePersonalMedicalExtensions
{
    /// <summary>
    /// Obține numele de afișare al poziției medicale din atributul Display
    /// </summary>
    /// <param name="pozitie">Poziția medicală</param>
    /// <returns>Numele de afișare în română</returns>
    public static string GetDisplayName(this PozitiePersonalMedical pozitie)
    {
        var type = typeof(PozitiePersonalMedical);
        var memberInfo = type.GetMember(pozitie.ToString()).FirstOrDefault();
        
        if (memberInfo != null)
        {
            var displayAttribute = memberInfo.GetCustomAttribute<DisplayAttribute>();
            if (displayAttribute != null && !string.IsNullOrEmpty(displayAttribute.Name))
            {
                return displayAttribute.Name;
            }
        }
        
        // Fallback la numele enum-ului dacă nu există DisplayAttribute
        return pozitie.ToString();
    }

    /// <summary>
    /// Verifică dacă poziția necesită licență medicală obligatorie
    /// </summary>
    /// <param name="pozitie">Poziția medicală</param>
    /// <returns>True dacă poziția necesită licență medicală</returns>
    public static bool RequiresMedicalLicense(this PozitiePersonalMedical pozitie)
    {
        return pozitie switch
        {
            PozitiePersonalMedical.Doctor => true,
            PozitiePersonalMedical.AsistentMedical => true,
            PozitiePersonalMedical.Radiolog => true,
            PozitiePersonalMedical.TehnicianMedical => false, // Poate necesita certificare specifică, dar nu licență medicală
            PozitiePersonalMedical.ReceptionerMedical => false,
            PozitiePersonalMedical.Laborant => false, // Poate necesita certificare de laborator, dar nu licență medicală
            _ => false
        };
    }

    /// <summary>
    /// Verifică dacă poziția necesită specializare medicală completă
    /// </summary>
    /// <param name="pozitie">Poziția medicală</param>
    /// <returns>True dacă poziția necesită specializare</returns>
    public static bool RequiresSpecialization(this PozitiePersonalMedical pozitie)
    {
        return pozitie switch
        {
            PozitiePersonalMedical.Doctor => true,
            PozitiePersonalMedical.Radiolog => true,
            PozitiePersonalMedical.AsistentMedical => false, // Poate avea specializare opțională
            PozitiePersonalMedical.TehnicianMedical => false,
            PozitiePersonalMedical.ReceptionerMedical => false,
            PozitiePersonalMedical.Laborant => false,
            _ => false
        };
    }

    /// <summary>
    /// Obține iconița Font Awesome asociată cu poziția medicală
    /// </summary>
    /// <param name="pozitie">Poziția medicală</param>
    /// <returns>Clasa CSS pentru iconița Font Awesome</returns>
    public static string GetIcon(this PozitiePersonalMedical pozitie)
    {
        return pozitie switch
        {
            PozitiePersonalMedical.Doctor => "fas fa-stethoscope",
            PozitiePersonalMedical.AsistentMedical => "fas fa-user-nurse",
            PozitiePersonalMedical.TehnicianMedical => "fas fa-tools",
            PozitiePersonalMedical.ReceptionerMedical => "fas fa-desktop",
            PozitiePersonalMedical.Radiolog => "fas fa-x-ray",
            PozitiePersonalMedical.Laborant => "fas fa-microscope",
            _ => "fas fa-user"
        };
    }

    /// <summary>
    /// Obține culoarea asociată cu poziția medicală pentru badge-uri și indicatori vizuali
    /// </summary>
    /// <param name="pozitie">Poziția medicală</param>
    /// <returns>Clasa CSS pentru culoare</returns>
    public static string GetColorClass(this PozitiePersonalMedical pozitie)
    {
        return pozitie switch
        {
            PozitiePersonalMedical.Doctor => "badge-doctor",
            PozitiePersonalMedical.AsistentMedical => "badge-nurse",
            PozitiePersonalMedical.TehnicianMedical => "badge-technician",
            PozitiePersonalMedical.ReceptionerMedical => "badge-receptionist",
            PozitiePersonalMedical.Radiolog => "badge-radiologist",
            PozitiePersonalMedical.Laborant => "badge-lab-technician",
            _ => "badge-default"
        };
    }

    /// <summary>
    /// Obține descrierea detaliată a poziției medicale
    /// </summary>
    /// <param name="pozitie">Poziția medicală</param>
    /// <returns>Descrierea detaliată în română</returns>
    public static string GetDescription(this PozitiePersonalMedical pozitie)
    {
        return pozitie switch
        {
            PozitiePersonalMedical.Doctor => 
                "Medic specialist sau generalist cu licență medicală și specializare completă",
            PozitiePersonalMedical.AsistentMedical => 
                "Asistent medical calificat cu licență de asistent medical",
            PozitiePersonalMedical.TehnicianMedical => 
                "Tehnician medical specializat în echipamente și proceduri tehnice",
            PozitiePersonalMedical.ReceptionerMedical => 
                "Personal de recepție specializat în proceduri medicale de front-desk",
            PozitiePersonalMedical.Radiolog => 
                "Specialist în imagistică medicală cu licență medicală și specializare în radiologie",
            PozitiePersonalMedical.Laborant => 
                "Tehnician de laborator specializat în analize medicale",
            _ => "Poziție medicală nespecificată"
        };
    }

    /// <summary>
    /// Verifică dacă poziția poate prescrie medicamente
    /// </summary>
    /// <param name="pozitie">Poziția medicală</param>
    /// <returns>True dacă poate prescrie medicamente</returns>
    public static bool CanPrescribeMedication(this PozitiePersonalMedical pozitie)
    {
        return pozitie switch
        {
            PozitiePersonalMedical.Doctor => true,
            PozitiePersonalMedical.Radiolog => true, // În limitele specializării
            PozitiePersonalMedical.AsistentMedical => false,
            PozitiePersonalMedical.TehnicianMedical => false,
            PozitiePersonalMedical.ReceptionerMedical => false,
            PozitiePersonalMedical.Laborant => false,
            _ => false
        };
    }

    /// <summary>
    /// Verifică dacă poziția poate efectua consultații medicale
    /// </summary>
    /// <param name="pozitie">Poziția medicală</param>
    /// <returns>True dacă poate efectua consultații</returns>
    public static bool CanPerformConsultations(this PozitiePersonalMedical pozitie)
    {
        return pozitie switch
        {
            PozitiePersonalMedical.Doctor => true,
            PozitiePersonalMedical.Radiolog => true, // Consultații de imagistică
            PozitiePersonalMedical.AsistentMedical => false, // Poate asista, dar nu consulta independent
            PozitiePersonalMedical.TehnicianMedical => false,
            PozitiePersonalMedical.ReceptionerMedical => false,
            PozitiePersonalMedical.Laborant => false,
            _ => false
        };
    }

    /// <summary>
    /// Obține nivelul de acces la informațiile pacienților
    /// </summary>
    /// <param name="pozitie">Poziția medicală</param>
    /// <returns>Nivelul de acces (1=limitat, 2=moderat, 3=complet)</returns>
    public static int GetPatientDataAccessLevel(this PozitiePersonalMedical pozitie)
    {
        return pozitie switch
        {
            PozitiePersonalMedical.Doctor => 3, // Acces complet
            PozitiePersonalMedical.Radiolog => 3, // Acces complet pentru imagistică
            PozitiePersonalMedical.AsistentMedical => 2, // Acces moderat
            PozitiePersonalMedical.TehnicianMedical => 2, // Acces moderat pentru proceduri
            PozitiePersonalMedical.Laborant => 2, // Acces la rezultate de laborator
            PozitiePersonalMedical.ReceptionerMedical => 1, // Acces limitat la date de contact
            _ => 1
        };
    }

    /// <summary>
    /// Obține lista certificărilor recomandate pentru poziție
    /// </summary>
    /// <param name="pozitie">Poziția medicală</param>
    /// <returns>Lista certificărilor recomandate</returns>
    public static List<string> GetRecommendedCertifications(this PozitiePersonalMedical pozitie)
    {
        return pozitie switch
        {
            PozitiePersonalMedical.Doctor => new List<string>
            {
                "Licența medicală",
                "Certificat de specializare",
                "Educație medicală continuă (EMC)",
                "Certificat RCP (Resuscitare Cardio-Pulmonară)"
            },
            PozitiePersonalMedical.AsistentMedical => new List<string>
            {
                "Licența de asistent medical",
                "Certificat de competențe",
                "Primul ajutor",
                "Certificat RCP"
            },
            PozitiePersonalMedical.TehnicianMedical => new List<string>
            {
                "Certificat de calificare tehnician medical",
                "Instruire securitate și sănătate",
                "Certificări specifice echipamentelor",
                "Primul ajutor"
            },
            PozitiePersonalMedical.ReceptionerMedical => new List<string>
            {
                "Instruire GDPR și confidențialitate",
                "Certificat în comunicare medicală",
                "Primul ajutor",
                "Instruire sistemă informaționale medicale"
            },
            PozitiePersonalMedical.Radiolog => new List<string>
            {
                "Licența medicală",
                "Specializarea în radiologie",
                "Certificări pentru echipamente de imagistică",
                "Protecția radiologică",
                "Educație medicală continuă"
            },
            PozitiePersonalMedical.Laborant => new List<string>
            {
                "Certificat de calificare laborant",
                "Certificări în siguranța laboratorului",
                "Primul ajutor",
                "Certificări specifice tipului de analize"
            },
            _ => new List<string> { "Instruire securitate și sănătate" }
        };
    }

    /// <summary>
    /// Verifică dacă poziția poate lucra în ture de noapte
    /// </summary>
    /// <param name="pozitie">Poziția medicală</param>
    /// <returns>True dacă poate lucra în ture de noapte</returns>
    public static bool CanWorkNightShifts(this PozitiePersonalMedical pozitie)
    {
        return pozitie switch
        {
            PozitiePersonalMedical.Doctor => true,
            PozitiePersonalMedical.AsistentMedical => true,
            PozitiePersonalMedical.TehnicianMedical => true, // Pentru urgențe
            PozitiePersonalMedical.Radiolog => true, // Pentru urgențe
            PozitiePersonalMedical.ReceptionerMedical => false, // De obicei program de zi
            PozitiePersonalMedical.Laborant => false, // De obicei program de zi
            _ => false
        };
    }

    /// <summary>
    /// Obține toate pozițiile medicale ca listă pentru dropdown-uri
    /// </summary>
    /// <returns>Lista tuturor pozițiilor cu display name</returns>
    public static List<(PozitiePersonalMedical Value, string Text)> GetAllPositionsForDropdown()
    {
        return Enum.GetValues<PozitiePersonalMedical>()
            .Select(p => (p, p.GetDisplayName()))
            .OrderBy(p => p.Item2)
            .ToList();
    }

    /// <summary>
    /// Filtrează pozițiile medicale pe baza nivelului de acces
    /// </summary>
    /// <param name="minimumAccessLevel">Nivelul minim de acces necesar</param>
    /// <returns>Lista pozițiilor cu acces suficient</returns>
    public static List<PozitiePersonalMedical> GetPositionsByAccessLevel(int minimumAccessLevel)
    {
        return Enum.GetValues<PozitiePersonalMedical>()
            .Where(p => p.GetPatientDataAccessLevel() >= minimumAccessLevel)
            .ToList();
    }

    /// <summary>
    /// Obține pozițiile care pot efectua o anumită acțiune
    /// </summary>
    /// <param name="action">Acțiunea dorită (prescribe, consult, etc.)</param>
    /// <returns>Lista pozițiilor care pot efectua acțiunea</returns>
    public static List<PozitiePersonalMedical> GetPositionsByCapability(string action)
    {
        return action.ToLowerInvariant() switch
        {
            "prescribe" or "prescriere" => Enum.GetValues<PozitiePersonalMedical>()
                .Where(p => p.CanPrescribeMedication())
                .ToList(),
            "consult" or "consultatie" => Enum.GetValues<PozitiePersonalMedical>()
                .Where(p => p.CanPerformConsultations())
                .ToList(),
            "license" or "licenta" => Enum.GetValues<PozitiePersonalMedical>()
                .Where(p => p.RequiresMedicalLicense())
                .ToList(),
            "specialization" or "specializare" => Enum.GetValues<PozitiePersonalMedical>()
                .Where(p => p.RequiresSpecialization())
                .ToList(),
            "night_shift" or "tura_noapte" => Enum.GetValues<PozitiePersonalMedical>()
                .Where(p => p.CanWorkNightShifts())
                .ToList(),
            _ => new List<PozitiePersonalMedical>()
        };
    }
}
