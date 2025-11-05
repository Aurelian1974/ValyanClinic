using MediatR;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using ValyanClinic.Application.Features.DepartamentManagement.Commands.CreateDepartament;
using ValyanClinic.Application.Features.DepartamentManagement.Commands.UpdateDepartament;
using ValyanClinic.Application.Features.DepartamentManagement.Queries.GetDepartamentById;
using ValyanClinic.Components.Pages.Administrare.Departamente.Models;
using ValyanClinic.Domain.Interfaces.Repositories;

namespace ValyanClinic.Components.Pages.Administrare.Departamente.Modals;

public partial class DepartamentFormModal : ComponentBase
{
    [Inject] private IMediator Mediator { get; set; } = default!;
    [Inject] private ILogger<DepartamentFormModal> Logger { get; set; } = default!;
    [Inject] private ITipDepartamentRepository TipDepartamentRepository { get; set; } = default!;

    [Parameter] public EventCallback OnDepartamentSaved { get; set; }
    [Parameter] public EventCallback OnClosed { get; set; }

    private bool IsVisible { get; set; }
    private bool IsLoading { get; set; }
    private bool IsSaving { get; set; }
    private bool HasError { get; set; }
    private string ErrorMessage { get; set; } = string.Empty;
    private DepartamentFormModel Model { get; set; } = new();

    // Dropdown data
    private List<TipDepartamentOption> TipDepartamentOptions { get; set; } = new();

    public class TipDepartamentOption
    {
        public Guid IdTipDepartament { get; set; }
        public string DenumireTipDepartament { get; set; } = string.Empty;
    }

    protected override async Task OnInitializedAsync()
    {
        Model = new DepartamentFormModel();
        Logger.LogInformation("DepartamentFormModal initialized");
        
        await LoadTipDepartamente();
    }

    private async Task LoadTipDepartamente()
    {
        try
        {
            Logger.LogInformation("Loading tip departamente for dropdown");
            var tipuri = await TipDepartamentRepository.GetAllAsync();
            
            TipDepartamentOptions = tipuri
                .Select(t => new TipDepartamentOption 
                { 
                    IdTipDepartament = t.IdTipDepartament, 
                    DenumireTipDepartament = t.DenumireTipDepartament 
                })
                .OrderBy(t => t.DenumireTipDepartament)
                .ToList();
            
            Logger.LogInformation("Loaded {Count} tip departamente", TipDepartamentOptions.Count);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error loading tip departamente");
            TipDepartamentOptions = new List<TipDepartamentOption>();
        }
    }

    public async Task OpenForAdd()
    {
        try
        {
            Logger.LogInformation("Opening modal for ADD");
            
            Model = new DepartamentFormModel();
            
            IsVisible = true;
            IsLoading = false;
            HasError = false;
            ErrorMessage = string.Empty;
            
            await InvokeAsync(StateHasChanged);
            
            Logger.LogInformation("Modal opened for ADD - TipDepartamente available: {Count}", TipDepartamentOptions.Count);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error opening modal for ADD");
            HasError = true;
            ErrorMessage = $"Eroare la deschiderea formularului: {ex.Message}";
        }
    }

    public async Task OpenForEdit(Guid departamentId)
    {
        try
        {
            Logger.LogInformation("Opening modal for EDIT: {Id}", departamentId);
            
            if (departamentId == Guid.Empty)
            {
                Logger.LogError("DepartamentId is Guid.Empty! Cannot load data");
                HasError = true;
                ErrorMessage = "ID invalid pentru editare";
                IsVisible = true;
                IsLoading = false;
                await InvokeAsync(StateHasChanged);
                return;
            }
            
            IsVisible = true;
            IsLoading = true;
            HasError = false;
            ErrorMessage = string.Empty;
            
            await InvokeAsync(StateHasChanged);
            
            Logger.LogInformation("Sending GetDepartamentByIdQuery for {Id}", departamentId);
            
            var query = new GetDepartamentByIdQuery(departamentId);
            var result = await Mediator.Send(query);
            
            if (result.IsSuccess && result.Value != null)
            {
                var data = result.Value;
                
                Logger.LogInformation("Data loaded from backend: Id={Id}", data.IdDepartament);
                
                Model = new DepartamentFormModel
                {
                    IdDepartament = data.IdDepartament,
                    IdTipDepartament = data.IdTipDepartament,
                    DenumireDepartament = data.DenumireDepartament,
                    DescriereDepartament = data.DescriereDepartament,
                    DenumireTipDepartament = data.DenumireTipDepartament
                };
                
                Logger.LogInformation("Model created: Id={Id}, IsEditMode={IsEditMode}", 
                    Model.IdDepartament, Model.IsEditMode);
            }
            else
            {
                HasError = true;
                ErrorMessage = result.Errors?.FirstOrDefault() ?? "Nu s-au putut incarca datele departamentului";
                Logger.LogWarning("Failed to load data for EDIT: {Error}", ErrorMessage);
            }
            
            IsLoading = false;
            await InvokeAsync(StateHasChanged);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error in OpenForEdit");
            HasError = true;
            ErrorMessage = $"Eroare la incarcarea datelor: {ex.Message}";
            IsLoading = false;
            await InvokeAsync(StateHasChanged);
        }
    }

