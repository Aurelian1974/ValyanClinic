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
            _logger.LogDebug("DEBUG CreatePersonalAsync: ENTRY - Starting creation for {Nume} {Prenume}", 
                personal.Nume, personal.Prenume);
            _logger.LogInformation("Creating personal: {Nume} {Prenume}", personal.Nume, personal.Prenume);

            _logger.LogDebug("DEBUG CreatePersonalAsync: Input validation - Nume: {Nume}, Prenume: {Prenume}, CNP: {CNP}, Cod_Angajat: {Cod_Angajat}, Departament: {Departament}, Functia: {Functia}, Id_Personal: {Id_Personal}, Utilizator: {Utilizator}", 
                personal.Nume, personal.Prenume, personal.CNP, personal.Cod_Angajat, personal.Departament, personal.Functia, personal.Id_Personal, utilizator);

            // Validare business logic
            _logger.LogDebug("DEBUG CreatePersonalAsync: Starting business validation...");
            var validationResult = await ValidatePersonalAsync(personal, isUpdate: false);
            if (!validationResult.IsValid)
            {
                _logger.LogWarning("DEBUG CreatePersonalAsync: Validation FAILED with {ErrorCount} errors: {Errors}", 
                    validationResult.Errors.Count, string.Join(", ", validationResult.Errors));
                return PersonalResult.ValidationFailure(validationResult.Errors);
            }
            _logger.LogDebug("DEBUG CreatePersonalAsync: Validation PASSED");

            // Business rules pentru crearea personalului
            _logger.LogDebug("DEBUG CreatePersonalAsync: Applying business rules for create...");
            var originalPersonal = personal; // Keep reference for comparison
            personal = ApplyBusinessRulesForCreate(personal, utilizator);
            
            _logger.LogDebug("DEBUG CreatePersonalAsync: Business rules applied - Data_Crearii: {Data_Crearii}, Data_Ultimei_Modificari: {Data_Ultimei_Modificari}, Status_Angajat: {Status_Angajat}, Creat_De: {Creat_De}, Modificat_De: {Modificat_De}", 
                personal.Data_Crearii, personal.Data_Ultimei_Modificari, personal.Status_Angajat, personal.Creat_De, personal.Modificat_De);

            // Verificare unicitate
            _logger.LogDebug("DEBUG CreatePersonalAsync: Checking uniqueness...");
            var (cnpExists, codExists) = await _repository.CheckUniqueAsync(personal.CNP, personal.Cod_Angajat);
            _logger.LogDebug("DEBUG CreatePersonalAsync: Uniqueness check results - CNP exists: {CNP_Exists}, Cod exists: {Cod_Exists}", 
                cnpExists, codExists);
            
            if (cnpExists)
            {
                _logger.LogWarning("DEBUG CreatePersonalAsync: CNP already exists - returning failure");
                return PersonalResult.Failure("CNP-ul exista deja in sistem");
            }
            if (codExists)
            {
                _logger.LogWarning("DEBUG CreatePersonalAsync: Code already exists - returning failure");
                return PersonalResult.Failure("Codul de angajat exista deja in sistem");
            }

            _logger.LogDebug("DEBUG CreatePersonalAsync: Calling repository.CreateAsync - Repository type: {RepositoryType}", 
                _repository.GetType().Name);
            
            var result = await _repository.CreateAsync(personal, utilizator);
            
            _logger.LogDebug("DEBUG CreatePersonalAsync: Repository.CreateAsync returned - Result is null: {IsNull}, Id_Personal: {Id_Personal}, NumeComplet: {NumeComplet}", 
                result == null, result?.Id_Personal, result?.NumeComplet);
            
            _logger.LogInformation("Personal created successfully: {PersonalId}", result?.Id_Personal);
            _logger.LogDebug("DEBUG CreatePersonalAsync: SUCCESS - Personal created with ID {PersonalId}", result?.Id_Personal);
            return PersonalResult.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "ERROR CreatePersonalAsync: Exception occurred - Type: {ExceptionType}", ex.GetType().Name);
            _logger.LogError(ex, "Error creating personal: {Nume} {Prenume}", personal.Nume, personal.Prenume);
            return PersonalResult.Failure($"Eroare la crearea personalului: {ex.Message}");
        }
    }

    public async Task<PersonalResult> UpdatePersonalAsync(Personal personal, string utilizator)
    {
        try
        {
            System.Diagnostics.Debug.WriteLine($"[UPDATE] === STARTING UPDATE ===");
            System.Diagnostics.Debug.WriteLine($"[UPDATE] INPUT Personal CNP: '{personal.CNP}'");
            
            _logger.LogDebug("DEBUG UpdatePersonalAsync: ENTRY - Starting update for {PersonalId}", personal.Id_Personal);
            _logger.LogInformation("Updating personal: {PersonalId}", personal.Id_Personal);

            _logger.LogDebug("DEBUG UpdatePersonalAsync: Input validation - Id_Personal: {Id_Personal}, Nume: {Nume}, Prenume: {Prenume}, CNP: {CNP}, Cod_Angajat: {Cod_Angajat}, Departament: {Departament}, Functia: {Functia}, Utilizator: {Utilizator}", 
                personal.Id_Personal, personal.Nume, personal.Prenume, personal.CNP, personal.Cod_Angajat, personal.Departament, personal.Functia, utilizator);

            // Verificare existenta
            _logger.LogDebug("DEBUG UpdatePersonalAsync: Checking if personal exists...");
            var existing = await _repository.GetByIdAsync(personal.Id_Personal);
            _logger.LogDebug("DEBUG UpdatePersonalAsync: Existing personal found: {Found}", existing != null);
            if (existing == null)
            {
                _logger.LogWarning("DEBUG UpdatePersonalAsync: Personal not found - returning failure");
                return PersonalResult.Failure("Personalul nu a fost gasit");
            }
            
            System.Diagnostics.Debug.WriteLine($"[UPDATE] EXISTING Personal CNP: '{existing.CNP}'");
            _logger.LogDebug("DEBUG UpdatePersonalAsync: Existing personal: {NumeComplet}", existing.NumeComplet);

            // Validare business logic ÎNAINTE de aplicarea business rules
            System.Diagnostics.Debug.WriteLine($"[UPDATE] BEFORE VALIDATION - Personal CNP: '{personal.CNP}'");
            _logger.LogDebug("DEBUG UpdatePersonalAsync: Starting business validation...");
            var validationResult = await ValidatePersonalAsync(personal, isUpdate: true);
            if (!validationResult.IsValid)
            {
                _logger.LogWarning("DEBUG UpdatePersonalAsync: Validation FAILED with {ErrorCount} errors: {Errors}", 
                    validationResult.Errors.Count, string.Join(", ", validationResult.Errors));
                return PersonalResult.ValidationFailure(validationResult.Errors);
            }
            _logger.LogDebug("DEBUG UpdatePersonalAsync: Validation PASSED");

            // Business rules pentru update
            _logger.LogDebug("DEBUG UpdatePersonalAsync: Applying business rules for update...");
            System.Diagnostics.Debug.WriteLine($"[UPDATE] BEFORE BUSINESS RULES - Personal CNP: '{personal.CNP}'");
            personal = ApplyBusinessRulesForUpdate(personal, existing, utilizator);
            System.Diagnostics.Debug.WriteLine($"[UPDATE] AFTER BUSINESS RULES - Personal CNP: '{personal.CNP}'");
            
            _logger.LogDebug("DEBUG UpdatePersonalAsync: Business rules applied - Data_Crearii: {Data_Crearii} (preserved), Data_Ultimei_Modificari: {Data_Ultimei_Modificari} (updated), Creat_De: {Creat_De} (preserved), Modificat_De: {Modificat_De} (updated)", 
                personal.Data_Crearii, personal.Data_Ultimei_Modificari, personal.Creat_De, personal.Modificat_De);

            // Verificare unicitate (exclude ID-ul curent)
            _logger.LogDebug("DEBUG UpdatePersonalAsync: Checking uniqueness (excluding current ID)...");
            var (cnpExists, codExists) = await _repository.CheckUniqueAsync(
                personal.CNP, 
                personal.Cod_Angajat, 
                personal.Id_Personal);
            
            _logger.LogDebug("DEBUG UpdatePersonalAsync: Uniqueness check results - CNP exists (excluding current): {CNP_Exists}, Cod exists (excluding current): {Cod_Exists}", 
                cnpExists, codExists);
            
            if (cnpExists)
            {
                _logger.LogWarning("DEBUG UpdatePersonalAsync: CNP already exists - returning failure");
                return PersonalResult.Failure("CNP-ul exista deja in sistem");
            }
            if (codExists)
            {
                _logger.LogWarning("DEBUG UpdatePersonalAsync: Code already exists - returning failure");
                return PersonalResult.Failure("Codul de angajat exista deja in sistem");
            }

            _logger.LogDebug("DEBUG UpdatePersonalAsync: Calling repository.UpdateAsync - Repository type: {RepositoryType}", 
                _repository.GetType().Name);
            
            var result = await _repository.UpdateAsync(personal, utilizator);
            
            _logger.LogDebug("DEBUG UpdatePersonalAsync: Repository.UpdateAsync returned - Result is null: {IsNull}, Id_Personal: {Id_Personal}, NumeComplet: {NumeComplet}, Data_Ultimei_Modificari: {Data_Ultimei_Modificari}", 
                result == null, result?.Id_Personal, result?.NumeComplet, result?.Data_Ultimei_Modificari);
            
            _logger.LogInformation("Personal updated successfully: {PersonalId}", result?.Id_Personal);
            _logger.LogDebug("DEBUG UpdatePersonalAsync: SUCCESS - Personal updated with ID {PersonalId}", result?.Id_Personal);
            return PersonalResult.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "ERROR UpdatePersonalAsync: Exception occurred - Type: {ExceptionType}", ex.GetType().Name);
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
            System.Diagnostics.Debug.WriteLine($"[VALIDATION] === STARTING VALIDATION (isUpdate: {isUpdate}) ===");
            System.Diagnostics.Debug.WriteLine($"[VALIDATION] Personal - Nume: '{personal.Nume}', Prenume: '{personal.Prenume}', CNP: '{personal.CNP}'");
            
            _logger.LogInformation("Starting validation for {Nume} {Prenume} with CNP {CNP} (isUpdate: {IsUpdate})", 
                personal.Nume, personal.Prenume, personal.CNP, isUpdate);

            // Validari de baza
            if (string.IsNullOrWhiteSpace(personal.Nume))
            {
                System.Diagnostics.Debug.WriteLine($"[VALIDATION] ERROR: Numele este obligatoriu");
                result.AddError("Numele este obligatoriu");
            }

            if (string.IsNullOrWhiteSpace(personal.Prenume))
            {
                System.Diagnostics.Debug.WriteLine($"[VALIDATION] ERROR: Prenumele este obligatoriu");
                result.AddError("Prenumele este obligatoriu");
            }

            if (string.IsNullOrWhiteSpace(personal.CNP))
            {
                System.Diagnostics.Debug.WriteLine($"[VALIDATION] ERROR: CNP-ul este obligatoriu");
                result.AddError("CNP-ul este obligatoriu");
            }
            else if (personal.CNP.Length != CNP_LENGTH)
            {
                System.Diagnostics.Debug.WriteLine($"[VALIDATION] ERROR: CNP length invalid - Length: {personal.CNP.Length}, Expected: {CNP_LENGTH}");
                result.AddError($"CNP-ul trebuie sa aiba exact {CNP_LENGTH} cifre");
            }
            else 
            {
                System.Diagnostics.Debug.WriteLine($"[VALIDATION] Calling IsValidCNP for CNP: '{personal.CNP}'");
                var cnpValid = IsValidCNP(personal.CNP);
                System.Diagnostics.Debug.WriteLine($"[VALIDATION] IsValidCNP returned: {cnpValid}");
                
                if (!cnpValid)
                {
                    System.Diagnostics.Debug.WriteLine($"[VALIDATION] ERROR: CNP validation failed");
                    result.AddError("CNP-ul nu este valid");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"[VALIDATION] SUCCESS: CNP validation passed");
                }
            }

            if (string.IsNullOrWhiteSpace(personal.Cod_Angajat))
                result.AddError("Codul de angajat este obligatoriu");

            if (string.IsNullOrWhiteSpace(personal.Functia))
                result.AddError("Functia este obligatorie");

            if (string.IsNullOrWhiteSpace(personal.Adresa_Domiciliu))
                result.AddError("Adresa de domiciliu este obligatorie");

            if (string.IsNullOrWhiteSpace(personal.Judet_Domiciliu))
                result.AddError("Judetul de domiciliu este obligatoriu");

            if (string.IsNullOrWhiteSpace(personal.Oras_Domiciliu))
                result.AddError("Orasul de domiciliu este obligatorie");

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

            System.Diagnostics.Debug.WriteLine($"[VALIDATION] === VALIDATION COMPLETE ===");
            System.Diagnostics.Debug.WriteLine($"[VALIDATION] Total errors: {result.Errors.Count}");
            System.Diagnostics.Debug.WriteLine($"[VALIDATION] Is valid: {result.IsValid}");
            
            if (result.Errors.Any())
            {
                System.Diagnostics.Debug.WriteLine($"[VALIDATION] Errors found:");
                for (int i = 0; i < result.Errors.Count; i++)
                {
                    System.Diagnostics.Debug.WriteLine($"[VALIDATION] {i + 1}. {result.Errors[i]}");
                }
            }

            _logger.LogInformation("Validation complete - Total errors: {ErrorCount}, IsValid: {IsValid}", 
                result.Errors.Count, result.IsValid);
            
            if (!result.IsValid)
            {
                _logger.LogWarning("Validation failed: {Errors}", string.Join(", ", result.Errors));
            }

            return result;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[VALIDATION] EXCEPTION: {ex.Message}");
            _logger.LogError(ex, "Exception in ValidatePersonalAsync");
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
        System.Diagnostics.Debug.WriteLine($"[BUSINESS RULES] === APPLYING UPDATE RULES ===");
        System.Diagnostics.Debug.WriteLine($"[BUSINESS RULES] INPUT Personal CNP: '{personal.CNP}'");
        System.Diagnostics.Debug.WriteLine($"[BUSINESS RULES] EXISTING Personal CNP: '{existing.CNP}'");
        
        // Pastrare date de creare originale
        personal.Data_Crearii = existing.Data_Crearii;
        personal.Creat_De = existing.Creat_De;

        // Setare date de modificare
        personal.Data_Ultimei_Modificari = DateTime.UtcNow;
        personal.Modificat_De = utilizator;

        // Business rule: normalizeaza datele la update
        personal.Nume = NormalizeName(personal.Nume);
        personal.Prenume = NormalizeName(personal.Prenume);
        
        // ATENȚIE AICI! Verifică dacă CNP-ul se modifică
        var originalCnp = personal.CNP;
        personal.CNP = personal.CNP.Trim();
        System.Diagnostics.Debug.WriteLine($"[BUSINESS RULES] CNP TRIM: '{originalCnp}' -> '{personal.CNP}'");
        
        personal.Cod_Angajat = personal.Cod_Angajat.Trim().ToUpper();

        // Business rule: email lowercase
        if (!string.IsNullOrEmpty(personal.Email_Personal))
            personal.Email_Personal = personal.Email_Personal.Trim().ToLower();
        
        if (!string.IsNullOrEmpty(personal.Email_Serviciu))
            personal.Email_Serviciu = personal.Email_Serviciu.Trim().ToLower();

        System.Diagnostics.Debug.WriteLine($"[BUSINESS RULES] FINAL Personal CNP: '{personal.CNP}'");
        System.Diagnostics.Debug.WriteLine($"[BUSINESS RULES] === BUSINESS RULES COMPLETE ===");
        
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
        try
        {
            System.Diagnostics.Debug.WriteLine($"[CNP VALIDATION] Starting validation for: '{cnp}'");
            
            if (string.IsNullOrWhiteSpace(cnp))
            {
                System.Diagnostics.Debug.WriteLine($"[CNP VALIDATION] FAILED - CNP is null or empty");
                return false;
            }

            // Remove any spaces or dashes that might be present
            var originalCnp = cnp;
            cnp = cnp.Replace(" ", "").Replace("-", "");
            
            System.Diagnostics.Debug.WriteLine($"[CNP VALIDATION] Original: '{originalCnp}', Cleaned: '{cnp}'");

            if (cnp.Length != 13)
            {
                System.Diagnostics.Debug.WriteLine($"[CNP VALIDATION] FAILED - Invalid length: {cnp.Length} (expected: 13)");
                return false;
            }

            if (!cnp.All(char.IsDigit))
            {
                var nonDigits = cnp.Where(c => !char.IsDigit(c)).ToArray();
                System.Diagnostics.Debug.WriteLine($"[CNP VALIDATION] FAILED - Non-digit characters: [{string.Join(", ", nonDigits)}]");
                return false;
            }

            // Algoritm de validare CNP romanesc
            var weights = new[] { 2, 7, 9, 1, 4, 6, 3, 5, 8, 2, 7, 9 };
            var sum = 0;

            System.Diagnostics.Debug.WriteLine($"[CNP VALIDATION] Starting calculation...");
            for (int i = 0; i < 12; i++)
            {
                var digit = int.Parse(cnp[i].ToString());
                var weight = weights[i];
                var product = digit * weight;
                sum += product;
                System.Diagnostics.Debug.WriteLine($"[CNP VALIDATION] Position {i + 1}: {digit} × {weight} = {product}, Sum: {sum}");
            }

            var remainder = sum % 11;
            var checkDigit = remainder == 10 ? 1 : remainder;
            var actualLastDigit = int.Parse(cnp[12].ToString());

            System.Diagnostics.Debug.WriteLine($"[CNP VALIDATION] Final calculation:");
            System.Diagnostics.Debug.WriteLine($"[CNP VALIDATION] - Total sum: {sum}");
            System.Diagnostics.Debug.WriteLine($"[CNP VALIDATION] - Remainder: {remainder}");
            System.Diagnostics.Debug.WriteLine($"[CNP VALIDATION] - Check digit: {checkDigit}");
            System.Diagnostics.Debug.WriteLine($"[CNP VALIDATION] - Actual last digit: {actualLastDigit}");

            var isValid = checkDigit == actualLastDigit;
            System.Diagnostics.Debug.WriteLine($"[CNP VALIDATION] Result: {(isValid ? "VALID" : "INVALID")}");

            return isValid;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[CNP VALIDATION] EXCEPTION: {ex.Message}");
            return false;
        }
    }

    #endregion
}
