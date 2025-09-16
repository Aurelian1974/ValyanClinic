using FluentValidation;
using ValyanClinic.Domain.Models;
using ValyanClinic.Domain.Enums;

namespace ValyanClinic.Domain.Validators;

/// <summary>
/// Validator FluentValidation pentru modelul Personal
/// Implementează toate regulile de business pentru personalul medical
/// SINCRONIZAT cu structura bazei de date Personal
/// </summary>
public class PersonalValidator : AbstractValidator<Personal>
{
    public PersonalValidator()
    {
        // === REGULI PENTRU IDENTIFICATORI UNICE ===
        
        RuleFor(x => x.Cod_Angajat)
            .NotEmpty()
            .WithMessage("Codul de angajat este obligatoriu")
            .Length(3, 20)
            .WithMessage("Codul de angajat trebuie să aibă între 3 și 20 de caractere")
            .Matches(@"^[A-Z0-9]+$")
            .WithMessage("Codul de angajat poate conține doar litere mari și cifre");

        RuleFor(x => x.CNP)
            .NotEmpty()
            .WithMessage("CNP-ul este obligatoriu")
            .Length(13)
            .WithMessage("CNP-ul trebuie să aibă exact 13 cifre")
            .Matches(@"^\d{13}$")
            .WithMessage("CNP-ul poate conține doar cifre")
            .Must(BeValidCNP)
            .WithMessage("CNP-ul nu este valid conform algoritmului oficial");

        // === REGULI PENTRU DATE PERSONALE DE BAZĂ ===
        
        RuleFor(x => x.Nume)
            .NotEmpty()
            .WithMessage("Numele este obligatoriu")
            .Length(2, 100)  // CORECTAT: DB permite 100, nu 50
            .WithMessage("Numele trebuie să aibă între 2 și 100 de caractere")
            .Matches(@"^[A-ZÀ-ÿa-z\s\-']+$")
            .WithMessage("Numele poate conține doar litere, spații, cratime și apostrofuri");

        RuleFor(x => x.Prenume)
            .NotEmpty()
            .WithMessage("Prenumele este obligatoriu")
            .Length(2, 100)  // CORECTAT: DB permite 100, nu 50
            .WithMessage("Prenumele trebuie să aibă între 2 și 100 de caractere")
            .Matches(@"^[A-ZÀ-ÿa-z\s\-']+$")
            .WithMessage("Prenumele poate conține doar litere, spații, cratime și apostrofuri");

        RuleFor(x => x.Nume_Anterior)
            .Length(2, 100)  // CORECTAT: DB permite 100, nu 50
            .WithMessage("Numele anterior trebuie să aibă între 2 și 100 de caractere")
            .Matches(@"^[A-ZÀ-ÿa-z\s\-']+$")
            .WithMessage("Numele anterior poate conține doar litere, spații, cratime și apostrofuri")
            .When(x => !string.IsNullOrEmpty(x.Nume_Anterior));

        RuleFor(x => x.Data_Nasterii)
            .NotEmpty()
            .WithMessage("Data nașterii este obligatorie")
            .Must(BeValidBirthDate)
            .WithMessage("Data nașterii trebuie să fie între 1940 și acum")
            .Must(BeMinimumAge)
            .WithMessage("Angajatul trebuie să aibă minimum 16 ani");

        // ADĂUGAT: Validare pentru Locul_Nasterii conform DB (nvarchar(200))
        RuleFor(x => x.Locul_Nasterii)
            .MaximumLength(200)
            .WithMessage("Locul nașterii nu poate depăși 200 de caractere")
            .When(x => !string.IsNullOrEmpty(x.Locul_Nasterii));

        RuleFor(x => x.Nationalitate)
            .NotEmpty()
            .WithMessage("Naționalitatea este obligatorie")
            .Length(2, 50)  // CONFIRMAT: DB permite 50
            .WithMessage("Naționalitatea trebuie să aibă între 2 și 50 de caractere");

        RuleFor(x => x.Cetatenie)
            .NotEmpty()
            .WithMessage("Cetățenia este obligatorie")
            .Length(2, 50)  // CONFIRMAT: DB permite 50
            .WithMessage("Cetățenia trebuie să aibă între 2 și 50 de caractere");

        // === REGULI PENTRU INFORMAȚII DE CONTACT ===

        RuleFor(x => x.Telefon_Personal)
            .MaximumLength(20)  // ADĂUGAT: Limitare conform DB
            .WithMessage("Telefonul personal nu poate depăși 20 de caractere")
            .Matches(@"^(\+40|0)[1-9]\d{8}$")
            .WithMessage("Formatul telefonului personal nu este valid (ex: 0722123456 sau +40722123456)")
            .When(x => !string.IsNullOrEmpty(x.Telefon_Personal));

        RuleFor(x => x.Telefon_Serviciu)
            .MaximumLength(20)  // ADĂUGAT: Limitare conform DB
            .WithMessage("Telefonul de serviciu nu poate depăși 20 de caractere")
            .Matches(@"^(\+40|0)[1-9]\d{8}$")
            .WithMessage("Formatul telefonului de serviciu nu este valid (ex: 0722123456 sau +40722123456)")
            .When(x => !string.IsNullOrEmpty(x.Telefon_Serviciu));

        RuleFor(x => x.Email_Personal)
            .MaximumLength(100)  // ADĂUGAT: Limitare conform DB
            .WithMessage("Email-ul personal nu poate depăși 100 de caractere")
            .EmailAddress()
            .WithMessage("Formatul email-ului personal nu este valid")
            .When(x => !string.IsNullOrEmpty(x.Email_Personal));

        RuleFor(x => x.Email_Serviciu)
            .MaximumLength(100)  // ADĂUGAT: Limitare conform DB
            .WithMessage("Email-ul de serviciu nu poate depăși 100 de caractere")
            .EmailAddress()
            .WithMessage("Formatul email-ului de serviciu nu este valid")
            .When(x => !string.IsNullOrEmpty(x.Email_Serviciu));

        // === REGULI PENTRU ADRESE ===

        RuleFor(x => x.Adresa_Domiciliu)
            .NotEmpty()
            .WithMessage("Adresa de domiciliu este obligatorie")
            .Length(10, 4000)  // CORECTAT: DB este nvarchar(MAX), dar limitare practică
            .WithMessage("Adresa de domiciliu trebuie să aibă între 10 și 4000 de caractere");

        RuleFor(x => x.Judet_Domiciliu)
            .NotEmpty()
            .WithMessage("Județul de domiciliu este obligatoriu")
            .Length(2, 50)  // CONFIRMAT: DB permite 50
            .WithMessage("Județul de domiciliu trebuie să aibă între 2 și 50 de caractere");

        RuleFor(x => x.Oras_Domiciliu)
            .NotEmpty()
            .WithMessage("Orașul de domiciliu este obligatoriu")
            .Length(2, 100)  // CONFIRMAT: DB permite 100
            .WithMessage("Orașul de domiciliu trebuie să aibă între 2 și 100 de caractere");

        RuleFor(x => x.Cod_Postal_Domiciliu)
            .MaximumLength(10)  // CONFIRMAT: DB permite 10
            .WithMessage("Codul poștal nu poate depăși 10 caractere")
            .Matches(@"^\d{6}$")
            .WithMessage("Codul poștal trebuie să aibă exact 6 cifre")
            .When(x => !string.IsNullOrEmpty(x.Cod_Postal_Domiciliu));

        // ADĂUGAT: Validări pentru adresa de reședință
        RuleFor(x => x.Adresa_Resedinta)
            .MaximumLength(4000)  // DB este nvarchar(MAX), limitare practică
            .WithMessage("Adresa de reședință nu poate depăși 4000 de caractere")
            .When(x => !string.IsNullOrEmpty(x.Adresa_Resedinta));

        RuleFor(x => x.Judet_Resedinta)
            .MaximumLength(50)  // CONFIRMAT: DB permite 50
            .WithMessage("Județul de reședință nu poate depăși 50 de caractere")
            .When(x => !string.IsNullOrEmpty(x.Judet_Resedinta));

        RuleFor(x => x.Oras_Resedinta)
            .MaximumLength(100)  // CONFIRMAT: DB permite 100
            .WithMessage("Orașul de reședință nu poate depăși 100 de caractere")
            .When(x => !string.IsNullOrEmpty(x.Oras_Resedinta));

        RuleFor(x => x.Cod_Postal_Resedinta)
            .MaximumLength(10)  // CONFIRMAT: DB permite 10
            .WithMessage("Codul poștal de reședință nu poate depăși 10 caractere")
            .Matches(@"^\d{6}$")
            .WithMessage("Codul poștal de reședință trebuie să aibă exact 6 cifre")
            .When(x => !string.IsNullOrEmpty(x.Cod_Postal_Resedinta));

        // === REGULI PENTRU DATE PROFESIONALE ===

        RuleFor(x => x.Functia)
            .NotEmpty()
            .WithMessage("Funcția este obligatorie")
            .Length(2, 100)  // CONFIRMAT: DB permite 100
            .WithMessage("Funcția trebuie să aibă între 2 și 100 de caractere");

        RuleFor(x => x.Departament)
            .NotNull()
            .WithMessage("Departamentul este obligatoriu")
            .IsInEnum()
            .WithMessage("Departamentul selectat nu este valid");

        // === REGULI PENTRU DOCUMENTE DE IDENTITATE ===

        RuleFor(x => x.Serie_CI)
            .MaximumLength(10)  // CONFIRMAT: DB permite 10
            .WithMessage("Seria CI nu poate depăși 10 caractere")
            .Matches(@"^[A-Z]{2}$")
            .WithMessage("Seria CI trebuie să aibă 2 litere mari (ex: AB)")
            .When(x => !string.IsNullOrEmpty(x.Serie_CI));

        RuleFor(x => x.Numar_CI)
            .MaximumLength(20)  // CONFIRMAT: DB permite 20
            .WithMessage("Numărul CI nu poate depăși 20 caractere")
            .Matches(@"^\d{6}$")
            .WithMessage("Numărul CI trebuie să aibă exact 6 cifre")
            .When(x => !string.IsNullOrEmpty(x.Numar_CI));

        RuleFor(x => x.Eliberat_CI_De)
            .MaximumLength(100)  // CONFIRMAT: DB permite 100
            .WithMessage("'Eliberat CI de' nu poate depăși 100 de caractere")
            .When(x => !string.IsNullOrEmpty(x.Eliberat_CI_De));

        RuleFor(x => x.Data_Eliberare_CI)
            .LessThanOrEqualTo(DateTime.Today)
            .WithMessage("Data eliberării CI nu poate fi în viitor")
            .When(x => x.Data_Eliberare_CI.HasValue);

        RuleFor(x => x.Valabil_CI_Pana)
            .GreaterThan(x => x.Data_Eliberare_CI)
            .WithMessage("Data expirării CI trebuie să fie după data eliberării")
            .When(x => x.Data_Eliberare_CI.HasValue && x.Valabil_CI_Pana.HasValue);

        // === REGULI PENTRU STATUS ȘI METADATA ===

        RuleFor(x => x.Status_Angajat)
            .IsInEnum()
            .WithMessage("Statusul angajatului nu este valid");

        RuleFor(x => x.Observatii)
            .MaximumLength(4000)  // DB este nvarchar(MAX), limitare practică pentru performanță
            .WithMessage("Observațiile nu pot depăși 4000 de caractere");

        // ADĂUGAT: Validări pentru câmpurile de audit
        RuleFor(x => x.Creat_De)
            .MaximumLength(50)  // CONFIRMAT: DB permite 50
            .WithMessage("'Creat de' nu poate depăși 50 de caractere")
            .When(x => !string.IsNullOrEmpty(x.Creat_De));

        RuleFor(x => x.Modificat_De)
            .MaximumLength(50)  // CONFIRMAT: DB permite 50
            .WithMessage("'Modificat de' nu poate depăși 50 de caractere")
            .When(x => !string.IsNullOrEmpty(x.Modificat_De));

        // === REGULI COMPLEXE DE BUSINESS ===
        
        // Verificare consistență telefon - trebuie să aibă cel puțin unul
        RuleFor(x => x)
            .Must(HaveAtLeastOnePhone)
            .WithMessage("Trebuie să existe cel puțin un număr de telefon (personal sau serviciu)")
            .WithName("Contact");

        // Verificare consistență CI - dacă există seria, trebuie să existe și numărul
        RuleFor(x => x)
            .Must(HaveCompleteCI)
            .WithMessage("Dacă este completată seria CI, trebuie completat și numărul CI")
            .WithName("Documente");

        // ADĂUGAT: Validare consistență stare civilă (nvarchar(100) în DB)
        RuleFor(x => x.Stare_Civila)
            .IsInEnum()
            .WithMessage("Starea civilă selectată nu este validă")
            .When(x => x.Stare_Civila.HasValue);
    }

