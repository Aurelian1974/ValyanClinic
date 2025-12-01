using FluentValidation;

namespace ValyanClinic.Application.Features.ProgramareManagement.Queries.CheckProgramareConflict;

/// <summary>
/// Validator pentru CheckProgramareConflictQuery.
/// </summary>
public class CheckProgramareConflictQueryValidator : AbstractValidator<CheckProgramareConflictQuery>
{
    public CheckProgramareConflictQueryValidator()
    {
        RuleFor(x => x.DoctorID)
     .NotEmpty().WithMessage("ID-ul medicului este obligatoriu.")
            .NotEqual(Guid.Empty).WithMessage("ID-ul medicului nu este valid.");

        RuleFor(x => x.DataProgramare)
            .NotEmpty().WithMessage("Data programării este obligatorie.");

        RuleFor(x => x.OraInceput)
   .NotEmpty().WithMessage("Ora de început este obligatorie.");

        RuleFor(x => x.OraSfarsit)
       .NotEmpty().WithMessage("Ora de sfârșit este obligatorie.");

        RuleFor(x => x)
      .Must(x => x.OraSfarsit > x.OraInceput)
.WithMessage("Ora de sfârșit trebuie să fie după ora de început.");
    }
}
