using Microsoft.AspNetCore.Components;
using ValyanClinic.Application.ViewModels;

namespace ValyanClinic.Components.Shared.Consultatie.Modals;

/// <summary>
/// Modal pentru adăugare analiză medicală recomandată
/// </summary>
public partial class AddAnalizaModal : ComponentBase
{
    [Parameter] public bool IsVisible { get; set; }
    [Parameter] public EventCallback OnClose { get; set; }
    [Parameter] public EventCallback<(string NumeAnaliza, string Prioritate, bool EsteCito, DateTime? DataProgramata, string IndicatiiMedic)> OnSave { get; set; }

    private List<AnalizaMedicalaListDto> _nomenclatorAnalize = new();
    private string _selectedAnalizaName = string.Empty;
    private string _selectedPrioritate = "Normala";
    private bool _esteCito;
    private DateTime? _dataProgramata;
    private string _indicatiiMedic = string.Empty;
    private bool _isSaving;
    private string? _errorMessage;

    protected override async Task OnParametersSetAsync()
    {
        if (IsVisible && !_nomenclatorAnalize.Any())
        {
            // TODO: Load from SearchAnalizeMedicaleQuery
            // Placeholder pentru demonstrație
            _nomenclatorAnalize = new List<AnalizaMedicalaListDto>
            {
                new() { NumeAnaliza = "Hemoleucogramă completă", NumeCategorie = "HEMATOLOGIE", NumeLaborator = "Synevo", Pret = 25.00m, Moneda = "RON" },
                new() { NumeAnaliza = "Glicemie (Glucoză)", NumeCategorie = "BIOCHIMIE", NumeLaborator = "Bioclinica", Pret = 10.00m, Moneda = "RON" },
                new() { NumeAnaliza = "Colesterol total", NumeCategorie = "BIOCHIMIE", NumeLaborator = "Synevo", Pret = 15.00m, Moneda = "RON" }
            };
        }
        await Task.CompletedTask;
    }

    private void OnAnalizaSelected(ChangeEventArgs e)
    {
        _selectedAnalizaName = e.Value?.ToString() ?? string.Empty;
    }

    private async Task HandleSave()
    {
        _isSaving = true;
        _errorMessage = null;

        try
        {
            await OnSave.InvokeAsync((
                _selectedAnalizaName,
                _selectedPrioritate,
                _esteCito,
                _dataProgramata,
                _indicatiiMedic
            ));

            // Reset form
            _selectedAnalizaName = string.Empty;
            _selectedPrioritate = "Normala";
            _esteCito = false;
            _dataProgramata = null;
            _indicatiiMedic = string.Empty;
        }
        catch (Exception ex)
        {
            _errorMessage = $"Eroare la salvare: {ex.Message}";
        }
        finally
        {
            _isSaving = false;
        }
    }

    private void HandleOverlayClick()
    {
        if (!_isSaving)
        {
            _ = OnClose.InvokeAsync();
        }
    }

    private void HandleClose()
    {
        if (!_isSaving)
        {
            _ = OnClose.InvokeAsync();
        }
    }
}
