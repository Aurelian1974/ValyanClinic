using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Syncfusion.Blazor.Grids;
using Syncfusion.Blazor.Notifications;
using Syncfusion.Blazor.DropDowns;
using Syncfusion.Blazor.Inputs;
using MediatR;
using ValyanClinic.Application.Features.PersonalManagement.Queries.GetPersonalList;
using ValyanClinic.Services.DataGrid;
using Microsoft.Extensions.Logging;

namespace ValyanClinic.Components.Pages.Administrare.Personal;

public partial class AdministrarePersonal : ComponentBase, IDisposable
{
    [Inject] private IMediator Mediator { get; set; } = default!;
    [Inject] private ILogger<AdministrarePersonal> Logger { get; set; } = default!;
    [Inject] private NavigationManager NavigationManager { get; set; } = default!;
    [Inject] private IFilterOptionsService FilterOptionsService { get; set; } = default!;

    // Grid reference
    private SfGrid<PersonalListDto>? GridRef;

    // Toast reference
    private SfToast? ToastRef;

    // Data for Syncfusion Grid (all data loaded once)
    private List<PersonalListDto> AllPersonalData { get; set; } = new();
    private List<PersonalListDto> OriginalData { get; set; } = new();
    private int TotalRecords { get; set; } = 0;
    
    // Page settings for Syncfusion Grid
    private int DefaultPageSize { get; set; } = 20;
    private int[] PageSizeArray = { 10, 20, 50, 100 };
    
    private PersonalListDto? SelectedPersonal { get; set; }

    // State
    private bool IsLoading { get; set; } = true;
    private bool HasError { get; set; } = false;
    private string? ErrorMessage { get; set; }

    // Advanced Filter State
    private bool IsAdvancedFilterExpanded { get; set; } = false;
    private string GlobalSearchText { get; set; } = string.Empty;
    private string? FilterStatus { get; set; }
    private string? FilterDepartament { get; set; }
    private string? FilterFunctie { get; set; }
    private string? FilterJudet { get; set; }
    
    // Debounce timer for global search
    private CancellationTokenSource? _searchDebounceTokenSource;
    private const int SearchDebounceMs = 500;

    // Filter Options - loaded from server
    private List<FilterOption> StatusOptions { get; set; } = new();
    private List<FilterOption> DepartamentOptions { get; set; } = new();
    private List<FilterOption> FunctieOptions { get; set; } = new();
    private List<FilterOption> JudetOptions { get; set; } = new();

    // Toast properties
    private string ToastTitle { get; set; } = string.Empty;
    private string ToastContent { get; set; } = string.Empty;
    private string ToastCssClass { get; set; } = string.Empty;

    private int ActiveFiltersCount => 
        (string.IsNullOrEmpty(FilterStatus) ? 0 : 1) +
        (string.IsNullOrEmpty(FilterDepartament) ? 0 : 1) +
        (string.IsNullOrEmpty(FilterFunctie) ? 0 : 1) +
        (string.IsNullOrEmpty(FilterJudet) ? 0 : 1) +
        (string.IsNullOrEmpty(GlobalSearchText) ? 0 : 1);

    protected override async Task OnInitializedAsync()
    {
        try
        {
            Logger.LogInformation("Initializare pagina Administrare Personal cu Syncfusion Paging");
            await LoadAllData();
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Eroare la initializarea componentei");
            HasError = true;
            ErrorMessage = $"Eroare la initializare: {ex.Message}";
            IsLoading = false;
            StateHasChanged();
        }
    }

    public void Dispose()
    {
        _searchDebounceTokenSource?.Cancel();
        _searchDebounceTokenSource?.Dispose();
    }

