using FluentValidation;

namespace ValyanClinic.Application.Features.ProgramareManagement.Queries.GetUpcomingProgramari;

public class GetUpcomingProgramariQueryValidator : AbstractValidator<GetUpcomingProgramariQuery>
{
    public GetUpcomingProgramariQueryValidator()
    {
        RuleFor(x => x.Days)
       .GreaterThan(0).WithMessage("Numărul de zile trebuie să fie mai mare decât 0.")
    .LessThanOrEqualTo(365).WithMessage("Numărul de zile nu poate depăși 365.");

      When(x => x.DoctorID.HasValue, () =>
        {
      RuleFor(x => x.DoctorID)
    .NotEqual(Guid.Empty).WithMessage("ID-ul medicului nu este valid.");
  });
    }
}
