using ValyanClinic.Domain.Common;
using ValyanClinic.Domain.Enums;

namespace ValyanClinic.Domain.Entities;

public class Appointment : BaseEntity
{
    public int PatientId { get; set; }
    public int DoctorId { get; set; }
    public DateTime AppointmentDate { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public AppointmentType Type { get; set; }
    public AppointmentStatus Status { get; set; } = AppointmentStatus.Scheduled;
    public string? Reason { get; set; }
    public string? Notes { get; set; }
    public decimal? Cost { get; set; }

    // Navigation properties
    public virtual Patient Patient { get; set; } = null!;
    public virtual Doctor Doctor { get; set; } = null!;

    public DateTime AppointmentDateTime => AppointmentDate.Date.Add(StartTime);
    public TimeSpan Duration => EndTime - StartTime;
}