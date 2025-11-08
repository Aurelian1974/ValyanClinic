using FluentValidation;

namespace ValyanClinic.Application.Features.ProgramareManagement.Commands.UpdateProgramare;

/// <summary>
/// Validator pentru UpdateProgramareCommand folosind FluentValidation.
/// Similar cu CreateProgramareCommandValidator, dar include și ProgramareID.
/// </summary>
public class UpdateProgramareCommandValidator : AbstractValidator<UpdateProgramareCommand>
{
  public UpdateProgramareCommandValidator()
    {
 // ==================== VALIDĂRI CÂMPURI OBLIGATORII ====================

        RuleFor(x => x.ProgramareID)
  .NotEmpty()
            .WithMessage("ID-ul programării este obligatoriu.")
     .NotEqual(Guid.Empty)
 .WithMessage("ID-ul programării nu este valid.");

   RuleFor(x => x.PacientID)
      .NotEmpty()
         .WithMessage("ID-ul pacientului este obligatoriu.")
    .NotEqual(Guid.Empty)
       .WithMessage("ID-ul pacientului nu este valid.");

        RuleFor(x => x.DoctorID)
   .NotEmpty()
      .WithMessage("ID-ul medicului este obligatoriu.")
      .NotEqual(Guid.Empty)
   .WithMessage("ID-ul medicului nu este valid.");

        RuleFor(x => x.DataProgramare)
      .NotEmpty()
  .WithMessage("Data programării este obligatorie.");
            // NOTE: La UPDATE, permitem date în trecut (pentru corecții istorice)

        RuleFor(x => x.OraInceput)
     .NotEmpty()
       .WithMessage("Ora de început este obligatorie.")
       .Must(ora => ora >= TimeSpan.Zero && ora < TimeSpan.FromHours(24))
        .WithMessage("Ora de început trebuie să fie între 00:00 și 23:59.");

        RuleFor(x => x.OraSfarsit)
            .NotEmpty()
  .WithMessage("Ora de sfârșit este obligatorie.")
      .Must(ora => ora >= TimeSpan.Zero && ora < TimeSpan.FromHours(24))
     .WithMessage("Ora de sfârșit trebuie să fie între 00:00 și 23:59.");

        RuleFor(x => x.Status)
       .NotEmpty()
      .WithMessage("Statusul programării este obligatoriu.");

        RuleFor(x => x.ModificatDe)
            .NotEmpty()
 .WithMessage("ID-ul utilizatorului este obligatoriu.")
  .NotEqual(Guid.Empty)
    .WithMessage("ID-ul utilizatorului nu este valid.");

        // ==================== VALIDĂRI BUSINESS LOGIC ====================

        // Validare: OraSfarsit > OraInceput
        RuleFor(x => x)
     .Must(x => x.OraSfarsit > x.OraInceput)
        .WithMessage("Ora de sfârșit trebuie să fie după ora de început.")
     .WithName("OraSfarsit");

        // Validare: Durata minimă 5 minute
    RuleFor(x => x)
        .Must(x => (x.OraSfarsit - x.OraInceput).TotalMinutes >= 5)
       .WithMessage("Durata programării trebuie să fie de cel puțin 5 minute.")
         .WithName("Durata");

   // Validare: Durata maximă 4 ore
      RuleFor(x => x)
            .Must(x => (x.OraSfarsit - x.OraInceput).TotalHours <= 4)
      .WithMessage("Durata programării nu poate depăși 4 ore.")
   .WithName("Durata");

      // Validare: Pacient diferit de Doctor
        RuleFor(x => x)
    .Must(x => x.PacientID != x.DoctorID)
          .WithMessage("Pacientul și medicul nu pot fi aceeași persoană.")
       .WithName("PacientID");

     // ==================== VALIDĂRI STATUS ====================

        // Validare: Status trebuie să fie unul dintre valorile permise
        RuleFor(x => x.Status)
      .Must(status => new[] { "Programata", "Confirmata", "CheckedIn", "InConsultatie", "Finalizata", "Anulata", "NoShow" }
        .Contains(status, StringComparer.OrdinalIgnoreCase))
  .WithMessage("Statusul trebuie să fie unul dintre: Programata, Confirmata, CheckedIn, InConsultatie, Finalizata, Anulata, NoShow.");

        // ==================== VALIDĂRI OPȚIONALE ====================

  // Validare TipProgramare (dacă e furnizat)
        When(x => !string.IsNullOrEmpty(x.TipProgramare), () =>
        {
            RuleFor(x => x.TipProgramare)
 .Must(tip => new[] { "ConsultatieInitiala", "ControlPeriodic", "Consultatie", "Investigatie", 
        "Procedura", "Urgenta", "Telemedicina", "LaDomiciliu" }
      .Contains(tip, StringComparer.OrdinalIgnoreCase))
 .WithMessage("Tipul programării trebuie să fie unul dintre: ConsultatieInitiala, ControlPeriodic, Consultatie, Investigatie, Procedura, Urgenta, Telemedicina, LaDomiciliu.");
 });

      // Validare Observatii (max 1000 caractere)
        When(x => !string.IsNullOrEmpty(x.Observatii), () =>
 {
   RuleFor(x => x.Observatii)
             .MaximumLength(1000)
      .WithMessage("Observațiile nu pot depăși 1000 de caractere.");
        });

  // ==================== VALIDĂRI PROGRAM ORAR (mai blânde pentru UPDATE) ====================

        // Validare: Ora de început între 07:00 și 20:00 (program clinică)
  // NOTE: Pentru UPDATE, permitem excepții (de exemplu, pentru corecții)
        When(x => x.DataProgramare.Date >= DateTime.Today, () =>
        {
     RuleFor(x => x.OraInceput)
    .Must(ora => ora >= TimeSpan.FromHours(7) && ora <= TimeSpan.FromHours(20))
          .WithMessage("Ora de început trebuie să fie între 07:00 și 20:00 (programul clinicii).")
       .When(x => x.TipProgramare != "Urgenta");

   RuleFor(x => x.OraSfarsit)
 .Must(ora => ora >= TimeSpan.FromHours(7) && ora <= TimeSpan.FromHours(21))
       .WithMessage("Ora de sfârșit trebuie să fie între 07:00 și 21:00 (programul clinicii).")
         .When(x => x.TipProgramare != "Urgenta");
 });

// ==================== VALIDĂRI TRANZIȚII STATUS ====================

      // Validare: Anumite tranziții de status nu sunt permise
        // De exemplu: Finalizata -> Programata (nu se poate reveni)
        // Această validare se poate extinde în funcție de business rules
    }
}
