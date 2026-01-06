namespace ValyanClinic.Domain.Entities;

/// <summary>
/// Entitate pentru Examenul Obiectiv al unei Consultații
/// Relație 1:1 cu Consultatie
/// </summary>
public class ConsultatieExamenObiectiv
{
    // ==================== PRIMARY KEY ====================
    public Guid Id { get; set; }

    // ==================== FOREIGN KEY ====================
    public Guid ConsultatieID { get; set; }

    // ==================== EXAMEN GENERAL ====================
    public string? StareGenerala { get; set; }
    public string? Tegumente { get; set; }
    public string? Mucoase { get; set; }
    public string? GanglioniLimfatici { get; set; }
    public string? Edeme { get; set; }

    // ==================== SEMNE VITALE ====================
    public decimal? Greutate { get; set; } // kg
    public decimal? Inaltime { get; set; } // cm
    public decimal? IMC { get; set; } // calculat automat
    public decimal? Temperatura { get; set; } // °C
    public string? TensiuneArteriala { get; set; } // ex: 120/80 mmHg
    public int? Puls { get; set; } // bpm
    public int? FreccventaRespiratorie { get; set; } // /min
    public int? SaturatieO2 { get; set; } // %
    public decimal? Glicemie { get; set; } // mg/dL

    // ==================== EXAMEN OBIECTIV DETALIAT ====================
    /// <summary>Text liber pentru examen obiectiv detaliat</summary>
    public string? ExamenObiectivDetaliat { get; set; }
    /// <summary>Alte observații clinice</summary>
    public string? AlteObservatiiClinice { get; set; }

    // ==================== AUDIT ====================
    public DateTime DataCreare { get; set; }
    public Guid CreatDe { get; set; }
    public DateTime? DataUltimeiModificari { get; set; }
    public Guid? ModificatDe { get; set; }

    // ==================== NAVIGATION PROPERTIES ====================
    public virtual Consultatie? Consultatie { get; set; }

    // ==================== COMPUTED PROPERTIES ====================
    public decimal? IMCCalculat
    {
        get
        {
            if (Greutate.HasValue && Inaltime.HasValue && Inaltime > 0)
            {
                var inaltimeMetri = Inaltime.Value / 100;
                return Math.Round(Greutate.Value / (inaltimeMetri * inaltimeMetri), 2);
            }
            return null;
        }
    }

    public string InterpretareIMC
    {
        get
        {
            var imc = IMCCalculat ?? IMC;
            if (!imc.HasValue) return "N/A";

            return imc.Value switch
            {
                < 18.5m => "Subponderal",
                >= 18.5m and < 25m => "Normal",
                >= 25m and < 30m => "Supraponderal",
                >= 30m and < 35m => "Obezitate grad I",
                >= 35m and < 40m => "Obezitate grad II",
                >= 40m => "Obezitate morbida"
            };
        }
    }
}
