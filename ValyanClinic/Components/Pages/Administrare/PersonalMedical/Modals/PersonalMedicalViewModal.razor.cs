using MediatR;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using ValyanClinic.Application.Features.PersonalMedicalManagement.Queries.GetPersonalMedicalById;

namespace ValyanClinic.Components.Pages.Administrare.PersonalMedical.Modals;

public partial class PersonalMedicalViewModal : ComponentBase
{
    [Inject] private IMediator Mediator { get; set; } = default!;
    [Inject] private ILogger<PersonalMedicalViewModal> Logger { get; set; } = default!;

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

       await InvokeAsync(StateHasChanged);
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
 }

    /// <summary>
    /// Reîncarcă datele pentru ID-ul curent (folosit după editări)
    /// </summary>
    public async Task RefreshData()
    {
        if (CurrentPersonalID != Guid.Empty && IsVisible)
 {
            Logger.LogInformation("Refreshing data for current PersonalID: {PersonalID}", CurrentPersonalID);
     await LoadPersonalMedicalData(CurrentPersonalID);
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

    private void SetActiveTab(string tabName)
    {
 ActiveTab = tabName;
     Logger.LogDebug("Tab changed to: {TabName}", tabName);
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
