using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MediatR;
using ValyanClinic.Application.Features.ProgramareManagement.Queries.GetProgramariByDate;
using System.Globalization;

namespace ValyanClinic.Services.Email;

/// <summary>
/// Implementare EmailService folosind SMTP (MailKit).
/// HIPAA Compliant, ideal pentru clinici medicale.
/// </summary>
public class EmailService : IEmailService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<EmailService> _logger;
    private readonly IMediator _mediator;

    public EmailService(
IConfiguration configuration,
    ILogger<EmailService> logger,
        IMediator mediator)
    {
        _configuration = configuration;
        _logger = logger;
        _mediator = mediator;
    }

    /// <inheritdoc />
    public async Task<bool> SendEmailAsync(EmailMessageDto message)
    {
        try
        {
            _logger.LogInformation("📧 Pregătire trimitere email către: {To}, Subiect: {Subject}",
                 message.To, message.Subject);

            // Validare
            if (string.IsNullOrEmpty(message.To) || string.IsNullOrEmpty(message.Subject))
            {
                _logger.LogWarning("⚠️ Email invalid - lipsesc câmpuri obligatorii");
                return false;
            }

            // Creează mesajul MimeKit
            var emailMessage = new MimeMessage();

            // FROM (expeditor)
            var fromEmail = _configuration["EmailSettings:FromEmail"] ?? "noreply@valyanclinic.ro";
            var fromName = _configuration["EmailSettings:FromName"] ?? "ValyanClinic";
            emailMessage.From.Add(new MailboxAddress(fromName, fromEmail));

            // TO (destinatar)
            emailMessage.To.Add(new MailboxAddress(
     message.ToName ?? message.To,
           message.To));

            // ✅ NEW: REPLY-TO (optional - unde vor raspunde pacientii)
            if (!string.IsNullOrEmpty(message.ReplyToEmail))
            {
                emailMessage.ReplyTo.Add(new MailboxAddress(
     message.ReplyToName ?? message.ReplyToEmail,
               message.ReplyToEmail));
            }

            // CC (optional)
            if (message.CcAddresses?.Any() == true)
            {
                foreach (var cc in message.CcAddresses)
                {
                    emailMessage.Cc.Add(MailboxAddress.Parse(cc));
                }
            }

            // BCC (optional)
            if (message.BccAddresses?.Any() == true)
            {
                foreach (var bcc in message.BccAddresses)
                {
                    emailMessage.Bcc.Add(MailboxAddress.Parse(bcc));
                }
            }

            // SUBJECT
            emailMessage.Subject = message.Subject;

            // BODY
            var bodyBuilder = new BodyBuilder();
            if (message.IsHtml)
            {
                bodyBuilder.HtmlBody = message.Body;
            }
            else
            {
                bodyBuilder.TextBody = message.Body;
            }

            // ATTACHMENTS (optional)
            if (message.Attachments?.Any() == true)
            {
                foreach (var attachment in message.Attachments)
                {
                    bodyBuilder.Attachments.Add(
                 attachment.FileName,
             attachment.Content,
            ContentType.Parse(attachment.ContentType));
                }
            }

            emailMessage.Body = bodyBuilder.ToMessageBody();

            // SMTP2GO Configuration
            var smtpHost = _configuration["EmailSettings:SmtpHost"] ?? "mail.smtp2go.com";
            var smtpPort = int.Parse(_configuration["EmailSettings:SmtpPort"] ?? "2525");
            var smtpUser = _configuration["EmailSettings:SmtpUser"];
            var smtpPassword = _configuration["EmailSettings:SmtpPassword"];
            var enableSsl = bool.Parse(_configuration["EmailSettings:EnableSsl"] ?? "true");

            // Validare credențiale
            if (string.IsNullOrEmpty(smtpUser) || string.IsNullOrEmpty(smtpPassword))
            {
                _logger.LogError("❌ SMTP credentials lipsesc din configurare!");
                return false;
            }

            // Trimite email prin SMTP
            using var smtp = new SmtpClient();

            // Log connection
            _logger.LogDebug("🔌 Conectare la SMTP: {Host}:{Port}", smtpHost, smtpPort);

            // Connect
            await smtp.ConnectAsync(
          smtpHost,
                smtpPort,
                    enableSsl ? SecureSocketOptions.StartTls : SecureSocketOptions.None);

            // Authenticate
            await smtp.AuthenticateAsync(smtpUser, smtpPassword);

            // Send
            await smtp.SendAsync(emailMessage);

            // Disconnect
            await smtp.DisconnectAsync(true);

            _logger.LogInformation("✅ Email trimis cu succes către {To}: {Subject}",
                    message.To, message.Subject);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Eroare la trimiterea email-ului către {To}", message.To);
            return false;
        }
    }

    /// <inheritdoc />
    public async Task<bool> SendAppointmentConfirmationAsync(Guid programareId)
    {
        _logger.LogInformation("📅 Trimitere confirmare programare: {ProgramareId}", programareId);

        // TODO: Implementare după ce ai templates HTML
        // 1. Load programare din DB
        // 2. Generează HTML cu detalii programare
        // 3. Trimite email cu SendEmailAsync()

        var message = new EmailMessageDto
        {
            To = "pacient@example.com", // TODO: Get from DB
            Subject = "✅ Confirmare Programare - ValyanClinic",
            Body = $@"
    <h2>Programarea dvs. a fost confirmată!</h2>
             <p><strong>ID Programare:</strong> {programareId}</p>
   <p>Vă așteptăm!</p>
       <p><em>Echipa ValyanClinic</em></p>
            ",
            IsHtml = true
        };

        return await SendEmailAsync(message);
    }

    /// <inheritdoc />
    public async Task<bool> SendAppointmentReminderAsync(Guid programareId)
    {
        _logger.LogInformation("⏰ Trimitere reminder programare: {ProgramareId}", programareId);

        // TODO: Implementare reminder (24h before appointment)
        return await Task.FromResult(false);
    }

    /// <inheritdoc />
    public async Task<bool> SendPasswordResetAsync(string email, string resetToken)
    {
        _logger.LogInformation("🔐 Trimitere reset parolă către: {Email}", email);

        var resetLink = $"https://localhost:5001/reset-password?token={resetToken}";

        var message = new EmailMessageDto
        {
            To = email,
            Subject = "🔐 Resetare Parolă - ValyanClinic",
            Body = $@"
<h2>Resetare Parolă</h2>
                <p>Ați solicitat resetarea parolei.</p>
             <p><a href=""{resetLink}"" style=""padding: 10px 20px; background: #3b82f6; color: white; text-decoration: none; border-radius: 8px;"">
        Resetează Parola
        </a></p>
           <p><em>Link-ul este valabil 24 ore.</em></p>
            <p>Dacă nu ați solicitat resetarea, ignorați acest email.</p>
   ",
            IsHtml = true
        };

        return await SendEmailAsync(message);
    }

    /// <inheritdoc />
    public async Task<bool> SendAppointmentCancellationAsync(Guid programareId, string reason)
    {
        _logger.LogInformation("❌ Trimitere notificare anulare: {ProgramareId}", programareId);

        // TODO: Implementare notificare anulare
        return await Task.FromResult(false);
    }

    /// <inheritdoc />
    public async Task<int> SendDailyAppointmentsEmailAsync(DateTime? targetDate = null)
    {
        // Default: mâine
        var sendDate = targetDate ?? DateTime.Today.AddDays(1);

        _logger.LogInformation("📧 Începere trimitere email-uri programări pentru data: {Date}",
    sendDate.ToString("dd.MM.yyyy"));

        try
        {
            // ==================== STEP 1: QUERY PROGRAMĂRI DIN DB ====================

            var query = new GetProgramariByDateQuery
            {
                Date = sendDate,
                DoctorID = null  // All doctors
            };

            var result = await _mediator.Send(query);

            if (!result.IsSuccess || result.Value == null || !result.Value.Any())
            {
                _logger.LogWarning("⚠️ Nu s-au găsit programări pentru data {Date}", sendDate.ToString("dd.MM.yyyy"));
                return 0;
            }

            var allProgramari = result.Value.ToList();
            _logger.LogInformation("✅ Găsite {Count} programări pentru data {Date}",
         allProgramari.Count, sendDate.ToString("dd.MM.yyyy"));

            // ==================== STEP 2: GRUPARE PE DOCTOR ====================

            var programariGroupedByDoctor = allProgramari
                .Where(p => !string.IsNullOrEmpty(p.DoctorEmail)) // Doar doctori cu email
               .GroupBy(p => p.DoctorID)
                   .ToList();

            if (!programariGroupedByDoctor.Any())
            {
                _logger.LogWarning("⚠️ Nu există doctori cu email configurat pentru programările din {Date}",
             sendDate.ToString("dd.MM.yyyy"));
                return 0;
            }

            _logger.LogInformation("📊 Găsiți {Count} doctori cu programări și email configurat",
       programariGroupedByDoctor.Count);

            // Log warning pentru doctori fără email
            var doctorsWithoutEmail = allProgramari
                .Where(p => string.IsNullOrEmpty(p.DoctorEmail))
                   .Select(p => p.DoctorNumeComplet)
                   .Distinct()
                        .ToList();

            if (doctorsWithoutEmail.Any())
            {
                _logger.LogWarning("⚠️ {Count} doctori NU au email configurat: {Doctors}",
                   doctorsWithoutEmail.Count, string.Join(", ", doctorsWithoutEmail));
            }

            // ==================== STEP 3: TRIMITERE EMAIL PENTRU FIECARE DOCTOR ====================

            int emailsSent = 0;
            int emailsFailed = 0;

            foreach (var doctorGroup in programariGroupedByDoctor)
            {
                var doctorProgramari = doctorGroup.OrderBy(p => p.OraInceput).ToList();
                var firstProgramare = doctorProgramari.First();

                var doctorName = firstProgramare.DoctorNumeComplet ?? "Doctor";
                var doctorEmail = firstProgramare.DoctorEmail!;
                var doctorSpecializare = firstProgramare.DoctorSpecializare;

                _logger.LogInformation("📧 Pregătire email pentru Dr. {Doctor} ({Email}) - {Count} programări",
                       doctorName, doctorEmail, doctorProgramari.Count);

                // Generează HTML body
                var emailBody = GenerateDoctorAppointmentsEmailBody(
                     doctorName,
              doctorSpecializare,
                 sendDate,
               doctorProgramari);

                // Creează mesajul email
                var message = new EmailMessageDto
                {
                    To = doctorEmail,
                    ToName = $"Dr. {doctorName}",
                    Subject = $"📅 Programările tale pentru {sendDate:dd.MM.yyyy} - ValyanClinic",
                    Body = emailBody,
                    IsHtml = true
                };

                // Trimite email
                var success = await SendEmailAsync(message);

                if (success)
                {
                    emailsSent++;
                    _logger.LogInformation("✅ Email trimis cu succes către Dr. {Doctor} ({Email})",
            doctorName, doctorEmail);
                }
                else
                {
                    emailsFailed++;
                    _logger.LogError("❌ Eroare la trimiterea email-ului către Dr. {Doctor} ({Email})",
                 doctorName, doctorEmail);
                }

                // Small delay to avoid rate limiting
                await Task.Delay(100);
            }

            // ==================== STEP 4: LOGGING FINAL ====================

            _logger.LogInformation(
               "🎉 Finalizare trimitere email-uri pentru {Date}: Success={Success}, Failed={Failed}, Total={Total}",
            sendDate.ToString("dd.MM.yyyy"), emailsSent, emailsFailed, emailsSent + emailsFailed);

            return emailsSent;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Eroare critică la trimiterea email-urilor pentru programările din {Date}",
                  sendDate.ToString("dd.MM.yyyy"));
            return 0;
        }
    }

    /// <summary>
    /// Generează HTML body pentru email-ul cu programările unui doctor.
    /// </summary>
    private string GenerateDoctorAppointmentsEmailBody(
           string doctorName,
           string? doctorSpecializare,
    DateTime date,
           List<Application.Features.ProgramareManagement.DTOs.ProgramareListDto> programari)
    {
        // Setează cultura română pentru formatare dată
        var culture = new CultureInfo("ro-RO");

        // Generează HTML rows pentru fiecare programare
        var programariHtml = string.Join("\n", programari.Select(p => $@"
       <tr style='border-bottom: 1px solid #e5e7eb;'>
         <td style='padding: 12px; text-align: left;'>{p.OraInceput:hh\:mm} - {p.OraSfarsit:hh\:mm}</td>
     <td style='padding: 12px; text-align: left;'><strong>{p.PacientNumeComplet}</strong></td>
 <td style='padding: 12px; text-align: left;'>{p.TipProgramare ?? "Consultație"}</td>
     <td style='padding: 12px; text-align: center;'>
<span style='background: {GetStatusColorForEmail(p.Status)}; 
     color: white; 
padding: 4px 10px; 
               border-radius: 12px; 
               font-size: 11px; 
       font-weight: 700;
            text-transform: uppercase;
            letter-spacing: 0.5px;'>
{GetStatusDisplayNameForEmail(p.Status)}
               </span>
       </td>
            </tr>"));

        // Template HTML complet
        return $@"
<!DOCTYPE html>
<html lang='ro'>
<head>
  <meta charset='UTF-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <title>Programările tale pentru {date:dd.MM.yyyy}</title>
</head>
<body style='font-family: -apple-system, BlinkMacSystemFont, ""Segoe UI"", Roboto, ""Helvetica Neue"", Arial, sans-serif; 
     margin: 0; 
    padding: 0; 
  background-color: #f9fafb;'>
    
 <div style='max-width: 800px; margin: 0 auto; padding: 20px;'>
        
      <!-- Header -->
 <div style='background: linear-gradient(135deg, #60a5fa 0%, #3b82f6 100%); 
          color: white; 
  padding: 30px 24px; 
  border-radius: 12px 12px 0 0; 
   box-shadow: 0 4px 12px rgba(59, 130, 246, 0.3);'>
            <h1 style='margin: 0 0 8px 0; font-size: 24px; font-weight: 700;'>
📅 Programările tale pentru {date.ToString("dd MMMM yyyy", culture)}
         </h1>
            <p style='margin: 0; font-size: 16px; opacity: 0.95;'>
Dr. {doctorName}
{(string.IsNullOrEmpty(doctorSpecializare) ? "" : $"<br/><span style='font-size: 14px; opacity: 0.85;'>{doctorSpecializare}</span>")}
        </p>
        </div>

      <!-- Body -->
        <div style='background: white; 
   padding: 30px 24px; 
    border-radius: 0 0 12px 12px; 
           box-shadow: 0 2px 8px rgba(0,0,0,0.08);'>
 
       <!-- Greeting -->
            <p style='color: #374151; font-size: 16px; line-height: 1.6; margin: 0 0 24px 0;'>
     <strong>Bună ziua, Dr. {doctorName}!</strong><br/>
      Aici sunt programările tale pentru <strong>{date.ToString("dddd, dd MMMM yyyy", culture)}</strong>:
      </p>

   <!-- Appointments Table -->
     <table style='width: 100%; 
               background: white; 
               border-radius: 8px; 
     overflow: hidden; 
  border-collapse: collapse;
  border: 1px solid #e5e7eb;
     box-shadow: 0 1px 3px rgba(0,0,0,0.08);'>
       <thead>
        <tr style='background: #3b82f6; color: white;'>
<th style='padding: 14px 12px; text-align: left; font-weight: 600; font-size: 13px; letter-spacing: 0.5px; text-transform: uppercase;'>Interval</th>
   <th style='padding: 14px 12px; text-align: left; font-weight: 600; font-size: 13px; letter-spacing: 0.5px; text-transform: uppercase;'>Pacient</th>
  <th style='padding: 14px 12px; text-align: left; font-weight: 600; font-size: 13px; letter-spacing: 0.5px; text-transform: uppercase;'>Tip</th>
    <th style='padding: 14px 12px; text-align: center; font-weight: 600; font-size: 13px; letter-spacing: 0.5px; text-transform: uppercase;'>Status</th>
     </tr>
  </thead>
<tbody>
                {programariHtml}
             </tbody>
      </table>

            <!-- Stats Box -->
            <div style='margin-top: 24px; 
   padding: 16px 20px; 
       background: linear-gradient(135deg, #dbeafe 0%, #bfdbfe 100%); 
            border-left: 4px solid #3b82f6; 
 border-radius: 8px;'>
      <p style='margin: 0; color: #1e40af; font-weight: 700; font-size: 15px;'>
 📊 Total programări: {programari.Count}
     </p>
            </div>

         <!-- Footer Note -->
     <p style='margin-top: 30px; 
  color: #64748b; 
  font-size: 14px; 
 line-height: 1.6; 
          padding-top: 20px; 
    border-top: 1px solid #e5e7eb;'>
  <strong>📌 Notă:</strong> Acest email a fost generat automat de sistemul ValyanClinic.<br/>
  Pentru întrebări sau modificări, vă rugăm contactați recepția clinicii.
        </p>
        </div>

        <!-- Footer -->
        <div style='text-align: center; 
   padding: 20px; 
    color: #94a3b8; 
        font-size: 12px; 
       line-height: 1.6;'>
   <p style='margin: 0 0 8px 0;'>
     © {DateTime.Now.Year} <strong>ValyanClinic</strong> - Sistem Integrat de Management Clinică
</p>
       <p style='margin: 0; opacity: 0.8;'>
       Email trimis automat la {DateTime.Now:dd.MM.yyyy HH:mm}
            </p>
        </div>

    </div>
</body>
</html>";
    }

    /// <summary>
    /// Returnează culoarea de background pentru badge-ul de status în email.
    /// </summary>
    private string GetStatusColorForEmail(string? status) => status?.ToLower() switch
    {
        "programata" => "#94a3b8",   // Gray
        "confirmata" => "#3b82f6",   // Blue
        "checkedin" => "#f59e0b",    // Orange
        "inconsultatie" => "#8b5cf6", // Purple
        "finalizata" => "#10b981",    // Green
        "anulata" => "#ef4444",      // Red
        _ => "#6b7280"         // Default Gray
    };

    /// <summary>
    /// Returnează numele display pentru status în email.
    /// </summary>
    private string GetStatusDisplayNameForEmail(string? status) => status?.ToLower() switch
    {
        "programata" => "Programată",
        "confirmata" => "Confirmată",
        "checkedin" => "Check-in",
        "inconsultatie" => "În consultație",
        "finalizata" => "Finalizată",
        "anulata" => "Anulată",
        _ => status ?? "Necunoscut"
    };
}
