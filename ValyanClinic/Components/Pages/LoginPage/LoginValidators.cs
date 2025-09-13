using FluentValidation;
using ValyanClinic.Domain.Models;

namespace ValyanClinic.Components.Pages.LoginPage;

/// <summary>
/// FluentValidation pentru LoginRequest - ORGANIZAT ÎN FOLDER LoginPage
/// Înlocuie?te Data Annotations pentru valid?ri mai robuste
/// </summary>
public class LoginRequestValidator : AbstractValidator<LoginRequest>
{
    public LoginRequestValidator()
    {
        RuleFor(x => x.Username)
            .NotEmpty()
            .WithMessage("Numele de utilizator este obligatoriu")
            .Length(3, 50)
            .WithMessage("Numele de utilizator trebuie s? aib? între 3 ?i 50 de caractere")
            .Matches("^[a-zA-Z0-9._-]+$")
            .WithMessage("Numele de utilizator poate con?ine doar litere, cifre, punct, underscore ?i liniu??");

        RuleFor(x => x.Password)
            .NotEmpty()
            .WithMessage("Parola este obligatorie")
            .MinimumLength(6)
            .WithMessage("Parola trebuie s? aib? cel pu?in 6 caractere")
            .MaximumLength(100)
            .WithMessage("Parola nu poate avea mai mult de 100 de caractere");

        // Reguli business suplimentare
        RuleFor(x => x.Username)
            .Must(BeValidUsernameFormat)
            .WithMessage("Numele de utilizator nu poate începe sau se termina cu punct sau underscore");
    }

    private static bool BeValidUsernameFormat(string username)
    {
        if (string.IsNullOrEmpty(username)) return false;
        
        // Nu poate începe sau se termina cu punct sau underscore
        return !username.StartsWith('.') && !username.EndsWith('.') &&
               !username.StartsWith('_') && !username.EndsWith('_');
    }
}