    private async Task LoadAllData()
    {
        try
        {
            IsLoading = true;
            HasError = false;
            ErrorMessage = null;

            Logger.LogInformation("Incarcare toate datele personal pentru grid cu paging nativ");

            // Load all data at once (without pagination for client-side operations)
            var query = new GetPersonalListQuery
            {
                PageNumber = 1,
                PageSize = int.MaxValue, // Get all records
                GlobalSearchText = null, // Don't apply server-side search initially
                FilterStatus = null,
                FilterDepartament = null,
                FilterFunctie = null,
                FilterJudet = null,
                SortColumn = "Nume", // Default sort
                SortDirection = "ASC"
            };

            var result = await Mediator.Send(query);

            if (result.IsSuccess && result.Value != null)
            {
                OriginalData = result.Value.ToList();
                AllPersonalData = new List<PersonalListDto>(OriginalData);
                TotalRecords = OriginalData.Count;
                
                Logger.LogInformation("Toate datele incarcate cu succes: {Count} angajati", TotalRecords);
                
                // Load filter options
                if (OriginalData.Any())
                {
                    await LoadFilterOptions();
                }

                // Apply client-side filtering (initially no filters, so shows all data)
                ApplyClientSideFiltering();
            }
            else
            {
                HasError = true;
                ErrorMessage = string.Join(", ", result.Errors ?? new List<string> { "Eroare necunoscuta" });
                Logger.LogWarning("Eroare la incarcarea datelor: {Message}", ErrorMessage);
                AllPersonalData = new List<PersonalListDto>();
                OriginalData = new List<PersonalListDto>();
                TotalRecords = 0;
            }
        }
        catch (Exception ex)
        {
            HasError = true;
            ErrorMessage = $"Eroare neasteptata: {ex.Message}";
            Logger.LogError(ex, "Eroare la incarcarea datelor personal");
            AllPersonalData = new List<PersonalListDto>();
            OriginalData = new List<PersonalListDto>();
            TotalRecords = 0;
        }
        finally
        {
            IsLoading = false;
            StateHasChanged();
        }
    }

    private void ApplyClientSideFiltering()
    {
        try
        {
            var filteredData = OriginalData.AsEnumerable();
            
            // Apply global search
            if (!string.IsNullOrEmpty(GlobalSearchText))
            {
                var searchLower = GlobalSearchText.ToLowerInvariant();
                filteredData = filteredData.Where(p => 
                    (p.Nume?.ToLowerInvariant().Contains(searchLower) ?? false) ||
                    (p.Prenume?.ToLowerInvariant().Contains(searchLower) ?? false) ||
                    (p.CNP?.ToLowerInvariant().Contains(searchLower) ?? false) ||
                    (p.Email_Personal?.ToLowerInvariant().Contains(searchLower) ?? false) ||
                    (p.Telefon_Personal?.ToLowerInvariant().Contains(searchLower) ?? false) ||
                    (p.Functia?.ToLowerInvariant().Contains(searchLower) ?? false) ||
                    (p.Departament?.ToLowerInvariant().Contains(searchLower) ?? false)
                );
            }

            // Apply specific filters
            if (!string.IsNullOrEmpty(FilterStatus))
            {
                filteredData = filteredData.Where(p => p.Status_Angajat == FilterStatus);
            }

            if (!string.IsNullOrEmpty(FilterDepartament))
            {
                filteredData = filteredData.Where(p => p.Departament == FilterDepartament);
            }

            if (!string.IsNullOrEmpty(FilterFunctie))
            {
                filteredData = filteredData.Where(p => p.Functia == FilterFunctie);
            }

            if (!string.IsNullOrEmpty(FilterJudet))
            {
                filteredData = filteredData.Where(p => p.Judet_Domiciliu == FilterJudet);
            }

            // Update the grid data source
            AllPersonalData = filteredData.ToList();
            TotalRecords = AllPersonalData.Count;
            
            Logger.LogInformation("Client-side filtering applied. Rezultate: {Count} din {Total}", 
                AllPersonalData.Count, OriginalData.Count);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Eroare la aplicarea filtrelor client-side");
            // În caz de eroare, păstrează datele originale
            AllPersonalData = new List<PersonalListDto>(OriginalData);
            TotalRecords = OriginalData.Count;
        }
    }