    /// <summary>
    /// Validează CNP-ul conform algoritmului oficial românesc
    /// </summary>
    private bool BeValidCNP(string cnp)
    {
        if (string.IsNullOrEmpty(cnp) || cnp.Length != 13)
            return false;

        if (!cnp.All(char.IsDigit))
            return false;

        // Algoritmul de validare CNP
        int[] weights = { 2, 7, 9, 1, 4, 6, 3, 5, 8, 2, 7, 9 };
        int sum = 0;

        for (int i = 0; i < 12; i++)
        {
            sum += int.Parse(cnp[i].ToString()) * weights[i];
        }

        int remainder = sum % 11;
        int checkDigit = remainder == 10 ? 1 : remainder;

        return checkDigit == int.Parse(cnp[12].ToString());
    }

    /// <summary>
    /// Verifică dacă data nașterii este într-un interval valid
    /// </summary>
    private bool BeValidBirthDate(DateTime birthDate)
    {
        return birthDate >= new DateTime(1940, 1, 1) && birthDate <= DateTime.Today;
    }

    /// <summary>
    /// Verifică dacă persoana are minimum 16 ani
    /// </summary>
    private bool BeMinimumAge(DateTime birthDate)
    {
        var age = DateTime.Today.Year - birthDate.Year;
        if (birthDate.Date > DateTime.Today.AddYears(-age))
            age--;
        
        return age >= 16;
    }

