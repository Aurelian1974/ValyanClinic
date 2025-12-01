using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Text.Json;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using ValyanClinic.Services.Email;
using System.Text.RegularExpressions;

namespace ValyanClinic.Components.Pages;

/// <summary>
/// Email Testing Suite - Pagină modernă pentru testarea configurării SMTP
/// Features: Templates, Live Preview, Validation, Statistics, History, Rich Text Editor, Template Manager
/// </summary>
public partial class TestEmail : ComponentBase
{
    [Inject] private IEmailService EmailService { get; set; } = default!;
    [Inject] private ILogger<TestEmail> Logger { get; set; } = default!;
    // JSRuntime is injected in .razor file with @inject

    // === State Properties ===
    private string TestEmailAddress { get; set; } = string.Empty;
    private string EmailSubject { get; set; } = "🎉 Test Email - ValyanClinic";
    private bool IsLoading { get; set; }
    private bool IsSuccess { get; set; }
    private string ResultMessage { get; set; } = string.Empty;

    // === Template System ===
    private string SelectedTemplate { get; set; } = "test";
    private Dictionary<string, TemplateInfo> Templates { get; set; } = new();

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

    // === Rich Text Editor ===
    private bool IsHtmlMode { get; set; }
    private string CustomMessageText { get; set; } = "Scrie mesajul tău aici...";
    private string CustomMessageHtml { get; set; } = "<h1>Email Personalizat</h1><p>Editează HTML-ul aici...</p>";
    private bool ShowEmojiPicker { get; set; }
    private List<string> Emojis { get; set; } = new()
    {
        "😀", "😃", "😄", "😁", "😊", "😇", "🙂", "🙃",
        "😉", "😌", "😍", "🥰", "😘", "😗", "😙", "😚",
        "😋", "😛", "😝", "😜", "🤪", "🤨", "🧐", "🤓",
        "😎", "🥳", "😏", "😒", "😞", "😔", "😟", "😕",
        "🙁", "☹️", "😣", "😖", "😫", "😩", "🥺", "😢",
        "👍", "👎", "👌", "✌️", "🤞", "🤟", "🤘", "🤙",
        "💪", "🙏", "✍️", "💅", "🤳", "💃", "🕺", "🎉",
        "🎊", "🎈", "🎁", "🏆", "🥇", "🥈", "🥉", "⭐",
        "🌟", "💫", "✨", "🔥", "💥", "💯", "✅", "❌",
        "💰", "💵", "💴", "💶", "💷", "📧", "📨", "📩",
        "📮", "📪", "📫", "📬", "📭", "📞", "☎️", "📱",
        "🏥", "🩺", "💊", "💉", "🩹", "🩸", "❤️", "💚"
    };

    // === 🆕 TEMPLATE MANAGER ===
    private bool ShowTemplateManager { get; set; }
    private bool IsCreatingTemplate { get; set; }
    private string? EditingTemplateKey { get; set; }
    private string NewTemplateName { get; set; } = string.Empty;
    private string NewTemplateDescription { get; set; } = string.Empty;
    private string NewTemplateIcon { get; set; } = "fas fa-envelope";
    private string NewTemplateHtml { get; set; } = string.Empty;
    private bool ShowDeleteConfirmation { get; set; }
    private string? DeletingTemplateKey { get; set; }

    // === Lifecycle Methods ===

    protected override async Task OnInitializedAsync()
    {
        await LoadDefaultTemplates();
        await LoadCustomTemplatesFromStorage();
    }

    private Task LoadDefaultTemplates()
    {
        Templates = new Dictionary<string, TemplateInfo>
        {
            ["test"] = new TemplateInfo("Test Basic", "fas fa-flask", "Email simplu de test", true, string.Empty),
            ["appointment"] = new TemplateInfo("Programare", "fas fa-calendar-check", "Confirmare programare", true, string.Empty),
            ["reminder"] = new TemplateInfo("Reminder", "fas fa-bell", "Reminder 24h înainte", true, string.Empty),
            ["custom"] = new TemplateInfo("Personalizat", "fas fa-edit", "Mesaj personalizat", true, string.Empty)
        };
        return Task.CompletedTask;
    }

