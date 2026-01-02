namespace ValyanClinic.Domain.Entities;

/// <summary>
/// Entitate pentru Detaliile unei Analize Medicale (parametri individuali)
/// Relație 1:N cu ConsultatieAnalizaMedicala - o analiză poate avea multiple parametri
/// </summary>
public class ConsultatieAnalizaDetaliu
{
    // ==================== PRIMARY KEY ====================
    public Guid Id { get; set; }

    // ==================== FOREIGN KEY ====================
    public Guid AnalizaMedicalaID { get; set; }

    // ==================== PARAMETRU INDIVIDUAL ====================
    public string NumeParametru { get; set; } = string.Empty;
    public string? CodParametru { get; set; }

    // ==================== VALOARE ====================
    public string Valoare { get; set; } = string.Empty;
    public string? UnitatiMasura { get; set; }
    public string? TipValoare { get; set; } // 'Numeric', 'Text', 'Boolean'

    // ==================== LIMITE NORMALE ====================
    public decimal? ValoareNormalaMin { get; set; }
    public decimal? ValoareNormalaMax { get; set; }
    public string? ValoareNormalaText { get; set; }

    // ==================== STATUS ====================
    public bool EsteAnormal { get; set; } = false;
    public string? NivelGravitate { get; set; } // 'Normal', 'Usor crescut', 'Moderat crescut', 'Sever crescut'

    // ==================== INTERPRETARE ====================
    public string? Observatii { get; set; }

    // ==================== AUDIT ====================
    public DateTime DataCreare { get; set; }

    // ==================== NAVIGATION PROPERTIES ====================
    public virtual ConsultatieAnalizaMedicala? AnalizaMedicala { get; set; }
}