    /// <summary>
    /// Verifică dacă există cel puțin un număr de telefon
    /// </summary>
    private bool HaveAtLeastOnePhone(Personal personal)
    {
        return !string.IsNullOrEmpty(personal.Telefon_Personal) || 
               !string.IsNullOrEmpty(personal.Telefon_Serviciu);
    }

    /// <summary>
    /// Verifică consistența datelor CI
    /// </summary>
    private bool HaveCompleteCI(Personal personal)
    {
        // Dacă nu este completată seria, e OK
        if (string.IsNullOrEmpty(personal.Serie_CI))
            return true;

        // Dacă e completată seria, trebuie să fie completat și numărul
        return !string.IsNullOrEmpty(personal.Numar_CI);
    }
}

/// <summary>
/// Validator pentru crearea unui nou personal (reguli suplimentare)
/// </summary>
public class PersonalCreateValidator : PersonalValidator
{
    public PersonalCreateValidator()
    {
        // Reguli suplimentare pentru crearea unui nou personal
        RuleFor(x => x.Id_Personal)
            .NotEmpty()
            .WithMessage("ID-ul personalului este obligatoriu")
            .Must(BeValidGuid)
            .WithMessage("ID-ul personalului trebuie să fie un GUID valid");

        // Data creării trebuie să fie setată (corectat pentru ora locală)
        RuleFor(x => x.Data_Crearii)
            .NotEmpty()
            .WithMessage("Data creării este obligatorie")
            .LessThanOrEqualTo(DateTime.Now.AddMinutes(1)) // Buffer de 1 minut pentru sincronizare
            .WithMessage("Data creării nu poate fi în viitor");

        // Creat_De trebuie să fie setat
        RuleFor(x => x.Creat_De)
            .NotEmpty()
            .WithMessage("Utilizatorul care creează înregistrarea trebuie specificat")
            .Length(2, 50)
            .WithMessage("Numele utilizatorului trebuie să aibă între 2 și 50 de caractere");
    }

