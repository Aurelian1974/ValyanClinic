namespace ValyanClinic.Domain.Entities;

/// <summary>
/// Entitate de domeniu pentru definirea permisiunilor disponibile în sistem.
/// Catalog de permisiuni disponibile pentru asignare la roluri.
/// </summary>
public class PermisiuneDefinitie
{
    /// <summary>Primary Key</summary>
    public Guid PermisiuneDefinitieID { get; set; }
    
    /// <summary>Codul unic al permisiunii (ex: Pacient.View)</summary>
    public string Cod { get; set; } = string.Empty;
    
    /// <summary>Categoria permisiunii (ex: Pacient, Consultatie, Programare)</summary>
    public string Categorie { get; set; } = string.Empty;
    
    /// <summary>Denumirea afișată în UI</summary>
    public string Denumire { get; set; } = string.Empty;
    
    /// <summary>Descrierea detaliată a permisiunii</summary>
    public string? Descriere { get; set; }
    
    /// <summary>Ordinea de afișare în categorie</summary>
    public int OrdineAfisare { get; set; }
    
    /// <summary>Dacă permisiunea este activă</summary>
    public bool EsteActiv { get; set; } = true;
}
