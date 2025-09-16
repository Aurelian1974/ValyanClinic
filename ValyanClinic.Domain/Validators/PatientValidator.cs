using FluentValidation;
using ValyanClinic.Domain.Entities;
using ValyanClinic.Domain.Enums;

namespace ValyanClinic.Domain.Validators;

/// <summary>
/// Validator FluentValidation pentru entitatea Patient
/// Implementează toate regulile de business pentru pacienții clinicii
/// </summary>
public class PatientValidator : AbstractValidator<Patient>
{
    public PatientValidator()
    {
        // === REGULI PENTRU DATE PERSONALE DE BAZĂ ===
        
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

        // === REGULI PENTRU CONTACT ===
        
        RuleFor(x => x.Email)
            .NotEmpty()
            .WithMessage("Email-ul este obligatoriu")
            .EmailAddress()
            .WithMessage("Formatul email-ului nu este valid")
            .Length(5, 100)
            .WithMessage("Email-ul trebuie să aibă între 5 și 100 de caractere");

        RuleFor(x => x.PhoneNumber)
            .NotEmpty()
            .WithMessage("Numărul de telefon este obligatoriu")
            .Matches(@"^(\+40|0)[1-9]\d{8}$")
            .WithMessage("Formatul telefonului nu este valid (ex: 0722123456 sau +40722123456)");

        // === REGULI PENTRU DATE DEMOGRAFICE ===
        
        RuleFor(x => x.DateOfBirth)
            .NotEmpty()
            .WithMessage("Data nașterii este obligatorie")
            .Must(BeValidBirthDate)
            .WithMessage("Data nașterii trebuie să fie între 1900 și acum")
            .Must(BeReasonableAge)
            .WithMessage("Vârsta pacientului trebuie să fie rezonabilă (sub 150 de ani)");

        RuleFor(x => x.Gender)
            .IsInEnum()
            .WithMessage("Genul selectat nu este valid");

        // === REGULI PENTRU CNP ===
        
        RuleFor(x => x.CNP)
            .NotEmpty()
            .WithMessage("CNP-ul este obligatoriu")
            .Length(13)
            .WithMessage("CNP-ul trebuie să aibă exact 13 cifre")
            .Matches(@"^\d{13}$")
            .WithMessage("CNP-ul poate conține doar cifre")
            .Must(BeValidCNP)
            .WithMessage("CNP-ul nu este valid conform algoritmului oficial")
            .Must((patient, cnp) => CNPMatchesDateOfBirthAndGender(cnp, patient.DateOfBirth, patient.Gender))
            .WithMessage("CNP-ul nu corespunde cu data nașterii sau genul specificat");

        // === REGULI PENTRU ADRESĂ ===
        
        RuleFor(x => x.Address)
            .NotEmpty()
            .WithMessage("Adresa este obligatorie")
            .Length(10, 200)
            .WithMessage("Adresa trebuie să aibă între 10 și 200 de caractere");

        // === REGULI PENTRU CONTACT DE URGENȚĂ ===
        
        RuleFor(x => x.EmergencyContactName)
            .NotEmpty()
            .WithMessage("Numele contactului de urgență este obligatoriu")
            .Length(2, 100)
            .WithMessage("Numele contactului de urgență trebuie să aibă între 2 și 100 de caractere")
            .Matches(@"^[A-ZÀ-ÿa-z\s\-']+$")
            .WithMessage("Numele contactului de urgență poate conține doar litere, spații, cratime și apostrofuri");

        RuleFor(x => x.EmergencyContactPhone)
            .NotEmpty()
            .WithMessage("Telefonul contactului de urgență este obligatoriu")
            .Matches(@"^(\+40|0)[1-9]\d{8}$")
            .WithMessage("Formatul telefonului de urgență nu este valid (ex: 0722123456 sau +40722123456)")
            .Must((patient, emergencyPhone) => emergencyPhone != patient.PhoneNumber)
            .WithMessage("Telefonul de urgență trebuie să fie diferit de telefonul pacientului");

        // === REGULI PENTRU INFORMAȚII MEDICALE OPȚIONALE ===
        
        RuleFor(x => x.BloodType)
            .Must(BeValidBloodType)
            .WithMessage("Grupa sanguină nu este validă (ex: A+, B-, AB+, O-)")
            .When(x => !string.IsNullOrEmpty(x.BloodType));

        RuleFor(x => x.Allergies)
            .MaximumLength(1000)
            .WithMessage("Alergiile nu pot depăși 1000 de caractere");

        RuleFor(x => x.MedicalHistory)
            .MaximumLength(5000)
            .WithMessage("Istoricul medical nu poate depăși 5000 de caractere");

        RuleFor(x => x.Notes)
            .MaximumLength(2000)
            .WithMessage("Notele nu pot depăși 2000 de caractere");

        // === REGULI COMPLEXE DE BUSINESS ===
        
        // Verifică că pacienții sub 18 ani au contacte de urgență diferite
        RuleFor(x => x)
            .Must(HaveValidEmergencyContactForMinors)
            .WithMessage("Pacienții sub 18 ani trebuie să aibă un contact de urgență adult (părinți/tutori)")
            .WithName("ContactUrgentaMinori");
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
        return birthDate >= new DateTime(1900, 1, 1) && birthDate <= DateTime.Today;
    }

