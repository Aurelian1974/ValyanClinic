using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using ValyanClinic.Services.Email;
using System.Text.RegularExpressions;

namespace ValyanClinic.Components.Pages;

/// <summary>
/// Email Testing Suite - Pagină modernă pentru testarea configurării SMTP
/// Features: Templates, Live Preview, Validation, Statistics, History
/// </summary>
public partial class TestEmail : ComponentBase
{
    [Inject] private IEmailService EmailService { get; set; } = default!;
    [Inject] private ILogger<TestEmail> Logger { get; set; } = default!;
    
    // === State Properties ===
    private string TestEmailAddress { get; set; } = string.Empty;
    private string EmailSubject { get; set; } = "🎉 Test Email - ValyanClinic";
    private bool IsLoading { get; set; }
    private bool IsSuccess { get; set; }
    private string ResultMessage { get; set; } = string.Empty;
    
    // === Template System ===
    private string SelectedTemplate { get; set; } = "test";
    private Dictionary<string, TemplateInfo> Templates { get; set; } = new()
    {
        ["test"] = new TemplateInfo("Test Basic", "fas fa-flask", "Email simplu de test"),
        ["appointment"] = new TemplateInfo("Programare", "fas fa-calendar-check", "Confirmare programare"),
        ["reminder"] = new TemplateInfo("Reminder", "fas fa-bell", "Reminder 24h înainte"),
        ["custom"] = new TemplateInfo("Personalizat", "fas fa-edit", "Mesaj personalizat")
    };

    // === Email Validation ===
    private bool IsEmailValid { get; set; }
    private string ValidationMessage { get; set; } = string.Empty;

    // === Preview ===
    private bool IsDarkPreview { get; set; }
    private bool IsRefreshing { get; set; }

    // === Statistics ===
    private int EmailsSentToday { get; set; }
    private int SuccessRate { get; set; } = 100;

    // === History ===
    private List<EmailSendRecord> RecentSends { get; set; } = new();

    // === Event Handlers ===

    private void SelectTemplate(string templateKey)
    {
        SelectedTemplate = templateKey;
        EmailSubject = templateKey switch
        {
            "test" => "🎉 Test Email - ValyanClinic",
            "appointment" => "✅ Confirmare Programare - ValyanClinic",
            "reminder" => "⏰ Reminder Programare - ValyanClinic",
            "custom" => "📧 Email Personalizat",
            _ => "Email - ValyanClinic"
        };
        StateHasChanged();
    }

    private void OnEmailInput(ChangeEventArgs e)
    {
        TestEmailAddress = e.Value?.ToString() ?? string.Empty;
        ValidateEmail();
    }

    private void ValidateEmail()
    {
        if (string.IsNullOrWhiteSpace(TestEmailAddress))
        {
            IsEmailValid = false;
            ValidationMessage = string.Empty;
            return;
        }

        var emailPattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
        IsEmailValid = Regex.IsMatch(TestEmailAddress, emailPattern);
        ValidationMessage = IsEmailValid ? "Valid ✓" : "Format invalid";
    }

    private string GetInputValidationClass()
    {
        if (string.IsNullOrEmpty(TestEmailAddress)) return string.Empty;
        return IsEmailValid ? "valid" : "invalid";
    }

    private void UseMyEmail()
    {
        TestEmailAddress = "admin@valyanclinic.ro";
        ValidateEmail();
    }

    private void ToggleDarkPreview()
    {
        IsDarkPreview = !IsDarkPreview;
    }

    private async Task RefreshPreview()
    {
        IsRefreshing = true;
        StateHasChanged();
        await Task.Delay(500);
        IsRefreshing = false;
        StateHasChanged();
    }

    private void ClearAlert()
    {
        ResultMessage = string.Empty;
    }

    private void ClearHistory()
    {
        RecentSends.Clear();
        EmailsSentToday = 0;
        SuccessRate = 100;
        StateHasChanged();
    }

    private string GetPreviewHtml()
    {
        return SelectedTemplate switch
        {
            "test" => GenerateTestEmailHtml(),
            "appointment" => GenerateAppointmentEmailHtml(),
            "reminder" => GenerateReminderEmailHtml(),
            "custom" => GenerateCustomEmailHtml(),
            _ => GenerateTestEmailHtml()
        };
    }

