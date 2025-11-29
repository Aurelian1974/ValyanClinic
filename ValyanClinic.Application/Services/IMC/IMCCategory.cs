namespace ValyanClinic.Application.Services.IMC;

/// <summary>
/// Categorii IMC conform standardelor medicale OMS
/// </summary>
public enum IMCCategory
{
    /// <summary>
    /// Date invalide sau insuficiente pentru calcul
    /// </summary>
    Invalid = 0,
    
    /// <summary>
    /// IMC &lt; 18.5 - Subponderal
    /// </summary>
    Subponderal = 1,
    
    /// <summary>
    /// IMC 18.5 - 24.9 - Greutate normală
    /// </summary>
    Normal = 2,
    
    /// <summary>
    /// IMC 25.0 - 29.9 - Supraponderal
    /// </summary>
    Supraponderal = 3,
    
    /// <summary>
    /// IMC 30.0 - 34.9 - Obezitate grad I
    /// </summary>
    Obezitate1 = 4,
    
    /// <summary>
    /// IMC 35.0 - 39.9 - Obezitate grad II
    /// </summary>
    Obezitate2 = 5,
    
    /// <summary>
    /// IMC ≥ 40.0 - Obezitate morbidă (grad III)
    /// </summary>
    ObezitateMorbida = 6
}
