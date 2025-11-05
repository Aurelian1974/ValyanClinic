namespace ValyanClinic.Application.Features.PacientPersonalMedicalManagement.DTOs;

/// <summary>
/// DTO pentru statistici generale despre relatiile pacient-doctor
/// </summary>
public class StatisticiRelatiiDto
{
    public int TotalRelatii { get; set; }
    public int RelatiiActive { get; set; }
    public int RelatiiInactive { get; set; }
    public int TotalDoctori { get; set; }
    public int DoctoriActivi { get; set; }
    public int TotalPacienti { get; set; }
    public int PacientiActivi { get; set; }
    public double? MediuZileAsociere { get; set; }
}
