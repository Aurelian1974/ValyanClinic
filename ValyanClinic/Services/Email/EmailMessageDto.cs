namespace ValyanClinic.Services.Email;

/// <summary>
/// DTO pentru trimiterea unui email.
/// </summary>
public class EmailMessageDto
{
    /// <summary>
    /// Adresa email destinatar (REQUIRED).
 /// </summary>
    public string To { get; set; } = string.Empty;

    /// <summary>
    /// Numele destinatarului (optional).
    /// </summary>
    public string? ToName { get; set; }

    /// <summary>
    /// Subiectul email-ului (REQUIRED).
    /// </summary>
    public string Subject { get; set; } = string.Empty;

    /// <summary>
 /// Corpul email-ului (HTML sau text simplu).
    /// </summary>
    public string Body { get; set; } = string.Empty;

    /// <summary>
    /// Dacă corpul este HTML (default: true).
  /// </summary>
    public bool IsHtml { get; set; } = true;

    /// <summary>
    /// ✅ NEW: Reply-To email address (pacientii vor raspunde aici).
    /// Foloseste pentru a seta reply address diferit de FROM.
    /// </summary>
    public string? ReplyToEmail { get; set; }

    /// <summary>
    /// ✅ NEW: Reply-To name (optional).
  /// </summary>
    public string? ReplyToName { get; set; }

    /// <summary>
  /// Liste de adrese CC (optional).
    /// </summary>
    public List<string>? CcAddresses { get; set; }

    /// <summary>
    /// Liste de adrese BCC (optional).
    /// </summary>
    public List<string>? BccAddresses { get; set; }

    /// <summary>
    /// Atașamente (optional).
    /// </summary>
    public List<EmailAttachment>? Attachments { get; set; }
}

/// <summary>
/// DTO pentru un atașament email.
/// </summary>
public class EmailAttachment
{
    public string FileName { get; set; } = string.Empty;
    public byte[] Content { get; set; } = Array.Empty<byte>();
    public string ContentType { get; set; } = "application/octet-stream";
}
