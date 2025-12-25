namespace ValyanClinic.Domain.Entities;

/// <summary>
/// Entitate de domeniu pentru asocierea Rol - Permisiune.
/// Tabelă de joncțiune many-to-many între roluri și permisiuni.
/// </summary>
public class RolPermisiune
{
    /// <summary>Primary Key</summary>
    public Guid RolPermisiuneID { get; set; }
    
    /// <summary>Foreign Key către Rol</summary>
    public Guid RolID { get; set; }
    
    /// <summary>Codul permisiunii (ex: Pacient.View, Consultatie.Create)</summary>
    public string Permisiune { get; set; } = string.Empty;
    
    /// <summary>Dacă permisiunea este acordată sau refuzată explicit</summary>
    public bool EsteAcordat { get; set; } = true;
    
    // Audit
    public string? CreatDe { get; set; }
    public DateTime DataCrearii { get; set; }
    
    // Navigation property
    public Rol? Rol { get; set; }
}
