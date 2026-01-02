namespace ValyanClinic.Application.Features.ConsultatieManagement.DTOs;

/// <summary>
/// DTO pentru Examenul Obiectiv al unei Consultații
/// </summary>
public class ConsultatieExamenObiectivDto
{
    public Guid? Id { get; set; }
    public Guid ConsultatieID { get; set; }
    
    // Examen General
    public string? StareGenerala { get; set; }
    public string? Constitutie { get; set; }
    public string? Atitudine { get; set; }
    public string? Facies { get; set; }
    public string? Tegumente { get; set; }
    public string? Mucoase { get; set; }
    public string? GangliniLimfatici { get; set; }
    public string? Edeme { get; set; }
    
    // Semne Vitale
    public decimal? Greutate { get; set; }
    public decimal? Inaltime { get; set; }
    public decimal? IMC { get; set; }
    public decimal? Temperatura { get; set; }
    public string? TensiuneArteriala { get; set; }
    public int? Puls { get; set; }
    public int? FreccventaRespiratorie { get; set; }
    public int? SaturatieO2 { get; set; }
    public decimal? Glicemie { get; set; }
    
    // Examen pe Aparate/Sisteme
    public string? ExamenCardiovascular { get; set; }
    public string? ExamenRespiratoriu { get; set; }
    public string? ExamenDigestiv { get; set; }
    public string? ExamenUrinar { get; set; }
    public string? ExamenNervos { get; set; }
    public string? ExamenLocomotor { get; set; }
    public string? ExamenEndocrin { get; set; }
    public string? ExamenORL { get; set; }
    public string? ExamenOftalmologic { get; set; }
    public string? ExamenDermatologic { get; set; }
    
    // Computed Properties
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
    
    // Audit
    public DateTime DataCreare { get; set; }
    public Guid CreatDe { get; set; }
    public DateTime? DataUltimeiModificari { get; set; }
    public Guid? ModificatDe { get; set; }
}