    /// <summary>
    /// Verifică dacă vârsta este rezonabilă (sub 150 de ani)
    /// </summary>
    private bool BeReasonableAge(DateTime birthDate)
    {
        var age = DateTime.Today.Year - birthDate.Year;
        if (birthDate.Date > DateTime.Today.AddYears(-age))
            age--;
        
        return age <= 150;
    }

    /// <summary>
    /// Verifică dacă CNP-ul corespunde cu data nașterii și genul
    /// </summary>
    private bool CNPMatchesDateOfBirthAndGender(string cnp, DateTime dateOfBirth, Gender gender)
    {
        if (string.IsNullOrEmpty(cnp) || cnp.Length != 13)
            return false;

        // Prima cifra indică sexul și secolul
        int firstDigit = int.Parse(cnp[0].ToString());
        
        // Extragere an, lună, zi din CNP
        int cnpYear = int.Parse(cnp.Substring(1, 2));
        int cnpMonth = int.Parse(cnp.Substring(3, 2));
        int cnpDay = int.Parse(cnp.Substring(5, 2));

        // Determinare secol complet bazat pe prima cifră
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
                fullYear = 2000 + cnpYear; // Rezidenți străini
                break;
            default:
                return false;
        }

        // Verificare dată
        try
        {
            var cnpDate = new DateTime(fullYear, cnpMonth, cnpDay);
            if (cnpDate.Date != dateOfBirth.Date)
                return false;
        }
        catch
        {
            return false; // Data din CNP nu este validă
        }

        // Verificare gen (cifre impare = masculin, cifre pare = feminin)
        bool cnpIsMale = firstDigit % 2 == 1;
        bool genderIsMale = gender == Gender.Male;

        return cnpIsMale == genderIsMale;
    }

    /// <summary>
    /// Verifică dacă grupa sanguină este validă
    /// </summary>
    private bool BeValidBloodType(string? bloodType)
    {
        if (string.IsNullOrEmpty(bloodType))
            return true;

        var validBloodTypes = new[] { "A+", "A-", "B+", "B-", "AB+", "AB-", "O+", "O-" };
        return validBloodTypes.Contains(bloodType.ToUpper());
    }

    /// <summary>
    /// Verifică dacă pacienții minori au contacte de urgență valide
    /// </summary>
    private bool HaveValidEmergencyContactForMinors(Patient patient)
    {
        var age = DateTime.Today.Year - patient.DateOfBirth.Year;
        if (patient.DateOfBirth.Date > DateTime.Today.AddYears(-age))
            age--;

        // Dacă pacientul este sub 18 ani
        if (age < 18)
        {
            // Trebuie să aibă nume și telefon de contact de urgență
            // și să nu fie același cu ale pacientului
            return !string.IsNullOrEmpty(patient.EmergencyContactName) &&
                   !string.IsNullOrEmpty(patient.EmergencyContactPhone) &&
                   patient.EmergencyContactPhone != patient.PhoneNumber &&
                   patient.EmergencyContactName.ToLower() != patient.FullName.ToLower();
        }

        return true; // Pentru adulți, nu e obligatoriu
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
        
        // ID-ul nu trebuie să fie setat manual (se generează automat)
        RuleFor(x => x.Id)
            .Equal(0)
            .WithMessage("ID-ul se generează automat și nu trebuie specificat");

        // Data creării trebuie să fie setată
        RuleFor(x => x.CreatedAt)
            .NotEmpty()
            .WithMessage("Data creării este obligatorie")
            .GreaterThan(DateTime.Now.AddMinutes(-5))
            .WithMessage("Data creării trebuie să fie aproape de momentul curent")
            .LessThanOrEqualTo(DateTime.Now.AddMinutes(1))
            .WithMessage("Data creării nu poate fi în viitor");
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
        
        // ID-ul trebuie să existe și să fie valid
        RuleFor(x => x.Id)
            .GreaterThan(0)
            .WithMessage("ID-ul pacientului trebuie să fie valid pentru actualizare");

        // Data creării nu se poate modifica
        RuleFor(x => x.CreatedAt)
            .NotEmpty()
            .WithMessage("Data creării trebuie să existe pentru actualizare")
            .LessThan(DateTime.Now.AddDays(-1))
            .WithMessage("Data creării pare să fi fost modificată - nu este permis");

        // Data ultimei modificări trebuie să fie setată și să fie după data creării
        RuleFor(x => x.UpdatedAt)
            .GreaterThan(x => x.CreatedAt)
            .WithMessage("Data ultimei modificări trebuie să fie după data creării")
            .LessThanOrEqualTo(DateTime.Now.AddMinutes(1))
            .WithMessage("Data ultimei modificări nu poate fi în viitor")
            .When(x => x.UpdatedAt.HasValue);
    }
}
