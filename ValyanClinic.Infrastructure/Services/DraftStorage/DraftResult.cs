namespace ValyanClinic.Infrastructure.Services.DraftStorage;

/// <summary>
/// Rezultatul operației de încărcare draft
/// </summary>
/// <typeparam name="T">Tipul datelor din draft</typeparam>
public class DraftResult<T> where T : class
{
    /// <summary>
    /// Indică dacă operația a avut succes
    /// </summary>
    public bool IsSuccess { get; set; }

    /// <summary>
    /// Datele din draft (null dacă draft-ul nu există sau e invalid)
    /// </summary>
    public T? Data { get; set; }

    /// <summary>
    /// Când a fost salvat draft-ul
    /// </summary>
    public DateTime? SavedAt { get; set; }

    /// <summary>
    /// Mesaj de eroare (dacă există)
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Tipul erorii
    /// </summary>
    public DraftErrorType ErrorType { get; set; }

    /// <summary>
    /// Draft încărcat cu succes
    /// </summary>
    public static DraftResult<T> Success(T data, DateTime savedAt) => new()
    {
        IsSuccess = true,
        Data = data,
        SavedAt = savedAt,
        ErrorType = DraftErrorType.None
    };

    /// <summary>
    /// Draft nu a fost găsit
    /// </summary>
    public static DraftResult<T> NotFound => new()
    {
        IsSuccess = false,
        ErrorMessage = "Draft not found",
        ErrorType = DraftErrorType.NotFound
    };

    /// <summary>
    /// Draft găsit dar datele sunt invalide (corrupt JSON, etc.)
    /// </summary>
    public static DraftResult<T> Invalid => new()
    {
        IsSuccess = false,
        ErrorMessage = "Invalid draft data",
        ErrorType = DraftErrorType.InvalidData
    };

    /// <summary>
    /// Draft expirat
    /// </summary>
    public static DraftResult<T> Expired => new()
    {
        IsSuccess = false,
        ErrorMessage = "Draft has expired",
        ErrorType = DraftErrorType.Expired
    };

    /// <summary>
    /// Eroare generică
    /// </summary>
    public static DraftResult<T> Error(string message) => new()
    {
        IsSuccess = false,
        ErrorMessage = message,
        ErrorType = DraftErrorType.Unknown
    };
}

/// <summary>
/// Tipuri de erori pentru draft-uri
/// </summary>
public enum DraftErrorType
{
    None,
    NotFound,
    InvalidData,
    Expired,
    StorageFull,
    Unknown
}
