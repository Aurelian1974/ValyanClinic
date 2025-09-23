using ValyanClinic.Domain.Models;
using ValyanClinic.Domain.Enums;
using ValyanClinic.Infrastructure.Repositories;
using ValyanClinic.Application.Validators;
using ValyanClinic.Application.Models;
using Microsoft.Extensions.Logging;

namespace ValyanClinic.Application.Services;

/// <summary>
/// PersonalMedical Service implementation cu business logic avansat și validări specifice medicale
/// Rich service conform best practices - focus pe validări medicale și business rules specifice
/// </summary>
public class PersonalMedicalService : IPersonalMedicalService
{
    private readonly IPersonalMedicalRepository _personalMedicalRepository;
    private readonly IDepartamentMedicalService _departamentMedicalService;
    private readonly IValidationService _validationService;
    private readonly ILogger<PersonalMedicalService> _logger;
    
    // Business rules constants pentru sectorul medical
    private const int MIN_LUNGIME_LICENTA = 5;
    private const int MAX_LUNGIME_LICENTA = 20;
    private const int MIN_VARSTA_DOCTOR = 25;
    private const int MAX_VARSTA_PERSONAL_MEDICAL = 75;

    public PersonalMedicalService(
        IPersonalMedicalRepository personalMedicalRepository,
        IDepartamentMedicalService departamentMedicalService,
        IValidationService validationService, 
        ILogger<PersonalMedicalService> logger)
    {
        _personalMedicalRepository = personalMedicalRepository;
        _departamentMedicalService = departamentMedicalService;
        _validationService = validationService;
        _logger = logger;
    }

    public async Task<PersonalMedicalPagedResult> GetPersonalMedicalAsync(PersonalMedicalSearchRequest request)
    {
        try
        {
            _logger.LogInformation("Getting personal medical with search: {SearchText}, Dept: {Departament}, Pozitie: {Pozitie}, Status: {Status}", 
                request.SearchText, request.Departament, request.Pozitie, request.Status);

            // Convert enum to string for repository layer
            string? pozitieStr = request.Pozitie?.ToString();
            string? statusStr = request.Status?.ToString();

            var (data, totalCount) = await _personalMedicalRepository.GetAllAsync(
                request.PageNumber,
                request.PageSize,
                request.SearchText,
                request.Departament,
                pozitieStr,
                statusStr,
                request.AreSpecializare,
                request.SortColumn,
                request.SortDirection);

            var totalPages = (int)Math.Ceiling((double)totalCount / request.PageSize);

            return new PersonalMedicalPagedResult(
                data,
                totalCount,
                request.PageNumber,
                request.PageSize,
                totalPages);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting personal medical data");
            throw;
        }
    }

