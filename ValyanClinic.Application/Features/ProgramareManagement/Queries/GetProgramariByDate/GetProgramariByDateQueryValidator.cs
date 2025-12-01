using FluentValidation;

namespace ValyanClinic.Application.Features.ProgramareManagement.Queries.GetProgramariByDate;

public class GetProgramariByDateQueryValidator : AbstractValidator<GetProgramariByDateQuery>
{
    public GetProgramariByDateQueryValidator()
    {
        RuleFor(x => x.Date)
            .NotEmpty().WithMessage("Data este obligatorie.");

        When(x => x.DoctorID.HasValue, () =>
        {
            RuleFor(x => x.DoctorID)
              .NotEqual(Guid.Empty).WithMessage("ID-ul medicului nu este valid.");
        });
    }
}