    private async Task SendTestEmail()
    {
        if (string.IsNullOrWhiteSpace(TestEmailAddress) || !IsEmailValid)
        {
            ResultMessage = "Te rog introdu o adresă de email validă!";
            IsSuccess = false;
            return;
        }
        
        IsLoading = true;
        ResultMessage = string.Empty;
        StateHasChanged();
     
        try
        {
            var message = new EmailMessageDto
            {
                To = TestEmailAddress,
                Subject = EmailSubject,
                Body = GetPreviewHtml(),
                IsHtml = true,
                ReplyToEmail = "clinica.valyan@gmail.com",
                ReplyToName = "ValyanClinic Support"
            };

            Logger.LogInformation("📧 Sending test email to: {Email} with template: {Template}", 
                TestEmailAddress, SelectedTemplate);
    
            var success = await EmailService.SendEmailAsync(message);
            
            if (success)
            {
                IsSuccess = true;
                ResultMessage = $"✅ Email trimis cu succes la {TestEmailAddress}!";
                Logger.LogInformation("✅ Test email sent successfully!");
                
                // Add to history
                RecentSends.Insert(0, new EmailSendRecord
                {
                    To = TestEmailAddress,
                    Subject = EmailSubject,
                    Success = true,
                    SentAt = DateTime.Now
                });
                
                EmailsSentToday++;
                UpdateSuccessRate();
            }
            else
            {
                IsSuccess = false;
                ResultMessage = "❌ Eroare la trimiterea email-ului. Verifică SMTP config.";
                Logger.LogError("❌ Failed to send test email");
                
                RecentSends.Insert(0, new EmailSendRecord
                {
                    To = TestEmailAddress,
                    Subject = EmailSubject,
                    Success = false,
                    SentAt = DateTime.Now
                });
                
                UpdateSuccessRate();
            }
        }
        catch (Exception ex)
        {
            IsSuccess = false;
            ResultMessage = $"❌ Excepție: {ex.Message}";
            Logger.LogError(ex, "❌ Exception while sending test email");
            
            RecentSends.Insert(0, new EmailSendRecord
            {
                To = TestEmailAddress,
                Subject = EmailSubject,
                Success = false,
                SentAt = DateTime.Now
            });
            
            UpdateSuccessRate();
        }
        finally
        {
            IsLoading = false;
            StateHasChanged();
        }
    }

    private void UpdateSuccessRate()
    {
        if (RecentSends.Any())
        {
            SuccessRate = (int)((double)RecentSends.Count(r => r.Success) / RecentSends.Count * 100);
        }
    }

    // === HTML Generation Methods ===

    private string GenerateTestEmailHtml()
    {
        return $@"
<div style='font-family: -apple-system, BlinkMacSystemFont, ""Segoe UI"", Roboto, sans-serif; max-width: 600px; margin: 0 auto;'>
    <div style='background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); color: white; padding: 2rem; border-radius: 16px 16px 0 0;'>
        <h1 style='margin: 0; font-size: 2rem; font-weight: 700;'>✅ Email Test Funcțional!</h1>
    </div>
    <div style='background: #f9fafb; padding: 2rem; border-radius: 0 0 16px 16px;'>
        <h2 style='color: #1f2937; font-size: 1.5rem; margin-top: 0;'>Felicitări! 🎉</h2>
        <p style='color: #4b5563; font-size: 1rem; line-height: 1.6;'>
            Email-ul tău este configurat corect și funcționează perfect!
        </p>
        <div style='background: white; padding: 1.5rem; border-left: 4px solid #667eea; border-radius: 8px; margin: 1.5rem 0;'>
            <p style='margin: 0; color: #6b7280;'><strong>Data:</strong> {DateTime.Now:dd.MM.yyyy HH:mm:ss}</p>
            <p style='margin: 0.5rem 0 0 0; color: #6b7280;'><strong>Template:</strong> {Templates[SelectedTemplate].Name}</p>
        </div>
        <h3 style='color: #374151; font-size: 1.1rem;'>🚀 Poți implementa:</h3>
        <ul style='color: #4b5563; line-height: 1.8;'>
            <li>📧 Confirmare programări</li>
            <li>⏰ Reminder-e automate</li>
            <li>🔐 Reset parolă</li>
            <li>📋 Notificări importante</li>
        </ul>
    </div>
    <div style='text-align: center; padding: 1.5rem; color: #9ca3af; font-size: 0.875rem;'>
        <p style='margin: 0;'>© {DateTime.Now.Year} ValyanClinic</p>
    </div>
</div>";
    }

