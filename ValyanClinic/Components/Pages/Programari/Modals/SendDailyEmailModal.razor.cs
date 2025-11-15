using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using ValyanClinic.Services.Email;
using MediatR;
using ValyanClinic.Application.Features.ProgramareManagement.Queries.GetProgramariByDate;

namespace ValyanClinic.Components.Pages.Programari.Modals;

public partial class SendDailyEmailModal : ComponentBase
{
    [Inject] private IEmailService EmailService { get; set; } = default!;
    [Inject] private ILogger<SendDailyEmailModal> Logger { get; set; } = default!;
    [Inject] private IMediator Mediator { get; set; } = default!;

    [Parameter] public bool IsVisible { get; set; }
    [Parameter] public EventCallback<bool> IsVisibleChanged { get; set; }
    [Parameter] public DateTime TargetDate { get; set; } = DateTime.Today.AddDays(1);
  [Parameter] public EventCallback<int> OnEmailsSent { get; set; }

    private bool IsLoading { get; set; }
    private bool IsSending { get; set; }
    private List<DoctorRecipientDto>? DoctorRecipients { get; set; }
  private int TotalAppointments { get; set; }

    protected override async Task OnParametersSetAsync()
{
        if (IsVisible && DoctorRecipients == null)
 {
     await LoadPreviewData();
      }
    }

    private async Task LoadPreviewData()
    {
IsLoading = true;
        StateHasChanged();

        try
     {
  Logger.LogInformation("📧 Loading preview data for {Date}", TargetDate.ToString("yyyy-MM-dd"));

   // ✅ Query real data from DB using MediatR
            var query = new GetProgramariByDateQuery
  {
  Date = TargetDate,  // ✅ FIXED: property name is Date, not DataProgramare
     DoctorID = null  // All doctors
    };

 var result = await Mediator.Send(query);

       if (!result.IsSuccess || result.Value == null || !result.Value.Any())
  {
     Logger.LogWarning("⚠️ No appointments found for {Date}", TargetDate.ToString("yyyy-MM-dd"));
   DoctorRecipients = new List<DoctorRecipientDto>();
        TotalAppointments = 0;
       return;
      }

   var allProgramari = result.Value.ToList();
  Logger.LogInformation("✅ Loaded {Count} appointments for {Date}", 
 allProgramari.Count, TargetDate.ToString("yyyy-MM-dd"));

    // Group by Doctor
    var groupedByDoctor = allProgramari
  .Where(p => !string.IsNullOrEmpty(p.DoctorEmail)) // Only doctors with email
          .GroupBy(p => p.DoctorID)
        .ToList();

     DoctorRecipients = groupedByDoctor.Select(g => new DoctorRecipientDto
       {
         DoctorID = g.Key,
        NumeComplet = g.First().DoctorNumeComplet ?? "Nume necunoscut",
    Email = g.First().DoctorEmail!,
    Specializare = g.First().DoctorSpecializare,
       NumarProgramari = g.Count(),
   Programari = g.OrderBy(p => p.OraInceput).ToList()  // ✅ NEW - include programările
  }).ToList();

      TotalAppointments = DoctorRecipients.Sum(d => d.NumarProgramari);

      Logger.LogInformation("✅ Loaded {DoctorCount} doctors with {TotalAppts} appointments",
       DoctorRecipients.Count, TotalAppointments);

 // Log warning for doctors without email
var doctorsWithoutEmail = allProgramari
              .Where(p => string.IsNullOrEmpty(p.DoctorEmail))
                .Select(p => p.DoctorNumeComplet)
    .Distinct()
              .ToList();

    if (doctorsWithoutEmail.Any())
       {
  Logger.LogWarning("⚠️ {Count} doctors without email will not receive notifications: {Doctors}",
        doctorsWithoutEmail.Count, string.Join(", ", doctorsWithoutEmail));
      }
        }
      catch (Exception ex)
        {
 Logger.LogError(ex, "❌ Error loading preview data");
       DoctorRecipients = new List<DoctorRecipientDto>();
     TotalAppointments = 0;
    }
        finally
  {
  IsLoading = false;
        StateHasChanged();
     }
    }

    private async Task HandleSend()
    {
  if (DoctorRecipients?.Any() != true)
 {
      return;
  }

        IsSending = true;
  StateHasChanged();

        try
    {
  Logger.LogInformation("📧 Sending emails to {Count} doctors for {Date}",
     DoctorRecipients.Count, TargetDate.ToString("yyyy-MM-dd"));

   var emailsSent = await EmailService.SendDailyAppointmentsEmailAsync(TargetDate);

  await OnEmailsSent.InvokeAsync(emailsSent);
  await Close();
   }
        catch (Exception ex)
        {
        Logger.LogError(ex, "❌ Error sending emails");
        }
        finally
     {
 IsSending = false;
    StateHasChanged();
   }
    }

    private async Task Close()
    {
        DoctorRecipients = null;
     TotalAppointments = 0;
        await IsVisibleChanged.InvokeAsync(false);
    }

    private void HandleOverlayClick()
    {
   if (!IsSending)
  {
_ = Close();
      }
}

    /// <summary>
    /// ✅ Helper - Status color pentru preview
    /// </summary>
    private string GetStatusColorPreview(string? status) => status?.ToLower() switch
    {
    "programata" => "#94a3b8",
        "confirmata" => "#3b82f6",
     "checkedin" => "#f59e0b",
        "inconsultatie" => "#8b5cf6",
        "finalizata" => "#10b981",
        "anulata" => "#ef4444",
      _ => "#6b7280"
    };

    /// <summary>
    /// ✅ Helper - Status display name pentru preview
    /// </summary>
    private string GetStatusDisplayPreview(string? status) => status?.ToLower() switch
    {
        "programata" => "Programată",
        "confirmata" => "Confirmată",
        "checkedin" => "Check-in",
"inconsultatie" => "În consultație",
   "finalizata" => "Finalizată",
        "anulata" => "Anulată",
        _ => status ?? "Necunoscut"
    };

    // DTO for doctor recipients
    public class DoctorRecipientDto
    {
        public Guid DoctorID { get; set; }
        public string NumeComplet { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
   public string? Specializare { get; set; }
      public int NumarProgramari { get; set; }
        
     /// <summary>
 /// ✅ NEW - Lista programărilor pentru acest doctor (pentru preview)
     /// </summary>
        public List<Application.Features.ProgramareManagement.DTOs.ProgramareListDto> Programari { get; set; } = new();
    }
}
