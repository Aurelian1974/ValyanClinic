using FluentValidation;

namespace ValyanClinic.Application.Features.AuthManagement.Commands.Login;

/// <summary>
/// Validator for LoginCommand using FluentValidation.
/// Ensures input data meets security and business requirements before processing.
/// </summary>
public class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    #region Constants

    /// <summary>
    /// Minimum username length
    /// </summary>
    private const int MIN_USERNAME_LENGTH = 3;

    /// <summary>
    /// Maximum username length
    /// </summary>
    private const int MAX_USERNAME_LENGTH = 100;

    /// <summary>
    /// Minimum password length
    /// </summary>
    private const int MIN_PASSWORD_LENGTH = 6;

    /// <summary>
    /// Maximum password length
    /// </summary>
    private const int MAX_PASSWORD_LENGTH = 100;

    #endregion

    /// <summary>
    /// Initializes a new instance of the <see cref="LoginCommandValidator"/> class.
    /// Defines validation rules for login command properties.
    /// </summary>
    public LoginCommandValidator()
    {
        // Username validation
        RuleFor(x => x.Username)
            .NotEmpty()
            .WithMessage("Numele de utilizator este obligatoriu")
            .Length(MIN_USERNAME_LENGTH, MAX_USERNAME_LENGTH)
            .WithMessage($"Numele de utilizator trebuie să aibă între {MIN_USERNAME_LENGTH} și {MAX_USERNAME_LENGTH} caractere")
            .Matches("^[a-zA-Z0-9._@-]+$")
            .WithMessage("Numele de utilizator poate conține doar litere, cifre și caracterele: . _ @ -");

        // Password validation
        RuleFor(x => x.Password)
            .NotEmpty()
            .WithMessage("Parola este obligatorie")
            .Length(MIN_PASSWORD_LENGTH, MAX_PASSWORD_LENGTH)
            .WithMessage($"Parola trebuie să aibă între {MIN_PASSWORD_LENGTH} și {MAX_PASSWORD_LENGTH} caractere");

        // RememberMe is optional, no validation needed

        // ResetPasswordOnFirstLogin is optional, no validation needed
    }
}
