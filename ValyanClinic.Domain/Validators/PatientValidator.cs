using FluentValidation;
using ValyanClinic.Domain.Entities;
using ValyanClinic.Domain.Enums;

namespace ValyanClinic.Domain.Validators;

/// <summary>
/// Validator FluentValidation pentru entitatea Patient
/// Implementeaza toate regulile de business pentru pacientii clinicii
/// </summary>
public class PatientValidator : AbstractValidator<Patient>
{
    public PatientValidator()
    {
        // === REGULI PENTRU DATE PERSONALE DE BAZa ===
        
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

        // === REGULI PENTRU CONTACT ===
        
        RuleFor(x => x.Email)
            .NotEmpty()
            .WithMessage("Email-ul este obligatoriu")
            .EmailAddress()
            .WithMessage("Formatul email-ului nu este valid")
            .Length(5, 100)
            .WithMessage("Email-ul trebuie sa aiba intre 5 si 100 de caractere");

        RuleFor(x => x.PhoneNumber)
            .NotEmpty()
            .WithMessage("Numarul de telefon este obligatoriu")
            .Matches(@"^(\+40|0)[1-9]\d{8}$")
            .WithMessage("Formatul telefonului nu este valid (ex: 0722123456 sau +40722123456)");

        // === REGULI PENTRU DATE DEMOGRAFICE ===
        
        RuleFor(x => x.DateOfBirth)
            .NotEmpty()
            .WithMessage("Data nasterii este obligatorie")
            .Must(BeValidBirthDate)
            .WithMessage("Data nasterii trebuie sa fie intre 1900 si acum")
            .Must(BeReasonableAge)
            .WithMessage("Varsta pacientului trebuie sa fie rezonabila (sub 150 de ani)");

        RuleFor(x => x.Gender)
            .IsInEnum()
            .WithMessage("Genul selectat nu este valid");

        // === REGULI PENTRU CNP ===
        
        RuleFor(x => x.CNP)
            .NotEmpty()
            .WithMessage("CNP-ul este obligatoriu")
            .Length(13)
            .WithMessage("CNP-ul trebuie sa aiba exact 13 cifre")
            .Matches(@"^\d{13}$")
            .WithMessage("CNP-ul poate contine doar cifre")
            .Must(BeValidCNP)
            .WithMessage("CNP-ul nu este valid conform algoritmului oficial")
            .Must((patient, cnp) => CNPMatchesDateOfBirthAndGender(cnp, patient.DateOfBirth, patient.Gender))
            .WithMessage("CNP-ul nu corespunde cu data nasterii sau genul specificat");

        // === REGULI PENTRU ADRESa ===
        
        RuleFor(x => x.Address)
            .NotEmpty()
            .WithMessage("Adresa este obligatorie")
            .Length(10, 200)
            .WithMessage("Adresa trebuie sa aiba intre 10 si 200 de caractere");

        // === REGULI PENTRU CONTACT DE URGENta ===
        
        RuleFor(x => x.EmergencyContactName)
            .NotEmpty()
            .WithMessage("Numele contactului de urgenta este obligatoriu")
            .Length(2, 100)
            .WithMessage("Numele contactului de urgenta trebuie sa aiba intre 2 si 100 de caractere")
            .Matches(@"^[A-ZÀ-ÿa-z\s\-']+$")
            .WithMessage("Numele contactului de urgenta poate contine doar litere, spatii, cratime si apostrofuri");

        RuleFor(x => x.EmergencyContactPhone)
            .NotEmpty()
            .WithMessage("Telefonul contactului de urgenta este obligatoriu")
            .Matches(@"^(\+40|0)[1-9]\d{8}$")
            .WithMessage("Formatul telefonului de urgenta nu este valid (ex: 0722123456 sau +40722123456)")
            .Must((patient, emergencyPhone) => emergencyPhone != patient.PhoneNumber)
            .WithMessage("Telefonul de urgenta trebuie sa fie diferit de telefonul pacientului");

        // === REGULI PENTRU INFORMAtII MEDICALE OPtIONALE ===
        
        RuleFor(x => x.BloodType)
            .Must(BeValidBloodType)
            .WithMessage("Grupa sanguina nu este valida (ex: A+, B-, AB+, O-)")
            .When(x => !string.IsNullOrEmpty(x.BloodType));

        RuleFor(x => x.Allergies)
            .MaximumLength(1000)
            .WithMessage("Alergiile nu pot depasi 1000 de caractere");

        RuleFor(x => x.MedicalHistory)
            .MaximumLength(5000)
            .WithMessage("Istoricul medical nu poate depasi 5000 de caractere");

        RuleFor(x => x.Notes)
            .MaximumLength(2000)
            .WithMessage("Notele nu pot depasi 2000 de caractere");

        // === REGULI COMPLEXE DE BUSINESS ===
        
        // Verifica ca pacientii sub 18 ani au contacte de urgenta diferite
        RuleFor(x => x)
            .Must(HaveValidEmergencyContactForMinors)
            .WithMessage("Pacientii sub 18 ani trebuie sa aiba un contact de urgenta adult (parinti/tutori)")
            .WithName("ContactUrgentaMinori");
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
        return birthDate >= new DateTime(1900, 1, 1) && birthDate <= DateTime.Today;
    }

    /// <summary>
    /// Verifica daca varsta este rezonabila (sub 150 de ani)
    /// </summary>
    private bool BeReasonableAge(DateTime birthDate)
    {
        var age = DateTime.Today.Year - birthDate.Year;
        if (birthDate.Date > DateTime.Today.AddYears(-age))
            age--;
        
        return age <= 150;
    }

