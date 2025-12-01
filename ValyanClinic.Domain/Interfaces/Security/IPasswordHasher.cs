namespace ValyanClinic.Domain.Interfaces.Security;

/// <summary>
/// Interface pentru hashing și verificare parole folosind BCrypt
/// </summary>
public interface IPasswordHasher
{
    /// <summary>
    /// Hash-uiește o parolă folosind BCrypt
    /// Generează automat salt-ul și îl include în hash
    /// </summary>
    /// <param name="password">Parola în plain text</param>
    /// <returns>Hash-ul BCrypt (include salt-ul)</returns>
    string HashPassword(string password);

    /// <summary>
    /// Verifică dacă o parolă corespunde unui hash BCrypt
    /// </summary>
    /// <param name="password">Parola în plain text</param>
    /// <param name="hash">Hash-ul BCrypt de verificat</param>
    /// <returns>True dacă parola este corectă</returns>
    bool VerifyPassword(string password, string hash);

    /// <summary>
    /// Generează o parolă aleatoare sigură
    /// </summary>
    /// <param name="length">Lungimea parolei (default 12)</param>
    /// <param name="includeSpecialChars">Include caractere speciale</param>
    /// <returns>Parola generată</returns>
    string GenerateRandomPassword(int length = 12, bool includeSpecialChars = true);
}
