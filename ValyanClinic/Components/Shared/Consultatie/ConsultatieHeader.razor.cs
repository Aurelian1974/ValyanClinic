using Microsoft.AspNetCore.Components;
using ValyanClinic.Application.Features.PacientManagement.Queries.GetPacientById;

namespace ValyanClinic.Components.Shared.Consultatie;

public partial class ConsultatieHeader : ComponentBase
{
    [Parameter] public PacientDetailDto? PacientInfo { get; set; }
    [Parameter] public bool IsLoading { get; set; }
    [Parameter] public DateTime? LastSaveTime { get; set; }
    [Parameter] public bool ShowDraftInfo { get; set; } = true;
    [Parameter] public EventCallback OnClose { get; set; }

    private async Task OnCloseClicked()
    {
        await OnClose.InvokeAsync();
    }

    private int GetAge(DateTime birthDate)
    {
        var today = DateTime.Today;
        var age = today.Year - birthDate.Year;
        if (birthDate.Date > today.AddYears(-age)) age--;
        return age;
    }

    private string GetTimeSince(DateTime savedTime)
    {
        var timeSince = DateTime.Now - savedTime;

        if (timeSince.TotalSeconds < 60)
            return "acum";
        else if (timeSince.TotalMinutes < 60)
            return $"acum {(int)timeSince.TotalMinutes} min";
        else if (timeSince.TotalHours < 24)
            return $"acum {(int)timeSince.TotalHours}h";
        else
            return $"acum {(int)timeSince.TotalDays} zile";
    }
}
