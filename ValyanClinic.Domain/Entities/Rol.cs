namespace ValyanClinic.Domain.Entities;

/// <summary>
/// Entitate de domeniu pentru Rol.
/// Definește rolurile din sistem cu permisiunile asociate.
/// </summary>
public class Rol
{
    /// <summary>Primary Key</summary>
    public Guid RolID { get; set; }
    
    /// <summary>Numele rolului (ex: Admin, Doctor, Asistent, Receptioner, Manager)</summary>
    public string Denumire { get; set; } = string.Empty;
    
    /// <summary>Descrierea rolului și responsabilităților</summary>
    public string? Descriere { get; set; }
    
    /// <summary>Dacă rolul este activ și poate fi atribuit utilizatorilor</summary>
    public bool EsteActiv { get; set; } = true;
    
    /// <summary>Dacă rolul este unul sistem (nu poate fi șters)</summary>
    public bool EsteSistem { get; set; } = false;
    
    /// <summary>Ordinea de afișare în liste</summary>
    public int OrdineAfisare { get; set; }
    
    // Audit
    public string? CreatDe { get; set; }
    public DateTime DataCrearii { get; set; }
    public string? ModificatDe { get; set; }
    public DateTime DataUltimeiModificari { get; set; }
    
    // Navigation property
    public ICollection<RolPermisiune> Permisiuni { get; set; } = new List<RolPermisiune>();
}
