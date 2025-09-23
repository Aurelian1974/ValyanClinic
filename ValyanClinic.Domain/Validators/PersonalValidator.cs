using FluentValidation;
using ValyanClinic.Domain.Models;
using ValyanClinic.Domain.Enums;

namespace ValyanClinic.Domain.Validators;

/// <summary>
/// Validator FluentValidation pentru modelul Personal
/// Implementeaza toate regulile de business pentru personalul medical
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
            .WithMessage("Codul de angajat trebuie sa aiba intre 3 si 20 de caractere")
            .Matches(@"^[A-Z0-9]+$")
            .WithMessage("Codul de angajat poate contine doar litere mari si cifre");

        RuleFor(x => x.CNP)
            .NotEmpty()
            .WithMessage("CNP-ul este obligatoriu")
            .Length(13)
            .WithMessage("CNP-ul trebuie sa aiba exact 13 cifre")
            .Matches(@"^\d{13}$")
            .WithMessage("CNP-ul poate contine doar cifre")
            .Must(BeValidCNP)
            .WithMessage("CNP-ul nu este valid conform algoritmului oficial");

        // === REGULI PENTRU DATE PERSONALE DE BAZa ===
        
        RuleFor(x => x.Nume)
            .NotEmpty()
            .WithMessage("Numele este obligatoriu")
            .Length(2, 100)  // CORECTAT: DB permite 100, nu 50
            .WithMessage("Numele trebuie sa aiba intre 2 si 100 de caractere")
            .Matches(@"^[A-ZÀ-ÿa-z\s\-']+$")
            .WithMessage("Numele poate contine doar litere, spatii, cratime si apostrofuri");

        RuleFor(x => x.Prenume)
            .NotEmpty()
            .WithMessage("Prenumele este obligatoriu")
            .Length(2, 100)  // CORECTAT: DB permite 100, nu 50
            .WithMessage("Prenumele trebuie sa aiba intre 2 si 100 de caractere")
            .Matches(@"^[A-ZÀ-ÿa-z\s\-']+$")
            .WithMessage("Prenumele poate contine doar litere, spatii, cratime si apostrofuri");

        RuleFor(x => x.Nume_Anterior)
            .Length(2, 100)  // CORECTAT: DB permite 100, nu 50
            .WithMessage("Numele anterior trebuie sa aiba intre 2 si 100 de caractere")
            .Matches(@"^[A-ZÀ-ÿa-z\s\-']+$")
            .WithMessage("Numele anterior poate contine doar litere, spatii, cratime si apostrofuri")
            .When(x => !string.IsNullOrEmpty(x.Nume_Anterior));

        RuleFor(x => x.Data_Nasterii)
            .NotEmpty()
            .WithMessage("Data nasterii este obligatorie")
            .Must(BeValidBirthDate)
            .WithMessage("Data nasterii trebuie sa fie intre 1940 si acum")
            .Must(BeMinimumAge)
            .WithMessage("Angajatul trebuie sa aiba minimum 16 ani");

        // ADaUGAT: Validare pentru Locul_Nasterii conform DB (nvarchar(200))
        RuleFor(x => x.Locul_Nasterii)
            .MaximumLength(200)
            .WithMessage("Locul nasterii nu poate depasi 200 de caractere")
            .When(x => !string.IsNullOrEmpty(x.Locul_Nasterii));

        RuleFor(x => x.Nationalitate)
            .NotEmpty()
            .WithMessage("Nationalitatea este obligatorie")
            .Length(2, 50)  // CONFIRMAT: DB permite 50
            .WithMessage("Nationalitatea trebuie sa aiba intre 2 si 50 de caractere");

        RuleFor(x => x.Cetatenie)
            .NotEmpty()
            .WithMessage("Cetatenia este obligatorie")
            .Length(2, 50)  // CONFIRMAT: DB permite 50
            .WithMessage("Cetatenia trebuie sa aiba intre 2 si 50 de caractere");

        // === REGULI PENTRU INFORMAtII DE CONTACT ===

        RuleFor(x => x.Telefon_Personal)
            .MaximumLength(20)  // ADaUGAT: Limitare conform DB
            .WithMessage("Telefonul personal nu poate depasi 20 de caractere")
            .Matches(@"^(\+40|0)[1-9]\d{8}$")
            .WithMessage("Formatul telefonului personal nu este valid (ex: 0722123456 sau +40722123456)")
            .When(x => !string.IsNullOrEmpty(x.Telefon_Personal));

        RuleFor(x => x.Telefon_Serviciu)
            .MaximumLength(20)  // ADaUGAT: Limitare conform DB
            .WithMessage("Telefonul de serviciu nu poate depasi 20 de caractere")
            .Matches(@"^(\+40|0)[1-9]\d{8}$")
            .WithMessage("Formatul telefonului de serviciu nu este valid (ex: 0722123456 sau +40722123456)")
            .When(x => !string.IsNullOrEmpty(x.Telefon_Serviciu));

        RuleFor(x => x.Email_Personal)
            .MaximumLength(100)  // ADaUGAT: Limitare conform DB
            .WithMessage("Email-ul personal nu poate depasi 100 de caractere")
            .EmailAddress()
            .WithMessage("Formatul email-ului personal nu este valid")
            .When(x => !string.IsNullOrEmpty(x.Email_Personal));

        RuleFor(x => x.Email_Serviciu)
            .MaximumLength(100)  // ADaUGAT: Limitare conform DB
            .WithMessage("Email-ul de serviciu nu poate depasi 100 de caractere")
            .EmailAddress()
            .WithMessage("Formatul email-ului de serviciu nu este valid")
            .When(x => !string.IsNullOrEmpty(x.Email_Serviciu));

        // === REGULI PENTRU ADRESE ===

        RuleFor(x => x.Adresa_Domiciliu)
            .NotEmpty()
            .WithMessage("Adresa de domiciliu este obligatorie")
            .Length(10, 4000)  // CORECTAT: DB este nvarchar(MAX), dar limitare practica
            .WithMessage("Adresa de domiciliu trebuie sa aiba intre 10 si 4000 de caractere");

        RuleFor(x => x.Judet_Domiciliu)
            .NotEmpty()
            .WithMessage("Judetul de domiciliu este obligatoriu")
            .Length(2, 50)  // CONFIRMAT: DB permite 50
            .WithMessage("Judetul de domiciliu trebuie sa aiba intre 2 si 50 de caractere");

        RuleFor(x => x.Oras_Domiciliu)
            .NotEmpty()
            .WithMessage("Orasul de domiciliu este obligatoriu")
            .Length(2, 100)  // CONFIRMAT: DB permite 100
            .WithMessage("Orasul de domiciliu trebuie sa aiba intre 2 si 100 de caractere");

        RuleFor(x => x.Cod_Postal_Domiciliu)
            .MaximumLength(10)  // CONFIRMAT: DB permite 10
            .WithMessage("Codul postal nu poate depasi 10 caractere")
            .Matches(@"^\d{6}$")
            .WithMessage("Codul postal trebuie sa aiba exact 6 cifre")
            .When(x => !string.IsNullOrEmpty(x.Cod_Postal_Domiciliu));

        // ADaUGAT: Validari pentru adresa de resedinta
        RuleFor(x => x.Adresa_Resedinta)
            .MaximumLength(4000)  // DB este nvarchar(MAX), limitare practica
            .WithMessage("Adresa de resedinta nu poate depasi 4000 de caractere")
            .When(x => !string.IsNullOrEmpty(x.Adresa_Resedinta));

        RuleFor(x => x.Judet_Resedinta)
            .MaximumLength(50)  // CONFIRMAT: DB permite 50
            .WithMessage("Judetul de resedinta nu poate depasi 50 de caractere")
            .When(x => !string.IsNullOrEmpty(x.Judet_Resedinta));

        RuleFor(x => x.Oras_Resedinta)
            .MaximumLength(100)  // CONFIRMAT: DB permite 100
            .WithMessage("Orasul de resedinta nu poate depasi 100 de caractere")
            .When(x => !string.IsNullOrEmpty(x.Oras_Resedinta));

        RuleFor(x => x.Cod_Postal_Resedinta)
            .MaximumLength(10)  // CONFIRMAT: DB permite 10
            .WithMessage("Codul postal de resedinta nu poate depasi 10 caractere")
            .Matches(@"^\d{6}$")
            .WithMessage("Codul postal de resedinta trebuie sa aiba exact 6 cifre")
            .When(x => !string.IsNullOrEmpty(x.Cod_Postal_Resedinta));

        // === REGULI PENTRU DATE PROFESIONALE ===

        RuleFor(x => x.Functia)
            .NotEmpty()
            .WithMessage("Functia este obligatorie")
            .Length(2, 100)  // CONFIRMAT: DB permite 100
            .WithMessage("Functia trebuie sa aiba intre 2 si 100 de caractere");

        RuleFor(x => x.Departament)
            .NotNull()
            .WithMessage("Departamentul este obligatoriu")
            .IsInEnum()
            .WithMessage("Departamentul selectat nu este valid");

        // === REGULI PENTRU DOCUMENTE DE IDENTITATE ===

        RuleFor(x => x.Serie_CI)
            .MaximumLength(10)  // CONFIRMAT: DB permite 10
            .WithMessage("Seria CI nu poate depasi 10 caractere")
            .Matches(@"^[A-Z]{2}$")
            .WithMessage("Seria CI trebuie sa aiba 2 litere mari (ex: AB)")
            .When(x => !string.IsNullOrEmpty(x.Serie_CI));

        RuleFor(x => x.Numar_CI)
            .MaximumLength(20)  // CONFIRMAT: DB permite 20
            .WithMessage("Numarul CI nu poate depasi 20 caractere")
            .Matches(@"^\d{6}$")
            .WithMessage("Numarul CI trebuie sa aiba exact 6 cifre")
            .When(x => !string.IsNullOrEmpty(x.Numar_CI));

        RuleFor(x => x.Eliberat_CI_De)
            .MaximumLength(100)  // CONFIRMAT: DB permite 100
            .WithMessage("'Eliberat CI de' nu poate depasi 100 de caractere")
            .When(x => !string.IsNullOrEmpty(x.Eliberat_CI_De));

        RuleFor(x => x.Data_Eliberare_CI)
            .LessThanOrEqualTo(DateTime.Today)
            .WithMessage("Data eliberarii CI nu poate fi in viitor")
            .When(x => x.Data_Eliberare_CI.HasValue);

        RuleFor(x => x.Valabil_CI_Pana)
            .GreaterThan(x => x.Data_Eliberare_CI)
            .WithMessage("Data expirarii CI trebuie sa fie dupa data eliberarii")
            .When(x => x.Data_Eliberare_CI.HasValue && x.Valabil_CI_Pana.HasValue);

        // === REGULI PENTRU STATUS sI METADATA ===

        RuleFor(x => x.Status_Angajat)
            .IsInEnum()
            .WithMessage("Statusul angajatului nu este valid");

        RuleFor(x => x.Observatii)
            .MaximumLength(4000)  // DB este nvarchar(MAX), limitare practica pentru performanta
            .WithMessage("Observatiile nu pot depasi 4000 de caractere");

        // ADaUGAT: Validari pentru campurile de audit
        RuleFor(x => x.Creat_De)
            .MaximumLength(50)  // CONFIRMAT: DB permite 50
            .WithMessage("'Creat de' nu poate depasi 50 de caractere")
            .When(x => !string.IsNullOrEmpty(x.Creat_De));

        RuleFor(x => x.Modificat_De)
            .MaximumLength(50)  // CONFIRMAT: DB permite 50
            .WithMessage("'Modificat de' nu poate depasi 50 de caractere")
            .When(x => !string.IsNullOrEmpty(x.Modificat_De));

        // === REGULI COMPLEXE DE BUSINESS ===
        
        // Verificare consistenta telefon - trebuie sa aiba cel putin unul
        RuleFor(x => x)
            .Must(HaveAtLeastOnePhone)
            .WithMessage("Trebuie sa existe cel putin un numar de telefon (personal sau serviciu)")
            .WithName("Contact");

        // Verificare consistenta CI - daca exista seria, trebuie sa existe si numarul
        RuleFor(x => x)
            .Must(HaveCompleteCI)
            .WithMessage("Daca este completata seria CI, trebuie completat si numarul CI")
            .WithName("Documente");

        // ADaUGAT: Validare consistenta stare civila (nvarchar(100) in DB)
        RuleFor(x => x.Stare_Civila)
            .IsInEnum()
            .WithMessage("Starea civila selectata nu este valida")
            .When(x => x.Stare_Civila.HasValue);
    }

    /// <summary>
    /// Valideaza CNP-ul conform algoritmului oficial romanesc
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
    /// Verifica daca data nasterii este intr-un interval valid
    /// </summary>
    private bool BeValidBirthDate(DateTime birthDate)
    {
        return birthDate >= new DateTime(1940, 1, 1) && birthDate <= DateTime.Today;
    }

    /// <summary>
    /// Verifica daca persoana are minimum 16 ani
    /// </summary>
    private bool BeMinimumAge(DateTime birthDate)
    {
        var age = DateTime.Today.Year - birthDate.Year;
        if (birthDate.Date > DateTime.Today.AddYears(-age))
            age--;
        
        return age >= 16;
    }

    /// <summary>
    /// Verifica daca exista cel putin un numar de telefon
    /// </summary>
    private bool HaveAtLeastOnePhone(Personal personal)
    {
        return !string.IsNullOrEmpty(personal.Telefon_Personal) || 
               !string.IsNullOrEmpty(personal.Telefon_Serviciu);
    }

    /// <summary>
    /// Verifica consistenta datelor CI
    /// </summary>
    private bool HaveCompleteCI(Personal personal)
    {
        // Daca nu este completata seria, e OK
        if (string.IsNullOrEmpty(personal.Serie_CI))
            return true;

        // Daca e completata seria, trebuie sa fie completat si numarul
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
            .WithMessage("ID-ul personalului trebuie sa fie un GUID valid");

        // Data crearii trebuie sa fie setata (corectat pentru ora locala)
        RuleFor(x => x.Data_Crearii)
            .NotEmpty()
            .WithMessage("Data crearii este obligatorie")
            .LessThanOrEqualTo(DateTime.Now.AddMinutes(1)) // Buffer de 1 minut pentru sincronizare
            .WithMessage("Data crearii nu poate fi in viitor");

        // Creat_De trebuie sa fie setat
        RuleFor(x => x.Creat_De)
            .NotEmpty()
            .WithMessage("Utilizatorul care creeaza inregistrarea trebuie specificat")
            .Length(2, 50)
            .WithMessage("Numele utilizatorului trebuie sa aiba intre 2 si 50 de caractere");
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
        // Pentru update, ID-ul trebuie sa existe si sa fie valid
        RuleFor(x => x.Id_Personal)
            .NotEmpty()
            .WithMessage("ID-ul personalului este obligatoriu pentru actualizare")
            .Must(BeValidGuid)
            .WithMessage("ID-ul personalului trebuie sa fie un GUID valid");

        // Data ultimei modificari trebuie sa fie setata (corectat pentru ora locala)
        RuleFor(x => x.Data_Ultimei_Modificari)
            .NotEmpty()
            .WithMessage("Data ultimei modificari este obligatorie")
            .LessThanOrEqualTo(DateTime.Now.AddMinutes(1)) // Buffer de 1 minut pentru sincronizare
            .WithMessage("Data ultimei modificari nu poate fi in viitor");

        // Modificat_De trebuie sa fie setat
        RuleFor(x => x.Modificat_De)
            .NotEmpty()
            .WithMessage("Utilizatorul care modifica inregistrarea trebuie specificat")
            .Length(2, 50)
            .WithMessage("Numele utilizatorului trebuie sa aiba intre 2 si 50 de caractere");

        // Data crearii trebuie sa existe si sa fie anterioara datei de modificare
        RuleFor(x => x.Data_Crearii)
            .NotEmpty()
            .WithMessage("Data crearii trebuie sa existe pentru actualizare")
            .LessThan(x => x.Data_Ultimei_Modificari)
            .WithMessage("Data crearii trebuie sa fie anterioara datei ultimei modificari");

        // Creat_De trebuie sa existe
        RuleFor(x => x.Creat_De)
            .NotEmpty()
            .WithMessage("Creatorul original al inregistrarii trebuie sa existe");
    }

    private bool BeValidGuid(Guid id)
    {
        return id != Guid.Empty;
    }
}
