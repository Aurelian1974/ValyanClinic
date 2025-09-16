using FluentValidation;
using ValyanClinic.Domain.Models;

namespace ValyanClinic.Domain.Validators;

/// <summary>
/// Validator FluentValidation pentru modelul LoginRequest
/// Implementează reguli de securitate și validare pentru autentificare
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
            .WithMessage("Numele de utilizator trebuie să aibă între 3 și 50 de caractere")
            .Must(BeValidUsername)
            .WithMessage("Numele de utilizator conține caractere nepermise");

        // === REGULI PENTRU PASSWORD ===
        
        RuleFor(x => x.Password)
            .NotEmpty()
            .WithMessage("Parola este obligatorie")
            .Length(6, 100)
            .WithMessage("Parola trebuie să aibă între 6 și 100 de caractere")
            .Must(BeSecurePassword)
            .WithMessage("Parola trebuie să conțină cel puțin o literă mare, o literă mică și o cifră");

        // === REGULI PENTRU REMEMBER ME ===
        
        // RememberMe este boolean, deci nu necesită validare specifică
        // Dar putem adăuga logică de business dacă e necesar
        RuleFor(x => x.RememberMe)
            .Must(BeValidRememberMeChoice)
            .WithMessage("Opțiunea 'Păstrează-mă logat' nu este validă în contextul curent");
    }

    /// <summary>
    /// Verifică dacă username-ul are formatul corect
    /// </summary>
    private bool BeValidUsername(string username)
    {
        if (string.IsNullOrEmpty(username))
            return false;

        // Username poate conține litere, cifre, puncte, cratime și underscore
        // Nu poate începe sau termina cu caractere speciale
        if (username.StartsWith(".") || username.EndsWith(".") ||
            username.StartsWith("_") || username.EndsWith("_") ||
            username.StartsWith("-") || username.EndsWith("-"))
        {
            return false;
        }

        // Verifică dacă conține doar caractere permise
        return username.All(c => char.IsLetterOrDigit(c) || c == '.' || c == '_' || c == '-');
    }

    /// <summary>
    /// Verifică dacă parola respectă criteriile de securitate
    /// </summary>
    private bool BeSecurePassword(string password)
    {
        if (string.IsNullOrEmpty(password))
            return false;

        // Verifică dacă parola conține:
        // - cel puțin o literă mare
        // - cel puțin o literă mică  
        // - cel puțin o cifră
        bool hasUpperCase = password.Any(char.IsUpper);
        bool hasLowerCase = password.Any(char.IsLower);
        bool hasDigit = password.Any(char.IsDigit);

        return hasUpperCase && hasLowerCase && hasDigit;
    }

    /// <summary>
    /// Verifică dacă opțiunea RememberMe este validă în contextul curent
    /// </summary>
    private bool BeValidRememberMeChoice(bool rememberMe)
    {
        // În prezent acceptăm orice valoare pentru RememberMe
        // Această metodă poate fi extinsă cu logică de business specifică
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
            .WithMessage("Parola curentă este obligatorie");

        RuleFor(x => x.NewPassword)
            .NotEmpty()
            .WithMessage("Parola nouă este obligatorie")
            .Length(8, 100)
            .WithMessage("Parola nouă trebuie să aibă între 8 și 100 de caractere")
            .Must(BeStrongPassword)
            .WithMessage("Parola nouă trebuie să conțină cel puțin: o literă mare, o literă mică, o cifră și un caracter special")
            .NotEqual(x => x.CurrentPassword)
            .WithMessage("Parola nouă trebuie să fie diferită de cea curentă");

        RuleFor(x => x.ConfirmNewPassword)
            .NotEmpty()
            .WithMessage("Confirmarea parolei este obligatorie")
            .Equal(x => x.NewPassword)
            .WithMessage("Confirmarea parolei nu corespunde cu parola nouă");
    }

    /// <summary>
    /// Verifică dacă parola nouă este suficient de puternică
    /// </summary>
    private bool BeStrongPassword(string password)
    {
        if (string.IsNullOrEmpty(password))
            return false;

        // Verifică dacă parola conține:
        // - cel puțin o literă mare
        // - cel puțin o literă mică  
        // - cel puțin o cifră
        // - cel puțin un caracter special
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
            .WithMessage("Email-ul trebuie să aparțină domeniului organizației");

        RuleFor(x => x.Username)
            .NotEmpty()
            .WithMessage("Numele de utilizator este obligatoriu")
            .Length(3, 50)
            .WithMessage("Numele de utilizator trebuie să aibă între 3 și 50 de caractere");
    }

    /// <summary>
    /// Verifică dacă email-ul aparține domeniului organizației
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
