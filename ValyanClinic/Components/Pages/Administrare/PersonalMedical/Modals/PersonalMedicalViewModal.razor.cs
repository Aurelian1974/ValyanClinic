using MediatR;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.Logging;
using ValyanClinic.Application.Features.PersonalMedicalManagement.Queries.GetPersonalMedicalById;
using ValyanClinic.Application.Features.PersonalMedicalManagement.Queries.GetPacientiByDoctor;
using ValyanClinic.Application.Interfaces;

namespace ValyanClinic.Components.Pages.Administrare.PersonalMedical.Modals;

public partial class PersonalMedicalViewModal : ComponentBase
{
    [Inject] private IMediator Mediator { get; set; } = default!;
    [Inject] private ILogger<PersonalMedicalViewModal> Logger { get; set; } = default!;
    [Inject] private IFieldPermissionService FieldPermissions { get; set; } = default!;
    [Inject] private AuthenticationStateProvider AuthStateProvider { get; set; } = default!;

    [Parameter] public EventCallback<Guid> OnEditRequested { get; set; }
    [Parameter] public EventCallback<Guid> OnDeleteRequested { get; set; }
    [Parameter] public EventCallback OnClosed { get; set; }

    private bool IsVisible { get; set; }
    private bool IsLoading { get; set; }
    private bool HasError { get; set; }
    private string ErrorMessage { get; set; } = string.Empty;
    private PersonalMedicalDetailDto? PersonalMedicalData { get; set; }
    private string ActiveTab { get; set; } = "date";
    private Guid CurrentPersonalID { get; set; }

    // ✅ ADDED: State pentru pacienți asociați
    private List<PacientAsociatDto> PacientiAsociati { get; set; } = new();
    private bool IsLoadingPacienti { get; set; }

    // Computed properties pentru filtrare
    private List<PacientAsociatDto> PacientiActivi =>
        PacientiAsociati.Where(p => p.EsteActiv).ToList();

    private List<PacientAsociatDto> PacientiInactivi =>
        PacientiAsociati.Where(p => !p.EsteActiv).ToList();

    public async Task Open(Guid personalID)
    {
        try
        {
            CurrentPersonalID = personalID;
            IsVisible = true;
            IsLoading = true;
            HasError = false;
            ErrorMessage = string.Empty;
            PersonalMedicalData = null;
            ActiveTab = "date";
            PacientiAsociati = new(); // ✅ ADDED: Reset listă pacienți

            await InvokeAsync(StateHasChanged);
            await LoadFieldPermissionsAsync();
            await LoadPersonalMedicalData(personalID);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error opening modal for PersonalID: {PersonalID}", personalID);
            HasError = true;
            ErrorMessage = $"Error opening modal: {ex.Message}";
            IsLoading = false;
            await InvokeAsync(StateHasChanged);
        }
    }

    public async Task Close()
    {
        IsVisible = false;
        await InvokeAsync(StateHasChanged);

        if (OnClosed.HasDelegate)
        {
            await OnClosed.InvokeAsync();
        }

        await Task.Delay(300);

        PersonalMedicalData = null;
        IsLoading = false;
        HasError = false;
        ErrorMessage = string.Empty;
        CurrentPersonalID = Guid.Empty;
        PacientiAsociati = new(); // ✅ ADDED: Reset listă pacienți
    }

    #region Field Permissions
    
    private bool CanViewField(string fieldName) => 
        FieldPermissions.CanViewField("Personal", fieldName);
    
