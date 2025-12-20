using FluentValidation;

namespace ValyanClinic.Application.Features.ConsultatieManagement.Commands.SaveConsultatieDraft;

/// <summary>
/// Validator pentru SaveConsultatieDraftCommand folosind FluentValidation
/// Validari MINIME pentru draft (auto-save) - doar ID-uri obligatorii
/// Permite salvare date partiale pentru WIP (Work In Progress)
/// </summary>
public class SaveConsultatieDraftCommandValidator : AbstractValidator<SaveConsultatieDraftCommand>
{
    public SaveConsultatieDraftCommandValidator()
    {
        // ==================== CAMPURI OBLIGATORII (MINIME pentru draft) ====================
        
        // ✅ ProgramareID este OPȚIONAL pentru draft (consultație poate fi creată fără programare)
        // Dacă este furnizat (HasValue și nu Empty), trebuie să fie valid
        When(x => x.ProgramareID.HasValue && x.ProgramareID.Value != Guid.Empty, () =>
        {
            RuleFor(x => x.ProgramareID)
                .Must(id => id.HasValue && id.Value != Guid.Empty)
                .WithMessage("ID-ul programarii nu este valid.");
        });

        RuleFor(x => x.PacientID)
            .NotEmpty().WithMessage("ID-ul pacientului este obligatoriu.")
            .NotEqual(Guid.Empty).WithMessage("ID-ul pacientului nu este valid.");

        RuleFor(x => x.MedicID)
            .NotEmpty().WithMessage("ID-ul medicului este obligatoriu.")
            .NotEqual(Guid.Empty).WithMessage("ID-ul medicului nu este valid.");

        RuleFor(x => x.CreatDeSauModificatDe)
            .NotEmpty().WithMessage("ID-ul utilizatorului este obligatoriu.")
            .NotEqual(Guid.Empty).WithMessage("ID-ul utilizatorului nu este valid.");

        // ==================== VALIDARI OPȚIONALE (SOFT) ====================
        
        // Doar validari de sanity check pentru valorile furnizate (daca exista)
        
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

        When(x => !string.IsNullOrEmpty(x.TensiuneArteriala), () =>
        {
            RuleFor(x => x.TensiuneArteriala)
                .Matches(@"^\d{2,3}/\d{2,3}$")
                .WithMessage("Tensiunea arteriala trebuie sa fie in format '120/80'.");
        });

        // ⚠️ NOTE: MotivPrezentare si DiagnosticPozitiv sunt opționale pentru draft
        // Vor fi validate strict in FinalizeConsulatieCommand
    }
}
