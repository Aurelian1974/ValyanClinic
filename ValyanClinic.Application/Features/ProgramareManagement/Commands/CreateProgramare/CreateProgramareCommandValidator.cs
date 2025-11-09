using FluentValidation;

namespace ValyanClinic.Application.Features.ProgramareManagement.Commands.CreateProgramare;

/// <summary>
/// Validator pentru CreateProgramareCommand folosind FluentValidation.
/// Definește toate regulile de validare pentru crearea unei programări.
/// </summary>
public class CreateProgramareCommandValidator : AbstractValidator<CreateProgramareCommand>
{
    public CreateProgramareCommandValidator()
    {
    // ==================== VALIDĂRI CÂMPURI OBLIGATORII ====================

     // ✅ UPDATED: PacientID opțional pentru SlotBlocat
      When(x => x.TipProgramare != "SlotBlocat", () =>
        {
RuleFor(x => x.PacientID)
       .NotEmpty()
     .WithMessage("ID-ul pacientului este obligatoriu.")
          .NotEqual(Guid.Empty)
      .WithMessage("ID-ul pacientului nu este valid.");
        });

        RuleFor(x => x.DoctorID)
            .NotEmpty()
    .WithMessage("ID-ul medicului este obligatoriu.")
      .NotEqual(Guid.Empty)
   .WithMessage("ID-ul medicului nu este valid.");

        RuleFor(x => x.DataProgramare)
            .NotEmpty()
.WithMessage("Data programării este obligatorie.")
          .Must(data => data.Date >= DateTime.Today)
            .WithMessage("Data programării nu poate fi în trecut. Vă rugăm să selectați o dată viitoare.");

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

        RuleFor(x => x.CreatDe)
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

// Validare: Pacient diferit de Doctor (nu poate fi același GUID) - excepție pentru SlotBlocat
        When(x => x.TipProgramare != "SlotBlocat", () =>
      {
       RuleFor(x => x)
        .Must(x => x.PacientID != x.DoctorID)
       .WithMessage("Pacientul și medicul nu pot fi aceeași persoană.")
 .WithName("PacientID");
        });

        // Validare: Data programării nu poate fi cu mai mult de 1 an în viitor
        RuleFor(x => x.DataProgramare)
         .Must(data => data.Date <= DateTime.Today.AddYears(1))
            .WithMessage("Data programării nu poate fi cu mai mult de 1 an în viitor.");

        // ==================== VALIDĂRI OPȚIONALE (CONDIȚIONAT) ====================

        // Validare Status (dacă e furnizat)
        RuleFor(x => x.Status)
            .NotEmpty()
       .WithMessage("Statusul programării este obligatoriu.")
        .Must(status => new[] { "Programata", "Confirmata", "CheckedIn", "InConsultatie", "Finalizata", "Anulata", "NoShow" }
      .Contains(status, StringComparer.OrdinalIgnoreCase))
            .WithMessage("Statusul trebuie să fie unul dintre: Programata, Confirmata, CheckedIn, InConsultatie, Finalizata, Anulata, NoShow.");

  // Validare TipProgramare (dacă e furnizat)
     When(x => !string.IsNullOrEmpty(x.TipProgramare), () =>
        {
    RuleFor(x => x.TipProgramare)
   .Must(tip => new[] { "ConsultatieInitiala", "ControlPeriodic", "Consultatie", "Investigatie", 
    "Procedura", "Urgenta", "Telemedicina", "LaDomiciliu", "SlotBlocat" }
       .Contains(tip, StringComparer.OrdinalIgnoreCase))
             .WithMessage("Tipul programării trebuie să fie unul dintre: ConsultatieInitiala, ControlPeriodic, Consultatie, Investigatie, Procedura, Urgenta, Telemedicina, LaDomiciliu, SlotBlocat.");
  });

        // Validare Observatii (max 1000 caractere)
   When(x => !string.IsNullOrEmpty(x.Observatii), () =>
        {
  RuleFor(x => x.Observatii)
      .MaximumLength(1000)
.WithMessage("Observațiile nu pot depăși 1000 de caractere.");
        });

        // ==================== VALIDĂRI PROGRAM ORAR (BUSINESS HOURS) ====================

    // Validare: Ora de început între 07:00 și 20:00 (program clinică)
        RuleFor(x => x.OraInceput)
            .Must(ora => ora >= TimeSpan.FromHours(7) && ora <= TimeSpan.FromHours(20))
            .WithMessage("Ora de început trebuie să fie între 07:00 și 20:00 (programul clinicii).")
     .When(x => x.TipProgramare != "Urgenta"); // Excepție pentru urgențe

        // Validare: Ora de sfârșit între 07:00 și 21:00
        RuleFor(x => x.OraSfarsit)
    .Must(ora => ora >= TimeSpan.FromHours(7) && ora <= TimeSpan.FromHours(21))
   .WithMessage("Ora de sfârșit trebuie să fie între 07:00 și 21:00 (programul clinicii).")
       .When(x => x.TipProgramare != "Urgenta"); // Excepție pentru urgențe

        // ==================== VALIDĂRI ZILE LUCRĂTOARE ====================

        // Validare: Nu se pot face programări în weekend (Sâmbătă/Duminică) - excepție pentru Urgente
        RuleFor(x => x.DataProgramare)
            .Must(data => data.DayOfWeek != DayOfWeek.Saturday && data.DayOfWeek != DayOfWeek.Sunday)
            .WithMessage("Programările în weekend (Sâmbătă/Duminică) nu sunt permise. Vă rugăm să selectați o zi lucrătoare.")
            .When(x => x.TipProgramare != "Urgenta");
    }
}
