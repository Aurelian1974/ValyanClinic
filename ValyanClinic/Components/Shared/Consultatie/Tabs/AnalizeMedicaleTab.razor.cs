using Microsoft.AspNetCore.Components;
using MediatR;
using Microsoft.Extensions.Logging;
using Syncfusion.Blazor.DropDowns;
using ValyanClinic.Application.ViewModels;
using ValyanClinic.Application.Features.AnalizeMedicale.Queries.GetAnalizeRecomandate;
using ValyanClinic.Application.Features.AnalizeMedicale.Queries.GetAnalizeEfectuate;
using ValyanClinic.Application.Features.AnalizeMedicale.Queries.SearchAnalizeMedicale;
using ValyanClinic.Application.Features.AnalizeMedicale.Commands.AddAnalizaRecomandataToConsultatie;
using ValyanClinic.Application.Features.AnalizeMedicale.Commands.DeleteAnalizaRecomandata;
using ValyanClinic.Application.Features.AnalizeMedicale.Commands.DeleteAnalizaEfectuata;
using ValyanClinic.Application.Features.AnalizeMedicale.Commands.ImportAnalizeEfectuate;
using ValyanClinic.Application.Services.Analize;
using ValyanClinic.Application.Features.Analize.Models;

namespace ValyanClinic.Components.Shared.Consultatie.Tabs;

/// <summary>
/// Tab Analize Medicale pentru consultație
/// Include: Analize recomandate + Analize efectuate (cu rezultate)
/// Folosește formular inline expandabil pentru adăugare (fără modal)
/// </summary>
public partial class AnalizeMedicaleTab : ComponentBase, IDisposable
{
    [Inject] private IMediator Mediator { get; set; } = default!;
    [Inject] private ILogger<AnalizeMedicaleTab> Logger { get; set; } = default!;
    [Inject] private IAnalizePdfParserService AnalizePdfParserService { get; set; } = default!;
    
    [Parameter] public Guid? ConsultatieId { get; set; }
    [Parameter] public Guid? CurrentUserId { get; set; }
    [Parameter] public EventCallback OnChanged { get; set; }
    [Parameter] public EventCallback OnSectionCompleted { get; set; }

    // ==================== STATE ====================
    private List<ConsultatieAnalizaRecomandataDto> _toateAnalizele = new();
    private List<ConsultatieAnalizaMedicalaDto> _analizeEfectuate = new();
    private HashSet<Guid> _expandedAnalize = new();

    // Guard pentru a preveni încărcări repetate
    private Guid? _lastLoadedConsultatieId;

    // Inline form state
    private bool _showAddForm;
    private bool _formSubmitted;
    private NewAnalizaFormModel _newAnaliza = new();
    private string? _formErrorMessage;
    
    // Autocomplete state
    private List<AnalizaMedicalaListDto> _analizeSuggestions = new();
    private Guid? _selectedAnalizaId;
    private string _selectedCategorie = string.Empty;
    private AnalizaMedicalaListDto? _selectedAnalizaDto;
    private SfAutoComplete<Guid?, AnalizaMedicalaListDto>? _autoCompleteRef;
    
    // Import PDF Modal
    private ImportAnalizePdfModal? _importPdfModal;
    
    // Loading & Error
    private bool _isLoading;
    private bool _isSaving;
    private string? _errorMessage;

    // Section completion
    private bool IsSectionCompleted => _toateAnalizele.Any() || _analizeEfectuate.Any();

    // Lista de analize recomandate (toate sunt recomandate acum)
    private List<ConsultatieAnalizaRecomandataDto> AnalizeRecomandate => 
        _toateAnalizele.OrderByDescending(a => a.DataRecomandare).ToList();

    // Lista de analize efectuate
    private List<ConsultatieAnalizaMedicalaDto> AnalizeEfectuate => 
        _analizeEfectuate.OrderByDescending(a => a.DataEfectuare ?? a.DataRecomandare).ToList();

    // Grupuri de analize efectuate pentru viewer
    private List<AnalizeMedicaleGroupDto> AnalizeEfectuateGroups => 
        ConvertToViewerGroups(_analizeEfectuate);
    
