namespace ValyanClinic.Services.Email;

/// <summary>
/// Service pentru trimiterea de email-uri prin SMTP2GO.
/// </summary>
public interface IEmailService
{
    /// <summary>
    /// Trimite un email generic.
    /// </summary>
    Task<bool> SendEmailAsync(EmailMessageDto message);

    /// <summary>
    /// Trimite email de confirmare programare.
    /// </summary>
    Task<bool> SendAppointmentConfirmationAsync(Guid programareId);

    /// <summary>
    /// Trimite reminder pentru programare (cu 24h înainte).
    /// </summary>
    Task<bool> SendAppointmentReminderAsync(Guid programareId);

    /// <summary>
    /// Trimite email de resetare parolă.
    /// </summary>
    Task<bool> SendPasswordResetAsync(string email, string resetToken);

    /// <summary>
    /// Trimite email de notificare anulare programare.
    /// </summary>
    Task<bool> SendAppointmentCancellationAsync(Guid programareId, string reason);

    /// <summary>
    /// ✅ NEW - Trimite email-uri către doctori cu programările lor pentru ziua următoare.
    /// </summary>
    /// <param name="targetDate">Data pentru care se trimit programările (default: mâine)</param>
    /// <returns>Numărul de email-uri trimise cu succes</returns>
    Task<int> SendDailyAppointmentsEmailAsync(DateTime? targetDate = null);
}
