using ValyanClinic.Application.DTOs;

namespace ValyanClinic.Application.Interfaces;

public interface IPatientService
{
    Task<IEnumerable<PatientDto>> GetAllPatientsAsync();
    Task<PatientDto?> GetPatientByIdAsync(int id);
    Task<PatientDto?> GetPatientByCNPAsync(string cnp);
    Task<IEnumerable<PatientDto>> SearchPatientsAsync(string searchTerm);
    Task<PatientDto> CreatePatientAsync(CreatePatientDto createPatientDto);
    Task<PatientDto> UpdatePatientAsync(int id, CreatePatientDto updatePatientDto);
    Task<bool> DeletePatientAsync(int id);
}

public interface IDoctorService
{
    Task<IEnumerable<DoctorDto>> GetAllDoctorsAsync();
    Task<DoctorDto?> GetDoctorByIdAsync(int id);
    Task<IEnumerable<DoctorDto>> GetDoctorsBySpecializationAsync(string specialization);
}

public interface IAppointmentService
{
    Task<IEnumerable<AppointmentDto>> GetAllAppointmentsAsync();
    Task<AppointmentDto?> GetAppointmentByIdAsync(int id);
    Task<IEnumerable<AppointmentDto>> GetAppointmentsByPatientIdAsync(int patientId);
    Task<IEnumerable<AppointmentDto>> GetAppointmentsByDoctorIdAsync(int doctorId);
    Task<IEnumerable<AppointmentDto>> GetTodayAppointmentsAsync();
    Task<AppointmentDto> CreateAppointmentAsync(CreateAppointmentDto createAppointmentDto);
    Task<bool> IsTimeSlotAvailableAsync(int doctorId, DateTime date, TimeSpan startTime, TimeSpan endTime);
}

public interface IDashboardService
{
    Task<DashboardStatsDto> GetDashboardStatsAsync();
}