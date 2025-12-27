using FluentValidation;

namespace ValyanClinic.Application.Validators;

/// <summary>
/// FluentValidation validator pentru PacientFormModel.
/// Conține reguli complexe pentru CNP (validare checksum), telefon (format românesc), etc.
/// </summary>
public class PacientFormModelValidator : AbstractValidator<PacientFormModelDto>
{
    public PacientFormModelValidator()
    {
        // ==================== CÂMPURI OBLIGATORII ====================

        RuleFor(x => x.Nume)
            .NotEmpty()
            .WithMessage("Numele este obligatoriu.")
            .MaximumLength(100)
            .WithMessage("Numele nu poate depăși 100 de caractere.")
            .Matches(@"^[a-zA-ZăâîșțĂÂÎȘȚ\s\-']+$")
            .When(x => !string.IsNullOrEmpty(x.Nume))
            .WithMessage("Numele poate conține doar litere, spații, cratime și apostrofuri.");

        RuleFor(x => x.Prenume)
            .NotEmpty()
            .WithMessage("Prenumele este obligatoriu.")
            .MaximumLength(100)
            .WithMessage("Prenumele nu poate depăși 100 de caractere.")
            .Matches(@"^[a-zA-ZăâîșțĂÂÎȘȚ\s\-']+$")
            .When(x => !string.IsNullOrEmpty(x.Prenume))
            .WithMessage("Prenumele poate conține doar litere, spații, cratime și apostrofuri.");

        RuleFor(x => x.Data_Nasterii)
            .NotEmpty()
            .WithMessage("Data nașterii este obligatorie.")
            .LessThan(DateTime.Now)
            .WithMessage("Data nașterii nu poate fi în viitor.")
            .GreaterThan(DateTime.Now.AddYears(-150))
            .WithMessage("Data nașterii nu este validă.");

        RuleFor(x => x.Sex)
            .NotEmpty()
            .WithMessage("Sexul este obligatoriu.")
            .Must(x => x == "M" || x == "F")
            .WithMessage("Sexul trebuie să fie M sau F.");

        // ==================== CNP - VALIDARE COMPLEXĂ ====================

        RuleFor(x => x.CNP)
            .Length(13)
            .When(x => !string.IsNullOrEmpty(x.CNP))
            .WithMessage("CNP-ul trebuie să conțină exact 13 cifre.")
            .Matches(@"^\d{13}$")
            .When(x => !string.IsNullOrEmpty(x.CNP))
            .WithMessage("CNP-ul poate conține doar cifre.")
            .Must(cnp => ValidateCNPChecksum(cnp))
            .When(x => !string.IsNullOrEmpty(x.CNP) && x.CNP.Length == 13)
            .WithMessage("CNP-ul nu este valid (checksum incorect).");
        
        // Validare consistență CNP cu data nașterii - folosește întregul model
        RuleFor(x => x)
            .Must(ValidateCNPDateConsistency)
            .When(x => !string.IsNullOrEmpty(x.CNP) && x.CNP.Length == 13)
            .WithMessage("CNP-ul nu corespunde cu data nașterii introdusă.");

        // ==================== TELEFON - FORMAT ROMÂNESC ====================

        RuleFor(x => x.Telefon)
            .Matches(@"^(\+40|0040|0)?[0-9]{9}$")
            .When(x => !string.IsNullOrEmpty(x.Telefon))
            .WithMessage("Format telefon invalid. Folosiți formatul: 0721234567 sau +40721234567");

        RuleFor(x => x.Telefon_Secundar)
            .Matches(@"^(\+40|0040|0)?[0-9]{9}$")
            .When(x => !string.IsNullOrEmpty(x.Telefon_Secundar))
            .WithMessage("Format telefon secundar invalid. Folosiți formatul: 0721234567 sau +40721234567");

        RuleFor(x => x.Telefon_Urgenta)
            .Matches(@"^(\+40|0040|0)?[0-9]{9}$")
            .When(x => !string.IsNullOrEmpty(x.Telefon_Urgenta))
            .WithMessage("Format telefon urgență invalid. Folosiți formatul: 0721234567 sau +40721234567");

        // ==================== EMAIL ====================

        RuleFor(x => x.Email)
            .EmailAddress()
            .When(x => !string.IsNullOrEmpty(x.Email))
            .WithMessage("Adresa de email nu este validă.");

        // ==================== COD POȘTAL ====================

        RuleFor(x => x.Cod_Postal)
            .Matches(@"^\d{6}$")
            .When(x => !string.IsNullOrEmpty(x.Cod_Postal))
            .WithMessage("Codul poștal trebuie să conțină exact 6 cifre.");

        // ==================== CNP ASIGURAT ====================

        RuleFor(x => x.CNP_Asigurat)
            .Length(13)
            .When(x => !string.IsNullOrEmpty(x.CNP_Asigurat))
            .WithMessage("CNP Asigurat trebuie să conțină exact 13 cifre.")
            .Matches(@"^\d{13}$")
            .When(x => !string.IsNullOrEmpty(x.CNP_Asigurat))
            .WithMessage("CNP Asigurat poate conține doar cifre.")
            .Must(ValidateCNPChecksumForAsigurat)
            .When(x => !string.IsNullOrEmpty(x.CNP_Asigurat) && x.CNP_Asigurat.Length == 13)
            .WithMessage("CNP Asigurat nu este valid (checksum incorect).");

        // ==================== ASIGURARE - VALIDARE CONDIȚIONATĂ ====================

        When(x => x.Asigurat, () =>
        {
            RuleFor(x => x)
                .Must(x => !string.IsNullOrEmpty(x.CNP_Asigurat) || !string.IsNullOrEmpty(x.Nr_Card_Sanatate))
                .WithMessage("Pentru pacienți asigurați, trebuie completat CNP Asigurat sau Nr. Card Sănătate.");
        });

        // ==================== NR. CARD SĂNĂTATE ====================

        RuleFor(x => x.Nr_Card_Sanatate)
            .Matches(@"^\d{12,20}$")
            .When(x => !string.IsNullOrEmpty(x.Nr_Card_Sanatate))
            .WithMessage("Numărul cardului de sănătate trebuie să conțină între 12 și 20 cifre.");
    }

