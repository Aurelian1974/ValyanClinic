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
    /// Tab-ul activ din modal
    /// </summary>
    private string ActiveTab { get; set; } = "personal";
    #endregion

    #region Lifecycle Methods
    protected override async Task OnParametersSetAsync()
    {
        if (IsVisible && PacientId.HasValue && PacientId.Value != Guid.Empty)
        {
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
        IsLoading = false;
        HasError = false;
        ErrorMessage = string.Empty;
        ActiveTab = "personal";
    }
    #endregion
}
