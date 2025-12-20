using FluentValidation;

namespace ValyanClinic.Application.Features.ConsultatieManagement.Commands.CreateConsultatie;

/// <summary>
/// Validator pentru CreateConsulatieCommand folosind FluentValidation
/// Defineste toate regulile de validare pentru crearea unei consultatii
/// </summary>
public class CreateConsulatieCommandValidator : AbstractValidator<CreateConsulatieCommand>
{
    public CreateConsulatieCommandValidator()
    {
        // ==================== CAMPURI OBLIGATORII ====================
        
        RuleFor(x => x.ProgramareID)
            .NotEmpty().WithMessage("ID-ul programarii este obligatoriu.")
            .NotEqual(Guid.Empty).WithMessage("ID-ul programarii nu este valid.");

        RuleFor(x => x.PacientID)
            .NotEmpty().WithMessage("ID-ul pacientului este obligatoriu.")
            .NotEqual(Guid.Empty).WithMessage("ID-ul pacientului nu este valid.");

        RuleFor(x => x.MedicID)
            .NotEmpty().WithMessage("ID-ul medicului este obligatoriu.")
            .NotEqual(Guid.Empty).WithMessage("ID-ul medicului nu este valid.");

        RuleFor(x => x.CreatDe)
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

        // Motiv Prezentare - recomandat dar nu obligatoriu la creare (draft)
        When(x => !string.IsNullOrEmpty(x.MotivPrezentare), () =>
        {
            RuleFor(x => x.MotivPrezentare)
                .MaximumLength(2000).WithMessage("Motivul prezentarii nu poate depasi 2000 de caractere.");
        });

        // Greutate - daca exista, trebuie in interval valid
        When(x => x.Greutate.HasValue, () =>
        {
            RuleFor(x => x.Greutate)
                .GreaterThan(0).WithMessage("Greutatea trebuie sa fie mai mare decat 0 kg.")
                .LessThanOrEqualTo(300).WithMessage("Greutatea nu poate depasi 300 kg.");
        });

        // Inaltime - daca exista, trebuie in interval valid
        When(x => x.Inaltime.HasValue, () =>
        {
            RuleFor(x => x.Inaltime)
                .GreaterThan(0).WithMessage("Inaltimea trebuie sa fie mai mare decat 0 cm.")
                .LessThanOrEqualTo(250).WithMessage("Inaltimea nu poate depasi 250 cm.");
        });

        // Temperatura - daca exista, trebuie in interval valid
        When(x => x.Temperatura.HasValue, () =>
        {
            RuleFor(x => x.Temperatura)
                .GreaterThanOrEqualTo(35).WithMessage("Temperatura trebuie sa fie cel putin 35°C.")
                .LessThanOrEqualTo(43).WithMessage("Temperatura nu poate depasi 43°C.");
        });

        // Puls - daca exista, trebuie in interval valid
        When(x => x.Puls.HasValue, () =>
        {
            RuleFor(x => x.Puls)
                .GreaterThan(0).WithMessage("Pulsul trebuie sa fie mai mare decat 0 bpm.")
                .LessThanOrEqualTo(250).WithMessage("Pulsul nu poate depasi 250 bpm.");
        });

        // Frecventa Respiratorie - daca exista, trebuie in interval valid
        When(x => x.FreccventaRespiratorie.HasValue, () =>
        {
            RuleFor(x => x.FreccventaRespiratorie)
                .GreaterThan(0).WithMessage("Frecventa respiratorie trebuie sa fie mai mare decat 0/min.")
                .LessThanOrEqualTo(60).WithMessage("Frecventa respiratorie nu poate depasi 60/min.");
        });

        // Saturatie O2 - daca exista, trebuie intre 0-100%
        When(x => x.SaturatieO2.HasValue, () =>
        {
            RuleFor(x => x.SaturatieO2)
                .GreaterThanOrEqualTo(0).WithMessage("Saturatia O2 trebuie sa fie intre 0 si 100%.")
                .LessThanOrEqualTo(100).WithMessage("Saturatia O2 trebuie sa fie intre 0 si 100%.");
        });

        // Tensiune Arteriala - format validare
        When(x => !string.IsNullOrEmpty(x.TensiuneArteriala), () =>
        {
            RuleFor(x => x.TensiuneArteriala)
                .Matches(@"^\d{2,3}/\d{2,3}$")
                .WithMessage("Tensiunea arteriala trebuie sa fie in format '120/80'.");
        });

        // Status - daca furnizat, trebuie din lista valida
        RuleFor(x => x.Status)
            .NotEmpty().WithMessage("Statusul consultatiei este obligatoriu.")
            .Must(status => new[] { "In desfasurare", "Finalizata", "Anulata" }
                .Contains(status, StringComparer.OrdinalIgnoreCase))
            .WithMessage("Statusul trebuie sa fie unul dintre: In desfasurare, Finalizata, Anulata.");
    }
}
