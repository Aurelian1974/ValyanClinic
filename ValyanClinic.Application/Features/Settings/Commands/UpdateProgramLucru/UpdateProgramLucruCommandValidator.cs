using FluentValidation;

namespace ValyanClinic.Application.Features.Settings.Commands.UpdateProgramLucru;

public class UpdateProgramLucruCommandValidator : AbstractValidator<UpdateProgramLucruCommand>
{
    public UpdateProgramLucruCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("ID-ul programului este obligatoriu.");

        RuleFor(x => x.ZiSaptamana)
            .InclusiveBetween(0, 6)
            .WithMessage("Ziua săptămânii trebuie să fie între 0 (Duminică) și 6 (Sâmbătă).");

        RuleFor(x => x.ModificatDe)
            .NotEmpty()
            .WithMessage("ID-ul utilizatorului care modifică este obligatoriu.");

        // Validare ora de început când clinica este deschisă
        When(x => x.EsteDeschis, () =>
        {
            RuleFor(x => x.OraInceput)
                .NotEmpty()
                .WithMessage("Ora de început este obligatorie când clinica este deschisă.")
                .Must(BeValidTimeFormat)
                .WithMessage("Formatul orei de început este invalid. Folosiți formatul HH:mm.");

            RuleFor(x => x.OraSfarsit)
                .NotEmpty()
                .WithMessage("Ora de sfârșit este obligatorie când clinica este deschisă.")
                .Must(BeValidTimeFormat)
                .WithMessage("Formatul orei de sfârșit este invalid. Folosiți formatul HH:mm.");

            RuleFor(x => x)
                .Must(x => BeValidTimeRange(x.OraInceput, x.OraSfarsit))
                .WithMessage("Ora de sfârșit trebuie să fie după ora de început.")
                .WithName("OraSfarsit");
        });

        // Validare pauză (opțională)
        When(x => !string.IsNullOrEmpty(x.PauzaInceput), () =>
        {
            RuleFor(x => x.PauzaInceput)
                .Must(BeValidTimeFormat)
                .WithMessage("Formatul orei de început pauză este invalid.");

            RuleFor(x => x.PauzaSfarsit)
                .NotEmpty()
                .WithMessage("Ora de sfârșit pauză este obligatorie dacă există oră de început pauză.")
                .Must(BeValidTimeFormat)
                .WithMessage("Formatul orei de sfârșit pauză este invalid.");

            RuleFor(x => x)
                .Must(x => BeValidTimeRange(x.PauzaInceput, x.PauzaSfarsit))
                .WithMessage("Ora de sfârșit pauză trebuie să fie după ora de început pauză.")
                .WithName("PauzaSfarsit");
        });

        // Observații - lungime maximă
        RuleFor(x => x.Observatii)
            .MaximumLength(500)
            .WithMessage("Observațiile nu pot depăși 500 de caractere.");
    }

    private static bool BeValidTimeFormat(string? time)
    {
        if (string.IsNullOrEmpty(time)) return false;
        return TimeSpan.TryParse(time, out _);
    }

    private static bool BeValidTimeRange(string? start, string? end)
    {
        if (string.IsNullOrEmpty(start) || string.IsNullOrEmpty(end)) return false;
        if (!TimeSpan.TryParse(start, out var startTime)) return false;
        if (!TimeSpan.TryParse(end, out var endTime)) return false;
        return endTime > startTime;
    }
}
