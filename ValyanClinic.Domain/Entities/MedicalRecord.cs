using ValyanClinic.Domain.Common;
using ValyanClinic.Domain.Enums;

namespace ValyanClinic.Domain.Entities;

public class MedicalRecord : BaseEntity
{
    public int PatientId { get; set; }
    public int DoctorId { get; set; }
    public int? AppointmentId { get; set; }
    public MedicalRecordType Type { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? Diagnosis { get; set; }
    public string? Treatment { get; set; }
    public string? Prescription { get; set; }
    public DateTime RecordDate { get; set; } = DateTime.UtcNow;
    public string? Attachments { get; set; } // JSON array of file paths

    // Navigation properties
    public virtual Patient Patient { get; set; } = null!;
    public virtual Doctor Doctor { get; set; } = null!;
    public virtual Appointment? Appointment { get; set; }
}