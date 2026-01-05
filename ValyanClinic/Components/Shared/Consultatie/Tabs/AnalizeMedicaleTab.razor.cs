using Microsoft.AspNetCore.Components;
using MediatR;
using Microsoft.Extensions.Logging;
using ValyanClinic.Application.ViewModels;
using ValyanClinic.Application.Features.AnalizeMedicale.Queries.GetAnalizeMedicaleByConsultatie;
using ValyanClinic.Application.Features.AnalizeMedicale.Commands.AddAnalizaToConsultatie;

namespace ValyanClinic.Components.Shared.Consultatie.Tabs;

/// <summary>
/// Tab Analize Medicale pentru consultație
/// Include: Analize recomandate + Analize efectuate (cu rezultate)
/// </summary>
public partial class AnalizeMedicaleTab : ComponentBase, IDisposable
{
    [Inject] private IMediator Mediator { get; set; } = default!;
    [Inject] private ILogger<AnalizeMedicaleTab> Logger { get; set; } = default!;
    
    [Parameter] public Guid? ConsultatieId { get; set; }
    [Parameter] public Guid? CurrentUserId { get; set; }
    [Parameter] public EventCallback OnChanged { get; set; }
    [Parameter] public EventCallback OnSectionCompleted { get; set; }

    private List<ConsultatieAnalizaMedicalaDto> _toateAnalizele = new();
    private HashSet<Guid> _expandedAnalize = new();

    // Modal states
    private bool _showAddAnalizaModal;
    private bool _showImportAnalizaModal;
    
    // Loading & Error
    private bool _isLoading;
    private bool _isSaving;
    private string? _errorMessage;

    // Section completion
    private bool IsSectionCompleted => AnalizeRecomandate.Any() || AnalizeEfectuate.Any();

    // Filtrare analize
    private List<ConsultatieAnalizaMedicalaDto> AnalizeRecomandate => 
        _toateAnalizele.Where(a => !a.AreRezultate).OrderByDescending(a => a.DataRecomandare).ToList();
    
    private List<ConsultatieAnalizaMedicalaDto> AnalizeEfectuate => 
        _toateAnalizele.Where(a => a.AreRezultate).OrderByDescending(a => a.DataEfectuare ?? a.DataRecomandare).ToList();

    // ==================== LIFECYCLE ====================
    protected override async Task OnInitializedAsync()
    {
        Logger.LogInformation("AnalizeMedicaleTab initialized for ConsultatieId: {ConsultatieId}", ConsultatieId);
    }

    protected override async Task OnParametersSetAsync()
    {
        if (ConsultatieId.HasValue)
        {
            await LoadAnalizeAsync();
        }
    }

    private async Task LoadAnalizeAsync()
    {
        if (!ConsultatieId.HasValue) return;

        _isLoading = true;
        _errorMessage = null;

        try
        {
            var query = new GetAnalizeMedicaleByConsultatieQuery { ConsultatieId = ConsultatieId.Value };
            var result = await Mediator.Send(query);
            
            if (result.IsSuccess && result.Value != null)
            {
                _toateAnalizele = result.Value.ToList();
                
                // Auto-expandăm analizele cu rezultate anormale
                foreach (var analiza in AnalizeEfectuate.Where(a => a.Detalii.Any(d => d.EsteAnormal)))
                {
                    _expandedAnalize.Add(analiza.Id);
                }

                Logger.LogInformation("Loaded {Count} analize for consultație {ConsultatieId}", 
                    _toateAnalizele.Count, ConsultatieId.Value);

                if (IsSectionCompleted)
                {
                    await OnSectionCompleted.InvokeAsync();
                }
            }
            else
            {
                _errorMessage = result.FirstError ?? "Eroare la încărcarea analizelor.";
                Logger.LogWarning("Failed to load analize: {Error}", _errorMessage);
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

    private void ShowAddAnalizaModal()
    {
        _showAddAnalizaModal = true;
        Logger.LogInformation("ShowAddAnalizaModal - opening modal, state={State}", _showAddAnalizaModal);
        StateHasChanged(); // FORCE UI UPDATE
    }

    private void CloseAddAnalizaModal()
    {
        _showAddAnalizaModal = false;
        StateHasChanged(); // FORCE UI UPDATE
    }

    private async Task HandleAddAnalizaSave((string NumeAnaliza, string Prioritate, bool EsteCito, DateTime? DataProgramata, string IndicatiiMedic) data)
    {
        _isSaving = true;
        _errorMessage = null;

        try
        {
            var command = new AddAnalizaToConsultatieCommand
            {
                ConsultatieID = ConsultatieId!.Value,
                PacientID = Guid.Empty, // TODO: Get from parent component
                NumeAnaliza = data.NumeAnaliza,
                Prioritate = data.Prioritate,
                EsteCito = data.EsteCito,
                DataProgramata = data.DataProgramata,
                IndicatiiMedic = data.IndicatiiMedic,
                CreatDe = CurrentUserId ?? Guid.Empty
            };

            var result = await Mediator.Send(command);

            if (result.IsSuccess)
            {
                Logger.LogInformation("Analiză adăugată cu succes: {AnalizaId}", result.Value);
                await LoadAnalizeAsync(); // Refresh
                await OnChanged.InvokeAsync();
                _showAddAnalizaModal = false; // Close modal
            }
            else
            {
                _errorMessage = result.FirstError ?? "Eroare la adăugare analiză.";
                Logger.LogWarning("Failed to add analiză: {Error}", _errorMessage);
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Exception adding analiză");
            _errorMessage = $"Eroare: {ex.Message}";
        }
        finally
        {
            _isSaving = false;
        }
    }

    private void ShowImportAnalizaModal()
    {
        _showImportAnalizaModal = true;
        Logger.LogInformation("ShowImportAnalizaModal - not implemented yet");
    }

    private void CloseImportAnalizaModal()
    {
        _showImportAnalizaModal = false;
    }

    private async Task DeleteAnaliza(Guid analizaId)
    {
        if (!await ConfirmDelete()) return;

        // TODO: Implement DeleteAnalizaMedicalaCommand when created
        Logger.LogInformation("Delete analiza {AnalizaId} - command not implemented yet", analizaId);
        
        // Temporary: Remove from local list
        _toateAnalizele.RemoveAll(a => a.Id == analizaId);
        await OnChanged.InvokeAsync();
    }

    private async Task<bool> ConfirmDelete()
    {
        // TODO: Implementare confirmare cu modal
        return await Task.FromResult(true);
    }

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

    private string GetCardClass(ConsultatieAnalizaMedicalaDto analiza)
    {
        return analiza.StatusAnaliza.ToLower() switch
        {
            "finalizata" => "status-finalizata",
            "programata" => "status-programata",
            "in curs" => "status-in-curs",
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

    public void Dispose()
    {
        Logger.LogDebug("AnalizeMedicaleTab disposed");
    }
}
