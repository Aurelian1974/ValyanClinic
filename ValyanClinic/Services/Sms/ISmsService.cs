namespace ValyanClinic.Services.Sms;

/// <summary>
/// Interface pentru serviciul de SMS.
/// Suportă trimitere SMS pentru notificări, reminder-uri și confirmări programări.
/// </summary>
public interface ISmsService
{
    /// <summary>
    /// Trimite un SMS generic către un număr de telefon.
    /// </summary>
    /// <param name="phoneNumber">Număr telefon (format: 07xxxxxxxx sau +407xxxxxxxx)</param>
    /// <param name="message">Conținutul mesajului (max 160 caractere pentru un SMS simplu)</param>
    /// <returns>True dacă SMS-ul a fost trimis cu succes</returns>
    Task<bool> SendSmsAsync(string phoneNumber, string message);

    /// <summary>
    /// Trimite SMS de confirmare programare către pacient.
    /// </summary>
    /// <param name="programareId">ID-ul programării</param>
    /// <returns>True dacă SMS-ul a fost trimis cu succes</returns>
    Task<bool> SendAppointmentConfirmationSmsAsync(Guid programareId);

    /// <summary>
    /// Trimite SMS reminder (cu X ore/zile înainte de programare).
    /// </summary>
    /// <param name="programareId">ID-ul programării</param>
    /// <param name="hoursBeforeAppointment">Câte ore înainte (ex: 24 = cu o zi înainte)</param>
    /// <returns>True dacă SMS-ul a fost trimis cu succes</returns>
    Task<bool> SendAppointmentReminderSmsAsync(Guid programareId, int hoursBeforeAppointment = 24);

    /// <summary>
    /// Trimite SMS de anulare programare către pacient.
    /// </summary>
    /// <param name="programareId">ID-ul programării anulate</param>
    /// <param name="reason">Motivul anulării (opțional)</param>
    /// <returns>True dacă SMS-ul a fost trimis cu succes</returns>
    Task<bool> SendAppointmentCancellationSmsAsync(Guid programareId, string? reason = null);

    /// <summary>
    /// Trimite SMS batch (mai multe SMS-uri simultan).
    /// Util pentru campanii sau notificări în masă.
    /// </summary>
    /// <param name="phoneNumbers">Lista de numere de telefon</param>
    /// <param name="message">Mesajul comun pentru toți destinatarii</param>
    /// <returns>Numărul de SMS-uri trimise cu succes</returns>
    Task<int> SendBulkSmsAsync(List<string> phoneNumbers, string message);

    /// <summary>
    /// Verifică dacă un număr de telefon este valid pentru trimitere SMS.
    /// </summary>
    /// <param name="phoneNumber">Număr telefon de verificat</param>
    /// <returns>True dacă numărul este valid</returns>
    bool IsValidPhoneNumber(string phoneNumber);
}
