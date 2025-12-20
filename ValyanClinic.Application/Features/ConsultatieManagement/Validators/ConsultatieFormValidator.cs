using FluentValidation;
using ValyanClinic.Application.Features.ConsultatieManagement.DTOs;

namespace ValyanClinic.Application.Features.ConsultatieManagement.Validators;

/// <summary>
/// Validator FluentValidation pentru ConsultatieFormDto
/// Folosit pentru validare client-side în Blazor
/// </summary>
public class ConsultatieFormValidator : AbstractValidator<ConsultatieFormDto>
{
    public ConsultatieFormValidator()
    {
        // ==================== CÂMPURI OBLIGATORII ====================

        RuleFor(x => x.PacientId)
            .NotEmpty().WithMessage("Pacientul este obligatoriu.")
            .NotEqual(Guid.Empty).WithMessage("ID-ul pacientului nu este valid.");

        // ==================== TAB 1: MOTIV PREZENTARE & ANTECEDENTE ====================

        RuleFor(x => x.MotivPrezentare)
            .NotEmpty().WithMessage("Motivul prezentării este obligatoriu.")
            .MaximumLength(1000).WithMessage("Motivul prezentării nu poate depăși 1000 caractere.");

        RuleFor(x => x.AntecedentePatologice)
            .MaximumLength(2000).WithMessage("Antecedentele patologice nu pot depăși 2000 caractere.");

        RuleFor(x => x.TratamenteActuale)
            .MaximumLength(1000).WithMessage("Tratamentele actuale nu pot depăși 1000 caractere.");

        // ==================== TAB 2: EXAMEN CLINIC ====================

        // Greutate
        When(x => x.Greutate.HasValue, () =>
        {
            RuleFor(x => x.Greutate)
                .GreaterThan(0.5m).WithMessage("Greutatea trebuie să fie cel puțin 0.5 kg.")
                .LessThanOrEqualTo(500m).WithMessage("Greutatea nu poate depăși 500 kg.");
        });

        // Înălțime
        When(x => x.Inaltime.HasValue, () =>
        {
            RuleFor(x => x.Inaltime)
                .GreaterThan(30m).WithMessage("Înălțimea trebuie să fie cel puțin 30 cm.")
                .LessThanOrEqualTo(250m).WithMessage("Înălțimea nu poate depăși 250 cm.");
        });

        // Tensiune Sistolică
        When(x => x.TensiuneSistolica.HasValue, () =>
        {
            RuleFor(x => x.TensiuneSistolica)
                .GreaterThanOrEqualTo(50).WithMessage("Tensiunea sistolică trebuie să fie cel puțin 50 mmHg.")
                .LessThanOrEqualTo(300).WithMessage("Tensiunea sistolică nu poate depăși 300 mmHg.");
        });

        // Tensiune Diastolică
        When(x => x.TensiuneDiastolica.HasValue, () =>
        {
            RuleFor(x => x.TensiuneDiastolica)
                .GreaterThanOrEqualTo(30).WithMessage("Tensiunea diastolică trebuie să fie cel puțin 30 mmHg.")
                .LessThanOrEqualTo(200).WithMessage("Tensiunea diastolică nu poate depăși 200 mmHg.");
        });

        // Puls
        When(x => x.Puls.HasValue, () =>
        {
            RuleFor(x => x.Puls)
                .GreaterThanOrEqualTo(30).WithMessage("Pulsul trebuie să fie cel puțin 30 bpm.")
                .LessThanOrEqualTo(250).WithMessage("Pulsul nu poate depăși 250 bpm.");
        });

        // Frecvența Respiratorie
        When(x => x.FreqventaRespiratorie.HasValue, () =>
        {
            RuleFor(x => x.FreqventaRespiratorie)
                .GreaterThanOrEqualTo(5).WithMessage("Frecvența respiratorie trebuie să fie cel puțin 5 resp/min.")
                .LessThanOrEqualTo(60).WithMessage("Frecvența respiratorie nu poate depăși 60 resp/min.");
        });

        // Temperatura
        When(x => x.Temperatura.HasValue, () =>
        {
            RuleFor(x => x.Temperatura)
                .GreaterThanOrEqualTo(32m).WithMessage("Temperatura trebuie să fie cel puțin 32°C.")
                .LessThanOrEqualTo(45m).WithMessage("Temperatura nu poate depăși 45°C.");
        });

        // SpO2
        When(x => x.SpO2.HasValue, () =>
        {
            RuleFor(x => x.SpO2)
                .GreaterThanOrEqualTo(50).WithMessage("SpO2 trebuie să fie cel puțin 50%.")
                .LessThanOrEqualTo(100).WithMessage("SpO2 nu poate depăși 100%.");
        });

        // Examen Obiectiv
        RuleFor(x => x.ExamenObiectiv)
            .MaximumLength(2000).WithMessage("Examenul obiectiv nu poate depăși 2000 caractere.");

        // Investigații Paraclinice
        RuleFor(x => x.InvestigatiiParaclinice)
            .MaximumLength(1500).WithMessage("Investigațiile paraclinice nu pot depăși 1500 caractere.");

        // ==================== TAB 3: DIAGNOSTIC & TRATAMENT ====================

        RuleFor(x => x.DiagnosticPrincipal)
            .NotEmpty().WithMessage("Diagnosticul principal este obligatoriu.")
            .MaximumLength(500).WithMessage("Diagnosticul principal nu poate depăși 500 caractere.");

        RuleFor(x => x.DiagnosticSecundar)
            .MaximumLength(500).WithMessage("Diagnosticul secundar nu poate depăși 500 caractere.");

        RuleFor(x => x.PlanTerapeutic)
            .MaximumLength(2000).WithMessage("Planul terapeutic nu poate depăși 2000 caractere.");

        RuleFor(x => x.Recomandari)
            .MaximumLength(1500).WithMessage("Recomandările nu pot depăși 1500 caractere.");

        // ==================== TAB 4: CONCLUZII ====================

        RuleFor(x => x.Concluzii)
            .MaximumLength(2000).WithMessage("Concluziile nu pot depăși 2000 caractere.");

        RuleFor(x => x.NoteUrmatoareaVizita)
            .MaximumLength(1000).WithMessage("Notele pentru vizita următoare nu pot depăși 1000 caractere.");

        // Data următoarei vizite - trebuie să fie în viitor
        When(x => x.DataUrmatoareiVizite.HasValue, () =>
        {
            RuleFor(x => x.DataUrmatoareiVizite)
                .GreaterThan(DateTime.Today)
                .WithMessage("Data următoarei vizite trebuie să fie în viitor.");
        });

        // ==================== VALIDĂRI COLECȚII ====================

        // Validare diagnostice
        RuleForEach(x => x.DiagnosisList)
            .SetValidator(new DiagnosisCardDtoValidator());

        // Validare medicamente
        RuleForEach(x => x.MedicationList)
            .SetValidator(new MedicationRowDtoValidator());
    }
}

