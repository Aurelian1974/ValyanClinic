using ValyanClinic.Application.DTOs;
using ValyanClinic.Application.Interfaces;
using ValyanClinic.Domain.Entities;
using ValyanClinic.Domain.Enums;

namespace ValyanClinic.Application.Services;

public class AppointmentService : IAppointmentService
{
    private readonly IAppointmentRepository _appointmentRepository;
    private readonly IPatientRepository _patientRepository;
    private readonly IDoctorRepository _doctorRepository;

    public AppointmentService(
        IAppointmentRepository appointmentRepository,
        IPatientRepository patientRepository,
        IDoctorRepository doctorRepository)
    {
        _appointmentRepository = appointmentRepository;
        _patientRepository = patientRepository;
        _doctorRepository = doctorRepository;
    }

    public async Task<IEnumerable<AppointmentDto>> GetAllAppointmentsAsync()
    {
        var appointments = await _appointmentRepository.GetAllAsync();
        return await MapToDtoListAsync(appointments);
    }

    public async Task<AppointmentDto?> GetAppointmentByIdAsync(int id)
    {
        var appointment = await _appointmentRepository.GetByIdAsync(id);
        return appointment != null ? await MapToDtoAsync(appointment) : null;
    }

    public async Task<IEnumerable<AppointmentDto>> GetAppointmentsByPatientIdAsync(int patientId)
    {
        var appointments = await _appointmentRepository.GetByPatientIdAsync(patientId);
        return await MapToDtoListAsync(appointments);
    }

    public async Task<IEnumerable<AppointmentDto>> GetAppointmentsByDoctorIdAsync(int doctorId)
    {
        var appointments = await _appointmentRepository.GetByDoctorIdAsync(doctorId);
        return await MapToDtoListAsync(appointments);
    }

    public async Task<IEnumerable<AppointmentDto>> GetTodayAppointmentsAsync()
    {
        var today = DateTime.Today;
        var appointments = await _appointmentRepository.GetByDateRangeAsync(today, today.AddDays(1));
        return await MapToDtoListAsync(appointments);
    }

    public async Task<AppointmentDto> CreateAppointmentAsync(CreateAppointmentDto createAppointmentDto)
    {
        // Validate time slot availability
        var isAvailable = await _appointmentRepository.IsTimeSlotAvailableAsync(
            createAppointmentDto.DoctorId,
            createAppointmentDto.AppointmentDate,
            createAppointmentDto.StartTime,
            createAppointmentDto.EndTime);

        if (!isAvailable)
            throw new InvalidOperationException("The selected time slot is not available.");

        var appointment = new Appointment
        {
            PatientId = createAppointmentDto.PatientId,
            DoctorId = createAppointmentDto.DoctorId,
            AppointmentDate = createAppointmentDto.AppointmentDate,
            StartTime = createAppointmentDto.StartTime,
            EndTime = createAppointmentDto.EndTime,
            Type = createAppointmentDto.Type,
            Reason = createAppointmentDto.Reason,
            Notes = createAppointmentDto.Notes,
            Cost = createAppointmentDto.Cost,
            Status = AppointmentStatus.Scheduled
        };

        var createdAppointment = await _appointmentRepository.CreateAsync(appointment);
        return await MapToDtoAsync(createdAppointment);
    }

    public async Task<bool> IsTimeSlotAvailableAsync(int doctorId, DateTime date, TimeSpan startTime, TimeSpan endTime)
    {
        return await _appointmentRepository.IsTimeSlotAvailableAsync(doctorId, date, startTime, endTime);
    }

    private async Task<AppointmentDto> MapToDtoAsync(Appointment appointment)
    {
        var patient = await _patientRepository.GetByIdAsync(appointment.PatientId);
        var doctor = await _doctorRepository.GetByIdAsync(appointment.DoctorId);

        return new AppointmentDto
        {
            Id = appointment.Id,
            PatientId = appointment.PatientId,
            PatientName = patient?.FullName ?? "Unknown Patient",
            DoctorId = appointment.DoctorId,
            DoctorName = doctor?.FullName ?? "Unknown Doctor",
            AppointmentDate = appointment.AppointmentDate,
            StartTime = appointment.StartTime,
            EndTime = appointment.EndTime,
            Type = appointment.Type,
            Status = appointment.Status,
            Reason = appointment.Reason,
            Notes = appointment.Notes,
            Cost = appointment.Cost
        };
    }

    private async Task<IEnumerable<AppointmentDto>> MapToDtoListAsync(IEnumerable<Appointment> appointments)
    {
        var result = new List<AppointmentDto>();
        foreach (var appointment in appointments)
        {
            result.Add(await MapToDtoAsync(appointment));
        }
        return result;
    }
}