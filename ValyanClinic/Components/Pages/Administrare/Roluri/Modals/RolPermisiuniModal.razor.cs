using MediatR;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using ValyanClinic.Application.Features.RolManagement.Commands.SetRolPermisiuni;
using ValyanClinic.Application.Features.RolManagement.Queries.GetPermisiuniDefinitii;
using ValyanClinic.Application.Features.RolManagement.Queries.GetRolById;

namespace ValyanClinic.Components.Pages.Administrare.Roluri.Modals;

public partial class RolPermisiuniModal : ComponentBase
{
    [Inject] private IMediator Mediator { get; set; } = default!;
    [Inject] private ILogger<RolPermisiuniModal> Logger { get; set; } = default!;

    [Parameter] public EventCallback OnPermisiuniSaved { get; set; }
    [Parameter] public EventCallback OnClosed { get; set; }

    private bool IsVisible { get; set; }
    private bool IsLoading { get; set; }
    private bool IsSaving { get; set; }
    private bool HasError { get; set; }
    private string ErrorMessage { get; set; } = string.Empty;
    
    private Guid CurrentRolId { get; set; }
    private string? RolDenumire { get; set; }
    private List<CategoriePermisiuniDto> CategoriiPermisiuni { get; set; } = new();
    private HashSet<string> SelectedPermisiuni { get; set; } = new();
    private HashSet<string> ExpandedCategories { get; set; } = new();
    private int TotalPermisiuni { get; set; }

    public async Task Open(Guid rolId)
    {
        try
        {
            Logger.LogInformation("Opening Permisiuni modal for Rol: {Id}", rolId);

            CurrentRolId = rolId;
            IsVisible = true;
            IsLoading = true;
            HasError = false;
            ErrorMessage = string.Empty;
            SelectedPermisiuni.Clear();
            ExpandedCategories.Clear();
            CategoriiPermisiuni.Clear();

            await InvokeAsync(StateHasChanged);
            
            // Load rol info and permisiuni in parallel
            var rolTask = Mediator.Send(new GetRolByIdQuery(rolId));
            var permisiuniTask = Mediator.Send(new GetPermisiuniDefinitiiQuery());

            await Task.WhenAll(rolTask, permisiuniTask);

            var rolResult = await rolTask;
            var permisiuniResult = await permisiuniTask;

            if (rolResult.IsSuccess && rolResult.Value != null)
            {
                RolDenumire = rolResult.Value.Denumire;
                SelectedPermisiuni = new HashSet<string>(rolResult.Value.Permisiuni);
                Logger.LogInformation("Loaded rol data with {Count} permissions", rolResult.Value.Permisiuni.Count);
            }
            else
            {
                HasError = true;
                ErrorMessage = rolResult.Errors?.FirstOrDefault() ?? "Nu s-au putut incarca datele rolului";
                Logger.LogWarning("Failed to load rol data: {Error}", ErrorMessage);
            }

            if (permisiuniResult.IsSuccess && permisiuniResult.Value != null)
            {
                CategoriiPermisiuni = permisiuniResult.Value;
                TotalPermisiuni = CategoriiPermisiuni.Sum(c => c.Permisiuni.Count);
                
                // Expand all categories by default
                foreach (var cat in CategoriiPermisiuni)
                {
                    ExpandedCategories.Add(cat.Categorie);
                }
                
                Logger.LogInformation("Loaded {Count} permission categories", CategoriiPermisiuni.Count);
            }
            else
            {
                HasError = true;
                ErrorMessage = permisiuniResult.Errors?.FirstOrDefault() ?? "Nu s-au putut incarca permisiunile";
                Logger.LogWarning("Failed to load permissions: {Error}", ErrorMessage);
            }

            IsLoading = false;
            await InvokeAsync(StateHasChanged);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error opening Permisiuni modal for {Id}", rolId);
            HasError = true;
            ErrorMessage = $"Eroare la deschiderea modalului: {ex.Message}";
            IsLoading = false;
            await InvokeAsync(StateHasChanged);
        }
    }

