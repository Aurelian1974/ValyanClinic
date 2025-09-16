using FluentValidation;
using ValyanClinic.Domain.Models;
using ValyanClinic.Domain.Enums;

namespace ValyanClinic.Domain.Validators;

/// <summary>
/// Validator FluentValidation pentru modelul User
/// Implementează toate regulile de business pentru utilizatorii sistemului
/// </summary>
public class UserValidator : AbstractValidator<User>
{
    public UserValidator()
    {
        // === REGULI PENTRU DATE PERSONALE ===
        
        RuleFor(x => x.FirstName)
            .NotEmpty()
            .WithMessage("Prenumele este obligatoriu")
            .Length(2, 50)
            .WithMessage("Prenumele trebuie să aibă între 2 și 50 de caractere")
            .Matches(@"^[A-ZÀ-ÿa-z\s\-']+$")
            .WithMessage("Prenumele poate conține doar litere, spații, cratime și apostrofuri");

        RuleFor(x => x.LastName)
            .NotEmpty()
            .WithMessage("Numele este obligatoriu")
            .Length(2, 50)
            .WithMessage("Numele trebuie să aibă între 2 și 50 de caractere")
            .Matches(@"^[A-ZÀ-ÿa-z\s\-']+$")
            .WithMessage("Numele poate conține doar litere, spații, cratime și apostrofuri");

        // === REGULI PENTRU AUTENTIFICARE ===
        
        RuleFor(x => x.Email)
            .NotEmpty()
            .WithMessage("Email-ul este obligatoriu")
            .EmailAddress()
            .WithMessage("Formatul email-ului nu este valid")
            .Must(BeValidBusinessEmail)
            .WithMessage("Email-ul trebuie să fie de la un domeniu profesional");

        RuleFor(x => x.Username)
            .NotEmpty()
            .WithMessage("Numele de utilizator este obligatoriu")
            .Length(3, 30)
            .WithMessage("Numele de utilizator trebuie să aibă între 3 și 30 de caractere")
            .Matches(@"^[a-zA-Z0-9._-]+$")
            .WithMessage("Numele de utilizator poate conține doar litere, cifre, puncte, cratime și underscore")
            .Must(BeValidUsername)
            .WithMessage("Numele de utilizator nu poate începe sau termina cu caractere speciale");

        // === REGULI PENTRU CONTACT ===
        
        RuleFor(x => x.Phone)
            .Matches(@"^(\+40|0)[1-9]\d{8}$")
            .WithMessage("Formatul telefonului nu este valid (ex: 0722123456 sau +40722123456)")
            .When(x => !string.IsNullOrEmpty(x.Phone));

        // === REGULI PENTRU ROLURI ȘI STATUS ===
        
        RuleFor(x => x.Role)
            .IsInEnum()
            .WithMessage("Rolul selectat nu este valid")
            .Must(BeValidRoleForSystem)
            .WithMessage("Rolul specificat nu este permis în acest sistem");

        RuleFor(x => x.Status)
            .IsInEnum()
            .WithMessage("Statusul selectat nu este valid");

        // === REGULI PENTRU INFORMAȚII PROFESIONALE ===
        
        RuleFor(x => x.Department)
            .Length(2, 100)
            .WithMessage("Departamentul trebuie să aibă între 2 și 100 de caractere")
            .When(x => !string.IsNullOrEmpty(x.Department));

        RuleFor(x => x.JobTitle)
            .Length(2, 100)
            .WithMessage("Funcția trebuie să aibă între 2 și 100 de caractere")
            .When(x => !string.IsNullOrEmpty(x.JobTitle));

        // === REGULI PENTRU METADATA ===
        
        RuleFor(x => x.CreatedDate)
            .NotEmpty()
            .WithMessage("Data creării este obligatorie")
            .LessThanOrEqualTo(DateTime.Now)
            .WithMessage("Data creării nu poate fi în viitor");

        RuleFor(x => x.LastLoginDate)
            .LessThanOrEqualTo(DateTime.Now)
            .WithMessage("Data ultimei autentificări nu poate fi în viitor")
            .GreaterThanOrEqualTo(x => x.CreatedDate)
            .WithMessage("Data ultimei autentificări nu poate fi anterioară datei creării")
            .When(x => x.LastLoginDate.HasValue);

        // === REGULI COMPLEXE DE BUSINESS ===
        
        // Verificare consistență rol-departament
        RuleFor(x => x)
            .Must(HaveConsistentRoleDepartment)
            .WithMessage("Combinația rol-departament nu este validă")
            .WithName("RolDepartament");

        // Verificare că doctorii au informații complete
        RuleFor(x => x)
            .Must(HaveCompleteInformationForDoctors)
            .WithMessage("Doctorii trebuie să aibă toate informațiile complete (departament și funcție)")
            .WithName("InformatiiDoctor")
            .When(x => x.Role == UserRole.Doctor);
    }

    /// <summary>
    /// Verifică dacă email-ul aparține unui domeniu profesional acceptat
    /// </summary>
    private bool BeValidBusinessEmail(string email)
    {
        if (string.IsNullOrEmpty(email))
            return false;

        var validDomains = new[]
        {
            "@valyanmed.ro",
            "@valyan.ro", 
            "@gmail.com", // temporary for development
            "@yahoo.com", // temporary for development
            "@hotmail.com" // temporary for development
        };

        return validDomains.Any(domain => email.ToLower().EndsWith(domain));
    }

