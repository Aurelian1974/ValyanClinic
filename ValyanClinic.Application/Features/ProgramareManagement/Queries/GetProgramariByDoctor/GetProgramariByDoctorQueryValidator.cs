using FluentValidation;

namespace ValyanClinic.Application.Features.ProgramareManagement.Queries.GetProgramariByDoctor;

public class GetProgramariByDoctorQueryValidator : AbstractValidator<GetProgramariByDoctorQuery>
{
    public GetProgramariByDoctorQueryValidator()
    {
        RuleFor(x => x.DoctorID)
       .NotEmpty().WithMessage("ID-ul medicului este obligatoriu.")
      .NotEqual(Guid.Empty).WithMessage("ID-ul medicului nu este valid.");

        When(x => x.DataStart.HasValue && x.DataEnd.HasValue, () =>
             {
                 RuleFor(x => x)
          .Must(x => x.DataStart!.Value <= x.DataEnd!.Value)
     .WithMessage("Data de început trebuie să fie înainte de data de sfârșit.");
             });
    }
}
