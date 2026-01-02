namespace ValyanClinic.Domain.Entities;

/// <summary>
/// Entitate pentru Analizele Medicale recomandate/efectuate în cadrul unei Consultații
/// Relație 1:N cu Consultatie - o consultație poate avea multiple analize
/// </summary>
public class ConsultatieAnalizaMedicala
{
    // ==================== PRIMARY KEY ====================
    public Guid Id { get; set; }

    // ==================== FOREIGN KEY ====================
    public Guid ConsultatieID { get; set; }

    // ==================== TIPUL ANALIZEI ====================
    public string TipAnaliza { get; set; } = string.Empty; // 'Laborator', 'Imagistica', 'EKG', 'Altele'
    public string NumeAnaliza { get; set; } = string.Empty;
    public string? CodAnaliza { get; set; }

    // ==================== STATUS ====================
    public string StatusAnaliza { get; set; } = "Recomandata"; 
    // 'Recomandata', 'Programata', 'In curs', 'Finalizata', 'Anulata'

    // ==================== DATE PROGRAMARE/EFECTUARE ====================
    public DateTime DataRecomandare { get; set; }
    public DateTime? DataProgramata { get; set; }
    public DateTime? DataEfectuare { get; set; }
    public string? LocEfectuare { get; set; }

    // ==================== PRIORITATE SI URGENTA ====================
    public string? Prioritate { get; set; } // 'Normala', 'Urgent', 'Foarte urgent'
    public bool EsteCito { get; set; } = false;

    // ==================== INDICATII CLINICE ====================
    public string? IndicatiiClinice { get; set; }
    public string? ObservatiiRecomandare { get; set; }

    // ==================== REZULTATE ====================
    public bool AreRezultate { get; set; } = false;
    public DateTime? DataRezultate { get; set; }
    public string? ValoareRezultat { get; set; }
    public string? UnitatiMasura { get; set; }
    public decimal? ValoareNormalaMin { get; set; }
    public decimal? ValoareNormalaMax { get; set; }
    public bool EsteInAfaraLimitelor { get; set; } = false;

    // ==================== INTERPRETARE MEDICALA ====================
    public string? InterpretareMedic { get; set; }
    public string? ConclusiiAnaliza { get; set; }

    // ==================== DOCUMENTE ATASATE ====================
    public string? CaleFisierRezultat { get; set; }
    public string? TipFisier { get; set; }
    public long? DimensiuneFisier { get; set; }

    // ==================== COSTURI ====================
    public decimal? Pret { get; set; }
    public bool Decontat { get; set; } = false;

    // ==================== LINK CATRE ALTE ENTITATI ====================
    public Guid? LaboratorID { get; set; }
    public Guid? MedicInterpretareID { get; set; }

    // ==================== AUDIT ====================
    public DateTime DataCreare { get; set; }
    public Guid CreatDe { get; set; }
    public DateTime? DataUltimeiModificari { get; set; }
    public Guid? ModificatDe { get; set; }

    // ==================== NAVIGATION PROPERTIES ====================
    public virtual Consultatie? Consultatie { get; set; }
    public virtual ICollection<ConsultatieAnalizaDetaliu>? Detalii { get; set; }
}
