using Microsoft.AspNetCore.Components;
using MediatR;
using ValyanClinic.Application.ViewModels;
using ValyanClinic.Application.Features.AnalizeMedicale.Queries;
using ValyanClinic.Application.Features.AnalizeMedicale.Commands;

namespace ValyanClinic.Components.Shared.Consultatie.Tabs;

/// <summary>
/// Tab Analize Medicale pentru consultație
/// Include: Analize recomandate + Analize efectuate (cu rezultate)
/// </summary>
public partial class AnalizeMedicaleTab : ComponentBase
{
    [Inject] private IMediator Mediator { get; set; } = default!;
    
    [Parameter] public Guid? ConsultatieId { get; set; }
    [Parameter] public Guid? CurrentUserId { get; set; }
    [Parameter] public EventCallback OnChanged { get; set; }
    [Parameter] public EventCallback OnSectionCompleted { get; set; }

    private List<ConsultatieAnalizaMedicalaDto> _toateAnalizele = new();
    private HashSet<Guid> _expandedAnalize = new();
    
    private bool IsSectionCompleted => AnalizeRecomandate.Any() || AnalizeEfectuate.Any();

    // Filtrare analize
    private List<ConsultatieAnalizaMedicalaDto> AnalizeRecomandate => 
        _toateAnalizele.Where(a => !a.AreRezultate).OrderByDescending(a => a.DataRecomandare).ToList();
    
    private List<ConsultatieAnalizaMedicalaDto> AnalizeEfectuate => 
        _toateAnalizele.Where(a => a.AreRezultate).OrderByDescending(a => a.DataEfectuare ?? a.DataRecomandare).ToList();

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

        var result = await Mediator.Send(new GetAnalizeMedicaleByConsultatieQuery(ConsultatieId.Value));
        if (result.IsSuccess)
        {
            _toateAnalizele = result.Value;
            
            // Auto-expandăm analizele cu rezultate anormale
            foreach (var analiza in AnalizeEfectuate.Where(a => a.Detalii.Any(d => d.EsteAnormal)))
            {
                _expandedAnalize.Add(analiza.Id);
            }
        }
    }

    private void ShowAddAnalizaModal()
    {
        // TODO: Implementare modal pentru adăugare analiză
        // Va deschide un modal cu formular pentru analiză nouă
    }

    private void ShowImportAnalizaModal()
    {
        // TODO: Implementare modal pentru import OCR
        // Va deschide un modal pentru încărcare PDF/scan și procesare OCR
    }

    private async Task DeleteAnaliza(Guid analizaId)
    {
        if (!await ConfirmDelete()) return;

        var result = await Mediator.Send(new DeleteAnalizaMedicalaCommand(analizaId));
        if (result.IsSuccess)
        {
            await LoadAnalizeAsync();
            await OnChanged.InvokeAsync();
        }
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

    private string GetPrioritateClass(string prioritate)
    {
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
}
