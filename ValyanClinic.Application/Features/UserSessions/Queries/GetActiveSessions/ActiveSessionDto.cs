namespace ValyanClinic.Application.Features.UserSessions.Queries.GetActiveSessions;

/// <summary>
/// DTO pentru sesiune activă în grid
/// </summary>
public class ActiveSessionDto
{
  public Guid SessionID { get; set; }
    public Guid UtilizatorID { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Rol { get; set; } = string.Empty;
    public string SessionToken { get; set; } = string.Empty;
    public string AdresaIP { get; set; } = string.Empty;
    public string? UserAgent { get; set; }
    public string? Dispozitiv { get; set; }
    public DateTime DataCreare { get; set; }
    public DateTime DataUltimaActivitate { get; set; }
    public DateTime DataExpirare { get; set; }
    public bool EsteActiva { get; set; }
    
    // Computed properties pentru UI
    public string DurataActivitate => CalculeazaDurata(DataCreare, DateTime.Now);
    public string DurataInactivitate => CalculeazaDurata(DataUltimaActivitate, DateTime.Now);
    public string TimeToExpire => CalculeazaDurata(DateTime.Now, DataExpirare);
    public bool ExpiraInCurând => (DataExpirare - DateTime.Now).TotalMinutes < 15;
    public string StatusBadgeClass => ExpiraInCurând ? "badge-warning" : "badge-success";
    public string StatusText => EsteActiva ? (ExpiraInCurând ? "Expira in curand" : "Activa") : "Inactiva";
    public string DeviceIcon => GetDeviceIcon(Dispozitiv);
    public string BrowserIcon => GetBrowserIcon(UserAgent);
    
 private static string CalculeazaDurata(DateTime start, DateTime end)
    {
        var diff = end - start;
 if (diff.TotalDays >= 1)
  return $"{(int)diff.TotalDays}z {diff.Hours}h";
        if (diff.TotalHours >= 1)
            return $"{(int)diff.TotalHours}h {diff.Minutes}m";
        if (diff.TotalMinutes >= 1)
            return $"{(int)diff.TotalMinutes}m {diff.Seconds}s";
        return $"{(int)diff.TotalSeconds}s";
    }
    
    private static string GetDeviceIcon(string? device)
    {
        if (string.IsNullOrEmpty(device)) return "fa-question";
      
        var deviceLower = device.ToLower();
        if (deviceLower.Contains("mobile") || deviceLower.Contains("android") || deviceLower.Contains("iphone"))
            return "fa-mobile-alt";
        if (deviceLower.Contains("tablet") || deviceLower.Contains("ipad"))
       return "fa-tablet-alt";
        return "fa-desktop";
    }
    
    private static string GetBrowserIcon(string? userAgent)
    {
   if (string.IsNullOrEmpty(userAgent)) return "fa-browser";
        
        var ua = userAgent.ToLower();
     if (ua.Contains("chrome")) return "fa-chrome";
        if (ua.Contains("firefox")) return "fa-firefox";
        if (ua.Contains("edge")) return "fa-edge";
        if (ua.Contains("safari")) return "fa-safari";
        if (ua.Contains("opera")) return "fa-opera";
        return "fa-browser";
    }
}
