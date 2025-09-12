using ValyanClinic.Domain.Entities;

namespace ValyanClinic.Application.Interfaces;

public interface IPatientRepository
{
    Task<IEnumerable<Patient>> GetAllAsync();
    Task<Patient?> GetByIdAsync(int id);
    Task<Patient?> GetByCNPAsync(string cnp);
    Task<IEnumerable<Patient>> SearchAsync(string searchTerm);
    Task<Patient> CreateAsync(Patient patient);
    Task<Patient> UpdateAsync(Patient patient);
    Task<bool> DeleteAsync(int id);
    Task<bool> ExistsAsync(int id);
}

public interface IDoctorRepository
{
    Task<IEnumerable<Doctor>> GetAllAsync();
    Task<Doctor?> GetByIdAsync(int id);
    Task<IEnumerable<Doctor>> GetBySpecializationAsync(string specialization);
    Task<Doctor> CreateAsync(Doctor doctor);
    Task<Doctor> UpdateAsync(Doctor doctor);
    Task<bool> DeleteAsync(int id);
    Task<bool> ExistsAsync(int id);
}

public interface IAppointmentRepository
{
    Task<IEnumerable<Appointment>> GetAllAsync();
    Task<Appointment?> GetByIdAsync(int id);
    Task<IEnumerable<Appointment>> GetByPatientIdAsync(int patientId);
    Task<IEnumerable<Appointment>> GetByDoctorIdAsync(int doctorId);
    Task<IEnumerable<Appointment>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);
    Task<Appointment> CreateAsync(Appointment appointment);
    Task<Appointment> UpdateAsync(Appointment appointment);
    Task<bool> DeleteAsync(int id);
    Task<bool> IsTimeSlotAvailableAsync(int doctorId, DateTime date, TimeSpan startTime, TimeSpan endTime, int? excludeAppointmentId = null);
}

public interface IMedicalRecordRepository
{
    Task<IEnumerable<MedicalRecord>> GetAllAsync();
    Task<MedicalRecord?> GetByIdAsync(int id);
    Task<IEnumerable<MedicalRecord>> GetByPatientIdAsync(int patientId);
    Task<IEnumerable<MedicalRecord>> GetByDoctorIdAsync(int doctorId);
    Task<MedicalRecord> CreateAsync(MedicalRecord medicalRecord);
    Task<MedicalRecord> UpdateAsync(MedicalRecord medicalRecord);
    Task<bool> DeleteAsync(int id);
}

public interface IUserRepository
{
    Task<IEnumerable<User>> GetAllAsync();
    Task<User?> GetByIdAsync(int id);
    Task<User?> GetByEmailAsync(string email);
    Task<User> CreateAsync(User user);
    Task<User> UpdateAsync(User user);
    Task<bool> DeleteAsync(int id);
    Task<bool> ExistsAsync(int id);
}