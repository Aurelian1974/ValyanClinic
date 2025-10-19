using MediatR;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Syncfusion.Blazor.Grids;
using ValyanClinic.Application.Features.PacientManagement.Queries.GetPacientList;
using ValyanClinic.Application.Features.PacientManagement.Commands.DeletePacient;

namespace ValyanClinic.Components.Pages.Pacienti;

public partial class AdministrarePacienti : ComponentBase, IDisposable
{
    [Inject] private IMediator Mediator { get; set; } = default!;
    [Inject] private IJSRuntime JSRuntime { get; set; } = default!;

    // Syncfusion Grid Reference
    private SfGrid<PacientListDto>? GridRef { get; set; }

    // State Management
    private bool IsLoading { get; set; }
    private bool HasError { get; set; }
    private string? ErrorMessage { get; set; }
    
    // Data
    private List<PacientListDto>? AllPacienti { get; set; }
    private List<PacientListDto> FilteredPacienti => ApplyClientFilters();

    // Filters
    private string SearchText { get; set; } = string.Empty;
    private string FilterActiv { get; set; } = string.Empty;
    private string FilterAsigurat { get; set; } = string.Empty;
    private string FilterJudet { get; set; } = string.Empty;
    private List<string> JudeteList { get; set; } = new();

    // Computed Properties
    private bool HasActiveFilters => 
        !string.IsNullOrEmpty(SearchText) || 
        !string.IsNullOrEmpty(FilterActiv) || 
        !string.IsNullOrEmpty(FilterAsigurat) || 
        !string.IsNullOrEmpty(FilterJudet);

    // Timer pentru debounce search
    private System.Timers.Timer? _searchDebounceTimer;

    // Modal States
    private bool ShowAddEditModal { get; set; }
    private bool ShowViewModal { get; set; }
    private bool ShowHistoryModal { get; set; }
    private bool ShowDocumentsModal { get; set; }
    private bool ShowDeleteModal { get; set; }
    private Guid? SelectedPacientId { get; set; }
    private string DeleteConfirmMessage { get; set; } = string.Empty;

    protected override async Task OnInitializedAsync()
    {
        await LoadDataAsync();
        await LoadJudeteAsync();
    }

    private async Task LoadDataAsync()
    {
        IsLoading = true;
        HasError = false;
        ErrorMessage = null;

        try
        {
            var query = new GetPacientListQuery
            {
                PageNumber = 1,
                PageSize = 10000, // Load all for client-side filtering
                SortColumn = "Nume",
                SortDirection = "ASC"
            };

            var result = await Mediator.Send(query);

            if (result.IsSuccess && result.Value != null)
            {
                AllPacienti = result.Value.Value?.ToList() ?? new List<PacientListDto>();
            }
            else
            {
                HasError = true;
                ErrorMessage = result.FirstError ?? "Eroare la încărcarea datelor.";
                AllPacienti = new List<PacientListDto>();
            }
        }
        catch (Exception ex)
        {
            HasError = true;
            ErrorMessage = $"Eroare neașteptată: {ex.Message}";
            AllPacienti = new List<PacientListDto>();
        }
        finally
        {
            IsLoading = false;
        }
    }

    private List<PacientListDto> ApplyClientFilters()
    {
        if (AllPacienti == null)
            return new List<PacientListDto>();

        var filtered = AllPacienti.AsEnumerable();

        // Search filter
        if (!string.IsNullOrWhiteSpace(SearchText))
        {
            var search = SearchText.ToLower();
            filtered = filtered.Where(p =>
                (p.NumeComplet?.ToLower().Contains(search) ?? false) ||
                (p.CNP?.ToLower().Contains(search) ?? false) ||
                (p.Telefon?.ToLower().Contains(search) ?? false) ||
                (p.Email?.ToLower().Contains(search) ?? false) ||
                (p.Cod_Pacient?.ToLower().Contains(search) ?? false)
            );
        }

        // Activ filter
        if (!string.IsNullOrEmpty(FilterActiv))
        {
            var isActiv = bool.Parse(FilterActiv);
            filtered = filtered.Where(p => p.Activ == isActiv);
        }

        // Asigurat filter
        if (!string.IsNullOrEmpty(FilterAsigurat))
        {
            var isAsigurat = bool.Parse(FilterAsigurat);
            filtered = filtered.Where(p => p.Asigurat == isAsigurat);
        }

        // Judet filter
        if (!string.IsNullOrEmpty(FilterJudet))
        {
            filtered = filtered.Where(p => p.Judet == FilterJudet);
        }

        return filtered.ToList();
    }

