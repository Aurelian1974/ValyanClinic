using Microsoft.Extensions.Logging;
using FluentValidation;
using ValyanClinic.Components.Pages.Models;
using ValyanClinic.Core.Common;
using ValyanClinic.Core.Services;
using ValyanClinic.Core.Exceptions;
using CoreValidationException = ValyanClinic.Core.Exceptions.ValidationException;

namespace ValyanClinic.Application.Services;

public interface IPatientService
{
    Task<Result<List<PatientListModel>>> GetPatientsAsync(PatientSearchRequest request);
    Task<Result<PatientListModel>> GetPatientByIdAsync(int id);
    Task<Result<int>> CreatePatientAsync(PatientListModel patient);
    Task<Result> UpdatePatientAsync(PatientListModel patient);
    Task<Result> DeletePatientAsync(int id);
    Task<Result<bool>> CheckCNPUniqueAsync(string cnp, int? excludeId = null);
}

public class PatientService : IPatientService
{
    private readonly ICacheService _cacheService;
    private readonly ILogger<PatientService> _logger;
    private readonly IValidator<PatientListModel> _patientCreateValidator;
    private readonly IValidator<PatientSearchRequest> _searchRequestValidator;
    
    // TODO: Add actual repository when implemented
    // private readonly IPatientRepository _patientRepository;

    public PatientService(
        ICacheService cacheService, 
        ILogger<PatientService> logger,
        IValidator<PatientListModel> patientCreateValidator,
        IValidator<PatientSearchRequest> searchRequestValidator)
    {
        _cacheService = cacheService;
        _logger = logger;
        _patientCreateValidator = patientCreateValidator;
        _searchRequestValidator = searchRequestValidator;
    }

    public async Task<Result<List<PatientListModel>>> GetPatientsAsync(PatientSearchRequest request)
    {
        try
        {
            // Validate search request
            var validationResult = await _searchRequestValidator.ValidateAsync(request);
            if (!validationResult.IsValid)
            {
                var errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
                return Result<List<PatientListModel>>.Failure(errors);
            }

            var cacheKey = $"patients_search_{request.SearchTerm}_{request.StatusFilter}_{request.PageNumber}_{request.PageSize}";
            
            var patients = await _cacheService.GetOrSetAsync(cacheKey, async () =>
            {
                // TODO: Replace with actual repository call
                return await SimulatePatientSearchAsync(request);
            }, TimeSpan.FromMinutes(5));

            _logger.LogInformation("Retrieved {Count} patients for search request", patients.Count);
            return Result<List<PatientListModel>>.Success(patients, $"Gasiti {patients.Count} pacienti");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving patients with request: {@Request}", request);
            return Result<List<PatientListModel>>.Failure("Eroare la cautarea pacientilor");
        }
    }

    public async Task<Result<PatientListModel>> GetPatientByIdAsync(int id)
    {
        try
        {
            if (id <= 0)
            {
                throw new CoreValidationException("ID-ul pacientului trebuie sa fie mai mare decat 0");
            }

            var cacheKey = $"patient_{id}";
            
            var patient = await _cacheService.GetOrSetAsync(cacheKey, async () =>
            {
                // TODO: Replace with actual repository call
                return await SimulateGetPatientByIdAsync(id);
            }, TimeSpan.FromMinutes(30));

            if (patient == null)
            {
                throw new NotFoundException("Pacient", id);
            }

            return Result<PatientListModel>.Success(patient);
        }
        catch (ValyanClinicException)
        {
            throw; // Re-throw custom exceptions
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving patient with ID: {PatientId}", id);
            return Result<PatientListModel>.Failure("Eroare la incarcarea pacientului");
        }
    }

