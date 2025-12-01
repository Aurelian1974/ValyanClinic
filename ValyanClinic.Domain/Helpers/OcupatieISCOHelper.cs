namespace ValyanClinic.Domain.Helpers;

/// <summary>
/// Helper class pentru computed properties si logica comuna ISCO-08
/// Elimina duplicarea de cod intre Entity si DTO
/// </summary>
public static class OcupatieISCOHelper
{
    /// <summary>
    /// Formateaza codul si denumirea concatenate
    /// </summary>
    public static string GetCodSiDenumire(string codISCO, string denumire)
        => $"{codISCO} - {denumire}";

    /// <summary>
    /// Returneaza numele descriptiv al nivelului ierarhic
    /// </summary>
    public static string GetNumeNivelIerarhic(byte nivel) => nivel switch
    {
        1 => "Grupa Majora",
        2 => "Subgrupa",
        3 => "Grupa Minora",
        4 => "Ocupatie",
        _ => "Necunoscut"
    };

    /// <summary>
    /// Calculeaza indentarea pentru afisarea ierarhica
    /// </summary>
    public static string GetIndentareIerarhica(byte nivel)
        => new string(' ', (nivel - 1) * 4);

    /// <summary>
    /// Verifica daca este o grupa (nu ocupatie finala)
    /// </summary>
    public static bool EsteGrupa(byte nivel) => nivel < 4;

    /// <summary>
    /// Verifica daca este ocupatie finala (nivel 4)
    /// </summary>
    public static bool EsteOcupatieFinal(byte nivel) => nivel == 4;

    /// <summary>
    /// Formateaza ID-ul GUID scurt (primele 8 caractere uppercase)
    /// </summary>
    public static string GetIdScurt(Guid id)
        => id.ToString("N")[..8].ToUpper();

    /// <summary>
    /// Returneaza text pentru status badge
    /// </summary>
    public static string GetStatusText(bool esteActiv)
        => esteActiv ? "Activ" : "Inactiv";

    /// <summary>
    /// Returneaza clasa CSS pentru status badge
    /// </summary>
    public static string GetStatusCssClass(bool esteActiv)
        => esteActiv ? "badge-success" : "badge-danger";
}
