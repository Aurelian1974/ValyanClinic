namespace ValyanClinic.Domain.Enums;

public enum Gender
{
    Male = 1,
    Female = 2,
    Other = 3
}

public enum AppointmentStatus
{
    Scheduled = 1,
    InProgress = 2,
    Completed = 3,
    Cancelled = 4,
    NoShow = 5
}

public enum AppointmentType
{
    Consultation = 1,
    FollowUp = 2,
    Surgery = 3,
    Emergency = 4,
    Vaccination = 5
}

public enum UserRole
{
    Administrator = 1,
    Doctor = 2,
    Nurse = 3,
    Receptionist = 4,
    Patient = 5
}

public enum MedicalRecordType
{
    Consultation = 1,
    Diagnosis = 2,
    Treatment = 3,
    Prescription = 4,
    LabResult = 5,
    ImagingResult = 6
}