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

    // Computed properties pentru UI - folosim helper class

    /// <summary>
    /// Formatarea scurtă a ID-ului GUID pentru afișare
    /// </summary>
    public string IdScurt => ValyanClinic.Domain.Helpers.OcupatieISCOHelper.GetIdScurt(Id);

    /// <summary>
    /// Codul și denumirea concatenate pentru afișare
    /// </summary>
    public string CodSiDenumire => ValyanClinic.Domain.Helpers.OcupatieISCOHelper.GetCodSiDenumire(CodISCO, DenumireOcupatie);

    /// <summary>
    /// Numele nivelului ierarhic pentru afișare
    /// </summary>
    public string NumeNivelIerarhic => ValyanClinic.Domain.Helpers.OcupatieISCOHelper.GetNumeNivelIerarhic(NivelIerarhic);

    /// <summary>
    /// Indentarea pentru afișarea ierarhică
    /// </summary>
    public string IndentareIerarhica => ValyanClinic.Domain.Helpers.OcupatieISCOHelper.GetIndentareIerarhica(NivelIerarhic);

    /// <summary>
    /// Verifică dacă este o grupă (nu ocupație finală)
    /// </summary>
    public bool EsteGrupa => ValyanClinic.Domain.Helpers.OcupatieISCOHelper.EsteGrupa(NivelIerarhic);

    /// <summary>
    /// Verifică dacă este ocupație finală
    /// </summary>
    public bool EsteOcupatieFinal => ValyanClinic.Domain.Helpers.OcupatieISCOHelper.EsteOcupatieFinal(NivelIerarhic);

    /// <summary>
    /// Status badge text pentru UI
    /// </summary>
    public string StatusText => ValyanClinic.Domain.Helpers.OcupatieISCOHelper.GetStatusText(EsteActiv);

    /// <summary>
    /// Status badge CSS class pentru UI
    /// </summary>
    public string StatusCssClass => ValyanClinic.Domain.Helpers.OcupatieISCOHelper.GetStatusCssClass(EsteActiv);
}
