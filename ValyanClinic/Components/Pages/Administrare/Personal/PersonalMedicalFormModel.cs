using ValyanClinic.Domain.Models;
using ValyanClinic.Domain.Enums;
using PersonalMedicalModel = ValyanClinic.Domain.Models.PersonalMedical;
using System.ComponentModel.DataAnnotations;

namespace ValyanClinic.Components.Pages.Administrare.Personal;

/// <summary>
/// Form model pentru PersonalMedical - adaptat pentru formulare UI
/// Similar cu PersonalFormModel dar specific pentru PersonalMedical
/// </summary>
public class PersonalMedicalFormModel
{
    // Identificatori
    public Guid PersonalID { get; set; } = Guid.NewGuid();

    // Date personale de bază
    [Required(ErrorMessage = "Numele este obligatoriu")]
    public string Nume { get; set; } = "";

    [Required(ErrorMessage = "Prenumele este obligatoriu")]
    public string Prenume { get; set; } = "";

    // Date medicale specifice
    public string? Specializare { get; set; }

    public string? NumarLicenta { get; set; }

    // Contact
    [EmailAddress(ErrorMessage = "Formatul email-ului este invalid")]
    public string? Email { get; set; }

    [Phone(ErrorMessage = "Formatul telefonului este invalid")]
    public string? Telefon { get; set; }

    // Departament și poziție
    public string? Departament { get; set; } // Text pentru compatibilitate

    [Required(ErrorMessage = "Poziția este obligatorie")]
    public PozitiePersonalMedical? Pozitie { get; set; }

    // Status
    public bool EsteActiv { get; set; } = true;

    // Relații cu departamente medicale (FK-uri din tabela Departamente)
    public Guid? CategorieID { get; set; }
    public Guid? SpecializareID { get; set; }
    public Guid? SubspecializareID { get; set; }

    // Lookup values pentru afișare (populate din JOIN-uri)
    public string? CategorieName { get; set; }
    public string? SpecializareName { get; set; }
    public string? SubspecializareName { get; set; }

    // Metadata
    public DateTime DataCreare { get; set; } = DateTime.Now;

    // Computed properties pentru validări și afișare
    public string NumeComplet => $"{Nume} {Prenume}";
    public bool EsteDoctorSauAsistent => 
        Pozitie == PozitiePersonalMedical.Doctor || 
        Pozitie == PozitiePersonalMedical.AsistentMedical;
    public bool AreNevieDeLicenta => EsteDoctorSauAsistent;

    // Metode de conversie către și din PersonalMedicalModel
    public static PersonalMedicalFormModel FromPersonalMedical(PersonalMedicalModel personalMedical)
    {
        return new PersonalMedicalFormModel
        {
            PersonalID = personalMedical.PersonalID,
            Nume = personalMedical.Nume,
            Prenume = personalMedical.Prenume,
            Specializare = personalMedical.Specializare,
            NumarLicenta = personalMedical.NumarLicenta,
            Telefon = personalMedical.Telefon,
            Email = personalMedical.Email,
            Departament = personalMedical.Departament,
            Pozitie = personalMedical.Pozitie,
            EsteActiv = personalMedical.EsteActiv,
            DataCreare = personalMedical.DataCreare,
            CategorieID = personalMedical.CategorieID,
            SpecializareID = personalMedical.SpecializareID,
            SubspecializareID = personalMedical.SubspecializareID,
            CategorieName = personalMedical.CategorieName,
            SpecializareName = personalMedical.SpecializareName,
            SubspecializareName = personalMedical.SubspecializareName
        };
    }

    public PersonalMedicalModel ToPersonalMedical()
    {
        var now = DateTime.Now;
        var isNewRecord = PersonalID == Guid.Empty || DataCreare == default;
        
        return new PersonalMedicalModel
        {
            PersonalID = PersonalID == Guid.Empty ? Guid.NewGuid() : PersonalID,
            Nume = Nume,
            Prenume = Prenume,
            Specializare = Specializare,
            NumarLicenta = NumarLicenta,
            Telefon = Telefon,
            Email = Email,
            Departament = Departament,
            Pozitie = Pozitie,
            EsteActiv = EsteActiv,
            DataCreare = isNewRecord ? now : DataCreare,
            CategorieID = CategorieID,
            SpecializareID = SpecializareID,
            SubspecializareID = SubspecializareID,
            CategorieName = CategorieName,
            SpecializareName = SpecializareName,
            SubspecializareName = SubspecializareName
        };
    }

