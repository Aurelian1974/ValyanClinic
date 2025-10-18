using MediatR;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using ValyanClinic.Application.Features.DepartamentManagement.Queries.GetDepartamentById;

namespace ValyanClinic.Components.Pages.Administrare.Departamente.Modals;

/// <summary>
/// Code-behind pentru DepartamentViewModal - Modal read-only pentru vizualizare detalii departament
/// </summary>
public partial class DepartamentViewModal : ComponentBase
{
    #region Injected Services
    [Inject] private IMediator Mediator { get; set; } = default!;
    [Inject] private ILogger<DepartamentViewModal> Logger { get; set; } = default!;
    #endregion

    #region Parameters
    /// <summary>
    /// Event callback cand se solicita editare din modal
    /// </summary>
    [Parameter] public EventCallback<Guid> OnEditRequested { get; set; }

    /// <summary>
    /// Event callback cand se solicita stergere din modal
    /// </summary>
    [Parameter] public EventCallback<Guid> OnDeleteRequested { get; set; }

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
    /// Datele departamentului incarcate
    /// </summary>
    private DepartamentDetailDto? DepartamentData { get; set; }

    /// <summary>
    /// ID-ul departamentului curent vizualizat
    /// </summary>
    private Guid CurrentDepartamentId { get; set; }
    #endregion

    #region Public Methods
    /// <summary>
    /// Deschide modalul pentru vizualizare departament
    /// </summary>
    /// <param name="departamentId">ID-ul departamentului de vizualizat</param>
    public async Task Open(Guid departamentId)
    {
        try
        {
            Logger.LogInformation("Deschidere modal vizualizare pentru departamentul {DepartamentId}", departamentId);

            CurrentDepartamentId = departamentId;
            IsVisible = true;
            IsLoading = true;
            HasError = false;
            ErrorMessage = string.Empty;
            DepartamentData = null;

            await InvokeAsync(StateHasChanged);

            await LoadDepartamentData(departamentId);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Eroare la deschiderea modalului pentru departamentul {DepartamentId}", departamentId);
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
        Logger.LogInformation("Inchidere modal vizualizare departament");
        
        IsVisible = false;
        await InvokeAsync(StateHasChanged);

        // Notify parent
        if (OnClosed.HasDelegate)
        {
            await OnClosed.InvokeAsync();
        }

        // Reset state after animation
        await Task.Delay(300);
        DepartamentData = null;
        IsLoading = false;
        HasError = false;
        ErrorMessage = string.Empty;
        CurrentDepartamentId = Guid.Empty;
    }
    #endregion

    #region Private Methods
    /// <summary>
    /// Incarca datele departamentului din backend
    /// </summary>
    private async Task LoadDepartamentData(Guid departamentId)
    {
        try
        {
            Logger.LogInformation("Incarca datele departamentului {DepartamentId} pentru vizualizare", departamentId);

            var query = new GetDepartamentByIdQuery(departamentId);
            var result = await Mediator.Send(query);

            if (result.IsSuccess && result.Value != null)
            {
                DepartamentData = result.Value;
                HasError = false;
                Logger.LogInformation("Date departament incarcate cu succes pentru {DepartamentId}", departamentId);
            }
            else
            {
                HasError = true;
                ErrorMessage = result.Errors?.FirstOrDefault() ?? "Nu s-au putut incarca datele departamentului";
                Logger.LogWarning("Eroare la incarcarea datelor departamentului {DepartamentId}: {Error}", 
                    departamentId, ErrorMessage);
            }
        }
        catch (Exception ex)
        {
            HasError = true;
            ErrorMessage = $"Eroare la incarcarea datelor: {ex.Message}";
            Logger.LogError(ex, "Exceptie la incarcarea datelor departamentului {DepartamentId}", departamentId);
        }
        finally
        {
            IsLoading = false;
            await InvokeAsync(StateHasChanged);
        }
    }

    /// <summary>
    /// Handler pentru click pe overlay (inchide modalul)
    /// </summary>
    private async Task HandleOverlayClick()
    {
        await Close();
    }

    /// <summary>
    /// Handler pentru butonul de editare
    /// </summary>
    private async Task HandleEdit()
    {
        if (CurrentDepartamentId != Guid.Empty)
        {
            Logger.LogInformation("Solicitare editare pentru departamentul {DepartamentId}", CurrentDepartamentId);
            
            if (OnEditRequested.HasDelegate)
            {
                await OnEditRequested.InvokeAsync(CurrentDepartamentId);
            }
        }
        else
        {
            Logger.LogWarning("HandleEdit: Nu se poate edita - CurrentDepartamentId este Guid.Empty");
        }
    }

    /// <summary>
    /// Handler pentru butonul de stergere
    /// </summary>
    private async Task HandleDelete()
    {
        if (CurrentDepartamentId == Guid.Empty)
        {
            Logger.LogWarning("HandleDelete: Nu se poate sterge - CurrentDepartamentId este Guid.Empty");
            return;
        }

        Logger.LogInformation("Solicitare stergere pentru departamentul {DepartamentId}", CurrentDepartamentId);
        
        if (OnDeleteRequested.HasDelegate)
        {
            await OnDeleteRequested.InvokeAsync(CurrentDepartamentId);
        }
    }
    #endregion
}
