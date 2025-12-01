using FluentValidation;

namespace ValyanClinic.Application.Features.ProgramareManagement.Queries.GetProgramareStatistics;

public class GetProgramareStatisticsQueryValidator : AbstractValidator<GetProgramareStatisticsQuery>
{
    public GetProgramareStatisticsQueryValidator()
    {
        When(x => x.DataStart.HasValue && x.DataEnd.HasValue, () =>
 {
     RuleFor(x => x)
          .Must(x => x.DataStart!.Value <= x.DataEnd!.Value)
       .WithMessage("Data de început trebuie să fie înainte de data de sfârșit.")
  .WithName("DataStart");

     RuleFor(x => x)
        .Must(x => (x.DataEnd!.Value - x.DataStart!.Value).TotalDays <= 365)
        .WithMessage("Intervalul de date pentru statistici nu poate depăși 365 de zile.")
        .WithName("DataEnd");
 });

        When(x => x.DoctorID.HasValue, () =>
             {
                 RuleFor(x => x.DoctorID)
         .NotEqual(Guid.Empty).WithMessage("ID-ul medicului nu este valid.");
             });
    }
}
