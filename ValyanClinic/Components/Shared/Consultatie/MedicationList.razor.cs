using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using ValyanClinic.Application.Features.ConsultatieManagement.DTOs;
using ValyanClinic.Application.Services.Medicamente;

namespace ValyanClinic.Components.Shared.Consultatie;

/// <summary>
/// Componentă pentru gestionarea listei de medicamente prescrise
/// ✅ NEW: With ANM nomenclator autocomplete support
/// </summary>
public partial class MedicationList : ComponentBase
{
    [Inject] private ILogger<MedicationList> Logger { get; set; } = default!;
    [Inject] private INomenclatorMedicamenteService MedicamenteService { get; set; } = default!;

    // ==================== PARAMETERS ====================
    
    [Parameter] public List<MedicationRowDto> Medications { get; set; } = new();
    
    [Parameter] public EventCallback<List<MedicationRowDto>> MedicationsChanged { get; set; }
    
    [Parameter] public EventCallback OnChanged { get; set; }
    
    [Parameter] public bool ShowValidation { get; set; } = false;

    // ==================== STATE ====================
    
    private MedicationRowDto NewMedication { get; set; } = new();
    
    private List<MedicamentNomenclator> MedicationSearchResults { get; set; } = new();
    
    private bool ShowAutocomplete { get; set; } = false;
    
    private string _lastSearchTerm = string.Empty;
    
    private System.Threading.CancellationTokenSource? _searchCts;

    // ==================== EDITING STATE ====================
    
    private Guid? EditingMedicationId { get; set; }
    
    private MedicationRowDto EditingMedication { get; set; } = new();

    // ==================== COMPUTED ====================
    
    private string ValidationCss => ShowValidation && string.IsNullOrWhiteSpace(NewMedication.Name) 
        ? "is-invalid" 
        : string.Empty;

    // ==================== EVENT HANDLERS ====================

    /// <summary>
    /// Handler pentru input în căutarea medicamentului
    /// </summary>
    private async Task OnMedicationSearchInput(ChangeEventArgs e)
    {
        var searchTerm = e.Value?.ToString() ?? string.Empty;
        NewMedication.Name = searchTerm; // Update binding
        
        // Anulăm căutarea anterioară
        _searchCts?.Cancel();
        _searchCts = new System.Threading.CancellationTokenSource();

        if (string.IsNullOrWhiteSpace(searchTerm) || searchTerm.Length < 2)
        {
            ShowAutocomplete = false;
            MedicationSearchResults.Clear();
            return;
        }

        // Debounce: așteptăm 300ms
        try
        {
            await Task.Delay(300, _searchCts.Token);
        }
        catch (TaskCanceledException)
        {
            return; // Căutarea a fost anulată
        }

        // Căutăm în nomenclator ANM
        await SearchMedicationsAsync(searchTerm, _searchCts.Token);
    }

    /// <summary>
    /// Caută medicamente în nomenclatorul ANM
    /// </summary>
    private async Task SearchMedicationsAsync(string searchTerm, CancellationToken cancellationToken)
    {
        try
        {
            var result = await MedicamenteService.SearchAsync(searchTerm, maxResults: 100, cancellationToken);
            
            if (result.IsSuccess && result.Value != null)
            {
                MedicationSearchResults = result.Value.ToList();
                ShowAutocomplete = MedicationSearchResults.Any();
                StateHasChanged();
            }
            else
            {
                Logger.LogWarning("Căutare medicamente eșuată: {Error}", result.FirstError);
                ShowAutocomplete = false;
            }
        }
        catch (OperationCanceledException)
        {
            // Ignore - căutarea a fost anulată
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Eroare la căutarea medicamentelor");
            ShowAutocomplete = false;
        }
    }

    /// <summary>
    /// Selectează un medicament din autocomplete
    /// </summary>
    private void SelectMedication(MedicamentNomenclator selected)
    {
        NewMedication.Name = selected.DenumireComerciala;
        
        // Pre-populăm dozajul dacă avem concentrația
        if (!string.IsNullOrEmpty(selected.Concentratie))
        {
            NewMedication.Dose = selected.Concentratie;
        }
        
        // Adăugăm forma farmaceutică în notes
        if (!string.IsNullOrEmpty(selected.FormaFarmaceutica))
        {
            NewMedication.Notes = selected.FormaFarmaceutica;
        }

        ShowAutocomplete = false;
        MedicationSearchResults.Clear();
        StateHasChanged();
    }

    /// <summary>
    /// Adaugă medicamentul în listă
    /// </summary>
    private async Task AddMedication()
    {
        if (!NewMedication.IsValid)
        {
            Logger.LogWarning("Încercare de adăugare medicament invalid");
            return;
        }

        Medications.Add(NewMedication);
        await MedicationsChanged.InvokeAsync(Medications);
        await OnChanged.InvokeAsync();

        // Reset form
        NewMedication = new MedicationRowDto();
        StateHasChanged();
    }

    /// <summary>
    /// Șterge un medicament din listă
    /// </summary>
    private async Task RemoveMedication(Guid medicationId)
    {
        var medication = Medications.FirstOrDefault(m => m.Id == medicationId);
        if (medication != null)
        {
            Medications.Remove(medication);
            await MedicationsChanged.InvokeAsync(Medications);
            await OnChanged.InvokeAsync();
            StateHasChanged();
        }
    }

    /// <summary>
    /// Începe editarea unui medicament
    /// </summary>
    private void StartEdit(MedicationRowDto medication)
    {
        EditingMedicationId = medication.Id;
        
        // Creăm o copie pentru editare
        EditingMedication = new MedicationRowDto
        {
            Id = medication.Id,
            Name = medication.Name,
            Dose = medication.Dose,
            Frequency = medication.Frequency,
            Duration = medication.Duration,
            Quantity = medication.Quantity,
            Notes = medication.Notes
        };
        
        StateHasChanged();
    }

    /// <summary>
    /// Salvează modificările
    /// </summary>
    private async Task SaveEdit()
    {
        if (!EditingMedication.IsValid)
        {
            Logger.LogWarning("Încercare de salvare medicament invalid");
            return;
        }

        var original = Medications.FirstOrDefault(m => m.Id == EditingMedicationId);
        if (original != null)
        {
            // Actualizăm medicamentul original cu valorile editate
            original.Name = EditingMedication.Name;
            original.Dose = EditingMedication.Dose;
            original.Frequency = EditingMedication.Frequency;
            original.Duration = EditingMedication.Duration;
            original.Quantity = EditingMedication.Quantity;
            original.Notes = EditingMedication.Notes;

            await MedicationsChanged.InvokeAsync(Medications);
            await OnChanged.InvokeAsync();
        }

        // Reset edit state
        EditingMedicationId = null;
        EditingMedication = new MedicationRowDto();
        StateHasChanged();
    }

    /// <summary>
    /// Anulează editarea
    /// </summary>
    private void CancelEdit()
    {
        EditingMedicationId = null;
        EditingMedication = new MedicationRowDto();
        StateHasChanged();
    }

    /// <summary>
    /// Resetează formularul
    /// </summary>
    private void ClearForm()
    {
        NewMedication = new MedicationRowDto();
        ShowAutocomplete = false;
        MedicationSearchResults.Clear();
        StateHasChanged();
    }

    // ==================== LIFECYCLE ====================

    public void Dispose()
    {
        _searchCts?.Cancel();
        _searchCts?.Dispose();
    }
}
