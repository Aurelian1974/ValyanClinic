using FluentValidation;
using ValyanClinic.Domain.Models;

namespace ValyanClinic.Domain.Validators;

/// <summary>
/// Validator FluentValidation pentru modelul LoginRequest
/// Implementeaza reguli de securitate si validare pentru autentificare
/// </summary>
public class LoginRequestValidator : AbstractValidator<LoginRequest>
{
    public LoginRequestValidator()
    {
        // === REGULI PENTRU USERNAME ===
        
        RuleFor(x => x.Username)
            .NotEmpty()
            .WithMessage("Numele de utilizator este obligatoriu")
            .Length(3, 50)
            .WithMessage("Numele de utilizator trebuie sa aiba intre 3 si 50 de caractere")
            .Must(BeValidUsername)
            .WithMessage("Numele de utilizator contine caractere nepermise");

        // === REGULI PENTRU PASSWORD ===
        
        RuleFor(x => x.Password)
            .NotEmpty()
            .WithMessage("Parola este obligatorie")
            .Length(6, 100)
            .WithMessage("Parola trebuie sa aiba intre 6 si 100 de caractere")
            .Must(BeSecurePassword)
            .WithMessage("Parola trebuie sa contina cel putin o litera mare, o litera mica si o cifra");

        // === REGULI PENTRU REMEMBER ME ===
        
        // RememberMe este boolean, deci nu necesita validare specifica
        // Dar putem adauga logica de business daca e necesar
        RuleFor(x => x.RememberMe)
            .Must(BeValidRememberMeChoice)
            .WithMessage("Optiunea 'Pastreaza-ma logat' nu este valida in contextul curent");
    }

    /// <summary>
    /// Verifica daca username-ul are formatul corect
    /// </summary>
    private bool BeValidUsername(string username)
    {
        if (string.IsNullOrEmpty(username))
            return false;

        // Username poate contine litere, cifre, puncte, cratime si underscore
        // Nu poate incepe sau termina cu caractere speciale
        if (username.StartsWith(".") || username.EndsWith(".") ||
            username.StartsWith("_") || username.EndsWith("_") ||
            username.StartsWith("-") || username.EndsWith("-"))
        {
            return false;
        }

        // Verifica daca contine doar caractere permise
        return username.All(c => char.IsLetterOrDigit(c) || c == '.' || c == '_' || c == '-');
    }

    /// <summary>
    /// Verifica daca parola respecta criteriile de securitate
    /// </summary>
    private bool BeSecurePassword(string password)
    {
        if (string.IsNullOrEmpty(password))
            return false;

        // Verifica daca parola contine:
        // - cel putin o litera mare
        // - cel putin o litera mica  
        // - cel putin o cifra
        bool hasUpperCase = password.Any(char.IsUpper);
        bool hasLowerCase = password.Any(char.IsLower);
        bool hasDigit = password.Any(char.IsDigit);

        return hasUpperCase && hasLowerCase && hasDigit;
    }

    /// <summary>
    /// Verifica daca optiunea RememberMe este valida in contextul curent
    /// </summary>
    private bool BeValidRememberMeChoice(bool rememberMe)
    {
        // in prezent acceptam orice valoare pentru RememberMe
        // Aceasta metoda poate fi extinsa cu logica de business specifica
        // De exemplu: interzicerea RememberMe pe computere publice
        
        return true;
    }
}

/// <summary>
/// Validator pentru schimbarea parolei
/// </summary>
public class ChangePasswordRequestValidator : AbstractValidator<ChangePasswordRequest>
{
    public ChangePasswordRequestValidator()
    {
        RuleFor(x => x.CurrentPassword)
            .NotEmpty()
            .WithMessage("Parola curenta este obligatorie");

        RuleFor(x => x.NewPassword)
            .NotEmpty()
            .WithMessage("Parola noua este obligatorie")
            .Length(8, 100)
            .WithMessage("Parola noua trebuie sa aiba intre 8 si 100 de caractere")
            .Must(BeStrongPassword)
            .WithMessage("Parola noua trebuie sa contina cel putin: o litera mare, o litera mica, o cifra si un caracter special")
            .NotEqual(x => x.CurrentPassword)
            .WithMessage("Parola noua trebuie sa fie diferita de cea curenta");

        RuleFor(x => x.ConfirmNewPassword)
            .NotEmpty()
            .WithMessage("Confirmarea parolei este obligatorie")
            .Equal(x => x.NewPassword)
            .WithMessage("Confirmarea parolei nu corespunde cu parola noua");
    }

    /// <summary>
    /// Verifica daca parola noua este suficient de puternica
    /// </summary>
    private bool BeStrongPassword(string password)
    {
        if (string.IsNullOrEmpty(password))
            return false;

        // Verifica daca parola contine:
        // - cel putin o litera mare
        // - cel putin o litera mica  
        // - cel putin o cifra
        // - cel putin un caracter special
        bool hasUpperCase = password.Any(char.IsUpper);
        bool hasLowerCase = password.Any(char.IsLower);
        bool hasDigit = password.Any(char.IsDigit);
        bool hasSpecialChar = password.Any(c => !char.IsLetterOrDigit(c));

        return hasUpperCase && hasLowerCase && hasDigit && hasSpecialChar;
    }
}

/// <summary>
/// Validator pentru resetarea parolei
/// </summary>
public class ResetPasswordRequestValidator : AbstractValidator<ResetPasswordRequest>
{
    public ResetPasswordRequestValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .WithMessage("Email-ul este obligatoriu")
            .EmailAddress()
            .WithMessage("Formatul email-ului nu este valid")
            .Must(BeValidBusinessEmail)
            .WithMessage("Email-ul trebuie sa apartina domeniului organizatiei");

        RuleFor(x => x.Username)
            .NotEmpty()
            .WithMessage("Numele de utilizator este obligatoriu")
            .Length(3, 50)
            .WithMessage("Numele de utilizator trebuie sa aiba intre 3 si 50 de caractere");
    }

    /// <summary>
    /// Verifica daca email-ul apartine domeniului organizatiei
    /// </summary>
    private bool BeValidBusinessEmail(string email)
    {
        if (string.IsNullOrEmpty(email))
            return false;

        var validDomains = new[]
        {
            "@valyanmed.ro",
            "@valyan.ro"
        };

        return validDomains.Any(domain => email.ToLower().EndsWith(domain));
    }
}

/// <summary>
/// Modele pentru validatoare
/// </summary>
public class ChangePasswordRequest
{
    public string CurrentPassword { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
    public string ConfirmNewPassword { get; set; } = string.Empty;
}

public class ResetPasswordRequest
{
    public string Email { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
}
