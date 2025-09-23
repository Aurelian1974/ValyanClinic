using ValyanClinic.Application.DTOs;
using ValyanClinic.Application.Interfaces;
using ValyanClinic.Domain.Entities;

namespace ValyanClinic.Application.Services;

public class PatientService : IPatientService
{
    private readonly IPatientRepository _patientRepository;

    public PatientService(IPatientRepository patientRepository)
    {
        _patientRepository = patientRepository;
    }

    public async Task<IEnumerable<PatientDto>> GetAllPatientsAsync()
    {
        var patients = await _patientRepository.GetAllAsync();
        return patients.Select(MapToDto);
    }

    public async Task<PatientDto?> GetPatientByIdAsync(int id)
    {
        var patient = await _patientRepository.GetByIdAsync(id);
        return patient != null ? MapToDto(patient) : null;
    }

    public async Task<PatientDto?> GetPatientByCNPAsync(string cnp)
    {
        var patient = await _patientRepository.GetByCNPAsync(cnp);
        return patient != null ? MapToDto(patient) : null;
    }

    public async Task<IEnumerable<PatientDto>> SearchPatientsAsync(string searchTerm)
    {
        var patients = await _patientRepository.SearchAsync(searchTerm);
        return patients.Select(MapToDto);
    }

    public async Task<PatientDto> CreatePatientAsync(CreatePatientDto createPatientDto)
    {
        var patient = MapToEntity(createPatientDto);
        var createdPatient = await _patientRepository.CreateAsync(patient);
        return MapToDto(createdPatient);
    }

    public async Task<PatientDto> UpdatePatientAsync(int id, CreatePatientDto updatePatientDto)
    {
        var existingPatient = await _patientRepository.GetByIdAsync(id);
        if (existingPatient == null)
            throw new ArgumentException($"Patient with ID {id} not found");

        UpdateEntityFromDto(existingPatient, updatePatientDto);
        var updatedPatient = await _patientRepository.UpdateAsync(existingPatient);
        return MapToDto(updatedPatient);
    }

    public async Task<bool> DeletePatientAsync(int id)
    {
        return await _patientRepository.DeleteAsync(id);
    }

    private static PatientDto MapToDto(Patient patient)
    {
        return new PatientDto
        {
            Id = patient.Id,
            FirstName = patient.FirstName,
            LastName = patient.LastName,
            Email = patient.Email,
            PhoneNumber = patient.PhoneNumber,
            DateOfBirth = patient.DateOfBirth,
            Gender = patient.Gender,
            Address = patient.Address,
            CNP = patient.CNP,
            EmergencyContactName = patient.EmergencyContactName,
            EmergencyContactPhone = patient.EmergencyContactPhone,
            BloodType = patient.BloodType,
            Allergies = patient.Allergies,
            MedicalHistory = patient.MedicalHistory,
            Notes = patient.Notes,
            FullName = patient.FullName,
            Age = patient.Age
        };
    }

    private static Patient MapToEntity(CreatePatientDto dto)
    {
        return new Patient
        {
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            Email = dto.Email,
            PhoneNumber = dto.PhoneNumber,
            DateOfBirth = dto.DateOfBirth,
            Gender = dto.Gender,
            Address = dto.Address,
            CNP = dto.CNP,
            EmergencyContactName = dto.EmergencyContactName,
            EmergencyContactPhone = dto.EmergencyContactPhone,
            BloodType = dto.BloodType,
            Allergies = dto.Allergies,
            MedicalHistory = dto.MedicalHistory,
            Notes = dto.Notes
        };
    }

    private static void UpdateEntityFromDto(Patient patient, CreatePatientDto dto)
    {
        patient.FirstName = dto.FirstName;
        patient.LastName = dto.LastName;
        patient.Email = dto.Email;
        patient.PhoneNumber = dto.PhoneNumber;
        patient.DateOfBirth = dto.DateOfBirth;
        patient.Gender = dto.Gender;
        patient.Address = dto.Address;
        patient.CNP = dto.CNP;
        patient.EmergencyContactName = dto.EmergencyContactName;
        patient.EmergencyContactPhone = dto.EmergencyContactPhone;
        patient.BloodType = dto.BloodType;
        patient.Allergies = dto.Allergies;
        patient.MedicalHistory = dto.MedicalHistory;
        patient.Notes = dto.Notes;
        patient.UpdatedAt = DateTime.Now; // CORECTAT: folosește ora locală în loc de UtcNow
    }
}
