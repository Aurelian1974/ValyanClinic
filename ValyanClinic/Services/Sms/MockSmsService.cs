using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MediatR;
using System.Text.RegularExpressions;
using ValyanClinic.Application.Features.ProgramareManagement.Queries.GetProgramareById;

namespace ValyanClinic.Services.Sms;

/// <summary>
/// ⚠️ MOCK Implementation - pentru testare UI fără cost.
/// 
/// PRODUCTION: Când ai buget, înlocuiește cu TwilioSmsService sau alt provider.
/// 
/// Setup Twilio (când ai buget):
/// 1. dotnet add package Twilio
/// 2. dotnet user-secrets set "TwilioSettings:AccountSid" "ACxxxx"
/// 3. dotnet user-secrets set "TwilioSettings:AuthToken" "xxxx"
/// 4. dotnet user-secrets set "TwilioSettings:PhoneNumber" "+1xxxx"
/// 5. builder.Services.AddScoped&lt;ISmsService, TwilioSmsService&gt;();
/// 
/// Estimare cost Twilio:
/// - $15 credit gratuit la sign-up (≈ 450 SMS-uri)
/// - $0.025/SMS în România după trial
/// - 50 programări/zi × 2 SMS = 100 SMS/zi = $75/lună
/// 
/// Alternative low-cost:
/// - SMS-Gateway.ro: 0.08 lei/SMS (românesc, fără contract)
/// - Vonage: €0.033/SMS
/// - SMS.TO: €0.035/SMS
/// </summary>
public class MockSmsService : ISmsService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<MockSmsService> _logger;
    private readonly IMediator _mediator;

    public MockSmsService(
        IConfiguration configuration,
  ILogger<MockSmsService> logger,
        IMediator mediator)
    {
        _configuration = configuration;
        _logger = logger;
        _mediator = mediator;
    }

    /// <inheritdoc />
    public async Task<bool> SendSmsAsync(string phoneNumber, string message)
    {
        // Validate phone number
        if (!IsValidPhoneNumber(phoneNumber))
        {
            _logger.LogWarning("⚠️ MOCK SMS - Invalid phone number: {Phone}", phoneNumber);
            return false;
        }

        // Simulate SMS sending
        _logger.LogInformation(
                   "📱 MOCK SMS SENT\n" +
             "   To: {Phone}\n" +
         "   Message: {Message}\n" +
        "   Length: {Length} chars\n" +
             "   Cost: $0.00 (MOCK MODE)",
                   phoneNumber, message, message.Length);

        // Simulate network delay
        await Task.Delay(100);

        return true;
    }

    /// <inheritdoc />
    public async Task<bool> SendAppointmentConfirmationSmsAsync(Guid programareId)
    {
        try
        {
            _logger.LogInformation("📱 Preparing confirmation SMS for appointment {ProgramareID}", programareId);

            // Get appointment details
            var query = new GetProgramareByIdQuery { ProgramareID = programareId };
            var result = await _mediator.Send(query);

            if (!result.IsSuccess || result.Value == null)
            {
                _logger.LogWarning("⚠️ Programarea {ProgramareID} nu a fost găsită", programareId);
                return false;
            }

            var programare = result.Value;

            // Validate patient has phone
            if (string.IsNullOrEmpty(programare.PacientTelefon))
            {
                _logger.LogWarning("⚠️ Pacientul {Pacient} nu are telefon configurat",
          programare.PacientNumeComplet);
                return false;
            }

            // Generate confirmation message
            var message = GenerateConfirmationMessage(programare);

            // Send SMS
            return await SendSmsAsync(programare.PacientTelefon, message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Eroare la trimiterea SMS-ului de confirmare pentru {ProgramareID}",
      programareId);
            return false;
        }
    }

    /// <inheritdoc />
    public async Task<bool> SendAppointmentReminderSmsAsync(Guid programareId, int hoursBeforeAppointment = 24)
    {
        try
        {
            _logger.LogInformation("📱 Preparing reminder SMS for appointment {ProgramareID} ({Hours}h before)",
              programareId, hoursBeforeAppointment);

            // Get appointment details
            var query = new GetProgramareByIdQuery { ProgramareID = programareId };
            var result = await _mediator.Send(query);

            if (!result.IsSuccess || result.Value == null)
            {
                _logger.LogWarning("⚠️ Programarea {ProgramareID} nu a fost găsită", programareId);
                return false;
            }

            var programare = result.Value;

            // Validate patient has phone
            if (string.IsNullOrEmpty(programare.PacientTelefon))
            {
                _logger.LogWarning("⚠️ Pacientul {Pacient} nu are telefon configurat",
                 programare.PacientNumeComplet);
                return false;
            }

            // Generate reminder message
            var message = GenerateReminderMessage(programare, hoursBeforeAppointment);

            // Send SMS
            return await SendSmsAsync(programare.PacientTelefon, message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Eroare la trimiterea SMS-ului reminder pentru {ProgramareID}",
             programareId);
            return false;
        }
    }

    /// <inheritdoc />
    public async Task<bool> SendAppointmentCancellationSmsAsync(Guid programareId, string? reason = null)
    {
        try
        {
            _logger.LogInformation("📱 Preparing cancellation SMS for appointment {ProgramareID}", programareId);

            // Get appointment details
            var query = new GetProgramareByIdQuery { ProgramareID = programareId };
            var result = await _mediator.Send(query);

            if (!result.IsSuccess || result.Value == null)
            {
                _logger.LogWarning("⚠️ Programarea {ProgramareID} nu a fost găsită", programareId);
                return false;
            }

            var programare = result.Value;

            // Validate patient has phone
            if (string.IsNullOrEmpty(programare.PacientTelefon))
            {
                _logger.LogWarning("⚠️ Pacientul {Pacient} nu are telefon configurat",
              programare.PacientNumeComplet);
                return false;
            }

            // Generate cancellation message
            var message = GenerateCancellationMessage(programare, reason);

            // Send SMS
            return await SendSmsAsync(programare.PacientTelefon, message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Eroare la trimiterea SMS-ului de anulare pentru {ProgramareID}",
               programareId);
            return false;
        }
    }

    /// <inheritdoc />
    public async Task<int> SendBulkSmsAsync(List<string> phoneNumbers, string message)
    {
        _logger.LogInformation("📱 Preparing bulk SMS to {Count} recipients", phoneNumbers.Count);

        int successCount = 0;

        foreach (var phoneNumber in phoneNumbers)
        {
            var success = await SendSmsAsync(phoneNumber, message);
            if (success) successCount++;

            // Small delay to avoid rate limiting (real providers)
            await Task.Delay(50);
        }

        _logger.LogInformation("✅ Bulk SMS complete: {Success}/{Total} sent successfully",
               successCount, phoneNumbers.Count);

        return successCount;
    }

    /// <inheritdoc />
    public bool IsValidPhoneNumber(string phoneNumber)
    {
        if (string.IsNullOrWhiteSpace(phoneNumber))
            return false;

        // Remove spaces, dashes, parentheses
        var cleaned = Regex.Replace(phoneNumber, @"[\s\-\(\)]", "");

        // Romanian phone number patterns:
        // 07xxxxxxxx (10 digits)
        // +407xxxxxxxx (12 digits with country code)
        // 004007xxxxxxxx (14 digits with 00 prefix)

        var patterns = new[]
        {
      @"^07\d{8}$",      // 07xxxxxxxx
        @"^\+407\d{8}$",    // +407xxxxxxxx
            @"^004007\d{8}$"  // 004007xxxxxxxx
        };

        return patterns.Any(pattern => Regex.IsMatch(cleaned, pattern));
    }

    // ==================== PRIVATE HELPERS - MESSAGE GENERATION ====================

    private string GenerateConfirmationMessage(Application.Features.ProgramareManagement.DTOs.ProgramareDetailDto programare)
    {
        return $"ValyanClinic: Programarea ta cu Dr. {programare.DoctorNumeComplet} " +
           $"pe data de {programare.DataProgramare:dd.MM.yyyy} la ora {programare.OraInceput:hh\\:mm} " +
                    $"a fost confirmată. Pentru reprogramare, sună la 0123456789.";
    }

    private string GenerateReminderMessage(
      Application.Features.ProgramareManagement.DTOs.ProgramareDetailDto programare,
   int hoursBeforeAppointment)
    {
        var timeUntil = hoursBeforeAppointment >= 24
            ? $"{hoursBeforeAppointment / 24} zi"
 : $"{hoursBeforeAppointment} ore";

        return $"ValyanClinic REMINDER: Ai programare cu Dr. {programare.DoctorNumeComplet} " +
      $"peste {timeUntil}, pe {programare.DataProgramare:dd.MM.yyyy} la {programare.OraInceput:hh\\:mm}. " +
               $"Pentru anulare, sună la 0123456789.";
    }

    private string GenerateCancellationMessage(
        Application.Features.ProgramareManagement.DTOs.ProgramareDetailDto programare,
 string? reason)
    {
        var baseMessage = $"ValyanClinic: Programarea ta cu Dr. {programare.DoctorNumeComplet} " +
               $"din data de {programare.DataProgramare:dd.MM.yyyy} la ora {programare.OraInceput:hh\\:mm} " +
          $"a fost anulată.";

        if (!string.IsNullOrEmpty(reason))
        {
            baseMessage += $" Motiv: {reason}.";
        }

        baseMessage += " Pentru reprogramare, sună la 0123456789.";

        return baseMessage;
    }
}
