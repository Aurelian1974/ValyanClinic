using FluentValidation;

namespace ValyanClinic.Application.Features.ProgramareManagement.Queries.GetDoctorSchedule;

public class GetDoctorScheduleQueryValidator : AbstractValidator<GetDoctorScheduleQuery>
{
    public GetDoctorScheduleQueryValidator()
    {
        RuleFor(x => x.DoctorID)
   .NotEmpty().WithMessage("ID-ul medicului este obligatoriu.")
      .NotEqual(Guid.Empty).WithMessage("ID-ul medicului nu este valid.");

        RuleFor(x => x.DataStart)
      .NotEmpty().WithMessage("Data de început este obligatorie.");

        RuleFor(x => x.DataEnd)
         .NotEmpty().WithMessage("Data de sfârșit este obligatorie.");

RuleFor(x => x)
    .Must(x => x.DataStart <= x.DataEnd)
     .WithMessage("Data de început trebuie să fie înainte de data de sfârșit.")
      .WithName("DataStart");

  RuleFor(x => x)
  .Must(x => (x.DataEnd - x.DataStart).TotalDays <= 365)
       .WithMessage("Intervalul de date nu poate depăși 365 de zile.")
     .WithName("DataEnd");
    }
}