    /// <summary>
    /// Convertește analizele efectuate în format pentru AnalizeMedicaleViewer
    /// </summary>
    private List<AnalizeMedicaleGroupDto> ConvertToViewerGroups(List<ConsultatieAnalizaMedicalaDto> analize)
    {
        if (analize == null || !analize.Any())
            return new List<AnalizeMedicaleGroupDto>();
        
        // Grupează după laborator și dată
        var grouped = analize
            .GroupBy(a => new { 
                Laborator = a.LocEfectuare ?? "Necunoscut",
                Data = a.DataEfectuare?.Date ?? a.DataRecomandare.Date
            })
            .Select(g => new AnalizeMedicaleGroupDto
            {
                DataDocument = g.Key.Data,
                SursaDocument = g.Key.Laborator,
                NumeDocument = $"Import {g.Key.Data:dd.MM.yyyy}",
                BatchId = Guid.NewGuid(), // Generăm un BatchId pentru grupare
                Analize = g.Select(a => new AnalizaMedicalaDto
                {
                    Id = a.Id,
                    ConsultatieId = a.ConsultatieID,
                    DataDocument = a.DataEfectuare ?? a.DataRecomandare,
                    NumeDocument = $"Import {(a.DataEfectuare ?? a.DataRecomandare):dd.MM.yyyy}",
                    SursaDocument = a.LocEfectuare,
                    Categorie = a.TipAnaliza,
                    NumeAnaliza = a.NumeAnaliza ?? "",
                    Rezultat = a.ValoareRezultat,
                    UnitateMasura = a.UnitatiMasura,
                    IntervalReferinta = FormatInterval(a.ValoareNormalaMin, a.ValoareNormalaMax),
                    InAfaraLimitelor = a.EsteInAfaraLimitelor,
                    DataCrearii = a.DataRecomandare
                }).ToList()
            })
            .OrderByDescending(g => g.DataDocument)
            .ToList();
        
        return grouped;
    }
    
    private string FormatInterval(decimal? min, decimal? max)
    {
        if (!min.HasValue && !max.HasValue) return "";
        if (!min.HasValue) return $"< {max:G}";
        if (!max.HasValue) return $"> {min:G}";
        return $"{min:G} - {max:G}";
    }

    // Maxim 15 rânduri per tabel
    private const int MaxRowsPerTable = 15;

    /// <summary>
    /// Returnează analizele pentru tabelul din stânga (prima jumătate)
    /// </summary>
    private List<ConsultatieAnalizaRecomandataDto> GetAnalizeStanga()
    {
        var analize = AnalizeRecomandate.Take(MaxRowsPerTable * 2).ToList(); // Maxim 30 total
        var count = analize.Count;
        
        // Prima jumătate (rotunjire în sus pentru impar)
        var countStanga = (count + 1) / 2;
        return analize.Take(Math.Min(countStanga, MaxRowsPerTable)).ToList();
    }

    /// <summary>
    /// Returnează analizele pentru tabelul din dreapta (a doua jumătate)
    /// </summary>
    private List<ConsultatieAnalizaRecomandataDto> GetAnalizeDreapta()
    {
        var analize = AnalizeRecomandate.Take(MaxRowsPerTable * 2).ToList(); // Maxim 30 total
        var count = analize.Count;
        
        // Prima jumătate (rotunjire în sus pentru impar)
        var countStanga = (count + 1) / 2;
        
        // A doua jumătate
        return analize.Skip(Math.Min(countStanga, MaxRowsPerTable)).Take(MaxRowsPerTable).ToList();
    }

    // ==================== LIFECYCLE ====================
    protected override async Task OnInitializedAsync()
    {
        Logger.LogInformation("AnalizeMedicaleTab initialized for ConsultatieId: {ConsultatieId}", ConsultatieId);
    }

    protected override async Task OnParametersSetAsync()
    {
        // Guard: încarcă datele doar dacă ConsultatieId s-a schimbat
        if (ConsultatieId.HasValue && ConsultatieId != _lastLoadedConsultatieId)
        {
            _lastLoadedConsultatieId = ConsultatieId;
            await LoadAnalizeAsync();
        }
    }

