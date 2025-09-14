using ValyanClinic.Domain.Models;
using ValyanClinic.Domain.Enums;
using ValyanClinic.Infrastructure.Repositories;
using Microsoft.Extensions.Logging;
using System.Text.RegularExpressions;

namespace ValyanClinic.Application.Services;

/// <summary>
/// Personal Service implementation cu business logic avansat
/// Rich service conform best practices - nu doar forwarding
/// </summary>
public class PersonalService : IPersonalService
{
    private readonly IPersonalRepository _repository;
    private readonly ILogger<PersonalService> _logger;
    
    // Business rules constants
    private const int MIN_VARSTA = 16;
    private const int MAX_VARSTA = 80;
    private const int CNP_LENGTH = 13;

    public PersonalService(IPersonalRepository repository, ILogger<PersonalService> logger)
    {
        _repository = repository;
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

            // Validare business logic
            var validationResult = await ValidatePersonalAsync(personal, isUpdate: false);
            if (!validationResult.IsValid)
            {
                return PersonalResult.ValidationFailure(validationResult.Errors);
            }

            // Business rules pentru crearea personalului
            personal = ApplyBusinessRulesForCreate(personal, utilizator);

            // Verificare unicitate
            var (cnpExists, codExists) = await _repository.CheckUniqueAsync(personal.CNP, personal.Cod_Angajat);
            if (cnpExists)
            {
                return PersonalResult.Failure("CNP-ul exista deja in sistem");
            }
            if (codExists)
            {
                return PersonalResult.Failure("Codul de angajat exista deja in sistem");
            }

            var result = await _repository.CreateAsync(personal, utilizator);
            
            _logger.LogInformation("Personal created successfully: {PersonalId}", result.Id_Personal);
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
                return PersonalResult.Failure("Personalul nu a fost gasit");
            }

            // Validare business logic
            var validationResult = await ValidatePersonalAsync(personal, isUpdate: true);
            if (!validationResult.IsValid)
            {
                return PersonalResult.ValidationFailure(validationResult.Errors);
            }

            // Business rules pentru update
            personal = ApplyBusinessRulesForUpdate(personal, existing, utilizator);

            // Verificare unicitate (exclude ID-ul curent)
            var (cnpExists, codExists) = await _repository.CheckUniqueAsync(
                personal.CNP, 
                personal.Cod_Angajat, 
                personal.Id_Personal);
            
            if (cnpExists)
            {
                return PersonalResult.Failure("CNP-ul exista deja in sistem");
            }
            if (codExists)
            {
                return PersonalResult.Failure("Codul de angajat exista deja in sistem");
            }

            var result = await _repository.UpdateAsync(personal, utilizator);
            
