using Microsoft.AspNetCore.Components;
using ValyanClinic.Application.Features.PacientPersonalMedicalManagement.DTOs;

namespace ValyanClinic.Components.Pages.Pacienti.Modals.Components;

/// <summary>
/// Code-behind for DoctoriTabContent component.
/// Displays associated doctors (active and inactive) for a patient.
/// </summary>
public partial class DoctoriTabContent
{
    [Parameter, EditorRequired]
    public List<DoctorAsociatDto> DoctoriAsociati { get; set; } = new();

    [Parameter]
    public bool IsLoading { get; set; }

    [Parameter]
    public EventCallback OnAddDoctor { get; set; }

    [Parameter]
    public EventCallback<DoctorAsociatDto> OnDeactivateDoctor { get; set; }

    [Parameter]
    public EventCallback<DoctorAsociatDto> OnReactivateDoctor { get; set; }

    private IEnumerable<DoctorAsociatDto> DoctoriActivi => 
        DoctoriAsociati.Where(d => d.EsteActiv).OrderByDescending(d => d.DataAsocierii);

    private IEnumerable<DoctorAsociatDto> DoctoriInactivi => 
        DoctoriAsociati.Where(d => !d.EsteActiv).OrderByDescending(d => d.DataDezactivarii);
}
