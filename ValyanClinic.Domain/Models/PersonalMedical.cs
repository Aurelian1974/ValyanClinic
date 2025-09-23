using ValyanClinic.Domain.Enums;

namespace ValyanClinic.Domain.Models;

/// <summary>
/// Model pentru personalul medical din clinica ValyanMed
/// Diferă de Personal prin focus pe date medicale specifice și departamente din DB
/// </summary>
public class PersonalMedical
{
    // Identificatori Unici
    public Guid PersonalID { get; set; }
    
    // Date Personale De Baza
    public string Nume { get; set; } = string.Empty;
    public string Prenume { get; set; } = string.Empty;
    
    // Date Medicale Specifice
    public string? Specializare { get; set; }
    public string? NumarLicenta { get; set; }
    
    // Contact
    public string? Telefon { get; set; }
    public string? Email { get; set; }
    
    // Departament și Poziție
    public string? Departament { get; set; } // Text pentru compatibilitate
    public PozitiePersonalMedical? Pozitie { get; set; }
    
    // Status și Metadata
    public bool EsteActiv { get; set; } = true;
    public DateTime DataCreare { get; set; } = DateTime.Now;
    
    // Relații cu Departamente (FK din tabela Departamente WHERE Tip = 'Medical')
    public Guid? CategorieID { get; set; }
    public Guid? SpecializareID { get; set; }
    public Guid? SubspecializareID { get; set; }
    
    // Lookup Values (populate din JOIN-uri la tabela Departamente)
    public string? CategorieName { get; set; }
    public string? SpecializareName { get; set; }
    public string? SubspecializareName { get; set; }
    
    // Computed Properties pentru UI
    public string NumeComplet => $"{Nume} {Prenume}";
    public string StatusDisplay => EsteActiv ? "Activ" : "Inactiv";
    public string PozitieDisplay => Pozitie?.GetDisplayName() ?? "Neprecizată";
    public string DepartamentDisplay => Departament ?? CategorieName ?? "Nedefinit";
    
    // Proprietăți pentru validări medicale
    public bool EsteDoctorSauAsistent => 
        Pozitie == PozitiePersonalMedical.Doctor || 
        Pozitie == PozitiePersonalMedical.AsistentMedical;
        
    public bool AreNevieDeLicenta => EsteDoctorSauAsistent;
    
    public bool AreLicentaValida => !string.IsNullOrWhiteSpace(NumarLicenta) && NumarLicenta.Length >= 5;
    
    // Proprietăți pentru specializări medicale
    public bool AreSpecializareCompleta => 
        CategorieID.HasValue && SpecializareID.HasValue;
        
    public string SpecializareCompleta
    {
        get
        {
            var parts = new List<string>();
            if (!string.IsNullOrEmpty(CategorieName)) parts.Add(CategorieName);
            if (!string.IsNullOrEmpty(SpecializareName)) parts.Add(SpecializareName);
            if (!string.IsNullOrEmpty(SubspecializareName)) parts.Add(SubspecializareName);
            
            return parts.Count > 0 ? string.Join(" / ", parts) : (Specializare ?? "Nespecificată");
        }
    }
    
    // Proprietăți pentru UI și business logic
    public string ContactDisplay
    {
        get
        {
            var contacts = new List<string>();
            if (!string.IsNullOrEmpty(Telefon)) contacts.Add($"📞 {Telefon}");
            if (!string.IsNullOrEmpty(Email)) contacts.Add($"✉️ {Email}");
            return contacts.Count > 0 ? string.Join(" | ", contacts) : "Fără contact";
        }
    }
    
    // Proprietăți pentru căutare și filtrare
    public string SearchableText => 
        $"{NumeComplet} {Email} {Telefon} {NumarLicenta} {Specializare} {CategorieName} {SpecializareName} {Departament}".ToLower();
        
    // Metode pentru business logic
    public bool MatchesSearchText(string? searchText)
    {
        if (string.IsNullOrWhiteSpace(searchText)) return true;
        return SearchableText.Contains(searchText.ToLower());
    }
    
    public bool MatchesDepartament(string? departament)
    {
        if (string.IsNullOrWhiteSpace(departament)) return true;
        return string.Equals(Departament, departament, StringComparison.OrdinalIgnoreCase) ||
               string.Equals(CategorieName, departament, StringComparison.OrdinalIgnoreCase);
    }
    
    public bool MatchesPozitie(PozitiePersonalMedical? pozitie)
    {
        if (!pozitie.HasValue) return true;
        return Pozitie == pozitie;
    }
    
    public bool MatchesStatus(bool? esteActiv)
    {
        if (!esteActiv.HasValue) return true;
        return EsteActiv == esteActiv;
    }
    
    // Metode pentru audit și tracking
    public bool IsValidForSave()
    {
        if (string.IsNullOrWhiteSpace(Nume) || string.IsNullOrWhiteSpace(Prenume))
            return false;
            
        if (AreNevieDeLicenta && !AreLicentaValida)
            return false;
            
        if (!string.IsNullOrWhiteSpace(Email) && !Email.Contains('@'))
            return false;
            
        return true;
    }
    
    public List<string> GetValidationErrors()
    {
        var errors = new List<string>();
        
        if (string.IsNullOrWhiteSpace(Nume))
            errors.Add("Numele este obligatoriu");
            
        if (string.IsNullOrWhiteSpace(Prenume))
            errors.Add("Prenumele este obligatoriu");
            
        if (AreNevieDeLicenta && !AreLicentaValida)
            errors.Add("Numărul de licență este obligatoriu pentru doctori și asistenți medicali");
            
        if (!string.IsNullOrWhiteSpace(Email) && !Email.Contains('@'))
            errors.Add("Formatul email-ului este invalid");
            
        if (!string.IsNullOrWhiteSpace(NumarLicenta) && NumarLicenta.Length < 5)
            errors.Add("Numărul de licență trebuie să aibă cel puțin 5 caractere");
            
        return errors;
    }
}
