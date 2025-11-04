namespace ValyanClinic.Application.Services;

/// <summary>
/// Interface pentru serviciu de business logic legat de Personal
/// Gestioneaza calcule complexe si validari care nu apartin UI-ului
/// </summary>
public interface IPersonalBusinessService
{
    /// <summary>
    /// Calculeaza varsta detaliata (ani, luni, zile) dintr-o data de nastere
    /// </summary>
    /// <param name="dataNasterii">Data nasterii persoanei</param>
    /// <returns>String formatat: "XX ani, XX luni, XX zile"</returns>
    string CalculeazaVarsta(DateTime dataNasterii);

    /// <summary>
    /// Calculeaza varsta detaliata din CNP
    /// </summary>
    /// <param name="cnp">CNP-ul persoanei (format: SAALLZZJJCCNNN)</param>
    /// <returns>String formatat: "XX ani, XX luni, XX zile" sau mesaj de eroare</returns>
    string CalculeazaVarstaFromCNP(string cnp);

    /// <summary>
    /// Valideaza un CNP conform algoritmului romanesc
    /// </summary>
    /// <param name="cnp">CNP-ul de validat</param>
    /// <returns>Tuplu cu (IsValid, ErrorMessage)</returns>
    (bool IsValid, string ErrorMessage) ValidateCNP(string cnp);

    /// <summary>
    /// Extrage data nasterii din CNP
    /// </summary>
    /// <param name="cnp">CNP-ul persoanei</param>
    /// <returns>Data nasterii sau null daca CNP invalid</returns>
    DateTime? ExtractDataNasteriiFromCNP(string cnp);

    /// <summary>
    /// Calculeaza timpul ramas pana la expirare (ani, luni, zile)
    /// </summary>
    /// <param name="dataExpirare">Data de expirare</param>
    /// <returns>String formatat cu timpul ramas sau mesaj expirat</returns>
    string CalculeazaExpiraIn(DateTime? dataExpirare);

    /// <summary>
    /// Determina clasa CSS pentru badge expirare
    /// </summary>
    /// <param name="dataExpirare">Data de expirare</param>
    /// <returns>Nume clasa CSS pentru badge</returns>
    string GetExpiraCssClass(DateTime? dataExpirare);
}
