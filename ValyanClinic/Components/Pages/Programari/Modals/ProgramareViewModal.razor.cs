using MediatR;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using ValyanClinic.Application.Features.ProgramareManagement.DTOs;
using ValyanClinic.Application.Features.ProgramareManagement.Queries.GetProgramareById;

namespace ValyanClinic.Components.Pages.Programari.Modals;

public partial class ProgramareViewModal : ComponentBase
{
    [Parameter] public bool IsVisible { get; set; }
    [Parameter] public EventCallback<bool> IsVisibleChanged { get; set; }
    [Parameter] public Guid ProgramareId { get; set; }
    [Parameter] public EventCallback<Guid> OnEditRequested { get; set; }

    [Inject] private IMediator Mediator { get; set; } = default!;
    [Inject] private ILogger<ProgramareViewModal> Logger { get; set; } = default!;

    private bool IsLoading = false;
    private ProgramareDetailDto? Programare = null;

    protected override async Task OnParametersSetAsync()
    {
        if (IsVisible && ProgramareId != Guid.Empty)
        {
            Logger.LogInformation("ProgramareViewModal opened for ID: {ID}", ProgramareId);
            await LoadProgramareDataAsync();
            StateHasChanged(); // ✅ CRITICAL: Force re-render after data load
        }
    }

    private async Task LoadProgramareDataAsync()
    {
        try
        {
            IsLoading = true;
            StateHasChanged(); // ✅ Show loading spinner

            var query = new GetProgramareByIdQuery(ProgramareId);
            var result = await Mediator.Send(query);

            if (result.IsSuccess && result.Value != null)
            {
                Programare = result.Value;
                Logger.LogInformation("✅ Programare loaded successfully: {ID} - {Pacient} cu {Doctor} la {Data}",
                    ProgramareId,
                    Programare.PacientNumeComplet,
                    Programare.DoctorNumeComplet,
                    Programare.DataProgramare.ToString("yyyy-MM-dd"));
            }
            else
            {
                Logger.LogWarning("⚠️ Failed to load programare {ID}. Errors: {Errors}",
                    ProgramareId,
                    string.Join(", ", result.Errors ?? new List<string>()));
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "❌ Eroare la încărcarea programării {ProgramareID}", ProgramareId);
        }
        finally
        {
            IsLoading = false;
            StateHasChanged(); // ✅ CRITICAL: Update UI after loading
        }
    }

    private async Task CloseModal()
    {
        Programare = null;
        await IsVisibleChanged.InvokeAsync(false);
    }

    private async Task OpenEditMode()
    {
        Logger.LogInformation("Edit mode requested for programare {ID}", ProgramareId);
        await OnEditRequested.InvokeAsync(ProgramareId);
        await CloseModal();
    }

    private bool CanEditProgramare(string status)
    {
        return status is "Programata" or "Confirmata";
    }

    private string GetStatusDisplay(string status)
    {
        return status switch
        {
            "Programata" => "Programată",
            "Confirmata" => "Confirmată",
            "CheckedIn" => "Check-in",
            "InConsultatie" => "În consultație",
            "Finalizata" => "Finalizată",
            "Anulata" => "Anulată",
            "NoShow" => "Nu s-a prezentat",
            _ => status
        };
    }

    private string GetStatusBadgeClass(string status)
    {
        return status switch
        {
            "Programata" => "secondary",
            "Confirmata" => "info",
            "CheckedIn" => "primary",
            "InConsultatie" => "warning",
            "Finalizata" => "success",
            "Anulata" => "danger",
            "NoShow" => "dark",
            _ => "secondary"
        };
    }

    private string GetTipProgramareDisplay(string? tipProgramare)
    {
        return tipProgramare switch
        {
            "ConsultatieInitiala" => "Consultație Inițială",
            "ControlPeriodic" => "Control Periodic",
            "Consultatie" => "Consultație",
            "Investigatie" => "Investigație",
            "Procedura" => "Procedură",
            "Urgenta" => "Urgență",
            "Telemedicina" => "Telemedicină",
            "LaDomiciliu" => "La Domiciliu",
            _ => tipProgramare ?? "-"
        };
    }

    private string GetTipProgramareBadgeClass(string? tipProgramare)
    {
        return tipProgramare switch
        {
            "ConsultatieInitiala" => "primary",
            "ControlPeriodic" => "info",
            "Consultatie" => "secondary",
            "Investigatie" => "warning",
            "Procedura" => "success",
            "Urgenta" => "danger",
            "Telemedicina" => "dark",
            "LaDomiciliu" => "purple",
            _ => "secondary"
        };
    }
}
