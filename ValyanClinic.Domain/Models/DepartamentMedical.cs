namespace ValyanClinic.Domain.Models;

/// <summary>
/// Model pentru departamentele medicale din tabela Departamente
/// IMPORTANT: Aceasta este o clasa pentru date din DB, NU un enum static
/// Departamentele se incarca dinamic din sp_Departamente_GetByTip cu @Tip = 'Medical'
/// </summary>
public class DepartamentMedical
{
    // Proprietati din baza de date
    public Guid DepartamentID { get; set; }
    public string Nume { get; set; } = string.Empty;
    public string Tip { get; set; } = string.Empty;
    
    // Computed Properties pentru UI
    public string DisplayName => Nume;
    
    // Proprietati pentru dropdown-uri si UI
    public string Value => DepartamentID.ToString();
    public string Text => Nume;
    
    // Proprietati pentru validari si business logic
    public bool IsMedical => string.Equals(Tip, "Medical", StringComparison.OrdinalIgnoreCase);
    
    // Metode pentru comparare si egalitate
    public override bool Equals(object? obj)
    {
        if (obj is DepartamentMedical other)
        {
            return DepartamentID == other.DepartamentID;
        }
        return false;
    }
    
    public override int GetHashCode()
    {
        return DepartamentID.GetHashCode();
    }
    
    public override string ToString()
    {
        return DisplayName;
    }
    
    // Factory methods pentru crearea instantelor
    public static DepartamentMedical Create(Guid departamentId, string nume, string tip = "Medical")
    {
        return new DepartamentMedical
        {
            DepartamentID = departamentId,
            Nume = nume,
            Tip = tip
        };
    }
    
    // Metode pentru validari
    public bool IsValid()
    {
        return DepartamentID != Guid.Empty && 
               !string.IsNullOrWhiteSpace(Nume) && 
               !string.IsNullOrWhiteSpace(Tip);
    }
    
    // Metode pentru cautare si filtrare
    public bool MatchesSearchText(string? searchText)
    {
        if (string.IsNullOrWhiteSpace(searchText)) return true;
        return Nume.Contains(searchText, StringComparison.OrdinalIgnoreCase) ||
               Tip.Contains(searchText, StringComparison.OrdinalIgnoreCase);
    }
}

/// <summary>
/// Model pentru dropdown options cu DepartamentMedical
/// Folosit pentru UI components Syncfusion
/// </summary>
public class DepartamentMedicalOption
{
    public Guid Value { get; set; }
    public string Text { get; set; } = string.Empty;
    public string Tip { get; set; } = string.Empty;
    
    public static DepartamentMedicalOption FromDepartamentMedical(DepartamentMedical departament)
    {
        return new DepartamentMedicalOption
        {
            Value = departament.DepartamentID,
            Text = departament.Nume,
            Tip = departament.Tip
        };
    }
    
    public static List<DepartamentMedicalOption> FromDepartamenteMedicale(IEnumerable<DepartamentMedical> departamente)
    {
        return departamente.Select(FromDepartamentMedical).ToList();
    }
}

/// <summary>
/// Container pentru toate tipurile de departamente medicale
/// Folosit pentru gruparea categoriilor, specializarilor si subspecializarilor
/// </summary>
public class DepartamenteMedicaleContainer
{
    public List<DepartamentMedical> Categorii { get; set; } = new();
    public List<DepartamentMedical> Specializari { get; set; } = new();
    public List<DepartamentMedical> Subspecializari { get; set; } = new();
    public List<DepartamentMedical> ToateDepartamentele { get; set; } = new();
    
    // Metode pentru gasirea departamentelor dupa tip
    public DepartamentMedical? FindCategorie(Guid id) => 
        Categorii.FirstOrDefault(c => c.DepartamentID == id);
        
    public DepartamentMedical? FindSpecializare(Guid id) => 
        Specializari.FirstOrDefault(s => s.DepartamentID == id);
        
    public DepartamentMedical? FindSubspecializare(Guid id) => 
        Subspecializari.FirstOrDefault(s => s.DepartamentID == id);
        
    public DepartamentMedical? FindAny(Guid id) => 
        ToateDepartamentele.FirstOrDefault(d => d.DepartamentID == id);
        
    // Proprietati pentru statistici si informatii
    public int TotalCategorii => Categorii.Count;
    public int TotalSpecializari => Specializari.Count;
    public int TotalSubspecializari => Subspecializari.Count;
    public int TotalDepartamente => ToateDepartamentele.Count;
    
    public bool IsEmpty => TotalDepartamente == 0;
    public bool HasData => !IsEmpty;
    
    // Metode pentru validari
    public bool IsValidConfiguration()
    {
        return TotalCategorii > 0 && TotalSpecializari > 0;
    }
}