    public async Task Close()
    {
        Logger.LogInformation("Closing modal");
        IsVisible = false;
        await InvokeAsync(StateHasChanged);

        if (OnClosed.HasDelegate)
        {
            await OnClosed.InvokeAsync();
        }

        await Task.Delay(300);
        
        Model = new DepartamentFormModel();
        IsLoading = false;
        HasError = false;
        ErrorMessage = string.Empty;
    }

    private async Task HandleOverlayClick()
    {
        // ❌ DEZACTIVAT: Nu închide modalul la click pe overlay
        // Modalele de tip form conțin date importante care nu trebuie pierdute
      // await Close();
      
        // 📝 OPȚIONAL: Poți adăuga un warning visual sau sunet
  // Pentru moment, nu facem nimic - modalul rămâne deschis
        return;
    }

    private async Task HandleSubmit()
    {
        try
        {
            Logger.LogInformation("HandleSubmit: IsEditMode={IsEditMode}, Id={Id}", 
                Model.IsEditMode, Model.IdDepartament);
            
            IsSaving = true;
            HasError = false;
            ErrorMessage = string.Empty;
            await InvokeAsync(StateHasChanged);

            if (Model.IsEditMode)
            {
                if (!Model.IdDepartament.HasValue || Model.IdDepartament.Value == Guid.Empty)
                {
                    Logger.LogError("IsEditMode=true but IdDepartament is invalid!");
                    HasError = true;
                    ErrorMessage = "ID invalid pentru editare";
                    IsSaving = false;
                    await InvokeAsync(StateHasChanged);
                    return;
                }
                
                Logger.LogInformation("Updating departament with ID: {Id}", Model.IdDepartament.Value);
                
                var command = new UpdateDepartamentCommand
                {
                    IdDepartament = Model.IdDepartament.Value,
                    IdTipDepartament = Model.IdTipDepartament,
                    DenumireDepartament = Model.DenumireDepartament,
                    DescriereDepartament = Model.DescriereDepartament
                };

                var result = await Mediator.Send(command);
                
                if (result.IsSuccess)
                {
                    Logger.LogInformation("Departament updated successfully: {Id}", Model.IdDepartament);
                    
                    if (OnDepartamentSaved.HasDelegate)
                    {
                        await OnDepartamentSaved.InvokeAsync();
                    }
                    
                    await Close();
                }
                else
                {
                    HasError = true;
                    ErrorMessage = string.Join(", ", result.Errors ?? new List<string> { "Eroare la actualizare" });
                    Logger.LogWarning("Failed to update departament: {Error}", ErrorMessage);
                }
            }
            else
            {
                Logger.LogInformation("Creating new departament");
                
                var command = new CreateDepartamentCommand
                {
                    IdTipDepartament = Model.IdTipDepartament,
                    DenumireDepartament = Model.DenumireDepartament,
                    DescriereDepartament = Model.DescriereDepartament
                };

                var result = await Mediator.Send(command);
                
                if (result.IsSuccess)
                {
                    Logger.LogInformation("Departament created successfully: {Id}", result.Value);
                    
                    if (OnDepartamentSaved.HasDelegate)
                    {
                        await OnDepartamentSaved.InvokeAsync();
                    }
                    
                    await Close();
                }
                else
                {
                    HasError = true;
                    ErrorMessage = string.Join(", ", result.Errors ?? new List<string> { "Eroare la creare" });
                    Logger.LogWarning("Failed to create departament: {Error}", ErrorMessage);
                }
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error in HandleSubmit");
            HasError = true;
            ErrorMessage = $"Eroare la salvare: {ex.Message}";
        }
        finally
        {
            IsSaving = false;
            await InvokeAsync(StateHasChanged);
        }
    }
}
