using ValyanClinic.Application.Authorization;

namespace ValyanClinic.Application.Interfaces;

/// <summary>
/// Serviciu pentru verificarea permisiunilor la nivel de câmp (input).
/// Permite granulare fină a accesului: un utilizator poate avea permisiunea 
/// de a edita un pacient, dar nu și de a modifica CNP-ul.
/// </summary>
public interface IFieldPermissionService
{
    /// <summary>
    /// Încarcă permisiunile pentru rolul curent din baza de date.
    /// Trebuie apelat o singură dată per request/component lifecycle.
    /// </summary>
    Task LoadPermissionsAsync(string roleDenumire);
    
    /// <summary>
    /// Verifică dacă utilizatorul are o permisiune specifică.
    /// </summary>
    /// <param name="permission">Codul permisiunii (ex: "Pacient.Edit.CNP")</param>
    /// <returns>True dacă are permisiunea</returns>
    bool HasPermission(string permission);
    
    /// <summary>
    /// Verifică dacă utilizatorul poate vizualiza un câmp.
    /// Verifică mai întâi permisiunea specifică, apoi fallback la permisiunea generală.
    /// </summary>
    /// <param name="entity">Entitatea (ex: "Pacient")</param>
    /// <param name="fieldName">Numele câmpului (ex: "CNP")</param>
    /// <returns>True dacă poate vizualiza</returns>
    bool CanViewField(string entity, string fieldName);
    
    /// <summary>
    /// Verifică dacă utilizatorul poate edita un câmp.
    /// Verifică mai întâi permisiunea specifică de câmp, apoi fallback la permisiunea generală de editare.
    /// </summary>
    /// <param name="entity">Entitatea (ex: "Pacient")</param>
    /// <param name="fieldName">Numele câmpului (ex: "CNP")</param>
    /// <returns>True dacă poate edita</returns>
    bool CanEditField(string entity, string fieldName);
    
    /// <summary>
    /// Verifică dacă utilizatorul are cel puțin una din permisiunile specificate.
    /// </summary>
    bool HasAnyPermission(params string[] permissions);
    
    /// <summary>
    /// Verifică dacă utilizatorul are toate permisiunile specificate.
    /// </summary>
    bool HasAllPermissions(params string[] permissions);
    
    /// <summary>
    /// Returnează toate permisiunile utilizatorului curent.
    /// </summary>
    IReadOnlyList<string> GetCurrentPermissions();
    
    /// <summary>
    /// Returnează starea unui câmp pentru UI (vizibil, readonly, editable).
    /// </summary>
    FieldState GetFieldState(string entity, string fieldName, bool isEditMode);
}

/// <summary>
/// Starea unui câmp pentru controlul UI
/// </summary>
public enum FieldState
{
    /// <summary>Câmpul nu este vizibil deloc</summary>
    Hidden,
    
    /// <summary>Câmpul este vizibil dar readonly</summary>
    ReadOnly,
    
    /// <summary>Câmpul este editabil</summary>
    Editable
}

/// <summary>
/// Informații complete despre starea unui câmp
/// </summary>
public record FieldPermissionInfo(
    string FieldName,
    FieldState State,
    bool CanView,
    bool CanEdit,
    string? DisabledReason = null
);

/// <summary>
/// Context transmis către UI components pentru field rendering
/// </summary>
public record FieldContext(FieldState State, bool IsReadOnly);
