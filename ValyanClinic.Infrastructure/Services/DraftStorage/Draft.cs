namespace ValyanClinic.Infrastructure.Services.DraftStorage;

/// <summary>
/// Model generic pentru draft-uri salvate în LocalStorage
/// </summary>
/// <typeparam name="T">Tipul datelor salvate în draft</typeparam>
public class Draft<T> where T : class
{
    /// <summary>
    /// ID-ul entității pentru care se salvează draft-ul
    /// </summary>
    public Guid EntityId { get; set; }

    /// <summary>
    /// ID-ul utilizatorului care a creat draft-ul
    /// </summary>
    public string UserId { get; set; } = string.Empty;

    /// <summary>
    /// Datele efectiv salvate în draft
    /// </summary>
    public T? Data { get; set; }

    /// <summary>
    /// Timestamp când a fost salvat draft-ul
    /// </summary>
    public DateTime SavedAt { get; set; }

    /// <summary>
    /// Versiunea draft-ului (pentru migrări viitoare)
    /// </summary>
    public int Version { get; set; } = 1;

    /// <summary>
    /// Metadata opțională (ex: user agent, IP, etc.)
    /// </summary>
    public Dictionary<string, string>? Metadata { get; set; }
}
