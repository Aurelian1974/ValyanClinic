using FluentValidation;

namespace ValyanClinic.Application.Features.ProgramareManagement.Commands.DeleteProgramare;

/// <summary>
/// Validator pentru DeleteProgramareCommand folosind FluentValidation.
/// Validări simple pentru ștergere.
/// </summary>
public class DeleteProgramareCommandValidator : AbstractValidator<DeleteProgramareCommand>
{
    public DeleteProgramareCommandValidator()
    {
        // ==================== VALIDĂRI CÂMPURI OBLIGATORII ====================

        RuleFor(x => x.ProgramareID)
    .NotEmpty()
          .WithMessage("ID-ul programării este obligatoriu.")
 .NotEqual(Guid.Empty)
       .WithMessage("ID-ul programării nu este valid.");

        RuleFor(x => x.ModificatDe)
             .NotEmpty()
               .WithMessage("ID-ul utilizatorului este obligatoriu.")
           .NotEqual(Guid.Empty)
                    .WithMessage("ID-ul utilizatorului nu este valid.");

        // ==================== VALIDĂRI OPȚIONALE ====================

        // Validare MotivAnulare (dacă e furnizat)
        When(x => !string.IsNullOrEmpty(x.MotivAnulare), () =>
       {
           RuleFor(x => x.MotivAnulare)
.MaximumLength(500)
.WithMessage("Motivul anulării nu poate depăși 500 de caractere.")
.MinimumLength(10)
  .WithMessage("Motivul anulării trebuie să conțină cel puțin 10 caractere pentru claritate.");
       });
    }
}
