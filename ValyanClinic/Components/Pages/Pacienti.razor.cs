using Microsoft.AspNetCore.Components;
using ValyanClinic.Components.Pages.Models;
using ValyanClinic.Core.Common;
using ValyanClinic.Components.Shared;

namespace ValyanClinic.Components.Pages;

public partial class Pacienti : ComponentBase, IDisposable
{
    [Inject] private NavigationManager Navigation { get; set; } = default!;
    [Inject] private ILogger<Pacienti> Logger { get; set; } = default!;
    
    // State Management
    private PatientFilterState FilterState { get; set; } = new();
    private List<PatientListModel> AllPatients { get; set; } = new();
    private bool IsLoading { get; set; } = true;
    private string? ErrorMessage { get; set; }
    private CancellationTokenSource? _searchCancellationTokenSource;

    // Properties for UI Binding
    private string SearchTerm 
    { 
        get => FilterState.SearchTerm; 
        set 
        { 
            FilterState.SearchTerm = value;
            _ = ApplyFiltersWithDebounceAsync();
        } 
    }

    private string SelectedStatus 
    { 
        get => FilterState.SelectedStatus; 
        set 
        { 
            FilterState.SelectedStatus = value;
            _ = LoadPatientsAsync();
        } 
    }

    protected override async Task OnInitializedAsync()
    {
        await LoadPatientsAsync();
    }