    private async Task LoadFilterOptions()
    {
        try
        {
            Logger.LogInformation("Generare optiuni de filtrare din datele incarcate...");
            
            if (OriginalData.Any())
            {
                StatusOptions = FilterOptionsService.GenerateOptions(OriginalData, p => p.Status_Angajat);
                DepartamentOptions = FilterOptionsService.GenerateOptions(OriginalData, p => p.Departament);
                FunctieOptions = FilterOptionsService.GenerateOptions(OriginalData, p => p.Functia);
                JudetOptions = FilterOptionsService.GenerateOptions(OriginalData, p => p.Judet_Domiciliu);
                
                Logger.LogInformation("Optiuni filtrare generate: Status={StatusCount}, Dept={DeptCount}, Func={FuncCount}, Judet={JudetCount}",
                    StatusOptions.Count, DepartamentOptions.Count, FunctieOptions.Count, JudetOptions.Count);
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Eroare la generarea optiunilor de filtrare");
            // În caz de eroare, inițializează liste goale
            StatusOptions = new List<FilterOption>();
            DepartamentOptions = new List<FilterOption>();
            FunctieOptions = new List<FilterOption>();
            JudetOptions = new List<FilterOption>();
        }
        
        await Task.CompletedTask;
    }

    private void ToggleAdvancedFilter()
    {
        IsAdvancedFilterExpanded = !IsAdvancedFilterExpanded;
        Logger.LogInformation("Advanced filter panel toggled: {State}", 
            IsAdvancedFilterExpanded ? "Expanded" : "Collapsed");
    }

    private void OnSearchInput(ChangeEventArgs e)
    {
        var newValue = e.Value?.ToString() ?? string.Empty;
        
        // Previne bucla infinita - nu face nimic daca valoarea nu s-a schimbat
        if (newValue == GlobalSearchText)
            return;
        
        GlobalSearchText = newValue;
        
        Logger.LogInformation("Search input changed: '{SearchText}'", GlobalSearchText);
        
        // Anulează task-ul anterior de debounce
        _searchDebounceTokenSource?.Cancel();
        _searchDebounceTokenSource?.Dispose();
        _searchDebounceTokenSource = new CancellationTokenSource();

        var localToken = _searchDebounceTokenSource.Token;

        // Executa search dupa debounce
        _ = Task.Run(async () =>
        {
            try
            {
                await Task.Delay(SearchDebounceMs, localToken);
                
                if (!localToken.IsCancellationRequested)
                {
                    Logger.LogInformation("Executing client-side search for: '{SearchText}'", GlobalSearchText);
                    
                    await InvokeAsync(() =>
                    {
                        ApplyClientSideFiltering();
                        StateHasChanged();
                    });
                }
            }
            catch (TaskCanceledException)
            {
                Logger.LogDebug("Search cancelled - user still typing");
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Eroare la executia search-ului");
            }
        }, localToken);
    }

    private async Task OnSearchKeyDown(KeyboardEventArgs e)
    {
        // Daca apasa Enter, executa imediat cautarea fara debounce
        if (e.Key == "Enter")
        {
            _searchDebounceTokenSource?.Cancel();
            
            Logger.LogInformation("Enter pressed - executing immediate search: '{SearchText}'", GlobalSearchText);
            
            ApplyClientSideFiltering();
            StateHasChanged();
            await Task.CompletedTask;
        }
    }

    private async Task ClearSearch()
    {
        Logger.LogInformation("Clearing search text");
        
        // Anulează orice search in curs
        _searchDebounceTokenSource?.Cancel();
        
        GlobalSearchText = string.Empty;
        ApplyClientSideFiltering();
        StateHasChanged();
        await Task.CompletedTask;
    }

    private async Task ApplyFilters()
    {
        Logger.LogInformation("Applying client-side filters: GlobalSearch={Search}, Status={Status}, Dept={Dept}",
            GlobalSearchText, FilterStatus, FilterDepartament);

        ApplyClientSideFiltering();
        StateHasChanged();
        await Task.CompletedTask;
    }

    private async Task ClearAllFilters()
    {
        Logger.LogInformation("Clearing all filters");
        
        GlobalSearchText = string.Empty;
        FilterStatus = null;
        FilterDepartament = null;
        FilterFunctie = null;
        FilterJudet = null;

        ApplyClientSideFiltering();
        StateHasChanged();
        await Task.CompletedTask;
    }

    private async Task ClearFilter(string filterName)
    {
        Logger.LogInformation("Clearing filter: {FilterName}", filterName);

        switch (filterName)
        {
            case nameof(FilterStatus):
                FilterStatus = null;
                break;
            case nameof(FilterDepartament):
                FilterDepartament = null;
                break;
            case nameof(FilterFunctie):
                FilterFunctie = null;
                break;
            case nameof(FilterJudet):
                FilterJudet = null;
                break;
            case nameof(GlobalSearchText):
                GlobalSearchText = string.Empty;
                break;
        }

        await ApplyFilters();
    }

    private async Task HandleRefresh()
    {
        Logger.LogInformation("Refresh date personal");
        await LoadAllData();
        await ShowToast("Succes", "Datele au fost reincarcate cu succes", "e-toast-success");
    }

    private void HandleAddNew()
    {
        Logger.LogInformation("Navigare catre adaugare personal");
        NavigationManager.NavigateTo("/administrare/personal/adauga");
    }

    #region Grid Selection Events

    private void OnRowSelected(RowSelectEventArgs<PersonalListDto> args)
    {
        SelectedPersonal = args.Data;
        Logger.LogInformation("Personal selectat: {PersonalId} - {NumeComplet}", 
            SelectedPersonal?.Id_Personal, SelectedPersonal?.NumeComplet);
        StateHasChanged();
    }

    private void OnRowDeselected(RowDeselectEventArgs<PersonalListDto> args)
    {
        SelectedPersonal = null;
        Logger.LogInformation("Selectie anulata");
        StateHasChanged();
    }

    #endregion

    #region Toolbar Action Methods

    private async Task HandleViewSelected()
    {
        if (SelectedPersonal == null) return;
        
        Logger.LogInformation("Vizualizare personal: {PersonalId}", SelectedPersonal.Id_Personal);
        NavigationManager.NavigateTo($"/administrare/personal/vizualizeaza/{SelectedPersonal.Id_Personal}");
        await Task.CompletedTask;
    }

    private async Task HandleEditSelected()
    {
        if (SelectedPersonal == null) return;
        
        Logger.LogInformation("Editare personal: {PersonalId}", SelectedPersonal.Id_Personal);
        NavigationManager.NavigateTo($"/administrare/personal/editeaza/{SelectedPersonal.Id_Personal}");
        await Task.CompletedTask;
    }

    private async Task HandleDeleteSelected()
    {
        if (SelectedPersonal == null) return;
        
        Logger.LogInformation("Stergere personal: {PersonalId}", SelectedPersonal.Id_Personal);
        
        await ShowToast("Atentie", 
            $"Functionalitatea de stergere pentru {SelectedPersonal.NumeComplet} va fi implementata", 
            "e-toast-warning");
    }

    #endregion

    private async Task OnCommandClicked(CommandClickEventArgs<PersonalListDto> args)
    {
        var personal = args.RowData;
        var commandName = args.CommandColumn?.ButtonOption?.Content ?? "";

        Logger.LogInformation("Comanda executata: {Command} pentru {PersonalId}", 
            commandName, personal.Id_Personal);

        switch (commandName)
        {
            case "Vizualizeaza":
                await HandleView(personal);
                break;
            case "Editeaza":
                await HandleEdit(personal);
                break;
            case "Sterge":
                await HandleDelete(personal);
                break;
        }
    }

    private async Task HandleView(PersonalListDto personal)
    {
        Logger.LogInformation("Vizualizare personal: {PersonalId}", personal.Id_Personal);
        NavigationManager.NavigateTo($"/administrare/personal/vizualizeaza/{personal.Id_Personal}");
        await Task.CompletedTask;
    }

    private async Task HandleEdit(PersonalListDto personal)
    {
        Logger.LogInformation("Editare personal: {PersonalId}", personal.Id_Personal);
        NavigationManager.NavigateTo($"/administrare/personal/editeaza/{personal.Id_Personal}");
        await Task.CompletedTask;
    }

    private async Task HandleDelete(PersonalListDto personal)
    {
        Logger.LogInformation("Stergere personal: {PersonalId}", personal.Id_Personal);
        
        await ShowToast("Atentie", 
            $"Functionalitatea de stergere pentru {personal.NumeComplet} va fi implementata", 
            "e-toast-warning");
    }

    private async Task ShowToast(string title, string content, string cssClass)
    {
        ToastTitle = title;
        ToastContent = content;
        ToastCssClass = cssClass;

        if (ToastRef != null)
        {
            await ToastRef.ShowAsync();
        }
    }
}
