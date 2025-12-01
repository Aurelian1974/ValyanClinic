using MediatR;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using ValyanClinic.Application.Features.PersonalManagement.Queries.GetPersonalById;

namespace ValyanClinic.Components.Pages.Administrare.Personal.Modals;

/// <summary>
/// Code-behind pentru PersonalViewModal - Modal read-only pentru vizualizare detalii personal
/// </summary>
public partial class PersonalViewModal : ComponentBase
{
    #region Injected Services
    [Inject] private IMediator Mediator { get; set; } = default!;
    [Inject] private ILogger<PersonalViewModal> Logger { get; set; } = default!;
    [Inject] private ValyanClinic.Application.Services.IPersonalBusinessService PersonalBusinessService { get; set; } = default!;
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
    /// Datele personalului incarcate
    /// </summary>
    private PersonalDetailDto? PersonalData { get; set; }

    /// <summary>
    /// Tab-ul activ din modal
    /// </summary>
    private string ActiveTab { get; set; } = "personal";

    /// <summary>
    /// ID-ul personalului curent vizualizat
    /// </summary>
    private Guid CurrentPersonalId { get; set; }
    #endregion

    #region Public Methods
    /// <summary>
    /// Deschide modalul pentru vizualizare personal
    /// </summary>
    /// <param name="personalId">ID-ul personalului de vizualizat</param>
    public async Task Open(Guid personalId)
    {
        try
        {
            CurrentPersonalId = personalId;
            IsVisible = true;
            IsLoading = true;
            HasError = false;
            ErrorMessage = string.Empty;
            PersonalData = null;
            ActiveTab = "personal";

            await InvokeAsync(StateHasChanged);

            await LoadPersonalData(personalId);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Eroare la deschiderea modalului pentru personalul {PersonalId}", personalId);
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
        PersonalData = null;
        IsLoading = false;
        HasError = false;
        ErrorMessage = string.Empty;
        CurrentPersonalId = Guid.Empty;
    }
    #endregion

    #region Private Methods
    /// <summary>
    /// Incarca datele personalului din backend
    /// </summary>
    private async Task LoadPersonalData(Guid personalId)
    {
        try
        {
            Logger.LogInformation("Incarca datele personalului {PersonalId} pentru vizualizare", personalId);

            var query = new GetPersonalByIdQuery(personalId);
            var result = await Mediator.Send(query);

            if (result.IsSuccess && result.Value != null)
            {
                PersonalData = result.Value;
                HasError = false;
                Logger.LogInformation("Date personal incarcate cu succes pentru {PersonalId}", personalId);
            }
            else
            {
                HasError = true;
                ErrorMessage = result.Errors?.FirstOrDefault() ?? "Nu s-au putut incarca datele personalului";
                Logger.LogWarning("Eroare la incarcarea datelor personalului {PersonalId}: {Error}",
                    personalId, ErrorMessage);
            }
        }
        catch (Exception ex)
        {
            HasError = true;
            ErrorMessage = $"Eroare la incarcarea datelor: {ex.Message}";
            Logger.LogError(ex, "Exceptie la incarcarea datelor personalului {PersonalId}", personalId);
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
    /// Handler pentru butonul de editare
    /// </summary>
    private async Task HandleEdit()
    {
        if (CurrentPersonalId != Guid.Empty)
        {
            Logger.LogInformation("=== Solicitare editare pentru ID: \"{Id}\" ===", CurrentPersonalId);

            await OnEditRequested.InvokeAsync(CurrentPersonalId); // Trimite CurrentPersonalId care este corect
            Logger.LogInformation("=== Edit invocat cu succes pentru ID: \"{Id}\" ===", CurrentPersonalId);
        }
        else
        {
            Logger.LogWarning("=== HandleEdit: Nu se poate edita - CurrentPersonalId este Guid.Empty ===");
        }
    }

    /// <summary>
    /// Handler pentru butonul de stergere
    /// </summary>
    private async Task HandleDelete()
    {
        if (CurrentPersonalId == Guid.Empty)
        {
            Logger.LogWarning("=== HandleDelete: Nu se poate sterge - CurrentPersonalId este Guid.Empty ===");
            return;
        }

        Logger.LogInformation("Solicitare stergere pentru personalul \"{Id}\"", CurrentPersonalId);
        await OnDeleteRequested.InvokeAsync(CurrentPersonalId);
    }

    /// <summary>
    /// Calculeaza varsta detaliata (ani, luni, zile) din data nasterii
    /// </summary>
    private string CalculeazaVarstaDetaliata(DateTime dataNasterii)
    {
        return PersonalBusinessService.CalculeazaVarsta(dataNasterii);
    }

    /// <summary>
    /// Calculeaza varsta detaliata (ani, luni, zile) din CNP
    /// </summary>
    private string CalculeazaVarstaDetaliataFromCNP(string cnp)
    {
        return PersonalBusinessService.CalculeazaVarstaFromCNP(cnp);
    }

    #region Expira In - Calcule si Stilizare

    /// <summary>
    /// Calculeaza timpul ramas pana la expirare (ani, luni, zile)
    /// </summary>
    private string CalculeazaExpiraIn(DateTime? validabilPana)
    {
        return PersonalBusinessService.CalculeazaExpiraIn(validabilPana);
    }

    /// <summary>
    /// Determina clasa CSS pentru badge-ul Expira in functie de timpul ramas
    /// </summary>
    private string GetExpiraInCssClass(DateTime? validabilPana)
    {
        return PersonalBusinessService.GetExpiraCssClass(validabilPana);
    }

    #endregion
    #endregion
}
