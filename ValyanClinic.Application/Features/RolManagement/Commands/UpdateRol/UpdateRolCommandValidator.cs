using FluentValidation;

namespace ValyanClinic.Application.Features.RolManagement.Commands.UpdateRol;

/// <summary>
/// Validator pentru UpdateRolCommand.
/// </summary>
public class UpdateRolCommandValidator : AbstractValidator<UpdateRolCommand>
{
    public UpdateRolCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("ID-ul rolului este obligatoriu.");

        RuleFor(x => x.Denumire)
            .NotEmpty().WithMessage("Denumirea rolului este obligatorie.")
            .MaximumLength(100).WithMessage("Denumirea nu poate depăși 100 de caractere.");

        RuleFor(x => x.Descriere)
            .MaximumLength(500).WithMessage("Descrierea nu poate depăși 500 de caractere.");

        RuleFor(x => x.OrdineAfisare)
            .GreaterThanOrEqualTo(0).WithMessage("Ordinea de afișare trebuie să fie >= 0.");
    }
}
