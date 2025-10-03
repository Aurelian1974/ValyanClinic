using FluentValidation;

namespace ValyanClinic.Application.Features.PersonalManagement.Commands.UpdatePersonal;

/// <summary>
/// Validator pentru UpdatePersonalCommand - ALINIAT CU DB REALA
/// </summary>
public class UpdatePersonalCommandValidator : AbstractValidator<UpdatePersonalCommand>
{
    public UpdatePersonalCommandValidator()
    {
        RuleFor(x => x.Id_Personal)
            .NotEmpty().WithMessage("ID-ul angajatului este obligatoriu");

        RuleFor(x => x.Cod_Angajat)
            .NotEmpty().WithMessage("Codul de angajat este obligatoriu")
            .MaximumLength(20).WithMessage("Codul de angajat nu poate depasi 20 caractere");

        RuleFor(x => x.Nume)
            .NotEmpty().WithMessage("Numele este obligatoriu")
            .MaximumLength(100).WithMessage("Numele nu poate depasi 100 caractere");

        RuleFor(x => x.Prenume)
            .NotEmpty().WithMessage("Prenumele este obligatoriu")
            .MaximumLength(100).WithMessage("Prenumele nu poate depasi 100 caractere");

        RuleFor(x => x.CNP)
            .NotEmpty().WithMessage("CNP-ul este obligatoriu")
            .Length(13).WithMessage("CNP-ul trebuie sa aiba exact 13 caractere")
            .Matches("^[0-9]+$").WithMessage("CNP-ul trebuie sa contina doar cifre");

        RuleFor(x => x.Telefon_Personal)
            .Matches(@"^(\+4|0)[0-9]{9}$").WithMessage("Numarul de telefon nu este valid")
            .When(x => !string.IsNullOrEmpty(x.Telefon_Personal));

        RuleFor(x => x.Email_Personal)
            .EmailAddress().WithMessage("Email-ul nu este valid")
            .MaximumLength(100).WithMessage("Email-ul nu poate depasi 100 caractere")
            .When(x => !string.IsNullOrEmpty(x.Email_Personal));

        RuleFor(x => x.Adresa_Domiciliu)
            .NotEmpty().WithMessage("Adresa de domiciliu este obligatorie");

        RuleFor(x => x.Judet_Domiciliu)
            .NotEmpty().WithMessage("Judetul de domiciliu este obligatoriu");

        RuleFor(x => x.Oras_Domiciliu)
            .NotEmpty().WithMessage("Orasul de domiciliu este obligatoriu");

        RuleFor(x => x.Data_Nasterii)
            .NotEmpty().WithMessage("Data nasterii este obligatorie")
            .LessThan(DateTime.Today).WithMessage("Data nasterii trebuie sa fie in trecut");

        RuleFor(x => x.Functia)
            .NotEmpty().WithMessage("Functia este obligatorie")
            .MaximumLength(100).WithMessage("Functia nu poate depasi 100 caractere");

        RuleFor(x => x.ModificatDe)
            .NotEmpty().WithMessage("Utilizatorul care modifica inregistrarea este obligatoriu");
    }
}
