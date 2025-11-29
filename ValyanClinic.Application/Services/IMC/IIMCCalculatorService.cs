namespace ValyanClinic.Application.Services.IMC;

/// <summary>
/// Serviciu pentru calculul și interpretarea Indicelui de Masă Corporală (IMC/BMI)
/// Implementează standardele Organizației Mondiale a Sănătății (OMS/WHO)
/// </summary>
public interface IIMCCalculatorService
{
    /// <summary>
    /// Calculează IMC bazat pe greutate și înălțime
    /// </summary>
    /// <param name="greutate">Greutatea în kilograme</param>
    /// <param name="inaltime">Înălțimea în centimetri</param>
    /// <returns>Rezultat complet cu valoare, categorie și interpretare</returns>
    IMCResult Calculate(decimal greutate, decimal inaltime);
    
    /// <summary>
    /// Verifică dacă valorile sunt valide pentru calcul
    /// </summary>
    /// <param name="greutate">Greutatea în kilograme</param>
    /// <param name="inaltime">Înălțimea în centimetri</param>
    /// <returns>True dacă valorile sunt în limite rezonabile</returns>
    bool AreValuesValid(decimal greutate, decimal inaltime);
    
    /// <summary>
    /// Calculează greutatea ideală bazată pe înălțime (formula Lorentz)
    /// </summary>
    /// <param name="inaltime">Înălțimea în centimetri</param>
    /// <param name="sex">Sex (M/F)</param>
    /// <returns>Greutatea ideală în kilograme</returns>
    decimal CalculateIdealWeight(decimal inaltime, string sex);
}