            _logger.LogInformation("Personal updated successfully: {PersonalId}", result.Id_Personal);
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
                return PersonalResult.Failure("Personalul nu a fost gasit");
            }

            // Business rule: nu poti sterge personal cu status deja Inactiv
            if (existing.Status_Angajat == StatusAngajat.Inactiv)
            {
                return PersonalResult.Failure("Personalul este deja inactiv");
            }

            var success = await _repository.DeleteAsync(id, utilizator);
            if (!success)
            {
                return PersonalResult.Failure("Eroare la stergerea personalului");
            }

            _logger.LogInformation("Personal deleted successfully: {PersonalId}", id);
            return PersonalResult.Success(existing);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting personal: {PersonalId}", id);
            return PersonalResult.Failure($"Eroare la stergerea personalului: {ex.Message}");
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

    public async Task<ValidationResult> ValidatePersonalAsync(Personal personal, bool isUpdate = false)
    {
        var result = new ValidationResult();

        try
        {
            // Validari de baza
            if (string.IsNullOrWhiteSpace(personal.Nume))
                result.AddError("Numele este obligatoriu");

            if (string.IsNullOrWhiteSpace(personal.Prenume))
                result.AddError("Prenumele este obligatoriu");

            if (string.IsNullOrWhiteSpace(personal.CNP))
                result.AddError("CNP-ul este obligatoriu");
            else if (personal.CNP.Length != CNP_LENGTH)
                result.AddError($"CNP-ul trebuie sa aiba exact {CNP_LENGTH} cifre");
            else if (!IsValidCNP(personal.CNP))
                result.AddError("CNP-ul nu este valid");

            if (string.IsNullOrWhiteSpace(personal.Cod_Angajat))
                result.AddError("Codul de angajat este obligatoriu");

            if (string.IsNullOrWhiteSpace(personal.Functia))
                result.AddError("Functia este obligatorie");

            if (string.IsNullOrWhiteSpace(personal.Adresa_Domiciliu))
                result.AddError("Adresa de domiciliu este obligatorie");

            if (string.IsNullOrWhiteSpace(personal.Judet_Domiciliu))
                result.AddError("Judetul de domiciliu este obligatoriu");

            if (string.IsNullOrWhiteSpace(personal.Oras_Domiciliu))
                result.AddError("Orasul de domiciliu este obligatoriu");

            // Validari varsta
            var varsta = CalculateAge(personal.Data_Nasterii);
            if (varsta < MIN_VARSTA)
                result.AddError($"Varsta minima este {MIN_VARSTA} ani");
            else if (varsta > MAX_VARSTA)
                result.AddError($"Varsta maxima este {MAX_VARSTA} ani");

            // Validari email
            if (!string.IsNullOrEmpty(personal.Email_Personal) && !IsValidEmail(personal.Email_Personal))
                result.AddError("Email-ul personal nu este valid");

            if (!string.IsNullOrEmpty(personal.Email_Serviciu) && !IsValidEmail(personal.Email_Serviciu))
                result.AddError("Email-ul de serviciu nu este valid");

            // Validari telefon
            if (!string.IsNullOrEmpty(personal.Telefon_Personal) && !IsValidPhoneNumber(personal.Telefon_Personal))
                result.AddError("Numarul de telefon personal nu este valid");

            if (!string.IsNullOrEmpty(personal.Telefon_Serviciu) && !IsValidPhoneNumber(personal.Telefon_Serviciu))
                result.AddError("Numarul de telefon de serviciu nu este valid");

            // Validari business specific
            if (personal.Departament == null)
                result.AddError("Departamentul este obligatoriu");

            // Validari CI
            if (personal.Data_Eliberare_CI.HasValue && personal.Valabil_CI_Pana.HasValue)
            {
                if (personal.Data_Eliberare_CI >= personal.Valabil_CI_Pana)
                    result.AddError("Data eliberarii CI trebuie sa fie anterioara datei de expirare");
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating personal");
            result.AddError("Eroare la validarea datelor");
            return result;
        }
    }

    #region Private Business Logic Methods

    private Personal ApplyBusinessRulesForCreate(Personal personal, string utilizator)
    {
        // Setare date sistem pentru crearea
        personal.Data_Crearii = DateTime.UtcNow;
        personal.Data_Ultimei_Modificari = DateTime.UtcNow;
        personal.Creat_De = utilizator;
        personal.Modificat_De = utilizator;

        // Business rule: personalul nou este mereu Activ
        personal.Status_Angajat = StatusAngajat.Activ;

        // Business rule: normalizeaza datele
        personal.Nume = NormalizeName(personal.Nume);
        personal.Prenume = NormalizeName(personal.Prenume);
        personal.CNP = personal.CNP.Trim();
        personal.Cod_Angajat = personal.Cod_Angajat.Trim().ToUpper();

        // Business rule: email lowercase
        if (!string.IsNullOrEmpty(personal.Email_Personal))
            personal.Email_Personal = personal.Email_Personal.Trim().ToLower();
        
        if (!string.IsNullOrEmpty(personal.Email_Serviciu))
            personal.Email_Serviciu = personal.Email_Serviciu.Trim().ToLower();

        return personal;
    }

    private Personal ApplyBusinessRulesForUpdate(Personal personal, Personal existing, string utilizator)
    {
        // Pastrare date de creare originale
        personal.Data_Crearii = existing.Data_Crearii;
        personal.Creat_De = existing.Creat_De;

        // Setare date de modificare
        personal.Data_Ultimei_Modificari = DateTime.UtcNow;
        personal.Modificat_De = utilizator;

        // Business rule: normalizeaza datele la update
        personal.Nume = NormalizeName(personal.Nume);
        personal.Prenume = NormalizeName(personal.Prenume);
        personal.CNP = personal.CNP.Trim();
        personal.Cod_Angajat = personal.Cod_Angajat.Trim().ToUpper();

        // Business rule: email lowercase
        if (!string.IsNullOrEmpty(personal.Email_Personal))
            personal.Email_Personal = personal.Email_Personal.Trim().ToLower();
        
        if (!string.IsNullOrEmpty(personal.Email_Serviciu))
            personal.Email_Serviciu = personal.Email_Serviciu.Trim().ToLower();

        return personal;
    }

    private static string NormalizeName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return name;

        // Prima litera mare, restul mici
        return char.ToUpper(name.Trim()[0]) + name.Trim()[1..].ToLower();
    }

    private static int CalculateAge(DateTime birthDate)
    {
        var today = DateTime.Today;
        var age = today.Year - birthDate.Year;
        if (birthDate.Date > today.AddYears(-age))
            age--;
        return age;
    }

    private static bool IsValidEmail(string email)
    {
        try
        {
            var regex = new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.IgnoreCase);
            return regex.IsMatch(email);
        }
        catch
        {
            return false;
        }
    }

    private static bool IsValidPhoneNumber(string phone)
    {
        if (string.IsNullOrWhiteSpace(phone))
            return true; // Optional field

        // Numere romanesti: 0123456789 sau +40123456789
        var regex = new Regex(@"^(\+40|0)[1-9]\d{8}$");
        return regex.IsMatch(phone.Replace(" ", "").Replace("-", ""));
    }

    private static bool IsValidCNP(string cnp)
    {
        if (string.IsNullOrWhiteSpace(cnp) || cnp.Length != 13)
            return false;

        if (!cnp.All(char.IsDigit))
            return false;

        // Algoritm de validare CNP romanesc
        var weights = new[] { 2, 7, 9, 1, 4, 6, 3, 5, 8, 2, 7, 9 };
        var sum = 0;

        for (int i = 0; i < 12; i++)
        {
            sum += int.Parse(cnp[i].ToString()) * weights[i];
        }

        var remainder = sum % 11;
        var checkDigit = remainder < 10 ? remainder : 1;

        return checkDigit == int.Parse(cnp[12].ToString());
    }

    #endregion
}