/// <summary>
/// Validator pentru DiagnosisCardDto
/// </summary>
public class DiagnosisCardDtoValidator : AbstractValidator<DiagnosisCardDto>
{
    public DiagnosisCardDtoValidator()
    {
        RuleFor(x => x.Code)
            .MaximumLength(20).WithMessage("Codul ICD-10 nu poate depăși 20 caractere.")
            .Matches(@"^[A-Z]\d{2}(\.\d{1,2})?$")
            .When(x => !string.IsNullOrWhiteSpace(x.Code))
            .WithMessage("Codul ICD-10 trebuie să fie în format valid (ex: J06.9, I10).");

        RuleFor(x => x.Name)
            .MaximumLength(200).WithMessage("Denumirea diagnosticului nu poate depăși 200 caractere.");

        RuleFor(x => x.Details)
            .MaximumLength(500).WithMessage("Detaliile diagnosticului nu pot depăși 500 caractere.");

        RuleFor(x => x.Type)
            .Must(t => t == "Principal" || t == "Secundar")
            .WithMessage("Tipul diagnosticului trebuie să fie 'Principal' sau 'Secundar'.");
    }
}

/// <summary>
/// Validator pentru MedicationRowDto
/// </summary>
public class MedicationRowDtoValidator : AbstractValidator<MedicationRowDto>
{
    public MedicationRowDtoValidator()
    {
        RuleFor(x => x.Name)
            .MaximumLength(100).WithMessage("Numele medicamentului nu poate depăși 100 caractere.");

        RuleFor(x => x.Dose)
            .MaximumLength(50).WithMessage("Doza nu poate depăși 50 caractere.");

        RuleFor(x => x.Frequency)
            .MaximumLength(50).WithMessage("Frecvența nu poate depăși 50 caractere.");

        RuleFor(x => x.Duration)
            .MaximumLength(50).WithMessage("Durata nu poate depăși 50 caractere.");

        RuleFor(x => x.Quantity)
            .MaximumLength(50).WithMessage("Cantitatea nu poate depăși 50 caractere.");

        RuleFor(x => x.Notes)
            .MaximumLength(200).WithMessage("Notele nu pot depăși 200 caractere.");
    }
}
