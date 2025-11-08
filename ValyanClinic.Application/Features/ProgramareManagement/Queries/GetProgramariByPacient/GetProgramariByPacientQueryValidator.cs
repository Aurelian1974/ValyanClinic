using FluentValidation;

namespace ValyanClinic.Application.Features.ProgramareManagement.Queries.GetProgramariByPacient;

public class GetProgramariByPacientQueryValidator : AbstractValidator<GetProgramariByPacientQuery>
{
  public GetProgramariByPacientQueryValidator()
    {
   RuleFor(x => x.PacientID)
       .NotEmpty().WithMessage("ID-ul pacientului este obligatoriu.")
     .NotEqual(Guid.Empty).WithMessage("ID-ul pacientului nu este valid.");
    }
}