    public async Task<PersonalMedical?> GetPersonalMedicalByIdAsync(Guid id)
    {
        try
        {
            _logger.LogInformation("Getting personal medical by ID: {PersonalId}", id);
            return await _personalMedicalRepository.GetByIdAsync(id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting personal medical by ID: {PersonalId}", id);
            throw;
        }
    }

    public async Task<PersonalMedicalResult> CreatePersonalMedicalAsync(PersonalMedical personalMedical, string utilizator)
    {
        try
        {
            _logger.LogInformation("Creating personal medical: {Nume} {Prenume}, Pozitie: {Pozitie}", 
                personalMedical.Nume, personalMedical.Prenume, personalMedical.Pozitie);

            // Business rules pentru crearea personalului medical (înainte de validare)
            personalMedical = ApplyBusinessRulesForCreate(personalMedical, utilizator);

            // Validări specifice medicale
            var validationResult = await ValidatePersonalMedicalAsync(personalMedical, false);
            if (!validationResult.IsValid)
            {
                _logger.LogWarning("Medical validation failed with {ErrorCount} errors: {Errors}", 
                    validationResult.Errors.Count, string.Join(", ", validationResult.Errors));
                return PersonalMedicalResult.ValidationFailure(validationResult.Errors);
            }

            // Verificare unicitate licență medicală
            if (!string.IsNullOrEmpty(personalMedical.NumarLicenta))
            {
                var licentaExists = await _personalMedicalRepository.CheckLicentaUnicityAsync(personalMedical.NumarLicenta);
                if (licentaExists)
                {
                    _logger.LogWarning("Medical license already exists: {NumarLicenta}", personalMedical.NumarLicenta);
                    return PersonalMedicalResult.Failure("Numărul de licență medicală există deja în sistem");
                }
            }

            // Verificare email unic în sectorul medical
            if (!string.IsNullOrEmpty(personalMedical.Email))
            {
                var emailExists = await _personalMedicalRepository.CheckEmailUnicityAsync(personalMedical.Email);
                if (emailExists)
                {
                    _logger.LogWarning("Medical email already exists: {Email}", personalMedical.Email);
                    return PersonalMedicalResult.Failure("Adresa de email există deja în sistemul medical");
                }
            }

            var result = await _personalMedicalRepository.CreateAsync(personalMedical, utilizator);
            
            _logger.LogInformation("Personal medical created successfully: {PersonalId}", result?.PersonalID);
            return PersonalMedicalResult.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating personal medical: {Nume} {Prenume}", personalMedical.Nume, personalMedical.Prenume);
            return PersonalMedicalResult.Failure($"Eroare la crearea personalului medical: {ex.Message}");
        }
    }

    public async Task<PersonalMedicalResult> UpdatePersonalMedicalAsync(PersonalMedical personalMedical, string utilizator)
    {
        try
        {
            _logger.LogInformation("Updating personal medical: {PersonalId}", personalMedical.PersonalID);

            // Verificare existenta
            var existing = await _personalMedicalRepository.GetByIdAsync(personalMedical.PersonalID);
            if (existing == null)
            {
                _logger.LogWarning("Personal medical not found for update: {PersonalId}", personalMedical.PersonalID);
                return PersonalMedicalResult.Failure("Personalul medical nu a fost găsit");
            }

            // Business rules pentru update (înainte de validare)
            personalMedical = ApplyBusinessRulesForUpdate(personalMedical, existing, utilizator);

            // Validări specifice medicale
            var validationResult = await ValidatePersonalMedicalAsync(personalMedical, true);
            if (!validationResult.IsValid)
            {
                _logger.LogWarning("Medical update validation failed with {ErrorCount} errors: {Errors}", 
                    validationResult.Errors.Count, string.Join(", ", validationResult.Errors));
                return PersonalMedicalResult.ValidationFailure(validationResult.Errors);
            }

            // Verificare unicitate licență (exclude ID-ul curent)
            if (!string.IsNullOrEmpty(personalMedical.NumarLicenta))
            {
                var licentaExists = await _personalMedicalRepository.CheckLicentaUnicityAsync(
                    personalMedical.NumarLicenta, personalMedical.PersonalID);
                if (licentaExists)
                {
                    _logger.LogWarning("Medical license already exists for update: {NumarLicenta}", personalMedical.NumarLicenta);
                    return PersonalMedicalResult.Failure("Numărul de licență medicală există deja în sistem");
                }
            }

            // Verificare unicitate email (exclude ID-ul curent)
            if (!string.IsNullOrEmpty(personalMedical.Email))
            {
                var emailExists = await _personalMedicalRepository.CheckEmailUnicityAsync(
                    personalMedical.Email, personalMedical.PersonalID);
                if (emailExists)
                {
                    _logger.LogWarning("Medical email already exists for update: {Email}", personalMedical.Email);
                    return PersonalMedicalResult.Failure("Adresa de email există deja în sistemul medical");
                }
            }

            var result = await _personalMedicalRepository.UpdateAsync(personalMedical, utilizator);
            
            _logger.LogInformation("Personal medical updated successfully: {PersonalId}", result?.PersonalID);
            return PersonalMedicalResult.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating personal medical: {PersonalId}", personalMedical.PersonalID);
            return PersonalMedicalResult.Failure($"Eroare la actualizarea personalului medical: {ex.Message}");
        }
    }

    public async Task<PersonalMedicalResult> DeletePersonalMedicalAsync(Guid id, string utilizator)
    {
        try
        {
            _logger.LogInformation("Deleting personal medical: {PersonalId}", id);

            var existing = await _personalMedicalRepository.GetByIdAsync(id);
            if (existing == null)
            {
                return PersonalMedicalResult.Failure("Personalul medical nu a fost găsit");
            }

            // Business rule: nu poti sterge personal medical care nu este deja inactiv
            if (!existing.EsteActiv)
            {
                return PersonalMedicalResult.Failure("Personalul medical este deja inactiv");
            }

            // Business rule: verifică dacă personalul medical are programări active
            var areProgamariActive = await _personalMedicalRepository.CheckActiveAppointmentsAsync(id);
            if (areProgamariActive)
            {
                return PersonalMedicalResult.Failure("Nu se poate șterge personalul medical cu programări active. Dezactivați mai întâi programările.");
            }

            var success = await _personalMedicalRepository.DeleteAsync(id, utilizator);
            if (!success)
            {
                return PersonalMedicalResult.Failure("Eroare la ștergerea personalului medical");
            }

            _logger.LogInformation("Personal medical deleted successfully: {PersonalId}", id);
            return PersonalMedicalResult.Success(existing);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting personal medical: {PersonalId}", id);
            return PersonalMedicalResult.Failure($"Eroare la ștergerea personalului medical: {ex.Message}");
        }
    }

    public async Task<PersonalMedicalStatistics> GetStatisticsAsync()
    {
        try
        {
            _logger.LogInformation("Getting personal medical statistics");

            var basicStats = await _personalMedicalRepository.GetStatisticsAsync();
            var distributiePerDepartament = await _personalMedicalRepository.GetDistributiePerDepartamentAsync();
            var distributiePerSpecializare = await _personalMedicalRepository.GetDistributiePerSpecializareAsync();
            
            return new PersonalMedicalStatistics(
                basicStats.TotalPersonalMedical,
                basicStats.PersonalMedicalActiv,
                basicStats.PersonalMedicalInactiv,
                basicStats.TotalDoctori,
                basicStats.TotalAsistenti,
                basicStats.TotalTehnicianiMedicali,
                distributiePerDepartament,
                distributiePerSpecializare
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting personal medical statistics");
            throw;
        }
    }

    public async Task<PersonalMedicalDropdownOptions> GetDropdownOptionsAsync()
    {
        try
        {
            _logger.LogInformation("Getting personal medical dropdown options");

            // Încarcă departamentele medicale din serviciul dedicat (NU din enum-uri!)
            var departamenteMedicaleTask = _departamentMedicalService.GetAllDepartamenteMedicaleAsync();
            var categoriiTask = _departamentMedicalService.GetCategoriiMedicaleAsync();
            var specializariTask = _departamentMedicalService.GetSpecializariMedicaleAsync();
            var subspecializariTask = _departamentMedicalService.GetSubspecializariMedicaleAsync();

            await Task.WhenAll(departamenteMedicaleTask, categoriiTask, specializariTask, subspecializariTask);

            var departamenteMedicale = (await departamenteMedicaleTask)
                .Select(x => new DropdownItem(x.DepartamentID.ToString(), x.Nume));
                
            var categorii = (await categoriiTask)
                .Select(x => new DropdownItem(x.DepartamentID.ToString(), x.Nume));
                
            var specializari = (await specializariTask)
                .Select(x => new DropdownItem(x.DepartamentID.ToString(), x.Nume));
                
            var subspecializari = (await subspecializariTask)
                .Select(x => new DropdownItem(x.DepartamentID.ToString(), x.Nume));

            // Poziții medicale din enum (acestea NU se schimbă dinamic)
            var pozitiiMedicale = Enum.GetValues<PozitiePersonalMedical>()
                .Select(p => new DropdownItem(p.ToString(), p.GetDisplayName()));

            return new PersonalMedicalDropdownOptions(
                departamenteMedicale,
                categorii,
                specializari,
                subspecializari,
                pozitiiMedicale
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting personal medical dropdown options");
            throw;
        }
    }

    public async Task<PersonalMedicalValidationResult> ValidatePersonalMedicalAsync(PersonalMedical personalMedical, bool isUpdate = false)
    {
        try
        {
            _logger.LogInformation("Validating personal medical: {Nume} {Prenume} with license {NumarLicenta} (isUpdate: {IsUpdate})", 
                personalMedical.Nume, personalMedical.Prenume, personalMedical.NumarLicenta, isUpdate);

            var result = PersonalMedicalValidationResult.Success();

            // Validări de bază
            if (string.IsNullOrWhiteSpace(personalMedical.Nume))
                result.AddError("Numele este obligatoriu");

            if (string.IsNullOrWhiteSpace(personalMedical.Prenume))
                result.AddError("Prenumele este obligatoriu");

            // Validări specifice medicale
            if (personalMedical.AreNevieDeLicenta && !personalMedical.AreLicentaValida)
                result.AddError("Numărul de licență medicală este obligatoriu pentru doctori și asistenți medicali");

            if (!string.IsNullOrEmpty(personalMedical.NumarLicenta))
            {
                if (personalMedical.NumarLicenta.Length < MIN_LUNGIME_LICENTA || 
                    personalMedical.NumarLicenta.Length > MAX_LUNGIME_LICENTA)
                {
                    result.AddError($"Numărul de licență medicală trebuie să aibă între {MIN_LUNGIME_LICENTA} și {MAX_LUNGIME_LICENTA} caractere");
                }

                // Verificare format licență medicală
                if (!IsValidLicenseFormat(personalMedical.NumarLicenta))
                {
                    result.AddError("Formatul numărului de licență medicală este invalid");
                }
            }

            // Validare email medical
            if (!string.IsNullOrEmpty(personalMedical.Email) && !IsValidEmail(personalMedical.Email))
                result.AddError("Formatul adresei de email este invalid");

            // Validare telefon medical
            if (!string.IsNullOrEmpty(personalMedical.Telefon) && !IsValidPhoneNumber(personalMedical.Telefon))
                result.AddError("Formatul numărului de telefon este invalid");

            // Validări pentru specializări medicale
            if (personalMedical.EsteDoctorSauAsistent && !personalMedical.AreSpecializareCompleta)
            {
                result.AddError("Doctorii și asistenții medicali trebuie să aibă cel puțin o categorie și o specializare");
            }

            // Validare business logic pentru poziții medicale
            var pozitieValidation = await ValidatePozitieBusinessRulesAsync(personalMedical);
            if (!pozitieValidation.IsValid)
            {
                result.Errors.AddRange(pozitieValidation.Errors);
            }

            if (result.IsValid)
            {
                _logger.LogInformation("Medical validation passed for {Nume} {Prenume}", personalMedical.Nume, personalMedical.Prenume);
            }
            else
            {
                _logger.LogWarning("Medical validation failed for {Nume} {Prenume}: {Errors}", 
                    personalMedical.Nume, personalMedical.Prenume, string.Join(", ", result.Errors));
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception in ValidatePersonalMedicalAsync");
            return PersonalMedicalValidationResult.Failure("Eroare la validarea datelor medicale");
        }
    }

    public async Task<bool> ValidateLicentaMedicalaAsync(string numarLicenta, string pozitie)
    {
        try
        {
            if (string.IsNullOrEmpty(numarLicenta) || string.IsNullOrEmpty(pozitie))
                return false;

            // Validare format
            if (!IsValidLicenseFormat(numarLicenta))
                return false;

            // Verificare în registrul medical național (simulat)
            return await SimulateNationalMedicalRegistryCheck(numarLicenta, pozitie);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating medical license: {NumarLicenta}", numarLicenta);
            return false;
        }
    }

    public async Task<List<string>> GetCertificariNecesareAsync(Guid personalId)
    {
        try
        {
            var personalMedical = await _personalMedicalRepository.GetByIdAsync(personalId);
            if (personalMedical == null)
                return new List<string>();

            var certificariNecesare = new List<string>();

            switch (personalMedical.Pozitie)
            {
                case PozitiePersonalMedical.Doctor:
                    certificariNecesare.AddRange(new[]
                    {
                        "Licența medicală valabilă",
                        "Certificat de specializare",
                        "Educație medicală continuă (EMC)"
                    });
                    break;

                case PozitiePersonalMedical.AsistentMedical:
                    certificariNecesare.AddRange(new[]
                    {
                        "Licența de asistent medical",
                        "Certificat de competențe",
                        "Primul ajutor"
                    });
                    break;

                case PozitiePersonalMedical.TehnicianMedical:
                    certificariNecesare.AddRange(new[]
                    {
                        "Certificat de calificare tehnician",
                        "Instruire securitate și sănătate"
                    });
                    break;

                default:
                    certificariNecesare.Add("Instruire securitate și sănătate");
                    break;
            }

            return certificariNecesare;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting required certifications for personal: {PersonalId}", personalId);
            return new List<string>();
        }
    }

    #region Private Business Logic Methods

    private PersonalMedical ApplyBusinessRulesForCreate(PersonalMedical personalMedical, string utilizator)
    {
        // Setare date sistem pentru crearea - folosește ora locală pentru consistență
        personalMedical.DataCreare = DateTime.Now;

        // Business rule: personalul medical nou este mereu Activ
        personalMedical.EsteActiv = true;

        // Business rule: normalizează datele medicale
        personalMedical = NormalizePersonalMedicalData(personalMedical);

        return personalMedical;
    }

    private PersonalMedical ApplyBusinessRulesForUpdate(PersonalMedical personalMedical, PersonalMedical existing, string utilizator)
    {
        // Păstrare data creării originale
        personalMedical.DataCreare = existing.DataCreare;

        // Business rule: normalizează datele medicale la update
        personalMedical = NormalizePersonalMedicalData(personalMedical);
        
        return personalMedical;
    }

    private PersonalMedical NormalizePersonalMedicalData(PersonalMedical personalMedical)
    {
        // Normalizare nume
        personalMedical.Nume = NormalizeName(personalMedical.Nume);
        personalMedical.Prenume = NormalizeName(personalMedical.Prenume);
        
        // Normalizare număr licență medicală
        if (!string.IsNullOrEmpty(personalMedical.NumarLicenta))
            personalMedical.NumarLicenta = personalMedical.NumarLicenta.Trim().ToUpper();

        // Normalizare specializare
        if (!string.IsNullOrEmpty(personalMedical.Specializare))
            personalMedical.Specializare = personalMedical.Specializare.Trim();

        // Normalizare email medical
        if (!string.IsNullOrEmpty(personalMedical.Email))
            personalMedical.Email = personalMedical.Email.Trim().ToLower();

        // Normalizare departament
        if (!string.IsNullOrEmpty(personalMedical.Departament))
            personalMedical.Departament = personalMedical.Departament.Trim();

        return personalMedical;
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

    private async Task<PersonalMedicalValidationResult> ValidatePozitieBusinessRulesAsync(PersonalMedical personalMedical)
    {
        var result = PersonalMedicalValidationResult.Success();

        // Business rules specifice pentru fiecare poziție medicală
        switch (personalMedical.Pozitie)
        {
            case PozitiePersonalMedical.Doctor:
                if (string.IsNullOrEmpty(personalMedical.NumarLicenta))
                    result.AddError("Doctorii trebuie să aibă obligatoriu numărul de licență medicală");
                    
                if (!personalMedical.AreSpecializareCompleta)
                    result.AddError("Doctorii trebuie să aibă cel puțin o specializare definită");
                break;

            case PozitiePersonalMedical.AsistentMedical:
                if (string.IsNullOrEmpty(personalMedical.NumarLicenta))
                    result.AddError("Asistenții medicali trebuie să aibă obligatoriu numărul de licență");
                break;

            case PozitiePersonalMedical.TehnicianMedical:
                if (personalMedical.AreSpecializareCompleta && string.IsNullOrEmpty(personalMedical.Specializare))
                    result.AddError("Tehnicianii medicali cu specializări trebuie să aibă specificată specializarea principală");
                break;
        }

        return result;
    }

    private bool IsValidLicenseFormat(string numarLicenta)
    {
        if (string.IsNullOrEmpty(numarLicenta))
            return false;

        // Format simplu: cel puțin 5 caractere, conține cifre și/sau litere
        return numarLicenta.Length >= 5 && 
               numarLicenta.Any(char.IsLetterOrDigit);
    }

    private bool IsValidEmail(string email)
    {
        if (string.IsNullOrEmpty(email))
            return false;

        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }

    private bool IsValidPhoneNumber(string telefon)
    {
        if (string.IsNullOrEmpty(telefon))
            return false;

        // Format simplu: cel puțin 7 cifre, poate conține +, -, spații, ()
        var numarCurat = new string(telefon.Where(c => char.IsDigit(c)).ToArray());
        return numarCurat.Length >= 7 && numarCurat.Length <= 15;
    }

    private async Task<bool> SimulateNationalMedicalRegistryCheck(string numarLicenta, string pozitie)
    {
        // Simulare verificare în registrul medical național
        // În implementarea reală ar fi o chiamare la un API extern
        await Task.Delay(100); // Simulare latență API
        
        // Pentru demonstrație, returnăm true pentru licențe care încep cu litere
        return numarLicenta.Length >= 5 && char.IsLetter(numarLicenta[0]);
    }

    #endregion
}
