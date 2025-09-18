using ValyanClinic.Domain.Models;
using ValyanClinic.Domain.Enums;
using ValyanClinic.Infrastructure.Repositories;
using ValyanClinic.Application.Validators;
using Microsoft.Extensions.Logging;

namespace ValyanClinic.Application.Services;

/// <summary>
/// Personal Service implementation cu business logic avansat și FluentValidation
/// Rich service conform best practices - nu doar forwarding
/// </summary>
public class PersonalService : IPersonalService
{
    private readonly IPersonalRepository _repository;
    private readonly IValidationService _validationService;
    private readonly ILogger<PersonalService> _logger;
    
    // Business rules constants
    private const int MIN_VARSTA = 16;
    private const int MAX_VARSTA = 80;
    private const int CNP_LENGTH = 13;

    public PersonalService(IPersonalRepository repository, IValidationService validationService, ILogger<PersonalService> logger)
    {
        _repository = repository;
        _validationService = validationService;
        _logger = logger;
    }

    public async Task<PersonalPagedResult> GetPersonalAsync(PersonalSearchRequest request)
    {
        try
        {
            _logger.LogInformation("Getting personal with search: {SearchText}, Dept: {Departament}, Status: {Status}", 
                request.SearchText, request.Departament, request.Status);

            var (data, totalCount) = await _repository.GetAllAsync(
                request.PageNumber,
                request.PageSize,
                request.SearchText,
                request.Departament,
                request.Status,
                request.SortColumn,
                request.SortDirection);

            var totalPages = (int)Math.Ceiling((double)totalCount / request.PageSize);

            return new PersonalPagedResult(
                data,
                totalCount,
                request.PageNumber,
                request.PageSize,
                totalPages);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting personal data");
            throw;
        }
    }