    public async Task<Result<int>> CreatePatientAsync(PatientListModel patient)
    {
        try
        {
            // Validate patient data
            var validationResult = await _patientCreateValidator.ValidateAsync(patient);
            if (!validationResult.IsValid)
            {
                var errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
                throw new CoreValidationException(errors);
            }

            // Business Logic: Check CNP uniqueness
            var cnpCheck = await CheckCNPUniqueAsync(patient.CNP);
            if (!cnpCheck.IsSuccess || !cnpCheck.Value)
            {
                throw new BusinessRuleException("CNP_DUPLICATE", "CNP-ul este deja folosit de alt pacient");
            }

            // Business Logic: Validate age vs CNP
            var calculatedAge = CalculateAgeFromCNP(patient.CNP);
            if (Math.Abs(patient.Age - calculatedAge) > 1)
            {
                _logger.LogWarning("Age mismatch for CNP {CNP}: provided {ProvidedAge}, calculated {CalculatedAge}", 
                    patient.CNP, patient.Age, calculatedAge);
            }

            // TODO: Replace with actual repository call
            var newPatientId = await SimulateCreatePatientAsync(patient);

            // Clear relevant caches
            await _cacheService.RemovePatternAsync("patients_search");
            
            _logger.LogInformation("Created new patient with ID: {PatientId}, CNP: {CNP}", newPatientId, patient.CNP);
            return Result<int>.Success(newPatientId, "Pacient creat cu succes");
        }
        catch (ValyanClinicException)
        {
            throw; // Re-throw custom exceptions
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating patient: {@Patient}", patient);
            return Result<int>.Failure("Eroare la crearea pacientului");
        }
    }

    public async Task<Result> UpdatePatientAsync(PatientListModel patient)
    {
        try
        {
            // Validate patient data
            var validationResult = await _patientCreateValidator.ValidateAsync(patient);
            if (!validationResult.IsValid)
            {
                var errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
                throw new CoreValidationException(errors);
            }

            // Business Logic: Check CNP uniqueness (excluding current patient)
            var cnpCheck = await CheckCNPUniqueAsync(patient.CNP, patient.Id);
            if (!cnpCheck.IsSuccess || !cnpCheck.Value)
            {
                throw new BusinessRuleException("CNP_DUPLICATE", "CNP-ul este deja folosit de alt pacient");
            }

            // TODO: Replace with actual repository call
            await SimulateUpdatePatientAsync(patient);

            // Clear relevant caches
            await _cacheService.RemoveAsync($"patient_{patient.Id}");
            await _cacheService.RemovePatternAsync("patients_search");
            
            _logger.LogInformation("Updated patient with ID: {PatientId}", patient.Id);
            return Result.Success("Pacient actualizat cu succes");
        }
        catch (ValyanClinicException)
        {
            throw; // Re-throw custom exceptions
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating patient: {@Patient}", patient);
            return Result.Failure("Eroare la actualizarea pacientului");
        }
    }

    public async Task<Result> DeletePatientAsync(int id)
    {
        try
        {
            if (id <= 0)
            {
                throw new CoreValidationException("ID-ul pacientului trebuie sa fie mai mare decat 0");
            }

            // Business Logic: Check if patient has active appointments
            var hasActiveAppointments = await SimulateCheckActiveAppointmentsAsync(id);
            if (hasActiveAppointments)
            {
                throw new BusinessRuleException("ACTIVE_APPOINTMENTS", "Nu se poate sterge pacientul - are programari active");
            }

            // TODO: Replace with actual repository call (soft delete)
            await SimulateDeletePatientAsync(id);

            // Clear relevant caches
            await _cacheService.RemoveAsync($"patient_{id}");
            await _cacheService.RemovePatternAsync("patients_search");
            
            _logger.LogInformation("Deleted patient with ID: {PatientId}", id);
            return Result.Success("Pacient sters cu succes");
        }
        catch (ValyanClinicException)
        {
            throw; // Re-throw custom exceptions
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting patient with ID: {PatientId}", id);
            return Result.Failure("Eroare la stergerea pacientului");
        }
    }

    public async Task<Result<bool>> CheckCNPUniqueAsync(string cnp, int? excludeId = null)
    {
        try
        {
            if (string.IsNullOrEmpty(cnp))
            {
                throw new CoreValidationException("CNP-ul este obligatoriu");
            }

            // TODO: Replace with actual repository call
            var isUnique = await SimulateCheckCNPUniqueAsync(cnp, excludeId);
            return Result<bool>.Success(isUnique);
        }
        catch (ValyanClinicException)
        {
            throw; // Re-throw custom exceptions
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking CNP uniqueness: {CNP}", cnp);
            return Result<bool>.Failure("Eroare la verificarea CNP-ului");
        }
    }