    /// <summary>
    /// Verifică dacă username-ul are formatul corect
    /// </summary>
    private bool BeValidUsername(string username)
    {
        if (string.IsNullOrEmpty(username))
            return false;

        // Nu poate începe sau termina cu caractere speciale
        return !username.StartsWith(".") && !username.EndsWith(".") &&
               !username.StartsWith("_") && !username.EndsWith("_") &&
               !username.StartsWith("-") && !username.EndsWith("-");
    }

    /// <summary>
    /// Verifică dacă rolul este valid pentru acest sistem
    /// </summary>
    private bool BeValidRoleForSystem(UserRole role)
    {
        // Toate rolurile sunt valide pentru sistem
        return Enum.IsDefined(typeof(UserRole), role);
    }

    /// <summary>
    /// Verifică consistența între rol și departament
    /// </summary>
    private bool HaveConsistentRoleDepartment(User user)
    {
        // Pentru anumite roluri, departamentul trebuie să fie specific
        switch (user.Role)
        {
            case UserRole.Doctor:
                // Doctorii trebuie să aibă un departament medical specificat
                return !string.IsNullOrEmpty(user.Department) && 
                       IsMedicalDepartment(user.Department);
                
            case UserRole.Nurse:
                // Asistentele trebuie să aibă un departament medical specificat
                return !string.IsNullOrEmpty(user.Department) && 
                       IsMedicalDepartment(user.Department);
                
            case UserRole.Administrator:
                // Administratorii pot fi în orice departament
                return true;
                
            case UserRole.Manager:
                // Managerii trebuie să aibă un departament specificat
                return !string.IsNullOrEmpty(user.Department);
                
            default:
                return true;
        }
    }

    /// <summary>
    /// Verifică dacă departamentul este unul medical
    /// </summary>
    private bool IsMedicalDepartment(string department)
    {
        if (string.IsNullOrEmpty(department))
            return false;

        var medicalDepartments = new[]
        {
            "cardiologie", "pneumologie", "neurologie", "pediatrie",
            "ginecologie", "chirurgie", "medicina interna", "radiologie",
            "laborator", "urgente", "ati", "farmacie"
        };

        return medicalDepartments.Any(d => 
            department.ToLower().Contains(d));
    }

    /// <summary>
    /// Verifică dacă doctorii au informații complete
    /// </summary>
    private bool HaveCompleteInformationForDoctors(User user)
    {
        if (user.Role != UserRole.Doctor)
            return true;

        // Doctorii trebuie să aibă departament și funcție
        return !string.IsNullOrEmpty(user.Department) && 
               !string.IsNullOrEmpty(user.JobTitle);
    }
}

/// <summary>
/// Validator pentru crearea unui nou utilizator
/// </summary>
public class UserCreateValidator : UserValidator
{
    public UserCreateValidator()
    {
        // Reguli suplimentare pentru crearea unui nou utilizator
        
        // ID-ul nu trebuie să fie setat manual (se generează automat)
        RuleFor(x => x.Id)
            .Equal(0)
            .WithMessage("ID-ul se generează automat și nu trebuie specificat");

        // Data creării trebuie să fie aproape de momentul curent
        RuleFor(x => x.CreatedDate)
            .GreaterThan(DateTime.Now.AddMinutes(-5))
            .WithMessage("Data creării trebuie să fie aproape de momentul curent")
            .LessThanOrEqualTo(DateTime.Now.AddMinutes(1))
            .WithMessage("Data creării nu poate fi în viitor");

        // Pentru utilizatori noi, LastLoginDate trebuie să fie null
        RuleFor(x => x.LastLoginDate)
            .Null()
            .WithMessage("Utilizatorii noi nu pot avea data ultimei autentificări setată");

        // Statusul pentru utilizatori noi trebuie să fie Active
        RuleFor(x => x.Status)
            .Equal(UserStatus.Active)
            .WithMessage("Utilizatorii noi trebuie să aibă statusul Activ");
    }
}

/// <summary>
/// Validator pentru actualizarea unui utilizator existent
/// </summary>
public class UserUpdateValidator : UserValidator
{
    public UserUpdateValidator()
    {
        // Reguli suplimentare pentru actualizarea unui utilizator
        
        // ID-ul trebuie să existe și să fie valid
        RuleFor(x => x.Id)
            .GreaterThan(0)
            .WithMessage("ID-ul utilizatorului trebuie să fie valid pentru actualizare");

        // Data creării nu se poate modifica
        RuleFor(x => x.CreatedDate)
            .NotEmpty()
            .WithMessage("Data creării trebuie să existe pentru actualizare")
            .LessThan(DateTime.Now.AddDays(-1))
            .WithMessage("Data creării pare să fi fost modificată - nu este permis");

        // Dacă este setat LastLoginDate, trebuie să fie după CreatedDate
        RuleFor(x => x.LastLoginDate)
            .GreaterThan(x => x.CreatedDate)
            .WithMessage("Data ultimei autentificări trebuie să fie după data creării")
            .When(x => x.LastLoginDate.HasValue);
    }
}