    private bool BeValidGuid(Guid id)
    {
        return id != Guid.Empty;
    }
}

/// <summary>
/// Validator pentru actualizarea unui personal existent
/// </summary>
public class PersonalUpdateValidator : PersonalValidator
{
    public PersonalUpdateValidator()
    {
        // Pentru update, ID-ul trebuie să existe și să fie valid
        RuleFor(x => x.Id_Personal)
            .NotEmpty()
            .WithMessage("ID-ul personalului este obligatoriu pentru actualizare")
            .Must(BeValidGuid)
            .WithMessage("ID-ul personalului trebuie să fie un GUID valid");

        // Data ultimei modificări trebuie să fie setată (corectat pentru ora locală)
        RuleFor(x => x.Data_Ultimei_Modificari)
            .NotEmpty()
            .WithMessage("Data ultimei modificări este obligatorie")
            .LessThanOrEqualTo(DateTime.Now.AddMinutes(1)) // Buffer de 1 minut pentru sincronizare
            .WithMessage("Data ultimei modificări nu poate fi în viitor");

        // Modificat_De trebuie să fie setat
        RuleFor(x => x.Modificat_De)
            .NotEmpty()
            .WithMessage("Utilizatorul care modifică înregistrarea trebuie specificat")
            .Length(2, 50)
            .WithMessage("Numele utilizatorului trebuie să aibă între 2 și 50 de caractere");

        // Data creării trebuie să existe și să fie anterioară datei de modificare
        RuleFor(x => x.Data_Crearii)
            .NotEmpty()
            .WithMessage("Data creării trebuie să existe pentru actualizare")
            .LessThan(x => x.Data_Ultimei_Modificari)
            .WithMessage("Data creării trebuie să fie anterioară datei ultimei modificări");

        // Creat_De trebuie să existe
        RuleFor(x => x.Creat_De)
            .NotEmpty()
            .WithMessage("Creatorul original al înregistrării trebuie să existe");
    }

    private bool BeValidGuid(Guid id)
    {
        return id != Guid.Empty;
    }
}