    // Validări specifice pentru PersonalMedical
    public List<string> GetValidationErrors()
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(Nume))
            errors.Add("Numele este obligatoriu");

        if (string.IsNullOrWhiteSpace(Prenume))
            errors.Add("Prenumele este obligatoriu");

        if (!Pozitie.HasValue)
            errors.Add("Poziția este obligatorie");

        // Validare licență medicală pentru doctori și asistenți
        if (AreNevieDeLicenta && string.IsNullOrWhiteSpace(NumarLicenta))
            errors.Add($"Numărul de licență este obligatoriu pentru poziția {Pozitie?.GetDisplayName()}");

        if (!string.IsNullOrWhiteSpace(NumarLicenta) && NumarLicenta.Length < 5)
            errors.Add("Numărul de licență trebuie să aibă cel puțin 5 caractere");

        // Validare email
        if (!string.IsNullOrWhiteSpace(Email) && !IsValidEmail(Email))
            errors.Add("Formatul email-ului este invalid");

        // Validare pentru specializare completă (opțional, dar recomandat)
        if (AreNevieDeLicenta && !CategorieID.HasValue)
            errors.Add("Categoria medicală este recomandată pentru doctori și asistenți");

        return errors;
    }

    public bool IsValid()
    {
        return GetValidationErrors().Count == 0;
    }

    // Metode helper pentru validări
    private static bool IsValidEmail(string email)
    {
        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }

    // Metode pentru gestionarea specializărilor medicale
    public void SetSpecializareCompleta(Guid? categorieId, Guid? specializareId, Guid? subspecializareId,
        string? categorieName = null, string? specializareName = null, string? subspecializareName = null)
    {
        CategorieID = categorieId;
        SpecializareID = specializareId;
        SubspecializareID = subspecializareId;
        
        CategorieName = categorieName;
        SpecializareName = specializareName;
        SubspecializareName = subspecializareName;
    }

    public void ClearSpecializareCompleta()
    {
        CategorieID = null;
        SpecializareID = null;
        SubspecializareID = null;
        
        CategorieName = null;
        SpecializareName = null;
        SubspecializareName = null;
    }

    // Metode pentru gestionarea pozițiilor cu validări automatice
    public void SetPozitie(PozitiePersonalMedical? pozitie)
    {
        Pozitie = pozitie;

        // Dacă poziția nu necesită licență, șterge licența existentă (opțional)
        if (pozitie.HasValue && !pozitie.Value.RequiresLicensaMedicala())
        {
            // Opțional: șterge licența pentru pozițiile care nu o necesită
            // NumarLicenta = null;
        }
    }

    // Clone method pentru backup/restore
    public PersonalMedicalFormModel Clone()
    {
        return new PersonalMedicalFormModel
        {
            PersonalID = PersonalID,
            Nume = Nume,
            Prenume = Prenume,
            Specializare = Specializare,
            NumarLicenta = NumarLicenta,
            Telefon = Telefon,
            Email = Email,
            Departament = Departament,
            Pozitie = Pozitie,
            EsteActiv = EsteActiv,
            DataCreare = DataCreare,
            CategorieID = CategorieID,
            SpecializareID = SpecializareID,
            SubspecializareID = SubspecializareID,
            CategorieName = CategorieName,
            SpecializareName = SpecializareName,
            SubspecializareName = SubspecializareName
        };
    }

    // Reset method pentru formulare noi
    public void Reset()
    {
        PersonalID = Guid.NewGuid();
        Nume = "";
        Prenume = "";
        Specializare = null;
        NumarLicenta = null;
        Telefon = null;
        Email = null;
        Departament = null;
        Pozitie = null;
        EsteActiv = true;
        DataCreare = DateTime.Now;
        
        ClearSpecializareCompleta();
    }
}

/// <summary>
/// Dropdown option helper pentru UI components
/// </summary>
public class PersonalMedicalDropdownOption<T>
{
    public PersonalMedicalDropdownOption(T value, string text)
    {
        Value = value;
        Text = text;
    }

    public T Value { get; set; }
    public string Text { get; set; }
}

/// <summary>
/// Extensii specifice pentru PersonalMedicalFormModel
/// </summary>
public static class PersonalMedicalFormModelExtensions
{
    /// <summary>
    /// Crează o listă de opțiuni dropdown pentru poziții medicale
    /// </summary>
    public static List<PersonalMedicalDropdownOption<PozitiePersonalMedical?>> GetPozitieOptions()
    {
        var options = new List<PersonalMedicalDropdownOption<PozitiePersonalMedical?>>
        {
            new(null, "Selectează poziția...")
        };

        foreach (PozitiePersonalMedical pozitie in Enum.GetValues<PozitiePersonalMedical>())
        {
            options.Add(new PersonalMedicalDropdownOption<PozitiePersonalMedical?>(
                pozitie, pozitie.GetDisplayName()));
        }

        return options;
    }

    /// <summary>
    /// Crează o listă de opțiuni dropdown pentru status
    /// </summary>
    public static List<PersonalMedicalDropdownOption<bool>> GetStatusOptions()
    {
        return new List<PersonalMedicalDropdownOption<bool>>
        {
            new(true, "Activ"),
            new(false, "Inactiv")
        };
    }

    /// <summary>
    /// Validează o instanță de PersonalMedicalFormModel
    /// </summary>
    public static (bool IsValid, List<string> Errors) ValidatePersonalMedicalFormModel(
        this PersonalMedicalFormModel model)
    {
        var errors = model.GetValidationErrors();
        return (errors.Count == 0, errors);
    }
}