    public async Task Close()
    {
        if (IsSaving) return;

        Logger.LogInformation("Closing Permisiuni modal");
        IsVisible = false;
        await InvokeAsync(StateHasChanged);

        if (OnClosed.HasDelegate)
        {
            await OnClosed.InvokeAsync();
        }

        await Task.Delay(300);

        RolDenumire = null;
        CategoriiPermisiuni.Clear();
        SelectedPermisiuni.Clear();
        ExpandedCategories.Clear();
        IsLoading = false;
        HasError = false;
        ErrorMessage = string.Empty;
        CurrentRolId = Guid.Empty;
    }

    private Task HandleOverlayClick()
    {
        // Nu închide la click pe overlay pentru modale cu modificări
        return Task.CompletedTask;
    }

    private void ToggleCategory(string categorie)
    {
        if (ExpandedCategories.Contains(categorie))
        {
            ExpandedCategories.Remove(categorie);
        }
        else
        {
            ExpandedCategories.Add(categorie);
        }
    }

    private void TogglePermission(string cod)
    {
        if (SelectedPermisiuni.Contains(cod))
        {
            SelectedPermisiuni.Remove(cod);
        }
        else
        {
            SelectedPermisiuni.Add(cod);
        }
        StateHasChanged();
    }

    private void SelectAll()
    {
        foreach (var categorie in CategoriiPermisiuni)
        {
            foreach (var permisiune in categorie.Permisiuni)
            {
                SelectedPermisiuni.Add(permisiune.Cod);
            }
        }
        StateHasChanged();
    }

    private void DeselectAll()
    {
        SelectedPermisiuni.Clear();
        StateHasChanged();
    }

    private void SelectAllInCategory(string categorie)
    {
        var cat = CategoriiPermisiuni.FirstOrDefault(c => c.Categorie == categorie);
        if (cat != null)
        {
            foreach (var permisiune in cat.Permisiuni)
            {
                SelectedPermisiuni.Add(permisiune.Cod);
            }
        }
        StateHasChanged();
    }

    private void DeselectAllInCategory(string categorie)
    {
        var cat = CategoriiPermisiuni.FirstOrDefault(c => c.Categorie == categorie);
        if (cat != null)
        {
            foreach (var permisiune in cat.Permisiuni)
            {
                SelectedPermisiuni.Remove(permisiune.Cod);
            }
        }
        StateHasChanged();
    }

    private int GetSelectedCountForCategory(string categorie)
    {
        var cat = CategoriiPermisiuni.FirstOrDefault(c => c.Categorie == categorie);
        if (cat == null) return 0;
        return cat.Permisiuni.Count(p => SelectedPermisiuni.Contains(p.Cod));
    }

    private string GetCategoryIcon(string categorie)
    {
        return categorie.ToLower() switch
        {
            "pacienti" => "fa-users",
            "consultatii" => "fa-stethoscope",
            "programari" => "fa-calendar-alt",
            "personal" => "fa-user-md",
            "rapoarte" => "fa-chart-bar",
            "administrare" => "fa-cogs",
            "sistem" => "fa-server",
            _ => "fa-folder"
        };
    }

    private async Task HandleSave()
    {
        try
        {
            Logger.LogInformation("Saving permissions for Rol: {Id}, Count: {Count}", CurrentRolId, SelectedPermisiuni.Count);

            IsSaving = true;
            HasError = false;
            ErrorMessage = string.Empty;
            await InvokeAsync(StateHasChanged);

            var command = new SetRolPermisiuniCommand
            {
                RolId = CurrentRolId,
                Permisiuni = SelectedPermisiuni.ToList()
            };

            var result = await Mediator.Send(command);

            if (result.IsSuccess)
            {
                Logger.LogInformation("Permissions saved successfully");
                await Close();
                await OnPermisiuniSaved.InvokeAsync();
            }
            else
            {
                HasError = true;
                ErrorMessage = string.Join(", ", result.Errors ?? new List<string> { "Eroare la salvare" });
                Logger.LogWarning("Save failed: {Error}", ErrorMessage);
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Exception during save");
            HasError = true;
            ErrorMessage = $"Eroare: {ex.Message}";
        }
        finally
        {
            IsSaving = false;
            await InvokeAsync(StateHasChanged);
        }
    }
}
