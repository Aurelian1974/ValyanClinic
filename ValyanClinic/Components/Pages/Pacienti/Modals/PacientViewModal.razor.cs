using MediatR;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using ValyanClinic.Application.Features.PacientManagement.Queries.GetPacientById;

namespace ValyanClinic.Components.Pages.Pacienti.Modals;

/// <summary>
/// Code-behind pentru PacientViewModal - Modal read-only pentru vizualizare detalii pacient
/// </summary>
public partial class PacientViewModal : ComponentBase
{
    #region Injected Services
    [Inject] private IMediator Mediator { get; set; } = default!;
    [Inject] private ILogger<PacientViewModal> Logger { get; set; } = default!;
    #endregion

    #region Parameters
    /// <summary>
    /// Event callback cand modalul se inchide
    /// </summary>
    [Parameter] public EventCallback OnClosed { get; set; }
    #endregion

    #region State Properties
    /// <summary>
    /// Vizibilitatea modalului
    /// </summary>
    private bool IsVisible { get; set; }

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
    /// Tab-ul activ din modal
    /// </summary>
    private string ActiveTab { get; set; } = "personal";

    /// <summary>
    /// ID-ul pacientului curent vizualizat
    /// </summary>
    private Guid CurrentPacientId { get; set; }
    #endregion

    #region Public Methods
    /// <summary>
    /// Deschide modalul pentru vizualizare pacient
    /// </summary>
    /// <param name="pacientId">ID-ul pacientului de vizualizat</param>
    public async Task Open(Guid pacientId)
    {
        try
        {
            CurrentPacientId = pacientId;
            IsVisible = true;
            IsLoading = true;
            HasError = false;
            ErrorMessage = string.Empty;
            PacientData = null;
            ActiveTab = "personal";

            await InvokeAsync(StateHasChanged);

            await LoadPacientData(pacientId);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Eroare la deschiderea modalului pentru pacientul {PacientId}", pacientId);
            HasError = true;
            ErrorMessage = $"Eroare la deschiderea modalului: {ex.Message}";
            IsLoading = false;
            await InvokeAsync(StateHasChanged);
        }
    }

    /// <summary>
    /// Inchide modalul
    /// </summary>
    public async Task Close()
    {
        IsVisible = false;
        await InvokeAsync(StateHasChanged);

        // Notify parent
        if (OnClosed.HasDelegate)
        {
            await OnClosed.InvokeAsync();
        }

        // Reset state after animation
        await Task.Delay(300);
        PacientData = null;
        IsLoading = false;
        HasError = false;
        ErrorMessage = string.Empty;
        CurrentPacientId = Guid.Empty;
    }
    #endregion

    #region Private Methods
    /// <summary>
    /// Incarca datele pacientului din backend
    /// </summary>
    private async Task LoadPacientData(Guid pacientId)
    {
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
    #endregion
}
