namespace ValyanClinic.Application.DTOs.Investigatii;

/// <summary>
/// DTO pentru nomenclator investigații imagistice
/// </summary>
public class NomenclatorInvestigatieImagisticaDto
{
    public Guid Id { get; set; }
    public string Cod { get; set; } = string.Empty;
    public string Denumire { get; set; } = string.Empty;
    public string? Categorie { get; set; }
    public string? Descriere { get; set; }
    public int Ordine { get; set; }
}

/// <summary>
/// DTO pentru nomenclator explorări funcționale
/// </summary>
public class NomenclatorExplorareFuncDto
{
    public Guid Id { get; set; }
    public string Cod { get; set; } = string.Empty;
    public string Denumire { get; set; } = string.Empty;
    public string? Categorie { get; set; }
    public string? Descriere { get; set; }
    public int Ordine { get; set; }
}

/// <summary>
/// DTO pentru nomenclator endoscopii
/// </summary>
public class NomenclatorEndoscopieDto
{
    public Guid Id { get; set; }
    public string Cod { get; set; } = string.Empty;
    public string Denumire { get; set; } = string.Empty;
    public string? Categorie { get; set; }
    public string? Descriere { get; set; }
    public int Ordine { get; set; }
}

/// <summary>
/// DTO pentru investigație imagistică recomandată
/// </summary>
public class InvestigatieImagisticaRecomandataDto
{
    public Guid Id { get; set; }
    public Guid ConsultatieID { get; set; }
    public Guid? InvestigatieNomenclatorID { get; set; }
    public string DenumireInvestigatie { get; set; } = string.Empty;
    public string? CodInvestigatie { get; set; }
    public string? RegiuneAnatomica { get; set; }
    public DateTime DataRecomandare { get; set; }
    public string? Prioritate { get; set; }
    public bool EsteCito { get; set; }
    public string? IndicatiiClinice { get; set; }
    public string? ObservatiiMedic { get; set; }
    public string Status { get; set; } = "Recomandata";
}

/// <summary>
/// DTO pentru explorare funcțională recomandată
/// </summary>
public class ExplorareRecomandataDto
{
    public Guid Id { get; set; }
    public Guid ConsultatieID { get; set; }
    public Guid? ExplorareNomenclatorID { get; set; }
    public string DenumireExplorare { get; set; } = string.Empty;
    public string? CodExplorare { get; set; }
    public DateTime DataRecomandare { get; set; }
    public string? Prioritate { get; set; }
    public bool EsteCito { get; set; }
    public string? IndicatiiClinice { get; set; }
    public string? ObservatiiMedic { get; set; }
    public string Status { get; set; } = "Recomandata";
}

/// <summary>
/// DTO pentru endoscopie recomandată
/// </summary>
public class EndoscopieRecomandataDto
{
    public Guid Id { get; set; }
    public Guid ConsultatieID { get; set; }
    public Guid? EndoscopieNomenclatorID { get; set; }
    public string DenumireEndoscopie { get; set; } = string.Empty;
    public string? CodEndoscopie { get; set; }
    public DateTime DataRecomandare { get; set; }
    public string? Prioritate { get; set; }
    public bool EsteCito { get; set; }
    public string? IndicatiiClinice { get; set; }
    public string? ObservatiiMedic { get; set; }
    public string Status { get; set; } = "Recomandata";
}

/// <summary>
/// DTO pentru investigație imagistică efectuată
/// </summary>
public class InvestigatieImagisticaEfectuataDto
{
    public Guid Id { get; set; }
    public Guid? RecomandareID { get; set; }
    public Guid? ConsultatieID { get; set; }
    public Guid PacientID { get; set; }
    public Guid? InvestigatieNomenclatorID { get; set; }
    public string DenumireInvestigatie { get; set; } = string.Empty;
    public string? CodInvestigatie { get; set; }
    public string? RegiuneAnatomica { get; set; }
    public DateTime DataEfectuare { get; set; }
    public string? CentrulMedical { get; set; }
    public string? MedicExecutant { get; set; }
    public string? Rezultat { get; set; }
    public string? Concluzii { get; set; }
    public string? CaleFisierRezultat { get; set; }
}

/// <summary>
/// DTO pentru explorare funcțională efectuată
/// </summary>
public class ExplorareEfectuataDto
{
    public Guid Id { get; set; }
    public Guid? RecomandareID { get; set; }
    public Guid? ConsultatieID { get; set; }
    public Guid PacientID { get; set; }
    public Guid? ExplorareNomenclatorID { get; set; }
    public string DenumireExplorare { get; set; } = string.Empty;
    public string? CodExplorare { get; set; }
    public DateTime DataEfectuare { get; set; }
    public string? CentrulMedical { get; set; }
    public string? MedicExecutant { get; set; }
    public string? Rezultat { get; set; }
    public string? Concluzii { get; set; }
    public string? ParametriMasurati { get; set; }
    public string? CaleFisierRezultat { get; set; }
}

/// <summary>
/// DTO pentru endoscopie efectuată
/// </summary>
public class EndoscopieEfectuataDto
{
    public Guid Id { get; set; }
    public Guid? RecomandareID { get; set; }
    public Guid? ConsultatieID { get; set; }
    public Guid PacientID { get; set; }
    public Guid? EndoscopieNomenclatorID { get; set; }
    public string DenumireEndoscopie { get; set; } = string.Empty;
    public string? CodEndoscopie { get; set; }
    public DateTime DataEfectuare { get; set; }
    public string? CentrulMedical { get; set; }
    public string? MedicExecutant { get; set; }
    public string? Rezultat { get; set; }
    public string? Concluzii { get; set; }
    public string? BiopsiiPrelevate { get; set; }
    public string? RezultatHistopatologic { get; set; }
    public string? CaleFisierRezultat { get; set; }
}