    private async Task LoadCustomTemplatesFromStorage()
    {
        try
        {
            var json = await JSRuntime.InvokeAsync<string>("localStorage.getItem", "email-templates");
            if (!string.IsNullOrEmpty(json))
            {
                var customTemplates = JsonSerializer.Deserialize<Dictionary<string, TemplateInfo>>(json);
                if (customTemplates != null)
                {
                    foreach (var template in customTemplates)
                    {
                        Templates[template.Key] = template.Value;
                    }
                    Logger.LogInformation("Loaded {Count} custom templates from storage", customTemplates.Count);
                }
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error loading custom templates from storage");
        }
    }

    private async Task SaveCustomTemplatesToStorage()
    {
        try
        {
            var customTemplates = Templates.Where(t => !t.Value.IsDefault).ToDictionary(t => t.Key, t => t.Value);
            var json = JsonSerializer.Serialize(customTemplates);
            await JSRuntime.InvokeVoidAsync("localStorage.setItem", "email-templates", json);
            Logger.LogInformation("Saved {Count} custom templates to storage", customTemplates.Count);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error saving custom templates to storage");
        }
    }

    // === Template Manager Methods ===

    private void OpenTemplateManager()
    {
        Logger.LogInformation("🔷 OpenTemplateManager called - Current state: ShowTemplateManager={ShowTemplateManager}", ShowTemplateManager);

        ShowTemplateManager = true;
        IsCreatingTemplate = false;
        EditingTemplateKey = null;

        Logger.LogInformation("🔷 After setting: ShowTemplateManager={ShowTemplateManager}", ShowTemplateManager);
        StateHasChanged();
        Logger.LogInformation("🔷 StateHasChanged called");
    }

    private void CloseTemplateManager()
    {
        Logger.LogInformation("🔷 CloseTemplateManager called");

        ShowTemplateManager = false;
        IsCreatingTemplate = false;
        EditingTemplateKey = null;
        ClearTemplateForm();

        StateHasChanged();
    }

    private void StartCreateTemplate()
    {
        IsCreatingTemplate = true;
        EditingTemplateKey = null;
        ClearTemplateForm();

        // Set default HTML template
        NewTemplateHtml = @"<div style='font-family: -apple-system, BlinkMacSystemFont, ""Segoe UI"", Roboto, sans-serif; max-width: 600px; margin: 0 auto;'>
    <div style='background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); color: white; padding: 2rem; border-radius: 16px 16px 0 0;'>
        <h1 style='margin: 0; font-size: 2rem;'>Titlu Email</h1>
    </div>
    <div style='background: #f9fafb; padding: 2rem; border-radius: 0 0 16px 16px;'>
        <p style='color: #374151;'>Conținutul email-ului...</p>
    </div>
    <div style='text-align: center; padding: 1.5rem; color: #9ca3af; font-size: 0.875rem;'>
        <p style='margin: 0;'>© " + DateTime.Now.Year + @" ValyanClinic</p>
    </div>
</div>";
    }

    private void StartEditTemplate(string templateKey)
    {
        if (!Templates.ContainsKey(templateKey) || Templates[templateKey].IsDefault)
        {
            Logger.LogWarning("Cannot edit default template: {TemplateKey}", templateKey);
            return;
        }

        EditingTemplateKey = templateKey;
        IsCreatingTemplate = false;

        var template = Templates[templateKey];
        NewTemplateName = template.Name;
        NewTemplateDescription = template.Description;
        NewTemplateIcon = template.Icon;
        NewTemplateHtml = template.CustomHtml;

        ShowTemplateManager = true;
    }

    private async Task SaveTemplate()
    {
        if (string.IsNullOrWhiteSpace(NewTemplateName))
        {
            ResultMessage = "❌ Numele template-ului este obligatoriu!";
            IsSuccess = false;
            return;
        }

        if (string.IsNullOrWhiteSpace(NewTemplateHtml))
        {
            ResultMessage = "❌ Conținutul HTML este obligatoriu!";
            IsSuccess = false;
            return;
        }

        try
        {
            string templateKey;

            if (EditingTemplateKey != null)
            {
                // Edit existing template
                templateKey = EditingTemplateKey;
                Templates[templateKey] = new TemplateInfo(
                    NewTemplateName,
                    NewTemplateIcon,
                    NewTemplateDescription,
                    false,
                    NewTemplateHtml
                );
                Logger.LogInformation("Updated template: {TemplateKey}", templateKey);
            }
            else
            {
                // Create new template
                templateKey = "custom_" + Guid.NewGuid().ToString("N")[..8];
                Templates[templateKey] = new TemplateInfo(
                    NewTemplateName,
                    NewTemplateIcon,
                    NewTemplateDescription,
                    false,
                    NewTemplateHtml
                );
                Logger.LogInformation("Created new template: {TemplateKey}", templateKey);
            }

            await SaveCustomTemplatesToStorage();

            ResultMessage = $"✅ Template '{NewTemplateName}' salvat cu succes!";
            IsSuccess = true;

            CloseTemplateManager();
            StateHasChanged();
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error saving template");
            ResultMessage = $"❌ Eroare la salvarea template-ului: {ex.Message}";
            IsSuccess = false;
        }
    }

    private void CancelTemplateEdit()
    {
        IsCreatingTemplate = false;
        EditingTemplateKey = null;
        ClearTemplateForm();
    }

    private void ClearTemplateForm()
    {
        NewTemplateName = string.Empty;
        NewTemplateDescription = string.Empty;
        NewTemplateIcon = "fas fa-envelope";
        NewTemplateHtml = string.Empty;
    }

    private void StartDeleteTemplate(string templateKey)
    {
        if (!Templates.ContainsKey(templateKey) || Templates[templateKey].IsDefault)
        {
            Logger.LogWarning("Cannot delete default template: {TemplateKey}", templateKey);
            return;
        }

        DeletingTemplateKey = templateKey;
        ShowDeleteConfirmation = true;
    }

    private async Task ConfirmDelete()
    {
        if (DeletingTemplateKey == null || !Templates.ContainsKey(DeletingTemplateKey))
        {
            return;
        }

        try
        {
            var templateName = Templates[DeletingTemplateKey].Name;
            Templates.Remove(DeletingTemplateKey);

            await SaveCustomTemplatesToStorage();

            Logger.LogInformation("Deleted template: {TemplateKey}", DeletingTemplateKey);

            ResultMessage = $"✅ Template '{templateName}' șters cu succes!";
            IsSuccess = true;

            // If deleted template was selected, switch to default
            if (SelectedTemplate == DeletingTemplateKey)
            {
                SelectedTemplate = "test";
            }

            CancelDelete();
            StateHasChanged();
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error deleting template");
            ResultMessage = $"❌ Eroare la ștergerea template-ului: {ex.Message}";
            IsSuccess = false;
        }
    }

    private void CancelDelete()
    {
        ShowDeleteConfirmation = false;
        DeletingTemplateKey = null;
    }

    // === Existing Event Handlers ===

    private void SelectTemplate(string templateKey)
    {
        SelectedTemplate = templateKey;
        EmailSubject = templateKey switch
        {
            "test" => "🎉 Test Email - ValyanClinic",
            "appointment" => "✅ Confirmare Programare - ValyanClinic",
            "reminder" => "⏰ Reminder Programare - ValyanClinic",
            "custom" => "📧 Email Personalizat",
            _ => Templates.ContainsKey(templateKey) ? $"📧 {Templates[templateKey].Name}" : "Email - ValyanClinic"
        };

        // Load custom HTML if it's a custom template
        if (Templates.ContainsKey(templateKey) && !Templates[templateKey].IsDefault && !string.IsNullOrEmpty(Templates[templateKey].CustomHtml))
        {
            CustomMessageHtml = Templates[templateKey].CustomHtml;
            CustomMessageText = Regex.Replace(CustomMessageHtml, "<.*?>", string.Empty);
        }

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

    // === Rich Text Editor Methods ===

    private void ToggleEditorMode(bool htmlMode)
    {
        IsHtmlMode = htmlMode;

        if (htmlMode && !string.IsNullOrEmpty(CustomMessageText))
        {
            CustomMessageHtml = $"<p>{CustomMessageText.Replace("\n", "</p><p>")}</p>";
        }
        else if (!htmlMode && !string.IsNullOrEmpty(CustomMessageHtml))
        {
            CustomMessageText = Regex.Replace(CustomMessageHtml, "<.*?>", string.Empty);
        }

        StateHasChanged();
    }

    private void InsertFormatting(string format)
    {
        var placeholder = "[text]";

        CustomMessageText += format switch
        {
            "bold" => $" <strong>{placeholder}</strong>",
            "italic" => $" <em>{placeholder}</em>",
            "underline" => $" <u>{placeholder}</u>",
            "h1" => $"\n<h1>{placeholder}</h1>\n",
            "ul" => $"\n<ul>\n  <li>{placeholder}</li>\n</ul>\n",
            "link" => $" <a href='#'>{placeholder}</a>",
            _ => ""
        };

        StateHasChanged();
    }

    private void ToggleEmojiPicker()
    {
        ShowEmojiPicker = !ShowEmojiPicker;
    }

    private void InsertEmoji(string emoji)
    {
        CustomMessageText += emoji;
        ShowEmojiPicker = false;
        StateHasChanged();
    }

    private string GetPreviewHtml()
    {
        if (Templates.ContainsKey(SelectedTemplate) && !Templates[SelectedTemplate].IsDefault && !string.IsNullOrEmpty(Templates[SelectedTemplate].CustomHtml))
        {
            return GenerateCustomTemplateHtml(Templates[SelectedTemplate].CustomHtml);
        }

        return SelectedTemplate switch
        {
            "test" => GenerateTestEmailHtml(),
            "appointment" => GenerateAppointmentEmailHtml(),
            "reminder" => GenerateReminderEmailHtml(),
            "custom" => GenerateCustomEmailHtmlFromEditor(),
            _ => GenerateTestEmailHtml()
        };
    }

    // === Send Email ===

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

    private string GenerateCustomEmailHtmlFromEditor()
    {
        var content = IsHtmlMode ? CustomMessageHtml : $"<p>{CustomMessageText.Replace("\n", "</p><p>")}</p>";

        return $@"
<div style='font-family: -apple-system, BlinkMacSystemFont, ""Segoe UI"", Roboto, sans-serif; max-width: 600px; margin: 0 auto;'>
    <div style='background: linear-gradient(135deg, #8b5cf6 0%, #7c3aed 100%); color: white; padding: 2rem; border-radius: 16px 16px 0 0;'>
        <h1 style='margin: 0; font-size: 2rem;'>📧 Email Personalizat</h1>
    </div>
    <div style='background: #f9fafb; padding: 2rem; border-radius: 0 0 16px 16px;'>
        {content}
    </div>
    <div style='text-align: center; padding: 1.5rem; color: #9ca3af; font-size: 0.875rem;'>
        <p style='margin: 0;'>© {DateTime.Now.Year} ValyanClinic</p>
    </div>
</div>";
    }

    private string GenerateCustomTemplateHtml(string customHtml)
    {
        return customHtml;
    }

    // === Helper Classes ===

    private class TemplateInfo
    {
        public string Name { get; set; }
        public string Icon { get; set; }
        public string Description { get; set; }
        public bool IsDefault { get; set; }
        public string CustomHtml { get; set; }

        public TemplateInfo(string name, string icon, string description, bool isDefault, string customHtml)
        {
            Name = name;
            Icon = icon;
            Description = description;
            IsDefault = isDefault;
            CustomHtml = customHtml;
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