    private async Task LoadFieldPermissionsAsync()
    {
        try
        {
            var authState = await AuthStateProvider.GetAuthenticationStateAsync();
            var userId = authState.User?.Identity?.Name;
            if (!string.IsNullOrEmpty(userId))
            {
                await FieldPermissions.LoadPermissionsAsync(userId);
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error loading field permissions");
        }
    }
    
    #endregion

    /// <summary>
    /// Reîncarcă datele pentru ID-ul curent (folosit după editări)
    /// </summary>
    public async Task RefreshData()
    {
        if (CurrentPersonalID != Guid.Empty && IsVisible)
        {
            Logger.LogInformation("Refreshing data for current PersonalID: {PersonalID}", CurrentPersonalID);
            await LoadPersonalMedicalData(CurrentPersonalID);

            // ✅ ADDED: Refresh și lista de pacienți dacă tab-ul este activ
            if (ActiveTab == "pacienti")
            {
                await LoadPacienti();
            }
        }
    }

    private async Task LoadPersonalMedicalData(Guid personalID)
    {
        try
        {
            Logger.LogInformation("Loading PersonalMedical data: {PersonalID}", personalID);

            var query = new GetPersonalMedicalByIdQuery(personalID);
            var result = await Mediator.Send(query);

            if (result.IsSuccess && result.Value != null)
            {
                PersonalMedicalData = result.Value;
                HasError = false;
                Logger.LogInformation("Data loaded successfully for {PersonalID}", personalID);

                // ✅ FIX: Încarcă pacienții imediat după încărcarea datelor doctorului
                // pentru a afișa counter-ul corect în tab
                await LoadPacienti();
            }
            else
            {
                HasError = true;
                ErrorMessage = result.Errors?.FirstOrDefault() ?? "Could not load personal medical data";
                Logger.LogWarning("Failed to load data for {PersonalID}: {Error}", personalID, ErrorMessage);
            }
        }
        catch (Exception ex)
        {
            HasError = true;
            ErrorMessage = $"Error loading data: {ex.Message}";
            Logger.LogError(ex, "Exception loading data for {PersonalID}", personalID);
        }
        finally
        {
            IsLoading = false;
            await InvokeAsync(StateHasChanged);
        }
    }

    // ✅ ADDED: Metodă pentru încărcarea pacienților
    private async Task LoadPacienti()
    {
        if (CurrentPersonalID == Guid.Empty) return;

        IsLoadingPacienti = true;
        await InvokeAsync(StateHasChanged);

        try
        {
            Logger.LogInformation("Loading pacienti for doctor: {DoctorID}", CurrentPersonalID);

            var query = new GetPacientiByDoctorQuery(CurrentPersonalID);
            var result = await Mediator.Send(query);

            if (result.IsSuccess && result.Value != null)
            {
                PacientiAsociati = result.Value;
                Logger.LogInformation("Loaded {Count} pacienti for doctor {DoctorID}",
                    PacientiAsociati.Count, CurrentPersonalID);
            }
            else
            {
                PacientiAsociati = new();
                Logger.LogWarning("Failed to load pacienti for doctor {DoctorID}: {Error}",
                    CurrentPersonalID, result.Errors?.FirstOrDefault());
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error loading pacienti for doctor {DoctorID}", CurrentPersonalID);
            PacientiAsociati = new();
        }
        finally
        {
            IsLoadingPacienti = false;
            await InvokeAsync(StateHasChanged);
        }
    }

    private async Task SetActiveTab(string tabName)
    {
        ActiveTab = tabName;
        Logger.LogDebug("Tab changed to: {TabName}", tabName);

        // ✅ UPDATED: Nu mai este nevoie să încărcăm pacienții aici
        // deoarece sunt încărcați deja la deschiderea modal-ului
        // Lăsăm codul doar pentru re-fetch manual dacă este necesar
        // if (tabName == "pacienti" && PacientiAsociati.Count == 0)
        // {
        //     await LoadPacienti();
        // }

        await InvokeAsync(StateHasChanged);
    }

    // ✅ ADDED: Helper pentru formatarea zilelor
    private string FormatZile(int zile)
    {
        if (zile < 30)
            return $"{zile} zile";
        else if (zile < 365)
            return $"{zile / 30} luni";
        else
            return $"{zile / 365} ani";
    }

    // ✅ ADDED: Helper pentru clasa badge-ului tip relație
    private string GetBadgeClass(string? tipRelatie)
    {
        return tipRelatie?.ToLower() switch
        {
            "medic primar" => "badge-primary",
            "medic consultant" => "badge-info",
            "medic specialist" => "badge-success",
            _ => "badge-secondary"
        };
    }

    private async Task HandleOverlayClick()
    {
        await Close();
    }

    private async Task HandleEdit()
    {
        if (CurrentPersonalID != Guid.Empty)
        {
            Logger.LogInformation("Edit requested for PersonalID: {PersonalID}", CurrentPersonalID);
            await OnEditRequested.InvokeAsync(CurrentPersonalID);
        }
    }

    private async Task HandleDelete()
    {
        if (CurrentPersonalID != Guid.Empty)
        {
            Logger.LogInformation("Delete requested for PersonalID: {PersonalID}", CurrentPersonalID);
            await OnDeleteRequested.InvokeAsync(CurrentPersonalID);
        }
    }
}