    /// <summary>
    /// Validates Romanian CNP checksum using the control digit algorithm.
    /// </summary>
    private static bool ValidateCNPChecksum(string? cnp)
    {
        if (string.IsNullOrEmpty(cnp) || cnp.Length != 13 || !cnp.All(char.IsDigit))
            return false;

        // Weights for CNP checksum calculation
        int[] weights = { 2, 7, 9, 1, 4, 6, 3, 5, 8, 2, 7, 9 };
        
        int sum = 0;
        for (int i = 0; i < 12; i++)
        {
            sum += (cnp[i] - '0') * weights[i];
        }

        int controlDigit = sum % 11;
        if (controlDigit == 10) controlDigit = 1;

        return (cnp[12] - '0') == controlDigit;
    }

    /// <summary>
    /// Validates that CNP matches the entered birth date.
    /// </summary>
    private static bool ValidateCNPDateConsistency(PacientFormModelDto model)
    {
        if (string.IsNullOrEmpty(model.CNP) || model.CNP.Length != 13)
            return true; // Skip if no CNP

        try
        {
            var cnp = model.CNP;
            var sexDigit = int.Parse(cnp[0].ToString());
            
            int century = sexDigit switch
            {
                1 or 2 => 1900,
                3 or 4 => 1800,
                5 or 6 => 2000,
                7 or 8 => 2000,
                _ => 1900
            };

            int year = century + int.Parse(cnp.Substring(1, 2));
            int month = int.Parse(cnp.Substring(3, 2));
            int day = int.Parse(cnp.Substring(5, 2));

            var cnpDate = new DateTime(year, month, day);
            
            // Allow 1 day tolerance for timezone issues
            return Math.Abs((cnpDate.Date - model.Data_Nasterii.Date).TotalDays) <= 1;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Validates CNP checksum for Asigurat (can be different person).
    /// </summary>
    private static bool ValidateCNPChecksumForAsigurat(string? cnp)
    {
        return ValidateCNPChecksum(cnp);
    }
}

/// <summary>
/// DTO pentru validare FluentValidation a formularului pacient.
/// </summary>
public class PacientFormModelDto
{
    public string Nume { get; set; } = string.Empty;
    public string Prenume { get; set; } = string.Empty;
    public string? CNP { get; set; }
    public string? Cod_Pacient { get; set; }
    public DateTime Data_Nasterii { get; set; }
    public string Sex { get; set; } = string.Empty;
    public string? Telefon { get; set; }
    public string? Telefon_Secundar { get; set; }
    public string? Email { get; set; }
    public string? Judet { get; set; }
    public string? Localitate { get; set; }
    public string? Adresa { get; set; }
    public string? Cod_Postal { get; set; }
    public bool Asigurat { get; set; }
    public string? CNP_Asigurat { get; set; }
    public string? Nr_Card_Sanatate { get; set; }
    public string? Casa_Asigurari { get; set; }
    public string? Alergii { get; set; }
    public string? Boli_Cronice { get; set; }
    public string? Medic_Familie { get; set; }
    public string? Persoana_Contact { get; set; }
    public string? Telefon_Urgenta { get; set; }
    public string? Relatie_Contact { get; set; }
    public bool Activ { get; set; }
    public string? Observatii { get; set; }
}
