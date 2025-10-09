namespace ValyanClinic.Application.Features.OcupatiiISCO.Queries.GetOcupatiiISCOList;

/// <summary>
/// DTO pentru listarea ocupațiilor ISCO-08
/// Optimizat pentru afișarea în grid-uri și UI
/// </summary>
public class OcupatieISCOListDto
{
    /// <summary>
    /// Identificatorul unic (GUID)
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Codul ISCO (1-4 cifre)
    /// </summary>
    public string CodISCO { get; set; } = string.Empty;

    /// <summary>
    /// Denumirea ocupației în română
    /// </summary>
    public string DenumireOcupatie { get; set; } = string.Empty;

    /// <summary>
    /// Denumirea ocupației în engleză
    /// </summary>
    public string? DenumireOcupatieEN { get; set; }

    /// <summary>
    /// Nivelul ierarhic (1-4)
    /// </summary>
    public byte NivelIerarhic { get; set; }

    /// <summary>
    /// Codul părinte în ierarhie
    /// </summary>
    public string? CodParinte { get; set; }

    /// <summary>
    /// Codul grupei majore
    /// </summary>
    public string? GrupaMajora { get; set; }

    /// <summary>
    /// Denumirea grupei majore
    /// </summary>
    public string? GrupaMajoraDenumire { get; set; }

    /// <summary>
    /// Descrierea ocupației
    /// </summary>
    public string? Descriere { get; set; }

    /// <summary>
    /// Starea (activ/inactiv)
    /// </summary>
    public bool EsteActiv { get; set; }

    /// <summary>
    /// Data creării
    /// </summary>
    public DateTime DataCrearii { get; set; }

    /// <summary>
    /// Utilizatorul care a creat înregistrarea
    /// </summary>
    public string? CreatDe { get; set; }

    // Computed properties pentru UI

    /// <summary>
    /// Formatarea scurtă a ID-ului GUID pentru afișare
    /// </summary>
    public string IdScurt => Id.ToString("N")[..8].ToUpper();

    /// <summary>
    /// Codul și denumirea concatenate pentru afișare
    /// </summary>
    public string CodSiDenumire => $"{CodISCO} - {DenumireOcupatie}";

    /// <summary>
    /// Numele nivelului ierarhic pentru afișare
    /// </summary>
    public string NumeNivelIerarhic => NivelIerarhic switch
    {
        1 => "Grupa Majoră",
        2 => "Subgrupa",
        3 => "Grupa Minoră",
        4 => "Ocupație",
        _ => "Necunoscut"
    };

    /// <summary>
    /// Indentarea pentru afișarea ierarhică
    /// </summary>
    public string IndentareIerarhica => new string(' ', (NivelIerarhic - 1) * 4);

    /// <summary>
    /// Verifică dacă este o grupă (nu ocupație finală)
    /// </summary>
    public bool EsteGrupa => NivelIerarhic < 4;

    /// <summary>
    /// Verifică dacă este ocupație finală
    /// </summary>
    public bool EsteOcupatieFinal => NivelIerarhic == 4;

    /// <summary>
    /// Status badge text pentru UI
    /// </summary>
    public string StatusText => EsteActiv ? "Activ" : "Inactiv";

    /// <summary>
    /// Status badge CSS class pentru UI
    /// </summary>
    public string StatusCssClass => EsteActiv ? "badge-success" : "badge-danger";
}
