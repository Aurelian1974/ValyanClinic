using ValyanClinic.Domain.Models;
using ValyanClinic.Domain.Enums;
using ValyanClinic.Infrastructure.Repositories;
using ValyanClinic.Application.Validators;
using ValyanClinic.Application.Models;
using Microsoft.Extensions.Logging;

namespace ValyanClinic.Application.Services;

/// <summary>
/// PersonalMedical Service implementation cu business logic avansat si validari specifice medicale
/// Rich service conform best practices - focus pe validari medicale si business rules specifice
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

            // Business rules pentru crearea personalului medical (inainte de validare)
            personalMedical = ApplyBusinessRulesForCreate(personalMedical, utilizator);

            // Validari specifice medicale
            var validationResult = await ValidatePersonalMedicalAsync(personalMedical, false);
            if (!validationResult.IsValid)
            {
                _logger.LogWarning("Medical validation failed with {ErrorCount} errors: {Errors}", 
                    validationResult.Errors.Count, string.Join(", ", validationResult.Errors));
                return PersonalMedicalResult.ValidationFailure(validationResult.Errors);
            }

            // Verificare unicitate licenta medicala
            if (!string.IsNullOrEmpty(personalMedical.NumarLicenta))
            {
                var licentaExists = await _personalMedicalRepository.CheckLicentaUnicityAsync(personalMedical.NumarLicenta);
                if (licentaExists)
                {
                    _logger.LogWarning("Medical license already exists: {NumarLicenta}", personalMedical.NumarLicenta);
                    return PersonalMedicalResult.Failure("Numarul de licenta medicala exista deja in sistem");
                }
            }

            // Verificare email unic in sectorul medical
            if (!string.IsNullOrEmpty(personalMedical.Email))
            {
                var emailExists = await _personalMedicalRepository.CheckEmailUnicityAsync(personalMedical.Email);
                if (emailExists)
                {
                    _logger.LogWarning("Medical email already exists: {Email}", personalMedical.Email);
                    return PersonalMedicalResult.Failure("Adresa de email exista deja in sistemul medical");
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
                return PersonalMedicalResult.Failure("Personalul medical nu a fost gasit");
            }

            // Business rules pentru update (inainte de validare)
            personalMedical = ApplyBusinessRulesForUpdate(personalMedical, existing, utilizator);

            // Validari specifice medicale
            var validationResult = await ValidatePersonalMedicalAsync(personalMedical, true);
            if (!validationResult.IsValid)
            {
                _logger.LogWarning("Medical update validation failed with {ErrorCount} errors: {Errors}", 
                    validationResult.Errors.Count, string.Join(", ", validationResult.Errors));
                return PersonalMedicalResult.ValidationFailure(validationResult.Errors);
            }

            // Verificare unicitate licenta (exclude ID-ul curent)
            if (!string.IsNullOrEmpty(personalMedical.NumarLicenta))
            {
                var licentaExists = await _personalMedicalRepository.CheckLicentaUnicityAsync(
                    personalMedical.NumarLicenta, personalMedical.PersonalID);
                if (licentaExists)
                {
                    _logger.LogWarning("Medical license already exists for update: {NumarLicenta}", personalMedical.NumarLicenta);
                    return PersonalMedicalResult.Failure("Numarul de licenta medicala exista deja in sistem");
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
                    return PersonalMedicalResult.Failure("Adresa de email exista deja in sistemul medical");
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
                return PersonalMedicalResult.Failure("Personalul medical nu a fost gasit");
            }

            // Business rule: nu poti sterge personal medical care nu este deja inactiv
            if (!existing.EsteActiv)
            {
                return PersonalMedicalResult.Failure("Personalul medical este deja inactiv");
            }

            // Business rule: verifica daca personalul medical are programari active
            var areProgamariActive = await _personalMedicalRepository.CheckActiveAppointmentsAsync(id);
            if (areProgamariActive)
            {
                return PersonalMedicalResult.Failure("Nu se poate sterge personalul medical cu programari active. Dezactivati mai intai programarile.");
            }

            var success = await _personalMedicalRepository.DeleteAsync(id, utilizator);
            if (!success)
            {
                return PersonalMedicalResult.Failure("Eroare la stergerea personalului medical");
            }

            _logger.LogInformation("Personal medical deleted successfully: {PersonalId}", id);
            return PersonalMedicalResult.Success(existing);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting personal medical: {PersonalId}", id);
            return PersonalMedicalResult.Failure($"Eroare la stergerea personalului medical: {ex.Message}");
        }
    }

    public async Task<PersonalMedicalStatistics> GetStatisticsAsync()
    {
        try
        {
            _logger.LogInformation("Getting personal medical statistics");

            // Get basic statistics first
            var basicStats = await _personalMedicalRepository.GetStatisticsAsync();
            
            // Try to get distribution data, but handle gracefully if stored procedures are missing
            Dictionary<string, int> distributiePerDepartament = new();
            Dictionary<string, int> distributiePerSpecializare = new();
            
            try
            {
                distributiePerDepartament = await _personalMedicalRepository.GetDistributiePerDepartamentAsync();
                _logger.LogDebug("Successfully loaded department distribution statistics");
            }
            catch (Microsoft.Data.SqlClient.SqlException sqlEx) when (sqlEx.Number == 2812) // Could not find stored procedure
            {
                _logger.LogWarning("Distribution per department stored procedure missing (sp_PersonalMedical_GetDistributiePerDepartament). Using empty distribution.");
                distributiePerDepartament = new Dictionary<string, int>();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to load department distribution, using empty distribution");
                distributiePerDepartament = new Dictionary<string, int>();
            }
            
            try
            {
                distributiePerSpecializare = await _personalMedicalRepository.GetDistributiePerSpecializareAsync();
                _logger.LogDebug("Successfully loaded specialization distribution statistics");
            }
            catch (Microsoft.Data.SqlClient.SqlException sqlEx) when (sqlEx.Number == 2812) // Could not find stored procedure
            {
                _logger.LogWarning("Distribution per specialization stored procedure missing (sp_PersonalMedical_GetDistributiePerSpecializare). Using empty distribution.");
                distributiePerSpecializare = new Dictionary<string, int>();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to load specialization distribution, using empty distribution");
                distributiePerSpecializare = new Dictionary<string, int>();
            }
            
            _logger.LogInformation("Personal medical statistics loaded successfully. Basic stats: Total={Total}, Active={Active}, Doctors={Doctors}, Distributions: Departments={DeptCount}, Specializations={SpecCount}", 
                basicStats.TotalPersonalMedical, basicStats.PersonalMedicalActiv, basicStats.TotalDoctori, 
                distributiePerDepartament.Count, distributiePerSpecializare.Count);
            
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

            // incarca departamentele medicale din serviciul dedicat (NU din enum-uri!)
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

            // Pozitii medicale din enum (acestea NU se schimba dinamic)
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

            // Validari de baza
            if (string.IsNullOrWhiteSpace(personalMedical.Nume))
                result.AddError("Numele este obligatoriu");

            if (string.IsNullOrWhiteSpace(personalMedical.Prenume))
                result.AddError("Prenumele este obligatoriu");

            // Validari specifice medicale
            if (personalMedical.AreNevieDeLicenta && !personalMedical.AreLicentaValida)
                result.AddError("Numarul de licenta medicala este obligatoriu pentru doctori si asistenti medicali");

            if (!string.IsNullOrEmpty(personalMedical.NumarLicenta))
            {
                if (personalMedical.NumarLicenta.Length < MIN_LUNGIME_LICENTA || 
                    personalMedical.NumarLicenta.Length > MAX_LUNGIME_LICENTA)
                {
                    result.AddError($"Numarul de licenta medicala trebuie sa aiba intre {MIN_LUNGIME_LICENTA} si {MAX_LUNGIME_LICENTA} caractere");
                }

                // Verificare format licenta medicala
                if (!IsValidLicenseFormat(personalMedical.NumarLicenta))
                {
                    result.AddError("Formatul numarului de licenta medicala este invalid");
                }
            }

            // Validare email medical
            if (!string.IsNullOrEmpty(personalMedical.Email) && !IsValidEmail(personalMedical.Email))
                result.AddError("Formatul adresei de email este invalid");

            // Validare telefon medical
            if (!string.IsNullOrEmpty(personalMedical.Telefon) && !IsValidPhoneNumber(personalMedical.Telefon))
                result.AddError("Formatul numarului de telefon este invalid");

            // Validari pentru specializari medicale
            if (personalMedical.EsteDoctorSauAsistent && !personalMedical.AreSpecializareCompleta)
            {
                result.AddError("Doctorii si asistentii medicali trebuie sa aiba cel putin o categorie si o specializare");
            }

            // Validare business logic pentru pozitii medicale
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

            // Verificare in registrul medical national (simulat)
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
                        "Licenta medicala valabila",
                        "Certificat de specializare",
                        "Educatie medicala continua (EMC)"
                    });
                    break;

                case PozitiePersonalMedical.AsistentMedical:
                    certificariNecesare.AddRange(new[]
                    {
                        "Licenta de asistent medical",
                        "Certificat de competente",
                        "Primul ajutor"
                    });
                    break;

                case PozitiePersonalMedical.TehnicianMedical:
                    certificariNecesare.AddRange(new[]
                    {
                        "Certificat de calificare tehnician",
                        "Instruire securitate si sanatate"
                    });
                    break;

                default:
                    certificariNecesare.Add("Instruire securitate si sanatate");
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
        // Setare date sistem pentru crearea - foloseste ora locala pentru consistenta
        personalMedical.DataCreare = DateTime.Now;

        // Business rule: personalul medical nou este mereu Activ
        personalMedical.EsteActiv = true;

        // Business rule: normalizeaza datele medicale
        personalMedical = NormalizePersonalMedicalData(personalMedical);

        return personalMedical;
    }

    private PersonalMedical ApplyBusinessRulesForUpdate(PersonalMedical personalMedical, PersonalMedical existing, string utilizator)
    {
        // Pastrare data crearii originale
        personalMedical.DataCreare = existing.DataCreare;

        // Business rule: normalizeaza datele medicale la update
        personalMedical = NormalizePersonalMedicalData(personalMedical);
        
        return personalMedical;
    }

    private PersonalMedical NormalizePersonalMedicalData(PersonalMedical personalMedical)
    {
        // Normalizare nume
        personalMedical.Nume = NormalizeName(personalMedical.Nume);
        personalMedical.Prenume = NormalizeName(personalMedical.Prenume);
        
        // Normalizare numar licenta medicala
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

        // Prima litera mare, restul mici pentru fiecare cuvant
        return string.Join(" ", name.Split(' ', StringSplitOptions.RemoveEmptyEntries)
            .Select(word => char.ToUpper(word[0]) + word[1..].ToLower()));
    }

    private async Task<PersonalMedicalValidationResult> ValidatePozitieBusinessRulesAsync(PersonalMedical personalMedical)
    {
        var result = PersonalMedicalValidationResult.Success();

        // Business rules specifice pentru fiecare pozitie medicala
        switch (personalMedical.Pozitie)
        {
            case PozitiePersonalMedical.Doctor:
                if (string.IsNullOrEmpty(personalMedical.NumarLicenta))
                    result.AddError("Doctorii trebuie sa aiba obligatoriu numarul de licenta medicala");
                    
                if (!personalMedical.AreSpecializareCompleta)
                    result.AddError("Doctorii trebuie sa aiba cel putin o specializare definita");
                break;

            case PozitiePersonalMedical.AsistentMedical:
                if (string.IsNullOrEmpty(personalMedical.NumarLicenta))
                    result.AddError("Asistentii medicali trebuie sa aiba obligatoriu numarul de licenta");
                break;

            case PozitiePersonalMedical.TehnicianMedical:
                if (personalMedical.AreSpecializareCompleta && string.IsNullOrEmpty(personalMedical.Specializare))
                    result.AddError("Tehnicianii medicali cu specializari trebuie sa aiba specificata specializarea principala");
                break;
        }

        return result;
    }

    private bool IsValidLicenseFormat(string numarLicenta)
    {
        if (string.IsNullOrEmpty(numarLicenta))
            return false;

        // Format simplu: cel putin 5 caractere, contine cifre si/sau litere
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

        // Format simplu: cel putin 7 cifre, poate contine +, -, spatii, ()
        var numarCurat = new string(telefon.Where(c => char.IsDigit(c)).ToArray());
        return numarCurat.Length >= 7 && numarCurat.Length <= 15;
    }

    private async Task<bool> SimulateNationalMedicalRegistryCheck(string numarLicenta, string pozitie)
    {
        // Simulare verificare in registrul medical national
        // in implementarea reala ar fi o chiamare la un API extern
        await Task.Delay(100); // Simulare latenta API
        
        // Pentru demonstratie, returnam true pentru licente care incep cu litere
        return numarLicenta.Length >= 5 && char.IsLetter(numarLicenta[0]);
    }

    #endregion
}
