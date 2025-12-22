namespace ValyanClinic.Services;

/// <summary>
/// Service pentru navigare la pagina de consultații fără ID-uri în URL
/// Scoped service - păstrează starea per circuit/sesiune Blazor
/// </summary>
public interface IConsultatieNavigationService
{
    /// <summary>
    /// ID-ul programării curente (obligatoriu - toate consultațiile vin din programări)
    /// </summary>
    Guid? ProgramareId { get; }
    
    /// <summary>
    /// ID-ul pacientului
    /// </summary>
    Guid? PacientId { get; }
    
    /// <summary>
    /// ID-ul consultației (pentru editare)
    /// </summary>
    Guid? ConsultatieId { get; }
    
    /// <summary>
    /// Indică dacă contextul de navigare este valid
    /// </summary>
    bool HasValidContext { get; }
    
    /// <summary>
    /// Setează contextul pentru navigare la consultație nouă din programare
    /// </summary>
    void SetContext(Guid programareId, Guid pacientId);
    
    /// <summary>
    /// Setează contextul pentru editare consultație existentă
    /// </summary>
    void SetEditContext(Guid consultatieId, Guid programareId, Guid pacientId);
    
    /// <summary>
    /// Șterge contextul (după finalizare sau navigare away)
    /// </summary>
    void ClearContext();
}

/// <summary>
/// Implementare scoped - fiecare circuit Blazor are propria instanță
/// </summary>
public class ConsultatieNavigationService : IConsultatieNavigationService
{
    public Guid? ProgramareId { get; private set; }
    public Guid? PacientId { get; private set; }
    public Guid? ConsultatieId { get; private set; }
    
    public bool HasValidContext => ProgramareId.HasValue && PacientId.HasValue;
    
    public void SetContext(Guid programareId, Guid pacientId)
    {
        ProgramareId = programareId;
        PacientId = pacientId;
        ConsultatieId = null; // Consultație nouă
    }
    
    public void SetEditContext(Guid consultatieId, Guid programareId, Guid pacientId)
    {
        ConsultatieId = consultatieId;
        ProgramareId = programareId;
        PacientId = pacientId;
    }
    
    public void ClearContext()
    {
        ProgramareId = null;
        PacientId = null;
        ConsultatieId = null;
    }
}