    public async Task<Personal?> GetPersonalByIdAsync(Guid id)
    {
        try
        {
            _logger.LogInformation("Getting personal by ID: {PersonalId}", id);
            return await _repository.GetByIdAsync(id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting personal by ID: {PersonalId}", id);
            throw;
        }
    }

    public async Task<PersonalResult> CreatePersonalAsync(Personal personal, string utilizator)
    {
        try
        {
            _logger.LogInformation("Creating personal: {Nume} {Prenume}", personal.Nume, personal.Prenume);

            // Business rules pentru crearea personalului (înainte de validare)
            personal = ApplyBusinessRulesForCreate(personal, utilizator);

            // Validare FluentValidation
            var validationResult = await _validationService.ValidateForCreateAsync(personal);
            if (!validationResult.IsValid)
            {
                _logger.LogWarning("Validation failed with {ErrorCount} errors: {Errors}", 
                    validationResult.Errors.Count, string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage)));
                return PersonalResult.ValidationFailure(validationResult.Errors.Select(e => e.ErrorMessage).ToList());
            }

            // Verificare unicitate
            var (cnpExists, codExists) = await _repository.CheckUniqueAsync(personal.CNP, personal.Cod_Angajat);
            
            if (cnpExists)
            {
                _logger.LogWarning("CNP already exists for personal creation: {CNP}", personal.CNP);
                return PersonalResult.Failure("CNP-ul există deja în sistem");
            }
            if (codExists)
            {
                _logger.LogWarning("Employee code already exists for personal creation: {CodAngajat}", personal.Cod_Angajat);
                return PersonalResult.Failure("Codul de angajat există deja în sistem");
            }

            var result = await _repository.CreateAsync(personal, utilizator);
            
            _logger.LogInformation("Personal created successfully: {PersonalId}", result?.Id_Personal);
            return PersonalResult.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating personal: {Nume} {Prenume}", personal.Nume, personal.Prenume);
            return PersonalResult.Failure($"Eroare la crearea personalului: {ex.Message}");
        }
    }

    public async Task<PersonalResult> UpdatePersonalAsync(Personal personal, string utilizator)
    {
        try
        {
            _logger.LogInformation("Updating personal: {PersonalId}", personal.Id_Personal);

            // Verificare existenta
            var existing = await _repository.GetByIdAsync(personal.Id_Personal);
            if (existing == null)
            {
                _logger.LogWarning("Personal not found for update: {PersonalId}", personal.Id_Personal);
                return PersonalResult.Failure("Personalul nu a fost găsit");
            }

            // Business rules pentru update (înainte de validare)
            personal = ApplyBusinessRulesForUpdate(personal, existing, utilizator);

            // Validare FluentValidation
            var validationResult = await _validationService.ValidateForUpdateAsync(personal);
            if (!validationResult.IsValid)
            {
                _logger.LogWarning("Update validation failed with {ErrorCount} errors: {Errors}", 
                    validationResult.Errors.Count, string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage)));
                return PersonalResult.ValidationFailure(validationResult.Errors.Select(e => e.ErrorMessage).ToList());
            }

            // Verificare unicitate (exclude ID-ul curent)
            var (cnpExists, codExists) = await _repository.CheckUniqueAsync(
                personal.CNP, 
                personal.Cod_Angajat, 
                personal.Id_Personal);
            
            if (cnpExists)
            {
                _logger.LogWarning("CNP already exists for personal update: {CNP}", personal.CNP);
                return PersonalResult.Failure("CNP-ul există deja în sistem");
            }
            if (codExists)
            {
                _logger.LogWarning("Employee code already exists for personal update: {CodAngajat}", personal.Cod_Angajat);
                return PersonalResult.Failure("Codul de angajat există deja în sistem");
            }

            var result = await _repository.UpdateAsync(personal, utilizator);
            
            _logger.LogInformation("Personal updated successfully: {PersonalId}", result?.Id_Personal);
            return PersonalResult.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating personal: {PersonalId}", personal.Id_Personal);
            return PersonalResult.Failure($"Eroare la actualizarea personalului: {ex.Message}");
        }
    }

    public async Task<PersonalResult> DeletePersonalAsync(Guid id, string utilizator)
    {
        try
        {
            _logger.LogInformation("Deleting personal: {PersonalId}", id);

            var existing = await _repository.GetByIdAsync(id);
            if (existing == null)
            {
                return PersonalResult.Failure("Personalul nu a fost găsit");
            }

            // Business rule: nu poti sterge personal cu status deja Inactiv
            if (existing.Status_Angajat == StatusAngajat.Inactiv)
            {
                return PersonalResult.Failure("Personalul este deja inactiv");
            }

            var success = await _repository.DeleteAsync(id, utilizator);
            if (!success)
            {
                return PersonalResult.Failure("Eroare la ștergerea personalului");
            }

            _logger.LogInformation("Personal deleted successfully: {PersonalId}", id);
            return PersonalResult.Success(existing);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting personal: {PersonalId}", id);
            return PersonalResult.Failure($"Eroare la ștergerea personalului: {ex.Message}");
        }
    }

    public async Task<PersonalStatistics> GetStatisticsAsync()
    {
        try
        {
            _logger.LogInformation("Getting personal statistics");

            var (total, activ, inactiv) = await _repository.GetStatisticsAsync();
            
            return new PersonalStatistics(total, activ, inactiv);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting personal statistics");
            throw;
        }
    }

    public async Task<PersonalDropdownOptions> GetDropdownOptionsAsync()
    {
        try
        {
            _logger.LogInformation("Getting dropdown options");

            var departamenteTask = _repository.GetDepartamenteAsync();
            var functiiTask = _repository.GetFunctiiAsync();
            var judeteTask = _repository.GetJudeteAsync();

            await Task.WhenAll(departamenteTask, functiiTask, judeteTask);

            var departamente = (await departamenteTask).Select(x => new DropdownItem(x.Value, x.Text));
            var functii = (await functiiTask).Select(x => new DropdownItem(x.Value, x.Text));
            var judete = (await judeteTask).Select(x => new DropdownItem(x.Value, x.Text));

            return new PersonalDropdownOptions(departamente, functii, judete);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting dropdown options");
            throw;
        }
    }

    public async Task<PersonalValidationResult> ValidatePersonalAsync(Personal personal, bool isUpdate = false)
    {
        try
        {
            _logger.LogInformation("Validating personal: {Nume} {Prenume} with CNP {CNP} (isUpdate: {IsUpdate})", 
                personal.Nume, personal.Prenume, personal.CNP, isUpdate);

            var validationResult = isUpdate 
                ? await _validationService.ValidateForUpdateAsync(personal)
                : await _validationService.ValidateForCreateAsync(personal);

            if (validationResult.IsValid)
            {
                _logger.LogInformation("Validation passed for {Nume} {Prenume}", personal.Nume, personal.Prenume);
                return PersonalValidationResult.Success();
            }
            else
            {
                var errors = validationResult.Errors.Select(e => e.ErrorMessage).ToArray();
                _logger.LogWarning("Validation failed for {Nume} {Prenume}: {Errors}", 
                    personal.Nume, personal.Prenume, string.Join(", ", errors));
                return PersonalValidationResult.Failure(errors);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception in ValidatePersonalAsync");
            return PersonalValidationResult.Failure("Eroare la validarea datelor");
        }
    }

    /// <summary>
    /// Generează următorul cod de angajat disponibil bazat pe pattern EMP001, EMP002, etc.
    /// </summary>
    public async Task<string> GetNextCodAngajatAsync()
    {
        try
        {
            _logger.LogInformation("Generating next employee code");
            return await _repository.GetNextCodAngajatAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating next employee code");
            
            // Fallback în caz de eroare
            return $"EMP{DateTime.Now:HHmmss}";
        }
    }

    #region Private Business Logic Methods

    private Personal ApplyBusinessRulesForCreate(Personal personal, string utilizator)
    {
        // Setare date sistem pentru crearea - folosește ora locală pentru consistență
        personal.Data_Crearii = DateTime.Now;
        personal.Data_Ultimei_Modificari = DateTime.Now;
        personal.Creat_De = utilizator;
        personal.Modificat_De = utilizator;

        // Business rule: personalul nou este mereu Activ
        personal.Status_Angajat = StatusAngajat.Activ;

        // Business rule: normalizeaza datele
        personal = NormalizePersonalData(personal);

        return personal;
    }

    private Personal ApplyBusinessRulesForUpdate(Personal personal, Personal existing, string utilizator)
    {
        // Pastrare date de creare originale
        personal.Data_Crearii = existing.Data_Crearii;
        personal.Creat_De = existing.Creat_De;

        // Setare date de modificare - folosește ora locală pentru consistență
        personal.Data_Ultimei_Modificari = DateTime.Now;
        personal.Modificat_De = utilizator;

        // Business rule: normalizeaza datele la update
        personal = NormalizePersonalData(personal);
        
        return personal;
    }

    private Personal NormalizePersonalData(Personal personal)
    {
        // Normalizare nume
        personal.Nume = NormalizeName(personal.Nume);
        personal.Prenume = NormalizeName(personal.Prenume);
        
        // Normalizare CNP și cod angajat
        personal.CNP = personal.CNP?.Trim() ?? string.Empty;
        personal.Cod_Angajat = personal.Cod_Angajat?.Trim().ToUpper() ?? string.Empty;

        // Normalizare email-uri
        if (!string.IsNullOrEmpty(personal.Email_Personal))
            personal.Email_Personal = personal.Email_Personal.Trim().ToLower();
        
        if (!string.IsNullOrEmpty(personal.Email_Serviciu))
            personal.Email_Serviciu = personal.Email_Serviciu.Trim().ToLower();

        return personal;
    }

    private static string NormalizeName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return name ?? string.Empty;

        name = name.Trim();
        if (string.IsNullOrEmpty(name))
            return name;

        // Prima literă mare, restul mici pentru fiecare cuvânt
        return string.Join(" ", name.Split(' ', StringSplitOptions.RemoveEmptyEntries)
            .Select(word => char.ToUpper(word[0]) + word[1..].ToLower()));
    }

    #endregion
}