    private Task LoadJudeteAsync()
    {
        try
        {
            JudeteList = new List<string>
            {
                "Bucuresti", "Alba", "Arad", "Arges", "Bacau", "Bihor", "Bistrita-Nasaud",
                "Botosani", "Brasov", "Braila", "Buzau", "Caras-Severin", "Calarasi",
                "Cluj", "Constanta", "Covasna", "Dambovita", "Dolj", "Galati", "Giurgiu",
                "Gorj", "Harghita", "Hunedoara", "Ialomita", "Iasi", "Ilfov", "Maramures",
                "Mehedinti", "Mures", "Neamt", "Olt", "Prahova", "Satu Mare", "Salaj",
                "Sibiu", "Suceava", "Teleorman", "Timis", "Tulcea", "Vaslui", "Valcea", "Vrancea"
            };
        }
        catch
        {
            JudeteList = new List<string>();
        }
        
        return Task.CompletedTask;
    }

    #region Filter & Search Methods

    private void HandleSearchKeyUp()
    {
        _searchDebounceTimer?.Stop();
        _searchDebounceTimer?.Dispose();

        _searchDebounceTimer = new System.Timers.Timer(300);
        _searchDebounceTimer.Elapsed += async (sender, e) =>
        {
            _searchDebounceTimer?.Dispose();
            await InvokeAsync(() =>
            {
                StateHasChanged();
            });
        };
        _searchDebounceTimer.AutoReset = false;
        _searchDebounceTimer.Start();
    }

    private void ClearSearch()
    {
        SearchText = string.Empty;
        StateHasChanged();
    }

    private void ApplyFilters()
    {
        StateHasChanged();
    }

    private void ClearAllFilters()
    {
        SearchText = string.Empty;
        FilterActiv = string.Empty;
        FilterAsigurat = string.Empty;
        FilterJudet = string.Empty;
        StateHasChanged();
    }

    #endregion

    #region Modal Methods

    private void OpenAddModal()
    {
        SelectedPacientId = null;
        ShowAddEditModal = true;
    }

    private void OpenViewModal(Guid pacientId)
    {
        SelectedPacientId = pacientId;
        ShowViewModal = true;
    }

    private void OpenEditModal(Guid pacientId)
    {
        SelectedPacientId = pacientId;
        ShowAddEditModal = true;
    }

    private void OpenHistoryModal(Guid pacientId)
    {
        SelectedPacientId = pacientId;
        ShowHistoryModal = true;
    }

    private void OpenDocumentsModal(Guid pacientId)
    {
        SelectedPacientId = pacientId;
        ShowDocumentsModal = true;
    }

    private void ToggleStatusConfirm(PacientListDto pacient)
    {
        SelectedPacientId = pacient.Id;
        var action = pacient.Activ ? "dezactivarea" : "activarea";
        DeleteConfirmMessage = $"Sunteți sigur că doriți {action} pacientului {pacient.NumeComplet}?";
        ShowDeleteModal = true;
    }

    private async Task HandleDeleteConfirmed()
    {
        if (!SelectedPacientId.HasValue)
            return;

        try
        {
            var pacient = AllPacienti?.FirstOrDefault(p => p.Id == SelectedPacientId.Value);
            if (pacient == null)
                return;

            var command = new DeletePacientCommand(
                SelectedPacientId.Value,
                "System",
                hardDelete: false
            );

            var result = await Mediator.Send(command);

            if (result.IsSuccess)
            {
                await JSRuntime.InvokeVoidAsync("alert", result.SuccessMessage ?? "Operațiune efectuată cu succes!");
                await LoadDataAsync();
            }
            else
            {
                await JSRuntime.InvokeVoidAsync("alert", $"Eroare: {result.FirstError}");
            }
        }
        catch (Exception ex)
        {
            await JSRuntime.InvokeVoidAsync("alert", $"Eroare la modificarea statusului: {ex.Message}");
        }
        finally
        {
            ShowDeleteModal = false;
            SelectedPacientId = null;
        }
    }

    private async Task HandleModalSaved()
    {
        await LoadDataAsync();
    }

    #endregion

    public void Dispose()
    {
        _searchDebounceTimer?.Dispose();
    }
}
