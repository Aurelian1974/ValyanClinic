using ValyanClinic.Domain.Entities;

namespace ValyanClinic.Application.Interfaces;

// DOAR interfete pentru repository-urile ce vor fi IMPLEMENTATE
// Nu mai tinem placeholder-uri goale

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

// NOTE: IDoctorRepository, IAppointmentRepository, IMedicalRecordRepository, IUserRepository
// au fost eliminate deoarece erau doar placeholder-uri fara implementari.
// IPersonalRepository exista deja implementat in Infrastructure/Repositories.