    private string GenerateAppointmentEmailHtml()
    {
        return $@"
<div style='font-family: -apple-system, BlinkMacSystemFont, ""Segoe UI"", Roboto, sans-serif; max-width: 600px; margin: 0 auto;'>
    <div style='background: linear-gradient(135deg, #10b981 0%, #059669 100%); color: white; padding: 2rem; border-radius: 16px 16px 0 0;'>
        <h1 style='margin: 0; font-size: 2rem;'>✅ Programare Confirmată</h1>
    </div>
    <div style='background: #f9fafb; padding: 2rem; border-radius: 0 0 16px 16px;'>
        <p style='color: #374151; font-size: 1.1rem;'>Bună ziua, <strong>Nume Pacient</strong>!</p>
        <p style='color: #4b5563;'>Programarea dumneavoastră a fost confirmată.</p>
        <div style='background: white; padding: 1.5rem; border-radius: 12px; margin: 1.5rem 0; box-shadow: 0 2px 8px rgba(0,0,0,0.1);'>
            <p style='color: #1f2937; font-size: 1.25rem; font-weight: 600; margin: 0;'>
                📅 {DateTime.Now.AddDays(1):dd MMMM yyyy} la {DateTime.Now.AddDays(1):HH:mm}
            </p>
            <p style='color: #6b7280; margin: 0.5rem 0 0 0;'>🩺 Dr. Maria Ionescu - Cardiologie</p>
        </div>
    </div>
    <div style='text-align: center; padding: 1.5rem; color: #9ca3af; font-size: 0.875rem;'>
        <p style='margin: 0;'>© {DateTime.Now.Year} ValyanClinic</p>
    </div>
</div>";
    }

    private string GenerateReminderEmailHtml()
    {
        return $@"
<div style='font-family: -apple-system, BlinkMacSystemFont, ""Segoe UI"", Roboto, sans-serif; max-width: 600px; margin: 0 auto;'>
    <div style='background: linear-gradient(135deg, #f59e0b 0%, #d97706 100%); color: white; padding: 2rem; border-radius: 16px 16px 0 0;'>
        <h1 style='margin: 0; font-size: 2rem;'>⏰ Reminder Programare</h1>
    </div>
    <div style='background: #f9fafb; padding: 2rem; border-radius: 0 0 16px 16px;'>
        <p style='color: #374151; font-size: 1.1rem;'>Bună ziua, <strong>Nume Pacient</strong>!</p>
        <p style='color: #4b5563;'>Reminder pentru programarea de <strong>mâine</strong>.</p>
        <div style='background: white; padding: 2rem; border-radius: 12px; margin: 1.5rem 0; text-align: center;'>
            <div style='font-size: 3rem; margin-bottom: 1rem;'>⏰</div>
            <p style='color: #1f2937; font-size: 1.5rem; font-weight: 700; margin: 0;'>
                {DateTime.Now.AddDays(1):dd MMMM yyyy}
            </p>
            <p style='color: #6b7280; font-size: 1.25rem; margin: 0.5rem 0 0 0;'>
                ora {DateTime.Now.AddDays(1):HH:mm}
            </p>
        </div>
    </div>
    <div style='text-align: center; padding: 1.5rem; color: #9ca3af; font-size: 0.875rem;'>
        <p style='margin: 0;'>© {DateTime.Now.Year} ValyanClinic</p>
    </div>
</div>";
    }

    private string GenerateCustomEmailHtml()
    {
        return $@"
<div style='font-family: -apple-system, BlinkMacSystemFont, ""Segoe UI"", Roboto, sans-serif; max-width: 600px; margin: 0 auto;'>
    <div style='background: linear-gradient(135deg, #8b5cf6 0%, #7c3aed 100%); color: white; padding: 2rem; border-radius: 16px 16px 0 0;'>
        <h1 style='margin: 0; font-size: 2rem;'>📧 Email Personalizat</h1>
    </div>
    <div style='background: #f9fafb; padding: 2rem; border-radius: 0 0 16px 16px;'>
        <p style='color: #374151;'>Template personalizat pentru email-ul tău.</p>
        <div style='background: white; padding: 1.5rem; border-radius: 12px; margin: 1.5rem 0;'>
            <p style='margin: 0; color: #6b7280;'>Editează acest template pentru mesajul tău.</p>
        </div>
    </div>
    <div style='text-align: center; padding: 1.5rem; color: #9ca3af; font-size: 0.875rem;'>
        <p style='margin: 0;'>© {DateTime.Now.Year} ValyanClinic</p>
    </div>
</div>";
    }

    // === Helper Classes ===

    private class TemplateInfo
    {
        public string Name { get; set; }
        public string Icon { get; set; }
        public string Description { get; set; }

        public TemplateInfo(string name, string icon, string description)
        {
            Name = name;
            Icon = icon;
            Description = description;
        }
    }

    private class EmailSendRecord
    {
        public string To { get; set; } = string.Empty;
        public string Subject { get; set; } = string.Empty;
        public bool Success { get; set; }
        public DateTime SentAt { get; set; }
    }
}
