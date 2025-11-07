using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using MediatR;
using ValyanClinic.Application.Features.Settings.Queries.GetSystemSettings;
using ValyanClinic.Application.Features.Settings.Commands.UpdateSystemSetting;

namespace ValyanClinic.Components.Pages.Administrare.SetariGenerale.Modals;

public partial class SettingEditModal : ComponentBase
{
    [Inject] private IMediator Mediator { get; set; } = default!;
    [Inject] private ILogger<SettingEditModal> Logger { get; set; } = default!;

    [Parameter] public EventCallback OnSettingSaved { get; set; }
    
    // State
    public bool IsVisible { get; private set; }
    private bool IsLoading { get; set; }
    private bool IsSaving { get; set; }
    private bool HasError { get; set; }
    private string ErrorMessage { get; set; } = string.Empty;

    // Data
    private SystemSettingDto? CurrentSetting { get; set; }
    private string NewValue { get; set; } = string.Empty;
    private string EditableDescriere { get; set; } = string.Empty;
  
    // Boolean dropdown with labels
    private List<BooleanOptionModel> BooleanOptionsWithLabels { get; set; } = new()
{
        new BooleanOptionModel { Value = "true", Label = "True (Activat)" },
      new BooleanOptionModel { Value = "false", Label = "False (Dezactivat)" }
    };

    /// <summary>
    /// Opens the modal for editing a setting
    /// </summary>
public void Open(SystemSettingDto setting)
    {
   Logger.LogInformation("Opening modal for setting: {Categorie}.{Cheie}", 
            setting.Categorie, setting.Cheie);
   
        // ✅ LOG pentru debugging
        Logger.LogInformation("TipDate: '{TipDate}' | IsBool: {IsBool} | IsInt: {IsInt}", 
  setting.TipDate, 
            IsBooleanType(setting.TipDate),
 IsIntegerType(setting.TipDate));
     
        CurrentSetting = setting;
      NewValue = setting.Valoare;
        EditableDescriere = setting.Descriere ?? string.Empty;
        IsVisible = true;
        HasError = false;
        ErrorMessage = string.Empty;
        
        StateHasChanged();
    }

    /// <summary>
    /// Closes the modal
    /// </summary>
    public void Close()
 {
   Logger.LogInformation("Closing modal");
        
        IsVisible = false;
      CurrentSetting = null;
      NewValue = string.Empty;
        EditableDescriere = string.Empty;
        HasError = false;
        ErrorMessage = string.Empty;
        
   StateHasChanged();
    }

    /// <summary>
    /// Handles click outside modal (close on overlay click)
    /// </summary>
    private void HandleOverlayClick()
    {
 Close();
    }

    /// <summary>
    /// Saves the setting value AND description
 /// </summary>
    private async Task SaveSetting()
    {
        if (CurrentSetting == null || string.IsNullOrWhiteSpace(NewValue))
        {
            HasError = true;
     ErrorMessage = "Valoarea nu poate fi goala";
            return;
      }

        try
    {
            IsSaving = true;
            StateHasChanged();

Logger.LogInformation("Updating setting: {Categorie}.{Cheie} to '{Value}' with description '{Desc}'", 
     CurrentSetting.Categorie, CurrentSetting.Cheie, NewValue, EditableDescriere);

      var command = new UpdateSystemSettingCommand(
     CurrentSetting.Categorie,
          CurrentSetting.Cheie,
                NewValue,
    EditableDescriere, // ✅ TRIMITEM DESCRIEREA
"admin"); // TODO: Get from auth context

            var result = await Mediator.Send(command);

            if (result.IsSuccess)
  {
Logger.LogInformation("Setting updated successfully");
        
       // Notify parent component
          await OnSettingSaved.InvokeAsync();
        
 Close();
       }
      else
        {
       HasError = true;
          ErrorMessage = result.ErrorsAsString ?? "Eroare la salvarea setarii";
                Logger.LogWarning("Failed to update setting: {Errors}", ErrorMessage);
            }
        }
        catch (Exception ex)
        {
   HasError = true;
            ErrorMessage = $"Eroare: {ex.Message}";
  Logger.LogError(ex, "Error saving setting");
  }
        finally
        {
      IsSaving = false;
 StateHasChanged();
        }
    }
}

/// <summary>
/// Model for Boolean dropdown options with labels
/// </summary>
public class BooleanOptionModel
{
    public string Value { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
}
