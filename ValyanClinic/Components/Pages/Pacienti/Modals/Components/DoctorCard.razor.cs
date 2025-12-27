using Microsoft.AspNetCore.Components;
using ValyanClinic.Application.Features.PacientPersonalMedicalManagement.DTOs;

namespace ValyanClinic.Components.Pages.Pacienti.Modals.Components;

/// <summary>
/// Code-behind for DoctorCard component.
/// Displays a single associated doctor with contact info and actions.
/// </summary>
public partial class DoctorCard
{
    [Parameter, EditorRequired]
    public DoctorAsociatDto Doctor { get; set; } = default!;

    [Parameter]
    public bool IsInactive { get; set; } = false;

    [Parameter]
    public EventCallback<DoctorAsociatDto> OnDeactivate { get; set; }

    [Parameter]
    public EventCallback<DoctorAsociatDto> OnReactivate { get; set; }

    private string FormatZile(int zile)
    {
        if (zile == 0) return "astăzi";
        if (zile == 1) return "1 zi";
        if (zile < 30) return $"{zile} zile";
        if (zile < 365)
        {
            int luni = zile / 30;
            return luni == 1 ? "1 lună" : $"{luni} luni";
        }
        int ani = zile / 365;
        return ani == 1 ? "1 an" : $"{ani} ani";
    }

    private string GetBadgeClass(string? tipRelatie)
    {
        return tipRelatie?.ToLower() switch
        {
            "medic curant" => "badge-primary",
            "medic de familie" => "badge-success",
            "specialist" => "badge-info",
            "chirurg" => "badge-warning",
            _ => "badge-secondary"
        };
    }
}