    // Helper Methods for Business Logic
    private static int CalculateAgeFromCNP(string cnp)
    {
        if (string.IsNullOrEmpty(cnp) || cnp.Length != 13)
            return 0;

        try
        {
            var yearDigits = cnp.Substring(1, 2);
            var monthDigits = cnp.Substring(3, 2);
            var dayDigits = cnp.Substring(5, 2);

            var year = int.Parse(yearDigits);
            var month = int.Parse(monthDigits);
            var day = int.Parse(dayDigits);

            // Determine century based on first digit
            var firstDigit = int.Parse(cnp.Substring(0, 1));
            year += firstDigit switch
            {
                1 or 2 => 1900,
                3 or 4 => 1800,
                5 or 6 => 2000,
                _ => 1900
            };

            var birthDate = new DateTime(year, month, day);
            var age = DateTime.Now.Year - birthDate.Year;
            if (DateTime.Now.DayOfYear < birthDate.DayOfYear)
                age--;

            return age;
        }
        catch
        {
            return 0;
        }
    }

    // Simulation Methods (TODO: Replace with actual repository calls)
    private async Task<List<PatientListModel>> SimulatePatientSearchAsync(PatientSearchRequest request)
    {
        await Task.Delay(200); // Simulate database call
        
        var allPatients = GetSimulatedPatients();
        
        var filtered = allPatients.AsEnumerable();
        
        if (!string.IsNullOrEmpty(request.SearchTerm))
        {
            var searchLower = request.SearchTerm.ToLowerInvariant();
            filtered = filtered.Where(p => 
                p.FullName.ToLowerInvariant().Contains(searchLower) ||
                p.Email.ToLowerInvariant().Contains(searchLower) ||
                p.Phone.Contains(request.SearchTerm) ||
                p.CNP.Contains(request.SearchTerm));
        }

        if (request.StatusFilter != FilterOptions.All)
        {
            var status = request.StatusFilter == FilterOptions.Active ? PatientStatus.Active : PatientStatus.Inactive;
            filtered = filtered.Where(p => p.Status == status);
        }

        return filtered
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToList();
    }

    private async Task<PatientListModel?> SimulateGetPatientByIdAsync(int id)
    {
        await Task.Delay(100);
        return GetSimulatedPatients().FirstOrDefault(p => p.Id == id);
    }

    private async Task<int> SimulateCreatePatientAsync(PatientListModel patient)
    {
        await Task.Delay(300);
        return Random.Shared.Next(1000, 9999);
    }

    private async Task SimulateUpdatePatientAsync(PatientListModel patient)
    {
        await Task.Delay(200);
    }

    private async Task SimulateDeletePatientAsync(int id)
    {
        await Task.Delay(150);
    }

    private async Task<bool> SimulateCheckActiveAppointmentsAsync(int patientId)
    {
        await Task.Delay(100);
        return patientId == 1; // Simulate that patient with ID 1 has active appointments
    }

    private async Task<bool> SimulateCheckCNPUniqueAsync(string cnp, int? excludeId)
    {
        await Task.Delay(100);
        var existingPatients = GetSimulatedPatients();
        return !existingPatients.Any(p => p.CNP == cnp && p.Id != excludeId);
    }

    private static List<PatientListModel> GetSimulatedPatients()
    {
        return new List<PatientListModel>
        {
            new() { 
                Id = 1, 
                FullName = "Maria Popescu", 
                Phone = "0721-123-456", 
                Email = "maria.popescu@email.com", 
                LastVisit = DateTime.Now.AddDays(-5),
                Status = PatientStatus.Active,
                Age = 34,
                CNP = "2850615123456"
            },
            new() { 
                Id = 2, 
                FullName = "Ion Ionescu", 
                Phone = "0722-654-321", 
                Email = "ion.ionescu@email.com", 
                LastVisit = DateTime.Now.AddDays(-12),
                Status = PatientStatus.Active,
                Age = 45,
                CNP = "1780312654321"
            },
            new() { 
                Id = 3, 
                FullName = "Ana Georgescu", 
                Phone = "0723-789-012", 
                Email = "ana.georgescu@email.com", 
                LastVisit = DateTime.Now.AddDays(-3),
                Status = PatientStatus.Inactive,
                Age = 28,
                CNP = "2950825789012"
            },
            new() { 
                Id = 4, 
                FullName = "Mihai Marinescu", 
                Phone = "0724-345-678", 
                Email = "mihai.marinescu@email.com", 
                LastVisit = DateTime.Now.AddDays(-8),
                Status = PatientStatus.Active,
                Age = 52,
                CNP = "1720504345678"
            },
            new() { 
                Id = 5, 
                FullName = "Elena Ionescu", 
                Phone = "0725-987-654", 
                Email = "elena.ionescu@email.com", 
                LastVisit = DateTime.Now.AddDays(-25),
                Status = PatientStatus.Active,
                Age = 29,
                CNP = "2940312987654"
            }
        };
    }
}