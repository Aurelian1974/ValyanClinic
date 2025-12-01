using FluentValidation;

namespace ValyanClinic.Application.Features.ProgramareManagement.Queries.GetProgramareList;

/// <summary>
/// Validator pentru GetProgramareListQuery.
/// Validează parametrii de paginare, filtrare și sortare.
/// </summary>
public class GetProgramareListQueryValidator : AbstractValidator<GetProgramareListQuery>
{
    public GetProgramareListQueryValidator()
    {
        // ==================== VALIDĂRI PAGINARE ====================

        RuleFor(x => x.PageNumber)
     .GreaterThan(0)
         .WithMessage("Numărul paginii trebuie să fie mai mare decât 0.");

        RuleFor(x => x.PageSize)
            .GreaterThan(0)
 .WithMessage("Dimensiunea paginii trebuie să fie mai mare decât 0.")
          .LessThanOrEqualTo(200)
       .WithMessage("Dimensiunea paginii nu poate depăși 200 de elemente.");

        // ==================== VALIDĂRI FILTRE OPȚIONALE ====================

        // Validare FilterDoctorID (dacă e furnizat)
        When(x => x.FilterDoctorID.HasValue, () =>
       {
           RuleFor(x => x.FilterDoctorID)
             .NotEqual(Guid.Empty)
          .WithMessage("ID-ul medicului nu este valid.");
       });

        // Validare FilterPacientID (dacă e furnizat)
        When(x => x.FilterPacientID.HasValue, () =>
 {
     RuleFor(x => x.FilterPacientID)
        .NotEqual(Guid.Empty)
      .WithMessage("ID-ul pacientului nu este valid.");
 });

        // Validare interval date
        When(x => x.FilterDataStart.HasValue && x.FilterDataEnd.HasValue, () =>
          {
              RuleFor(x => x)
              .Must(x => x.FilterDataStart!.Value <= x.FilterDataEnd!.Value)
        .WithMessage("Data de început trebuie să fie înainte de data de sfârșit.")
                 .WithName("FilterDataStart");
          });

        // Validare GlobalSearchText (min 2 caractere dacă e furnizat)
        When(x => !string.IsNullOrEmpty(x.GlobalSearchText), () =>
   {
       RuleFor(x => x.GlobalSearchText)
       .MinimumLength(2)
     .WithMessage("Textul de căutare trebuie să conțină cel puțin 2 caractere.");
   });

        // ==================== VALIDĂRI SORTARE ====================

        // Validare SortColumn (whitelist)
        RuleFor(x => x.SortColumn)
          .Must(col => new[] { "DataProgramare", "OraInceput", "Status", "PacientNume", "DoctorNume", "DataCreare" }
            .Contains(col, StringComparer.OrdinalIgnoreCase))
               .WithMessage("Coloana de sortare trebuie să fie una dintre: DataProgramare, OraInceput, Status, PacientNume, DoctorNume, DataCreare.");

        // Validare SortDirection (ASC sau DESC)
        RuleFor(x => x.SortDirection)
       .Must(dir => new[] { "ASC", "DESC" }
    .Contains(dir, StringComparer.OrdinalIgnoreCase))
   .WithMessage("Direcția de sortare trebuie să fie ASC sau DESC.");

        // ==================== VALIDĂRI STATUS & TIP (OPȚIONAL) ====================

        // Validare FilterStatus (dacă e furnizat)
        When(x => !string.IsNullOrEmpty(x.FilterStatus), () =>
      {
          RuleFor(x => x.FilterStatus)
        .Must(status => new[] { "Programata", "Confirmata", "CheckedIn", "InConsultatie", "Finalizata", "Anulata", "NoShow" }
           .Contains(status, StringComparer.OrdinalIgnoreCase))
         .WithMessage("Statusul trebuie să fie unul dintre: Programata, Confirmata, CheckedIn, InConsultatie, Finalizata, Anulata, NoShow.");
      });

        // Validare FilterTipProgramare (dacă e furnizat)
        When(x => !string.IsNullOrEmpty(x.FilterTipProgramare), () =>
        {
            RuleFor(x => x.FilterTipProgramare)
   .Must(tip => new[] { "ConsultatieInitiala", "ControlPeriodic", "Consultatie", "Investigatie",
  "Procedura", "Urgenta", "Telemedicina", "LaDomiciliu" }
       .Contains(tip, StringComparer.OrdinalIgnoreCase))
   .WithMessage("Tipul programării trebuie să fie unul dintre: ConsultatieInitiala, ControlPeriodic, Consultatie, Investigatie, Procedura, Urgenta, Telemedicina, LaDomiciliu.");
        });
    }
}
