using ValyanClinic.Application.DTOs;

namespace ValyanClinic.Application.Interfaces;

// DOAR interfete pentru serviciile IMPLEMENTATE si FUNCTIONALE
// Nu mai tinem placeholder-uri goale

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

// NOTE: IDocorService, IAppointmentService, IDashboardService au fost eliminate
// deoarece erau doar placeholder-uri goale.
// Vor fi readaugate cand vor fi implementate cu functionalitate reala.
