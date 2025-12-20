using FluentValidation;

namespace ValyanClinic.Application.Features.ConsultatieManagement.Commands.FinalizeConsultatie;

/// <summary>
/// Validator pentru FinalizeConsulatieCommand folosind FluentValidation
/// Validari STRICTE pentru finalizare - campuri critice obligatorii
/// Asigura integritatea datelor pentru consultatie finalizata
/// NOTE: MotivPrezentare si DiagnosticPozitiv sunt validate in SP (stored procedure)
/// </summary>
public class FinalizeConsulatieCommandValidator : AbstractValidator<FinalizeConsulatieCommand>
{
    public FinalizeConsulatieCommandValidator()
    {
        // ==================== CAMPURI OBLIGATORII ====================
        
        RuleFor(x => x.ConsultatieID)
            .NotEmpty().WithMessage("ID-ul consultatiei este obligatoriu.")
            .NotEqual(Guid.Empty).WithMessage("ID-ul consultatiei nu este valid.");

        RuleFor(x => x.ModificatDe)
            .NotEmpty().WithMessage("ID-ul utilizatorului este obligatoriu.")
            .NotEqual(Guid.Empty).WithMessage("ID-ul utilizatorului nu este valid.");

        RuleFor(x => x.DurataMinute)
            .GreaterThan(0).WithMessage("Durata consultatiei trebuie sa fie mai mare decat 0 minute.")
            .LessThanOrEqualTo(480).WithMessage("Durata consultatiei nu poate depasi 480 de minute (8 ore).");

        // ==================== BUSINESS RULES ====================

        // Durata minima 5 minute (consultatie reala)
        RuleFor(x => x.DurataMinute)
            .GreaterThanOrEqualTo(5)
            .WithMessage("Durata consultatiei trebuie sa fie de cel putin 5 minute pentru o consultatie valida.");

        // Durata maxima rezonabila 240 minute (4 ore)
        When(x => x.DurataMinute > 240, () =>
        {
            RuleFor(x => x.DurataMinute)
                .LessThanOrEqualTo(240)
                .WithMessage("Durata consultatiei depaseste 4 ore. Verifica corectitudinea datelor.");
        }).Otherwise(() =>
        {
            // Durata normala - no extra validation
        });

        // ⚠️ NOTE IMPORTANTE:
        // 1. MotivPrezentare si DiagnosticPozitiv sunt validate in sp_Consultatie_Finalize
        //    SP va returna eroare daca lipsesc -> Handler va captura in Result<bool>.Failure
        // 2. Aceasta validare se concentreaza pe parametrii de finalizare (ConsultatieID, DurataMinute)
        // 3. Validarea business logic complexa (status consultatie, status programare) e in SP cu transaction
    }
}
