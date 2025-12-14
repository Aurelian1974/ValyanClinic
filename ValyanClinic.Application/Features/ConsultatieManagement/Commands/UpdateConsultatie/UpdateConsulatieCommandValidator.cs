using FluentValidation;

namespace ValyanClinic.Application.Features.ConsultatieManagement.Commands.UpdateConsultatie;

/// <summary>
/// Validator pentru UpdateConsulatieCommand folosind FluentValidation
/// Validari similare cu Create dar include si ConsultatieID
/// </summary>
public class UpdateConsulatieCommandValidator : AbstractValidator<UpdateConsulatieCommand>
{
    public UpdateConsulatieCommandValidator()
    {
        // ==================== CAMPURI OBLIGATORII ====================
        
        RuleFor(x => x.ConsultatieID)
            .NotEmpty().WithMessage("ID-ul consultatiei este obligatoriu.")
            .NotEqual(Guid.Empty).WithMessage("ID-ul consultatiei nu este valid.");

        RuleFor(x => x.ProgramareID)
            .NotEmpty().WithMessage("ID-ul programarii este obligatoriu.")
            .NotEqual(Guid.Empty).WithMessage("ID-ul programarii nu este valid.");

        RuleFor(x => x.PacientID)
            .NotEmpty().WithMessage("ID-ul pacientului este obligatoriu.")
            .NotEqual(Guid.Empty).WithMessage("ID-ul pacientului nu este valid.");

        RuleFor(x => x.MedicID)
            .NotEmpty().WithMessage("ID-ul medicului este obligatoriu.")
            .NotEqual(Guid.Empty).WithMessage("ID-ul medicului nu este valid.");

        RuleFor(x => x.ModificatDe)
            .NotEmpty().WithMessage("ID-ul utilizatorului este obligatoriu.")
            .NotEqual(Guid.Empty).WithMessage("ID-ul utilizatorului nu este valid.");

        RuleFor(x => x.TipConsultatie)
            .NotEmpty().WithMessage("Tipul consultatiei este obligatoriu.")
            .MaximumLength(50).WithMessage("Tipul consultatiei nu poate depasi 50 de caractere.");

        RuleFor(x => x.DataConsultatie)
            .NotEmpty().WithMessage("Data consultatiei este obligatorie.")
            .LessThanOrEqualTo(DateTime.Today).WithMessage("Data consultatiei nu poate fi in viitor.");

        RuleFor(x => x.OraConsultatie)
            .NotEmpty().WithMessage("Ora consultatiei este obligatorie.")
            .Must(ora => ora >= TimeSpan.Zero && ora < TimeSpan.FromHours(24))
            .WithMessage("Ora consultatiei trebuie sa fie intre 00:00 si 23:59.");

        // ==================== BUSINESS LOGIC VALIDATIONS ====================

        // Pacient diferit de Medic
        RuleFor(x => x)
            .Must(x => x.PacientID != x.MedicID)
            .WithMessage("Pacientul si medicul nu pot fi aceeasi persoana.")
            .WithName("PacientID");

        // ==================== VALIDARI OPȚIONALE (CONDIȚIONAL) ====================

        // Semne Vitale - interval valid (identic cu Create)
        When(x => x.Greutate.HasValue, () =>
        {
            RuleFor(x => x.Greutate)
                .GreaterThan(0).WithMessage("Greutatea trebuie sa fie mai mare decat 0 kg.")
                .LessThanOrEqualTo(300).WithMessage("Greutatea nu poate depasi 300 kg.");
        });

        When(x => x.Inaltime.HasValue, () =>
        {
            RuleFor(x => x.Inaltime)
                .GreaterThan(0).WithMessage("Inaltimea trebuie sa fie mai mare decat 0 cm.")
                .LessThanOrEqualTo(250).WithMessage("Inaltimea nu poate depasi 250 cm.");
        });

        When(x => x.Temperatura.HasValue, () =>
        {
            RuleFor(x => x.Temperatura)
                .GreaterThanOrEqualTo(35).WithMessage("Temperatura trebuie sa fie cel putin 35°C.")
                .LessThanOrEqualTo(43).WithMessage("Temperatura nu poate depasi 43°C.");
        });

        When(x => x.Puls.HasValue, () =>
        {
            RuleFor(x => x.Puls)
                .GreaterThan(0).WithMessage("Pulsul trebuie sa fie mai mare decat 0 bpm.")
                .LessThanOrEqualTo(250).WithMessage("Pulsul nu poate depasi 250 bpm.");
        });

        When(x => x.FreccventaRespiratorie.HasValue, () =>
        {
            RuleFor(x => x.FreccventaRespiratorie)
                .GreaterThan(0).WithMessage("Frecventa respiratorie trebuie sa fie mai mare decat 0/min.")
                .LessThanOrEqualTo(60).WithMessage("Frecventa respiratorie nu poate depasi 60/min.");
        });

        When(x => x.SaturatieO2.HasValue, () =>
        {
            RuleFor(x => x.SaturatieO2)
                .GreaterThanOrEqualTo(0).WithMessage("Saturatia O2 trebuie sa fie intre 0 si 100%.")
                .LessThanOrEqualTo(100).WithMessage("Saturatia O2 trebuie sa fie intre 0 si 100%.");
        });

        When(x => !string.IsNullOrEmpty(x.TensiuneArteriala), () =>
        {
            RuleFor(x => x.TensiuneArteriala)
                .Matches(@"^\d{2,3}/\d{2,3}$")
                .WithMessage("Tensiunea arteriala trebuie sa fie in format '120/80'.");
        });

        // Status validare
        RuleFor(x => x.Status)
            .NotEmpty().WithMessage("Statusul consultatiei este obligatoriu.")
            .Must(status => new[] { "In desfasurare", "Finalizata", "Anulata" }
                .Contains(status, StringComparer.OrdinalIgnoreCase))
            .WithMessage("Statusul trebuie sa fie unul dintre: In desfasurare, Finalizata, Anulata.");

        // DurataMinute - daca consultatie finalizata, durata trebuie > 0
        When(x => x.Status == "Finalizata", () =>
        {
            RuleFor(x => x.DurataMinute)
                .GreaterThan(0).WithMessage("Durata consultatiei trebuie sa fie mai mare decat 0 minute pentru consultatie finalizata.");
        });
    }
}