    // ==================== DATA LOADING ====================
    private async Task LoadAnalizeAsync()
    {
        if (!ConsultatieId.HasValue || _isLoading) return;

        _isLoading = true;
        _errorMessage = null;

        try
        {
            // Încarcă analizele recomandate
            var queryRecomandate = new GetAnalizeRecomandateQuery(ConsultatieId.Value);
            var resultRecomandate = await Mediator.Send(queryRecomandate);
            
            if (resultRecomandate.IsSuccess && resultRecomandate.Value != null)
            {
                _toateAnalizele = resultRecomandate.Value.ToList();
                Logger.LogInformation("Loaded {Count} analize recomandate for consultație {ConsultatieId}", 
                    _toateAnalizele.Count, ConsultatieId.Value);
            }
            else
            {
                _errorMessage = resultRecomandate.FirstError ?? "Eroare la încărcarea analizelor recomandate.";
                Logger.LogWarning("Failed to load analize recomandate: {Error}", _errorMessage);
            }
            
            // Încarcă analizele efectuate (cu rezultate)
            var queryEfectuate = new GetAnalizeEfectuateQuery(ConsultatieId.Value);
            var resultEfectuate = await Mediator.Send(queryEfectuate);
            
            if (resultEfectuate.IsSuccess && resultEfectuate.Value != null)
            {
                _analizeEfectuate = resultEfectuate.Value.ToList();
                Logger.LogInformation("Loaded {Count} analize efectuate for consultație {ConsultatieId}", 
                    _analizeEfectuate.Count, ConsultatieId.Value);
            }
            else
            {
                Logger.LogWarning("Failed to load analize efectuate: {Error}", resultEfectuate.FirstError);
            }

            if (IsSectionCompleted)
            {
                await OnSectionCompleted.InvokeAsync();
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Exception loading analize medicale");
            _errorMessage = $"Eroare: {ex.Message}";
        }
        finally
        {
            _isLoading = false;
        }
    }

    // ==================== INLINE FORM METHODS ====================
    private void ToggleAddForm()
    {
        _showAddForm = !_showAddForm;
        if (_showAddForm)
        {
            ResetForm();
        }
        Logger.LogDebug("ToggleAddForm - showAddForm={ShowAddForm}", _showAddForm);
    }

    private void CancelAddForm()
    {
        _showAddForm = false;
        ResetForm();
    }

    private void ResetForm()
    {
        _newAnaliza = new NewAnalizaFormModel();
        _formSubmitted = false;
        _formErrorMessage = null;
        _selectedAnalizaId = null;
        _selectedCategorie = string.Empty;
        _selectedAnalizaDto = null;
        _analizeSuggestions.Clear();
    }

    private async Task HandleAddAnaliza()
    {
        _formSubmitted = true;
        _formErrorMessage = null;

        // Validare - trebuie să selecteze o analiză din lista (autocomplete)
        if (_selectedAnalizaId == null || string.IsNullOrWhiteSpace(_newAnaliza.NumeAnaliza))
        {
            _formErrorMessage = "Selectați o analiză din listă.";
            return;
        }

        _isSaving = true;

        try
        {
            var command = new AddAnalizaRecomandataCommand
            {
                ConsultatieID = ConsultatieId!.Value,
                AnalizaNomenclatorID = _selectedAnalizaId,
                NumeAnaliza = _newAnaliza.NumeAnaliza,
                TipAnaliza = _selectedCategorie,
                Prioritate = _newAnaliza.Prioritate,
                EsteCito = _newAnaliza.EsteCito,
                CreatDe = CurrentUserId ?? Guid.Empty
            };

            var result = await Mediator.Send(command);

            if (result.IsSuccess)
            {
                Logger.LogInformation("Analiză adăugată cu succes: {AnalizaId}", result.Value);
                await LoadAnalizeAsync(); // Refresh lista
                await OnChanged.InvokeAsync();
                
                // Reset form dar rămâne deschis pentru adăugări multiple
                ResetForm();
                
                // Opțional: închide formularul după adăugare
                // _showAddForm = false;
            }
            else
            {
                _formErrorMessage = result.FirstError ?? "Eroare la adăugare analiză.";
                Logger.LogWarning("Failed to add analiză: {Error}", _formErrorMessage);
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Exception adding analiză");
            _formErrorMessage = $"Eroare: {ex.Message}";
        }
        finally
        {
            _isSaving = false;
        }
    }

    // ==================== IMPORT MODAL - PDF PARSER ====================
    private async Task ShowImportAnalizaModal()
    {
        if (_importPdfModal != null)
        {
            await _importPdfModal.ShowAsync();
        }
        Logger.LogInformation("ShowImportAnalizaModal opened for ConsultatieId: {ConsultatieId}", ConsultatieId);
    }

    private async Task OnImportAnalizePdfComplete(List<AnalizaImportDto> analize)
    {
        Logger.LogInformation("Import completed with {Count} analize efectuate to add", analize.Count);
        
        if (!ConsultatieId.HasValue || !CurrentUserId.HasValue)
        {
            _errorMessage = "Lipsesc datele necesare pentru import.";
            return;
        }
        
        _isSaving = true;
        
        try
        {
            // Folosim ImportAnalizeEfectuateCommand pentru a salva în tabela de analize efectuate cu valori
            var command = new ImportAnalizeEfectuateCommand
            {
                ConsultatieID = ConsultatieId.Value,
                Analize = analize,
                Laborator = analize.FirstOrDefault()?.Laborator ?? "Necunoscut",
                DataRecoltare = DateTime.Now.ToString("yyyy-MM-dd"),
                CreatDe = CurrentUserId.Value
            };

            var result = await Mediator.Send(command);

            if (result.IsSuccess)
            {
                Logger.LogInformation("Successfully imported {Count} analize efectuate", analize.Count);
            }
            else
            {
                Logger.LogWarning("Failed to import analize efectuate: {Error}", result.FirstError);
                _errorMessage = result.FirstError;
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Exception importing analize efectuate");
            _errorMessage = $"Eroare la import: {ex.Message}";
        }
        finally
        {
            _isSaving = false;
        }
        
        // Refresh lista
        await LoadAnalizeAsync();
        await OnChanged.InvokeAsync();
    }

    // ==================== DELETE ====================
    private async Task DeleteAnaliza(Guid analizaId)
    {
        if (!await ConfirmDelete()) return;

        _isSaving = true;
        
        try
        {
            var command = new DeleteAnalizaRecomandataCommand(analizaId);
            var result = await Mediator.Send(command);

            if (result.IsSuccess)
            {
                Logger.LogInformation("Analiză ștearsă cu succes: {AnalizaId}", analizaId);
                
                // Refresh lista din DB
                await LoadAnalizeAsync();
                await OnChanged.InvokeAsync();
            }
            else
            {
                Logger.LogWarning("Failed to delete analiză: {Error}", result.FirstError);
                _errorMessage = result.FirstError ?? "Eroare la ștergerea analizei.";
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Exception deleting analiză {AnalizaId}", analizaId);
            _errorMessage = $"Eroare: {ex.Message}";
        }
        finally
        {
            _isSaving = false;
        }
    }

    private async Task DeleteAnalizaEfectuata(Guid analizaId)
    {
        if (!await ConfirmDelete()) return;

        _isSaving = true;
        
        try
        {
            var command = new DeleteAnalizaEfectuataCommand(analizaId);
            var result = await Mediator.Send(command);

            if (result.IsSuccess)
            {
                Logger.LogInformation("Analiză efectuată ștearsă cu succes: {AnalizaId}", analizaId);
                
                // Refresh lista din DB
                await LoadAnalizeAsync();
                await OnChanged.InvokeAsync();
            }
            else
            {
                Logger.LogWarning("Failed to delete analiză efectuată: {Error}", result.FirstError);
                _errorMessage = result.FirstError ?? "Eroare la ștergerea analizei efectuate.";
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Exception deleting analiză efectuată {AnalizaId}", analizaId);
            _errorMessage = $"Eroare: {ex.Message}";
        }
        finally
        {
            _isSaving = false;
        }
    }

    private async Task<bool> ConfirmDelete()
    {
        // TODO: Implementare confirmare cu modal
        return await Task.FromResult(true);
    }

    // ==================== EXPAND/COLLAPSE ====================
    private void ToggleAnalizaExpanded(Guid analizaId)
    {
        if (_expandedAnalize.Contains(analizaId))
        {
            _expandedAnalize.Remove(analizaId);
        }
        else
        {
            _expandedAnalize.Add(analizaId);
        }
    }

    private bool IsAnalizaExpanded(Guid analizaId) => _expandedAnalize.Contains(analizaId);

    // ==================== HELPERS ====================
    private string GetCardClass(ConsultatieAnalizaRecomandataDto analiza)
    {
        return analiza.Status.ToLower() switch
        {
            "efectuata" => "status-finalizata",
            "programata" => "status-programata",
            _ => "status-recomandata"
        };
    }

    private string GetPrioritateClass(string? prioritate)
    {
        if (string.IsNullOrEmpty(prioritate)) return "secondary";
        
        return prioritate.ToLower() switch
        {
            "urgent" => "danger",
            "foarte urgent" => "danger",
            "normala" => "info",
            _ => "secondary"
        };
    }

    private string GetStatusIcon(string status)
    {
        return status.ToLower() switch
        {
            "recomandata" => "📋",
            "programata" => "📅",
            "in curs" => "⏳",
            "finalizata" => "✅",
            "anulata" => "❌",
            _ => "📄"
        };
    }

    // ==================== AUTOCOMPLETE METHODS ====================
    private async Task OnAnalizaFiltering(FilteringEventArgs args)
    {
        if (string.IsNullOrEmpty(args.Text) || args.Text.Length < 2)
        {
            args.PreventDefaultAction = true;
            _analizeSuggestions = new List<AnalizaMedicalaListDto>();
            return;
        }

        args.PreventDefaultAction = true;
        
        try
        {
            var query = new SearchAnalizeMedicaleQuery
            {
                SearchTerm = args.Text,
                PageSize = 20,
                DoarActive = true
            };

            var result = await Mediator.Send(query);
            
            if (result.IsSuccess && result.Value?.Value != null)
            {
                var items = result.Value.Value.ToList();
                Logger.LogDebug("Found {Count} analize for search term: {Term}", items.Count, args.Text);
                
                // Folosim Query pentru a filtra - Syncfusion va afișa rezultatele
                var q = new Syncfusion.Blazor.Data.Query();
                await _autoCompleteRef!.FilterAsync(items, q);
            }
            else
            {
                _analizeSuggestions = new List<AnalizaMedicalaListDto>();
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error searching analize medicale");
            _analizeSuggestions = new List<AnalizaMedicalaListDto>();
        }
    }

    private void OnAnalizaSelected(ChangeEventArgs<Guid?, AnalizaMedicalaListDto> args)
    {
        if (args.ItemData != null)
        {
            _selectedAnalizaDto = args.ItemData;
            _newAnaliza.NumeAnaliza = args.ItemData.NumeAnaliza;
            _newAnaliza.AnalizaId = args.ItemData.AnalizaID;
            _selectedCategorie = args.ItemData.NumeCategorie;
            _selectedAnalizaId = args.ItemData.AnalizaID;
            
            Logger.LogDebug("Selected analiza: {Name}, Category: {Category}", 
                args.ItemData.NumeAnaliza, args.ItemData.NumeCategorie);
        }
        else
        {
            _selectedAnalizaDto = null;
            _newAnaliza.NumeAnaliza = string.Empty;
            _newAnaliza.AnalizaId = null;
            _selectedCategorie = string.Empty;
            _selectedAnalizaId = null;
        }
    }

    // ==================== DISPOSE ====================
    public void Dispose()
    {
        Logger.LogDebug("AnalizeMedicaleTab disposed");
    }

    // ==================== INNER CLASS - Form Model ====================
    private class NewAnalizaFormModel
    {
        public Guid? AnalizaId { get; set; }
        public string NumeAnaliza { get; set; } = string.Empty;
        public string Prioritate { get; set; } = "Normala";
        public bool EsteCito { get; set; }
    }
}
