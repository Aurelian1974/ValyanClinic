namespace ValyanClinic.Services.Sms;

/// <summary>
/// DTO pentru configurare mesaj SMS.
/// </summary>
public class SmsMessageDto
{
    /// <summary>
    /// Număr telefon destinatar (format: 07xxxxxxxx sau +407xxxxxxxx)
    /// </summary>
    public string PhoneNumber { get; set; } = string.Empty;

    /// <summary>
    /// Numele destinatarului (opțional, pentru personalizare mesaj)
    /// </summary>
    public string? RecipientName { get; set; }

    /// <summary>
    /// Conținutul mesajului SMS
    /// Limită: 160 caractere pentru un SMS simplu, 1530 pentru concatenat
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
  /// Prioritate mesaj (Normal, High, Urgent)
    /// </summary>
    public SmsPriority Priority { get; set; } = SmsPriority.Normal;

    /// <summary>
    /// Timestamp programat pentru trimitere (opțional, pentru SMS scheduled)
    /// </summary>
    public DateTime? ScheduledSendTime { get; set; }

    /// <summary>
    /// ID unic pentru tracking (generat automat dacă null)
    /// </summary>
    public string? TrackingId { get; set; }
}

/// <summary>
/// Enum pentru prioritate SMS
/// </summary>
public enum SmsPriority
{
    Normal = 0,
    High = 1,
    Urgent = 2
}

/// <summary>
/// DTO pentru rezultatul trimiterii SMS
/// </summary>
public class SmsResultDto
{
    public bool Success { get; set; }
    public string? MessageSid { get; set; }
    public string? Status { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime SentAt { get; set; }
    public decimal? Cost { get; set; }
}
