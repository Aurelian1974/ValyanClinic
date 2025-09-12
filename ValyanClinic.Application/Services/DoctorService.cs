using ValyanClinic.Application.DTOs;
using ValyanClinic.Application.Interfaces;
using ValyanClinic.Domain.Entities;

namespace ValyanClinic.Application.Services;

public class DoctorService : IDoctorService
{
    private readonly IDoctorRepository _doctorRepository;

    public DoctorService(IDoctorRepository doctorRepository)
    {
        _doctorRepository = doctorRepository;
    }

    public async Task<IEnumerable<DoctorDto>> GetAllDoctorsAsync()
    {
        var doctors = await _doctorRepository.GetAllAsync();
        return doctors.Select(MapToDto);
    }

    public async Task<DoctorDto?> GetDoctorByIdAsync(int id)
    {
        var doctor = await _doctorRepository.GetByIdAsync(id);
        return doctor != null ? MapToDto(doctor) : null;
    }

    public async Task<IEnumerable<DoctorDto>> GetDoctorsBySpecializationAsync(string specialization)
    {
        var doctors = await _doctorRepository.GetBySpecializationAsync(specialization);
        return doctors.Select(MapToDto);
    }

    private static DoctorDto MapToDto(Doctor doctor)
    {
        return new DoctorDto
        {
            Id = doctor.Id,
            FirstName = doctor.FirstName,
            LastName = doctor.LastName,
            Email = doctor.Email,
            PhoneNumber = doctor.PhoneNumber,
            Specialization = doctor.Specialization,
            LicenseNumber = doctor.LicenseNumber,
            LicenseExpiryDate = doctor.LicenseExpiryDate,
            Bio = doctor.Bio,
            ConsultationFee = doctor.ConsultationFee,
            IsActive = doctor.IsActive,
            FullName = doctor.FullName
        };
    }
}