using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.Logging;
using ValyanClinic.Application.Features.PacientManagement.Queries.GetPacientById;
using ValyanClinic.Application.Features.PacientPersonalMedicalManagement.Queries.GetDoctoriByPacient;
using ValyanClinic.Application.Features.PacientPersonalMedicalManagement.DTOs;
using ValyanClinic.Application.Interfaces;

namespace ValyanClinic.Components.Pages.Pacienti.Modals;

/// <summary>
/// Code-behind pentru PacientViewModal - Modal read-only pentru vizualizare detalii pacient
/// </summary>
public partial class PacientViewModal : ComponentBase
{
    #region Injected Services
    [Inject] private IMediator Mediator { get; set; } = default!;
    [Inject] private ILogger<PacientViewModal> Logger { get; set; } = default!;
    [Inject] private IFieldPermissionService FieldPermissions { get; set; } = default!;
    [Inject] private AuthenticationStateProvider AuthStateProvider { get; set; } = default!;
    #endregion

    #region Field Permission Helpers
    private bool _permissionsLoaded;
    
    private bool CanViewField(string fieldName) => 
        FieldPermissions.CanViewField("Pacient", fieldName);
    
    private async Task LoadFieldPermissionsAsync()
    {
        if (_permissionsLoaded) return;
        
        try
        {
            var authState = await AuthStateProvider.GetAuthenticationStateAsync();
            var roleClaim = authState.User.FindFirst(ClaimTypes.Role);
            
            if (roleClaim != null)
            {
                await FieldPermissions.LoadPermissionsAsync(roleClaim.Value);
                _permissionsLoaded = true;
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to load field permissions");
        }
    }
    #endregion

    #region Parameters
    /// <summary>
    /// Vizibilitatea modalului
    /// </summary>
    [Parameter] public bool IsVisible { get; set; }

    /// <summary>
    /// Event callback pentru schimbarea vizibilității
    /// </summary>
    [Parameter] public EventCallback<bool> IsVisibleChanged { get; set; }

    /// <summary>
    /// ID-ul pacientului de vizualizat
    /// </summary>
    [Parameter] public Guid? PacientId { get; set; }

    /// <summary>
    /// Event callback cand modalul se inchide
    /// </summary>
    [Parameter] public EventCallback OnClosed { get; set; }
    #endregion

    #region State Properties
    /// <summary>
    /// Indicator de incarcare date
    /// </summary>
    private bool IsLoading { get; set; }

    /// <summary>
    /// Indicator de eroare
    /// </summary>
    private bool HasError { get; set; }

    /// <summary>
    /// Mesaj de eroare
    /// </summary>
    private string ErrorMessage { get; set; } = string.Empty;

    /// <summary>
    /// Datele pacientului incarcate
    /// </summary>
    private PacientDetailDto? PacientData { get; set; }

    /// <summary>
    /// Lista doctorilor asociați
    /// </summary>
    private List<DoctorAsociatDto> DoctoriAsociati { get; set; } = new();

    /// <summary>
    /// Doctori activi (computed property)
    /// </summary>
    private List<DoctorAsociatDto> DoctoriActivi => DoctoriAsociati.Where(d => d.EsteActiv).ToList();

    /// <summary>
    /// Doctori inactivi (computed property)
    /// </summary>
    private List<DoctorAsociatDto> DoctoriInactivi => DoctoriAsociati.Where(d => !d.EsteActiv).ToList();

    /// <summary>
    /// Indicator de încărcare doctori
    /// </summary>
    private bool IsLoadingDoctori { get; set; }

    /// <summary>
    /// Tab-ul activ din modal
    /// </summary>
    private string ActiveTab { get; set; } = "personal";
    #endregion

    #region Lifecycle Methods
    protected override async Task OnParametersSetAsync()
    {
        if (IsVisible && PacientId.HasValue && PacientId.Value != Guid.Empty)
        {
            // ✅ Încarcă permisiunile pentru vizualizare
            await LoadFieldPermissionsAsync();
            await LoadPacientData(PacientId.Value);
        }
    }
    #endregion

    #region Private Methods
    /// <summary>
    /// Incarca datele pacientului din backend
    /// </summary>
    private async Task LoadPacientData(Guid pacientId)
    {
        IsLoading = true;
        HasError = false;

        try
        {
            Logger.LogInformation("Incarca datele pacientului {PacientId} pentru vizualizare", pacientId);

            var query = new GetPacientByIdQuery(pacientId);
            var result = await Mediator.Send(query);

            if (result.IsSuccess && result.Value != null)
            {
                PacientData = result.Value;
                HasError = false;
                Logger.LogInformation("Date pacient incarcate cu succes pentru {PacientId}", pacientId);

                // Încarcă și doctorii asociați
                await LoadDoctoriAsociati(pacientId);
            }
            else
            {
                HasError = true;
                ErrorMessage = result.Errors?.FirstOrDefault() ?? "Nu s-au putut incarca datele pacientului";
                Logger.LogWarning("Eroare la incarcarea datelor pacientului {PacientId}: {Error}",
                    pacientId, ErrorMessage);
            }
        }
        catch (Exception ex)
        {
            HasError = true;
            ErrorMessage = $"Eroare la incarcarea datelor: {ex.Message}";
            Logger.LogError(ex, "Exceptie la incarcarea datelor pacientului {PacientId}", pacientId);
        }
        finally
        {
            IsLoading = false;
            await InvokeAsync(StateHasChanged);
        }
    }

    /// <summary>
    /// Încarcă lista de doctori asociați pacientului
    /// </summary>
    private async Task LoadDoctoriAsociati(Guid pacientId)
    {
        IsLoadingDoctori = true;

        try
        {
            Logger.LogInformation("[PacientViewModal] Loading doctori for PacientID: {PacientId}", pacientId);

            var query = new GetDoctoriByPacientQuery(pacientId, ApenumereActivi: false);
            var result = await Mediator.Send(query);

            if (result.IsSuccess && result.Value != null)
            {
                DoctoriAsociati = result.Value;
                Logger.LogInformation("[PacientViewModal] Loaded {Count} doctori", DoctoriAsociati.Count);
            }
            else
            {
                DoctoriAsociati = new List<DoctorAsociatDto>();
                Logger.LogWarning("[PacientViewModal] Failed to load doctori: {Error}", result.FirstError);
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "[PacientViewModal] Exception loading doctori");
            DoctoriAsociati = new List<DoctorAsociatDto>();
        }
        finally
        {
            IsLoadingDoctori = false;
        }
    }

    /// <summary>
    /// Seteaza tab-ul activ
    /// </summary>
    private void SetActiveTab(string tabName)
    {
        ActiveTab = tabName;
        Logger.LogDebug("Tab activ schimbat in: {TabName}", tabName);
    }

    /// <summary>
    /// Handler pentru click pe overlay (inchide modalul)
    /// </summary>
    private async Task HandleOverlayClick()
    {
        await Close();
    }

    /// <summary>
    /// Inchide modalul
    /// </summary>
    private async Task Close()
    {
        IsVisible = false;
        await IsVisibleChanged.InvokeAsync(false);

        // Notify parent
        if (OnClosed.HasDelegate)
        {
            await OnClosed.InvokeAsync();
        }

        // Reset state after animation
        await Task.Delay(300);
        PacientData = null;
        DoctoriAsociati = new();
        IsLoading = false;
        IsLoadingDoctori = false;
        HasError = false;
        ErrorMessage = string.Empty;
        ActiveTab = "personal";
    }

    /// <summary>
    /// Returnează clasa CSS pentru badge-ul tipului de relație
    /// </summary>
    private string GetBadgeClass(string? tipRelatie)
    {
        return tipRelatie switch
        {
            "MedicPrimar" => "badge-primary",
            "Specialist" => "badge-info",
            "MedicConsultant" => "badge-success",
            "MedicDeGarda" => "badge-warning",
            "MedicFamilie" => "badge-secondary",
            _ => "badge-secondary"
        };
    }

    /// <summary>
    /// Formatează numărul de zile în text friendly
    /// </summary>
    private string FormatZile(int zile)
    {
        if (zile < 30)
            return $"{zile} zile";
        if (zile < 365)
            return $"{zile / 30} luni";
        return $"{zile / 365} ani";
    }
    #endregion
}