    /// <summary>
    /// Verifica daca CNP-ul corespunde cu data nasterii si genul
    /// </summary>
    private bool CNPMatchesDateOfBirthAndGender(string cnp, DateTime dateOfBirth, Gender gender)
    {
        if (string.IsNullOrEmpty(cnp) || cnp.Length != 13)
            return false;

        // Prima cifra indica sexul si secolul
        int firstDigit = int.Parse(cnp[0].ToString());
        
        // Extragere an, luna, zi din CNP
        int cnpYear = int.Parse(cnp.Substring(1, 2));
        int cnpMonth = int.Parse(cnp.Substring(3, 2));
        int cnpDay = int.Parse(cnp.Substring(5, 2));

        // Determinare secol complet bazat pe prima cifra
        int fullYear;
        switch (firstDigit)
        {
            case 1:
            case 2:
                fullYear = 1900 + cnpYear;
                break;
            case 3:
            case 4:
                fullYear = 1800 + cnpYear;
                break;
            case 5:
            case 6:
                fullYear = 2000 + cnpYear;
                break;
            case 7:
            case 8:
                fullYear = 2000 + cnpYear; // Rezidenti straini
                break;
            default:
                return false;
        }

        // Verificare data
        try
        {
            var cnpDate = new DateTime(fullYear, cnpMonth, cnpDay);
            if (cnpDate.Date != dateOfBirth.Date)
                return false;
        }
        catch
        {
            return false; // Data din CNP nu este valida
        }

        // Verificare gen (cifre impare = masculin, cifre pare = feminin)
        bool cnpIsMale = firstDigit % 2 == 1;
        bool genderIsMale = gender == Gender.Male;

        return cnpIsMale == genderIsMale;
    }

    /// <summary>
    /// Verifica daca grupa sanguina este valida
    /// </summary>
    private bool BeValidBloodType(string? bloodType)
    {
        if (string.IsNullOrEmpty(bloodType))
            return true;

        var validBloodTypes = new[] { "A+", "A-", "B+", "B-", "AB+", "AB-", "O+", "O-" };
        return validBloodTypes.Contains(bloodType.ToUpper());
    }

    /// <summary>
    /// Verifica daca pacientii minori au contacte de urgenta valide
    /// </summary>
    private bool HaveValidEmergencyContactForMinors(Patient patient)
    {
        var age = DateTime.Today.Year - patient.DateOfBirth.Year;
        if (patient.DateOfBirth.Date > DateTime.Today.AddYears(-age))
            age--;

        // Daca pacientul este sub 18 ani
        if (age < 18)
        {
            // Trebuie sa aiba nume si telefon de contact de urgenta
            // si sa nu fie acelasi cu ale pacientului
            return !string.IsNullOrEmpty(patient.EmergencyContactName) &&
                   !string.IsNullOrEmpty(patient.EmergencyContactPhone) &&
                   patient.EmergencyContactPhone != patient.PhoneNumber &&
                   patient.EmergencyContactName.ToLower() != patient.FullName.ToLower();
        }

        return true; // Pentru adulti, nu e obligatoriu
    }
}

/// <summary>
/// Validator pentru crearea unui nou pacient
/// </summary>
public class PatientCreateValidator : PatientValidator
{
    public PatientCreateValidator()
    {
        // Reguli suplimentare pentru crearea unui nou pacient
        
        // ID-ul nu trebuie sa fie setat manual (se genereaza automat)
        RuleFor(x => x.Id)
            .Equal(0)
            .WithMessage("ID-ul se genereaza automat si nu trebuie specificat");

        // Data crearii trebuie sa fie setata
        RuleFor(x => x.CreatedAt)
            .NotEmpty()
            .WithMessage("Data crearii este obligatorie")
            .GreaterThan(DateTime.Now.AddMinutes(-5))
            .WithMessage("Data crearii trebuie sa fie aproape de momentul curent")
            .LessThanOrEqualTo(DateTime.Now.AddMinutes(1))
            .WithMessage("Data crearii nu poate fi in viitor");
    }
}

/// <summary>
/// Validator pentru actualizarea unui pacient existent
/// </summary>
public class PatientUpdateValidator : PatientValidator
{
    public PatientUpdateValidator()
    {
        // Reguli suplimentare pentru actualizarea unui pacient
        
        // ID-ul trebuie sa existe si sa fie valid
        RuleFor(x => x.Id)
            .GreaterThan(0)
            .WithMessage("ID-ul pacientului trebuie sa fie valid pentru actualizare");

        // Data crearii nu se poate modifica
        RuleFor(x => x.CreatedAt)
            .NotEmpty()
            .WithMessage("Data crearii trebuie sa existe pentru actualizare")
            .LessThan(DateTime.Now.AddDays(-1))
            .WithMessage("Data crearii pare sa fi fost modificata - nu este permis");

        // Data ultimei modificari trebuie sa fie setata si sa fie dupa data crearii
        RuleFor(x => x.UpdatedAt)
            .GreaterThan(x => x.CreatedAt)
            .WithMessage("Data ultimei modificari trebuie sa fie dupa data crearii")
            .LessThanOrEqualTo(DateTime.Now.AddMinutes(1))
            .WithMessage("Data ultimei modificari nu poate fi in viitor")
            .When(x => x.UpdatedAt.HasValue);
    }
}
