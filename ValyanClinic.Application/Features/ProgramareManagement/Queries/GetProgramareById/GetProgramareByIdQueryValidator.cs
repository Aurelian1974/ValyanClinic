using FluentValidation;

namespace ValyanClinic.Application.Features.ProgramareManagement.Queries.GetProgramareById;

/// <summary>
/// Validator pentru GetProgramareByIdQuery.
/// </summary>
public class GetProgramareByIdQueryValidator : AbstractValidator<GetProgramareByIdQuery>
{
    public GetProgramareByIdQueryValidator()
    {
        RuleFor(x => x.ProgramareID)
      .NotEmpty()
     .WithMessage("ID-ul programării este obligatoriu.")
        .NotEqual(Guid.Empty)
       .WithMessage("ID-ul programării nu este valid.");
    }
}
