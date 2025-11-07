using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using MediatR;
using ValyanClinic.Application.Features.Settings.Queries.GetSystemSettings;
using Syncfusion.Blazor.Grids;
using ValyanClinic.Components.Pages.Administrare.SetariGenerale.Modals;

namespace ValyanClinic.Components.Pages.Administrare.SetariGenerale;

public partial class SetariAutentificare : ComponentBase, IDisposable
{
    [Inject] private IMediator Mediator { get; set; } = default!;
    [Inject] private ILogger<SetariAutentificare> Logger { get; set; } = default!;

    private SfGrid<SystemSettingDto>? GridRef;
    private SettingEditModal? EditModalRef;
    
    private List<SystemSettingDto> Settings = new();
    private bool IsLoading = true;
    private bool _disposed = false;

    protected override async Task OnInitializedAsync()
    {
    try
    {
            Logger.LogInformation("Loading authentication settings...");
       await LoadSettingsAsync();
   }
        catch (Exception ex)
        {
       Logger.LogError(ex, "Error loading settings");
    }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task LoadSettingsAsync()
    {
 try
   {
          var autentificareResult = await Mediator.Send(new GetSystemSettingsQuery("Autentificare"));
         var securitateResult = await Mediator.Send(new GetSystemSettingsQuery("Securitate"));

            if (autentificareResult.IsSuccess && securitateResult.IsSuccess)
 {
          Settings = autentificareResult.Value!
           .Concat(securitateResult.Value!)
   .OrderBy(s => s.Categorie)
  .ThenBy(s => s.Cheie)
     .ToList();

                Logger.LogInformation("Loaded {Count} settings", Settings.Count);
      }
       else
            {
    Logger.LogWarning("Failed to load settings");
          }
        }
        catch (Exception ex)
        {
     Logger.LogError(ex, "Error loading settings from mediator");
        throw;
        }
    }

    private void OpenEditDialog(SystemSettingDto setting)
    {
        Logger.LogInformation("Opening edit dialog for setting: {Cheie}", setting.Cheie);
  
   if (EditModalRef != null)
     {
          EditModalRef.Open(setting);
        }
        else
  {
     Logger.LogWarning("EditModalRef is null");
        }
    }

    private async Task HandleSettingSaved()
    {
   Logger.LogInformation("Setting saved - reloading data");
        
   await LoadSettingsAsync();
        StateHasChanged();
    }

    public void Dispose()
{
        if (_disposed) return;
        _disposed = true;
        
        Settings?.Clear();
        GridRef = null;
        EditModalRef = null;
    }
}
