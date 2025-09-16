using FluentValidation;
using ValyanClinic.Domain.Models;
using ValyanClinic.Domain.Enums;

namespace ValyanClinic.Domain.Validators;

/// <summary>
/// Validator FluentValidation pentru modelul User
/// Implementeaza toate regulile de business pentru utilizatorii sistemului
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
            .WithMessage("Prenumele trebuie sa aiba intre 2 si 50 de caractere")
            .Matches(@"^[A-ZÀ-ÿa-z\s\-']+$")
            .WithMessage("Prenumele poate contine doar litere, spatii, cratime si apostrofuri");

        RuleFor(x => x.LastName)
            .NotEmpty()
            .WithMessage("Numele este obligatoriu")
            .Length(2, 50)
            .WithMessage("Numele trebuie sa aiba intre 2 si 50 de caractere")
            .Matches(@"^[A-ZÀ-ÿa-z\s\-']+$")
            .WithMessage("Numele poate contine doar litere, spatii, cratime si apostrofuri");

        // === REGULI PENTRU AUTENTIFICARE ===
        
        RuleFor(x => x.Email)
            .NotEmpty()
            .WithMessage("Email-ul este obligatoriu")
            .EmailAddress()
            .WithMessage("Formatul email-ului nu este valid")
            .Must(BeValidBusinessEmail)
            .WithMessage("Email-ul trebuie sa fie de la un domeniu profesional");

        RuleFor(x => x.Username)
            .NotEmpty()
            .WithMessage("Numele de utilizator este obligatoriu")
            .Length(3, 30)
            .WithMessage("Numele de utilizator trebuie sa aiba intre 3 si 30 de caractere")
            .Matches(@"^[a-zA-Z0-9._-]+$")
            .WithMessage("Numele de utilizator poate contine doar litere, cifre, puncte, cratime si underscore")
            .Must(BeValidUsername)
            .WithMessage("Numele de utilizator nu poate incepe sau termina cu caractere speciale");

        // === REGULI PENTRU CONTACT ===
        
        RuleFor(x => x.Phone)
            .Matches(@"^(\+40|0)[1-9]\d{8}$")
            .WithMessage("Formatul telefonului nu este valid (ex: 0722123456 sau +40722123456)")
            .When(x => !string.IsNullOrEmpty(x.Phone));

        // === REGULI PENTRU ROLURI SI STATUS ===
        
        RuleFor(x => x.Role)
            .IsInEnum()
            .WithMessage("Rolul selectat nu este valid")
            .Must(BeValidRoleForSystem)
            .WithMessage("Rolul specificat nu este permis in acest sistem");

        RuleFor(x => x.Status)
            .IsInEnum()
            .WithMessage("Statusul selectat nu este valid");

        // === REGULI PENTRU INFORMATII PROFESIONALE ===
        
        RuleFor(x => x.Department)
            .Length(2, 100)
            .WithMessage("Departamentul trebuie sa aiba intre 2 si 100 de caractere")
            .When(x => !string.IsNullOrEmpty(x.Department));

        RuleFor(x => x.JobTitle)
            .Length(2, 100)
            .WithMessage("Functia trebuie sa aiba intre 2 si 100 de caractere")
            .When(x => !string.IsNullOrEmpty(x.JobTitle));

        // === REGULI PENTRU METADATA ===
        
        RuleFor(x => x.CreatedDate)
            .NotEmpty()
            .WithMessage("Data crearii este obligatorie")
            .LessThanOrEqualTo(DateTime.Now)
            .WithMessage("Data crearii nu poate fi in viitor");

        RuleFor(x => x.LastLoginDate)
            .LessThanOrEqualTo(DateTime.Now)
            .WithMessage("Data ultimei autentificari nu poate fi in viitor")
            .GreaterThanOrEqualTo(x => x.CreatedDate)
            .WithMessage("Data ultimei autentificari nu poate fi anterioara datei crearii")
            .When(x => x.LastLoginDate.HasValue);

        // === REGULI COMPLEXE DE BUSINESS ===
        
        // Verificare consistenta rol-departament
        RuleFor(x => x)
            .Must(HaveConsistentRoleDepartment)
            .WithMessage("Combinatia rol-departament nu este valida")
            .WithName("RolDepartament");

        // Verificare ca doctorii au informatii complete
        RuleFor(x => x)
            .Must(HaveCompleteInformationForDoctors)
            .WithMessage("Doctorii trebuie sa aiba toate informatiile complete (departament si functie)")
            .WithName("InformatiiDoctor")
            .When(x => x.Role == UserRole.Doctor);
    }

    /// <summary>
    /// Verifica daca email-ul apartine unui domeniu profesional acceptat
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
    /// Verifica daca username-ul are formatul corect
    /// </summary>
    private bool BeValidUsername(string username)
    {
        if (string.IsNullOrEmpty(username))
            return false;

        // Nu poate incepe sau termina cu caractere speciale
        return !username.StartsWith(".") && !username.EndsWith(".") &&
               !username.StartsWith("_") && !username.EndsWith("_") &&
               !username.StartsWith("-") && !username.EndsWith("-");
    }

    /// <summary>
    /// Verifica daca rolul este valid pentru acest sistem
    /// </summary>
    private bool BeValidRoleForSystem(UserRole role)
    {
        // Toate rolurile sunt valide pentru sistem
        return Enum.IsDefined(typeof(UserRole), role);
    }

    /// <summary>
    /// Verifica consistenta intre rol si departament
    /// </summary>
    private bool HaveConsistentRoleDepartment(User user)
    {
        // Pentru anumite roluri, departamentul trebuie sa fie specific
        switch (user.Role)
        {
            case UserRole.Doctor:
                // Doctorii trebuie sa aiba un departament medical specificat
                return !string.IsNullOrEmpty(user.Department) && 
                       IsMedicalDepartment(user.Department);
                
            case UserRole.Nurse:
                // Asistentele trebuie sa aiba un departament medical specificat
                return !string.IsNullOrEmpty(user.Department) && 
                       IsMedicalDepartment(user.Department);
                
            case UserRole.Administrator:
                // Administratorii pot fi in orice departament
                return true;
                
            case UserRole.Manager:
                // Managerii trebuie sa aiba un departament specificat
                return !string.IsNullOrEmpty(user.Department);
                
            default:
                return true;
        }
    }

    /// <summary>
    /// Verifica daca departamentul este unul medical
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
    /// Verifica daca doctorii au informatii complete
    /// </summary>
    private bool HaveCompleteInformationForDoctors(User user)
    {
        if (user.Role != UserRole.Doctor)
            return true;

        // Doctorii trebuie sa aiba departament si functie
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
        
        // ID-ul nu trebuie sa fie setat manual (se genereaza automat)
        RuleFor(x => x.Id)
            .Equal(0)
            .WithMessage("ID-ul se genereaza automat si nu trebuie specificat");

        // Data crearii trebuie sa fie aproape de momentul curent
        RuleFor(x => x.CreatedDate)
            .GreaterThan(DateTime.Now.AddMinutes(-5))
            .WithMessage("Data crearii trebuie sa fie aproape de momentul curent")
            .LessThanOrEqualTo(DateTime.Now.AddMinutes(1))
            .WithMessage("Data crearii nu poate fi in viitor");

        // Pentru utilizatori noi, LastLoginDate trebuie sa fie null
        RuleFor(x => x.LastLoginDate)
            .Null()
            .WithMessage("Utilizatorii noi nu pot avea data ultimei autentificari setata");

        // Statusul pentru utilizatori noi trebuie sa fie Active
        RuleFor(x => x.Status)
            .Equal(UserStatus.Active)
            .WithMessage("Utilizatorii noi trebuie sa aiba statusul Activ");
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
        
        // ID-ul trebuie sa existe si sa fie valid
        RuleFor(x => x.Id)
            .GreaterThan(0)
            .WithMessage("ID-ul utilizatorului trebuie sa fie valid pentru actualizare");

        // Data crearii nu se poate modifica
        RuleFor(x => x.CreatedDate)
            .NotEmpty()
            .WithMessage("Data crearii trebuie sa existe pentru actualizare")
            .LessThan(DateTime.Now.AddDays(-1))
            .WithMessage("Data crearii pare sa fi fost modificata - nu este permis");

        // Daca este setat LastLoginDate, trebuie sa fie dupa CreatedDate
        RuleFor(x => x.LastLoginDate)
            .GreaterThan(x => x.CreatedDate)
            .WithMessage("Data ultimei autentificari trebuie sa fie dupa data crearii")
            .When(x => x.LastLoginDate.HasValue);
    }
}