    private async Task LoadPatientsAsync()
    {
        IsLoading = true;
        ErrorMessage = null;

        try
        {
            var result = await SimulatePatientLoadAsync();
            
            if (result.IsSuccess)
            {
                AllPatients = result.Value ?? new List<PatientListModel>();
                Logger.LogInformation("Loaded {Count} patients successfully", AllPatients.Count);
            }
            else
            {
                ErrorMessage = string.Join(", ", result.Errors);
                Logger.LogWarning("Failed to load patients: {Errors}", string.Join(", ", result.Errors));
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Eroare la incarcarea pacientilor: {ex.Message}";
            Logger.LogError(ex, "Exception occurred while loading patients");
        }
        finally
        {
            IsLoading = false;
            StateHasChanged();
        }
    }

    private async Task ApplyFiltersWithDebounceAsync()
    {
        // Cancel previous search
        _searchCancellationTokenSource?.Cancel();
        _searchCancellationTokenSource = new CancellationTokenSource();

        try
        {
            // Debounce search input
            await Task.Delay(300, _searchCancellationTokenSource.Token);
            await LoadPatientsAsync();
        }
        catch (OperationCanceledException)
        {
            // Search was cancelled due to new input
        }
    }

    private IEnumerable<PatientListModel> GetFilteredPatients()
    {
        var filtered = AllPatients.AsEnumerable();
        
        if (!string.IsNullOrEmpty(FilterState.SearchTerm))
        {
            var searchLower = FilterState.SearchTerm.ToLowerInvariant();
            filtered = filtered.Where(p => 
                p.FullName.ToLowerInvariant().Contains(searchLower) ||
                p.Email.ToLowerInvariant().Contains(searchLower) ||
                p.Phone.Contains(FilterState.SearchTerm) ||
                p.CNP.Contains(FilterState.SearchTerm));
        }

        if (!string.IsNullOrEmpty(FilterState.SelectedStatus))
        {
            if (Enum.TryParse<PatientStatus>(FilterState.SelectedStatus, out var status))
            {
                filtered = filtered.Where(p => p.Status == status);
            }
        }
        
        return filtered.OrderBy(p => p.FullName);
    }

    private void ResetFilters()
    {
        FilterState = new PatientFilterState();
        _ = LoadPatientsAsync();
    }

    private async Task AddNewPatientAsync()
    {
        Logger.LogInformation("Navigating to add new patient page");
        Navigation.NavigateTo("/pacienti/add");
    }

    private async Task ViewPatientAsync(int patientId)
    {
        Logger.LogInformation("Navigating to view patient {PatientId}", patientId);
        Navigation.NavigateTo($"/pacienti/{patientId}");
    }

    private async Task EditPatientAsync(int patientId)
    {
        Logger.LogInformation("Navigating to edit patient {PatientId}", patientId);
        Navigation.NavigateTo($"/pacienti/{patientId}/edit");
    }

    private string GetStatusDisplayName(PatientStatus status)
    {
        return status switch
        {
            PatientStatus.Active => "Activ",
            PatientStatus.Inactive => "Inactiv",
            PatientStatus.Suspended => "Suspendat",
            _ => "Necunoscut"
        };
    }

    private string GetStatusCssClass(PatientStatus status)
    {
        return status switch
        {
            PatientStatus.Active => "status-active",
            PatientStatus.Inactive => "status-inactive",
            PatientStatus.Suspended => "status-suspended",
            _ => "status-unknown"
        };
    }

    // Helper methods for PageHeader
    private string GetPatientCountSubtitle()
    {
        var totalCount = AllPatients.Count;
        var activeCount = AllPatients.Count(p => p.Status == PatientStatus.Active);
        return $"{totalCount} pacienti inregistrati, {activeCount} activi";
    }

    private List<PageHeader.BreadcrumbItem> GetBreadcrumbs()
    {
        return new List<PageHeader.BreadcrumbItem>
        {
            new("Dashboard", "/"),
            new("Administrare", "#"),
            new("Pacienti", "/pacienti")
        };
    }

    // Temporary simulation method until service is properly implemented
    private async Task<Result<List<PatientListModel>>> SimulatePatientLoadAsync()
    {
        await Task.Delay(500);

        var patients = new List<PatientListModel>
        {
            new() { 
                Id = 1, 
                FullName = "Maria Popescu", 
                Phone = "0721-123-456", 
                Email = "maria.popescu@email.com", 
                LastVisit = DateTime.Now.AddDays(-5),
                Status = Models.PatientStatus.Active,
                Age = 34,
                CNP = "2850615123456"
            },
            new() { 
                Id = 2, 
                FullName = "Ion Ionescu", 
                Phone = "0722-654-321", 
                Email = "ion.ionescu@email.com", 
                LastVisit = DateTime.Now.AddDays(-12),
                Status = Models.PatientStatus.Active,
                Age = 45,
                CNP = "1780312654321"
            },
            new() { 
                Id = 3, 
                FullName = "Ana Georgescu", 
                Phone = "0723-789-012", 
                Email = "ana.georgescu@email.com", 
                LastVisit = DateTime.Now.AddDays(-3),
                Status = Models.PatientStatus.Inactive,
                Age = 28,
                CNP = "2950825789012"
            },
            new() { 
                Id = 4, 
                FullName = "Mihai Marinescu", 
                Phone = "0724-345-678", 
                Email = "mihai.marinescu@email.com", 
                LastVisit = DateTime.Now.AddDays(-8),
                Status = Models.PatientStatus.Active,
                Age = 52,
                CNP = "1720504345678"
            },
            new() { 
                Id = 5, 
                FullName = "Elena Ionescu", 
                Phone = "0725-987-654", 
                Email = "elena.ionescu@email.com", 
                LastVisit = DateTime.Now.AddDays(-25),
                Status = Models.PatientStatus.Active,
                Age = 29,
                CNP = "2940312987654"
            },
            new() { 
                Id = 6, 
                FullName = "Gheorghe Marin", 
                Phone = "0726-456-789", 
                Email = "gheorghe.marin@email.com", 
                LastVisit = DateTime.Now.AddDays(-45),
                Status = Models.PatientStatus.Suspended,
                Age = 67,
                CNP = "1560504456789"
            }
        };

        return Result<List<PatientListModel>>.Success(patients, "Pacienti incarcati cu succes");
    }

    public void Dispose()
    {
        _searchCancellationTokenSource?.Cancel();
        _searchCancellationTokenSource?.Dispose();
    }
}