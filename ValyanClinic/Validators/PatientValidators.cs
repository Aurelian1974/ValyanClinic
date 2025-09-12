using FluentValidation;
using ValyanClinic.Components.Pages.Models;

namespace ValyanClinic.Components.Pages.Validators;

public class PatientSearchRequestValidator : AbstractValidator<PatientSearchRequest>
{
    public PatientSearchRequestValidator()
    {
        RuleFor(x => x.PageNumber)
            .GreaterThan(0)
            .WithMessage("Numarul paginii trebuie sa fie mai mare decat 0");

        RuleFor(x => x.PageSize)
            .InclusiveBetween(5, 100)
            .WithMessage("Dimensiunea paginii trebuie sa fie intre 5 si 100");

        RuleFor(x => x.SearchTerm)
            .MaximumLength(100)
            .WithMessage("Termenul de cautare nu poate depasi 100 de caractere")
            .Must(BeValidSearchTerm)
            .WithMessage("Termenul de cautare contine caractere nevalide");
    }

    private bool BeValidSearchTerm(string? searchTerm)
    {
        if (string.IsNullOrEmpty(searchTerm))
            return true;

        // Check for potentially dangerous characters
        var invalidChars = new[] { '<', '>', '"', '\'', '&', ';' };
        return !invalidChars.Any(searchTerm.Contains);
    }
}

public class PatientCreateRequestValidator : AbstractValidator<PatientListModel>
{
    public PatientCreateRequestValidator()
    {
        RuleFor(x => x.FullName)
            .NotEmpty()
            .WithMessage("Numele complet este obligatoriu")
            .MaximumLength(200)
            .WithMessage("Numele nu poate depasi 200 de caractere")
            .Must(BeValidName)
            .WithMessage("Numele trebuie sa contina doar litere, spatii si caractere romanesti");

        RuleFor(x => x.Email)
            .EmailAddress()
            .When(x => !string.IsNullOrEmpty(x.Email))
            .WithMessage("Adresa de email nu este valida")
            .MaximumLength(254)
            .WithMessage("Adresa de email nu poate depasi 254 de caractere");

        RuleFor(x => x.Phone)
            .NotEmpty()
            .WithMessage("Numarul de telefon este obligatoriu")
            .Matches(@"^0[2-9]\d{8}$")
            .WithMessage("Numarul de telefon trebuie sa fie in format romanesc valid (ex: 0721234567)");

        RuleFor(x => x.CNP)
            .NotEmpty()
            .WithMessage("CNP-ul este obligatoriu")
            .Length(13)
            .WithMessage("CNP-ul trebuie sa aiba exact 13 cifre")
            .Matches(@"^\d{13}$")
            .WithMessage("CNP-ul trebuie sa contina doar cifre")
            .Must(BeValidCNP)
            .WithMessage("CNP-ul nu este valid");

        RuleFor(x => x.Age)
            .InclusiveBetween(0, 150)
            .WithMessage("Varsta trebuie sa fie intre 0 si 150 de ani");

        RuleFor(x => x.Status)
            .IsInEnum()
            .WithMessage("Statusul pacientului nu este valid");
    }

    private bool BeValidName(string? name)
    {
        if (string.IsNullOrEmpty(name))
            return false;

        // Allow letters, spaces, hyphens, and Romanian characters
        return System.Text.RegularExpressions.Regex.IsMatch(name, 
            @"^[a-zA-Z?גמ???ÂÎ??\s\-\.]+$");
    }

    private bool BeValidCNP(string? cnp)
    {
        if (string.IsNullOrEmpty(cnp) || cnp.Length != 13)
            return false;

        if (!cnp.All(char.IsDigit))
            return false;

        // Validate CNP checksum algorithm
        var weights = new[] { 2, 7, 9, 1, 4, 6, 3, 5, 8, 2, 7, 9 };
        var sum = 0;

        for (int i = 0; i < 12; i++)
        {
            sum += (cnp[i] - '0') * weights[i];
        }

        var remainder = sum % 11;
        var checkDigit = remainder < 10 ? remainder : 1;

        return checkDigit == (cnp[12] - '0');
    }
}

public class PatientUpdateRequestValidator : AbstractValidator<PatientListModel>
{
    public PatientUpdateRequestValidator()
    {
        Include(new PatientCreateRequestValidator());

        RuleFor(x => x.Id)
            .GreaterThan(0)
            .WithMessage("ID-ul pacientului trebuie sa fie mai mare decat 0");
    }
}

// Validator for filtering and search operations
public class PatientFilterValidator : AbstractValidator<PatientFilterState>
{
    public PatientFilterValidator()
    {
        RuleFor(x => x.SearchTerm)
            .MaximumLength(100)
            .WithMessage("Termenul de cautare nu poate depasi 100 de caractere")
            .Must(BeValidSearchTerm)
            .WithMessage("Termenul de cautare contine caractere nevalide");

        RuleFor(x => x.SelectedStatus)
            .Must(BeValidStatus)
            .WithMessage("Statusul selectat nu este valid");
    }

    private bool BeValidSearchTerm(string? searchTerm)
    {
        if (string.IsNullOrEmpty(searchTerm))
            return true;

        var invalidChars = new[] { '<', '>', '"', '\'', '&', ';' };
        return !invalidChars.Any(searchTerm.Contains);
    }

    private bool BeValidStatus(string? status)
    {
        if (string.IsNullOrEmpty(status))
            return true;

        return Enum.TryParse<PatientStatus>(status, out _);
    }
}