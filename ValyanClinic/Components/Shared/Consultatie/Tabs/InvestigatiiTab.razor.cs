using MediatR;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using Syncfusion.Blazor.DropDowns;
using ValyanClinic.Application.Features.ConsultatieManagement.Commands.CreateConsultatie;
using ValyanClinic.Application.DTOs.Investigatii;
using ValyanClinic.Application.Features.Investigatii.Queries;
using ValyanClinic.Application.Features.Investigatii.Commands;

namespace ValyanClinic.Components.Shared.Consultatie.Tabs;

/// <summary>
/// Tab Investigații pentru consultație - Design simplificat cu 3 carduri
/// Include: Imagistice, Explorări Funcționale, Endoscopii + Legacy text fields
/// </summary>
public partial class InvestigatiiTab : ComponentBase
{
    [Inject] private IMediator Mediator { get; set; } = default!;
    [Inject] private ILogger<InvestigatiiTab> Logger { get; set; } = default!;
    
    [Parameter] public CreateConsultatieCommand Model { get; set; } = new();
    [Parameter] public bool IsActive { get; set; }
    [Parameter] public EventCallback OnChanged { get; set; }
    [Parameter] public EventCallback OnSectionCompleted { get; set; }
    [Parameter] public bool ShowValidation { get; set; } = false;
    [Parameter] public Guid? PacientId { get; set; }
    [Parameter] public Guid? ConsultatieId { get; set; }
    [Parameter] public Guid? CurrentUserId { get; set; }

    // ==================== STATE ====================
    private bool IsSectionCompleted => IsInvestigatiiCompleted();
    
    // Nomenclatoare complete
    private List<NomenclatorInvestigatieImagisticaDto> _nomenclatorImagistice = new();
    private List<NomenclatorExplorareFuncDto> _nomenclatorExplorari = new();
    private List<NomenclatorEndoscopieDto> _nomenclatorEndoscopii = new();
    
    // Liste filtrate pe baza categoriei selectate
    private List<NomenclatorInvestigatieImagisticaDto> _filteredImagistice = new();
    private List<NomenclatorExplorareFuncDto> _filteredExplorari = new();
    private List<NomenclatorEndoscopieDto> _filteredEndoscopii = new();
    
    // Categorii extrase din nomenclatoare
    private List<string> _categoriiImagistice = new();
    private List<string> _categoriiExplorari = new();
    private List<string> _categoriiEndoscopii = new();
    
    // Investigații recomandate (adăugate)
    private List<InvestigatieImagisticaRecomandataDto> _investigatiiImagisticeRecomandate = new();
    private List<ExplorareRecomandataDto> _explorariRecomandate = new();
    private List<EndoscopieRecomandataDto> _endoscopiiRecomandate = new();
    
    // Categorii selectate
    private string? _selectedCategorieImagistice;
    private string? _selectedCategorieExplorari;
    private string? _selectedCategorieEndoscopii;
    
    // Selected IDs pentru dropdown-uri
    private Guid? _selectedImagisticaId;
    private Guid? _selectedExplorareId;
    private Guid? _selectedEndoscopieId;
    
    // Observații pentru fiecare card
    private string _observatiiImagistice = string.Empty;
    private string _observatiiExplorari = string.Empty;
    private string _observatiiEndoscopii = string.Empty;
    
    // Denumiri selectate (pentru a le salva)
    private string _selectedImagisticaDenumire = string.Empty;
    private string? _selectedImagisticaCod;
    private string _selectedExplorareDenumire = string.Empty;
    private string? _selectedExplorareCod;
    private string _selectedEndoscopieDenumire = string.Empty;
    private string? _selectedEndoscopieCod;
    
    // Guard pentru a preveni încărcări repetate
    private Guid? _lastLoadedConsultatieId;
    private bool _nomenclatoareLoaded = false;

    protected override async Task OnParametersSetAsync()
    {
        // Încarcă nomenclatoarele o singură dată
        if (!_nomenclatoareLoaded)
        {
            await LoadNomenclatoareAsync();
            _nomenclatoareLoaded = true;
        }
        
        // Încarcă investigațiile când se setează ConsultatieId
        if (ConsultatieId.HasValue && ConsultatieId != _lastLoadedConsultatieId)
        {
            _lastLoadedConsultatieId = ConsultatieId;
            await LoadInvestigatiiAsync();
        }
    }

    private async Task LoadNomenclatoareAsync()
    {
        try
        {
            var imagisticeTask = Mediator.Send(new GetNomenclatorInvestigatiiImagisticeQuery());
            var explorariTask = Mediator.Send(new GetNomenclatorExplorariFuncQuery());
            var endoscopiiTask = Mediator.Send(new GetNomenclatorEndoscopiiQuery());
            
            await Task.WhenAll(imagisticeTask, explorariTask, endoscopiiTask);
            
            if (imagisticeTask.Result.IsSuccess)
            {
                _nomenclatorImagistice = imagisticeTask.Result.Value.ToList();
                _categoriiImagistice = _nomenclatorImagistice
                    .Select(x => x.Categorie)
                    .Where(x => !string.IsNullOrEmpty(x))
                    .Distinct()
                    .OrderBy(x => x)
                    .ToList()!;
            }
            
            if (explorariTask.Result.IsSuccess)
            {
                _nomenclatorExplorari = explorariTask.Result.Value.ToList();
                _categoriiExplorari = _nomenclatorExplorari
                    .Select(x => x.Categorie)
                    .Where(x => !string.IsNullOrEmpty(x))
                    .Distinct()
                    .OrderBy(x => x)
                    .ToList()!;
            }
            
            if (endoscopiiTask.Result.IsSuccess)
            {
                _nomenclatorEndoscopii = endoscopiiTask.Result.Value.ToList();
                _categoriiEndoscopii = _nomenclatorEndoscopii
                    .Select(x => x.Categorie)
                    .Where(x => !string.IsNullOrEmpty(x))
                    .Distinct()
                    .OrderBy(x => x)
                    .ToList()!;
            }
                
            Logger.LogDebug("Nomenclatoare incarcate: {Imagistice} imagistice ({CatI} categorii), {Explorari} explorari ({CatE} categorii), {Endoscopii} endoscopii ({CatEnd} categorii)",
                _nomenclatorImagistice.Count, _categoriiImagistice.Count,
                _nomenclatorExplorari.Count, _categoriiExplorari.Count,
                _nomenclatorEndoscopii.Count, _categoriiEndoscopii.Count);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Eroare la incarcarea nomenclatoarelor");
        }
    }

    private async Task LoadInvestigatiiAsync()
    {
        if (!ConsultatieId.HasValue) return;
        
        try
        {
            var imagisticeTask = Mediator.Send(new GetInvestigatiiImagisticeRecomandateByConsultatieQuery(ConsultatieId.Value));
            var explorariTask = Mediator.Send(new GetExplorariRecomandateByConsultatieQuery(ConsultatieId.Value));
            var endoscopiiTask = Mediator.Send(new GetEndoscopiiRecomandateByConsultatieQuery(ConsultatieId.Value));
            
            await Task.WhenAll(imagisticeTask, explorariTask, endoscopiiTask);
            
            if (imagisticeTask.Result.IsSuccess)
                _investigatiiImagisticeRecomandate = imagisticeTask.Result.Value.ToList();
            if (explorariTask.Result.IsSuccess)
                _explorariRecomandate = explorariTask.Result.Value.ToList();
            if (endoscopiiTask.Result.IsSuccess)
                _endoscopiiRecomandate = endoscopiiTask.Result.Value.ToList();
                
            StateHasChanged();
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Eroare la incarcarea investigatiilor pentru consultatia {ConsultatieId}", ConsultatieId);
        }
    }

    // ==================== CATEGORY CHANGE EVENTS ====================
    private void OnCategorieImagisticeChanged(ChangeEventArgs<string, string> args)
    {
        _selectedCategorieImagistice = args.Value;
        _selectedImagisticaId = null;
        
        if (!string.IsNullOrEmpty(_selectedCategorieImagistice))
        {
            _filteredImagistice = _nomenclatorImagistice
                .Where(x => x.Categorie == _selectedCategorieImagistice)
                .OrderBy(x => x.Ordine)
                .ToList();
        }
        else
        {
            _filteredImagistice.Clear();
        }
    }

    private void OnCategorieExplorariChanged(ChangeEventArgs<string, string> args)
    {
        _selectedCategorieExplorari = args.Value;
        _selectedExplorareId = null;
        
        if (!string.IsNullOrEmpty(_selectedCategorieExplorari))
        {
            _filteredExplorari = _nomenclatorExplorari
                .Where(x => x.Categorie == _selectedCategorieExplorari)
                .OrderBy(x => x.Ordine)
                .ToList();
        }
        else
        {
            _filteredExplorari.Clear();
        }
    }

    private void OnCategorieEndoscopiiChanged(ChangeEventArgs<string, string> args)
    {
        _selectedCategorieEndoscopii = args.Value;
        _selectedEndoscopieId = null;
        
        if (!string.IsNullOrEmpty(_selectedCategorieEndoscopii))
        {
            _filteredEndoscopii = _nomenclatorEndoscopii
                .Where(x => x.Categorie == _selectedCategorieEndoscopii)
                .OrderBy(x => x.Ordine)
                .ToList();
        }
        else
        {
            _filteredEndoscopii.Clear();
        }
    }

    // ==================== SELECTION EVENTS ====================
    private void OnImagisticaSelected(ChangeEventArgs<Guid?, NomenclatorInvestigatieImagisticaDto> args)
    {
        _selectedImagisticaId = args.Value;
        if (args.ItemData != null)
        {
            _selectedImagisticaDenumire = args.ItemData.Denumire;
            _selectedImagisticaCod = args.ItemData.Cod;
            Logger.LogDebug("Imagistica selected: {Id} - {Denumire}", args.Value, args.ItemData.Denumire);
        }
    }

    private void OnExplorareSelected(ChangeEventArgs<Guid?, NomenclatorExplorareFuncDto> args)
    {
        _selectedExplorareId = args.Value;
        if (args.ItemData != null)
        {
            _selectedExplorareDenumire = args.ItemData.Denumire;
            _selectedExplorareCod = args.ItemData.Cod;
            Logger.LogDebug("Explorare selected: {Id} - {Denumire}", args.Value, args.ItemData.Denumire);
        }
    }

    private void OnEndoscopieSelected(ChangeEventArgs<Guid?, NomenclatorEndoscopieDto> args)
    {
        _selectedEndoscopieId = args.Value;
        if (args.ItemData != null)
        {
            _selectedEndoscopieDenumire = args.ItemData.Denumire;
            _selectedEndoscopieCod = args.ItemData.Cod;
            Logger.LogDebug("Endoscopie selected: {Id} - {Denumire}", args.Value, args.ItemData.Denumire);
        }
    }

    // ==================== ADD METHODS ====================
    private async Task AddImagisticaAsync()
    {
        Logger.LogInformation("AddImagisticaAsync called: ConsultatieId={ConsultatieId}, SelectedId={SelectedId}", 
            ConsultatieId, _selectedImagisticaId);
            
        if (!ConsultatieId.HasValue)
        {
            Logger.LogWarning("ConsultatieId is null - cannot add investigation");
            return;
        }
        
        if (!_selectedImagisticaId.HasValue)
        {
            Logger.LogWarning("SelectedImagisticaId is null - please select an investigation");
            return;
        }
        
        try
        {
            var command = new AddInvestigatieImagisticaRecomandataCommand
            {
                ConsultatieID = ConsultatieId.Value,
                InvestigatieNomenclatorID = _selectedImagisticaId,
                DenumireInvestigatie = _selectedImagisticaDenumire,
                CodInvestigatie = _selectedImagisticaCod,
                RegiuneAnatomica = _observatiiImagistice, // Folosim observații ca regiune
                Prioritate = "Normala",
                EsteCito = false,
                IndicatiiClinice = null,
                CreatDe = CurrentUserId ?? Guid.Empty
            };
            
            Logger.LogInformation("Sending AddInvestigatieImagisticaRecomandataCommand for {Denumire}", _selectedImagisticaDenumire);
            
            var result = await Mediator.Send(command);
            
            Logger.LogInformation("Result: IsSuccess={IsSuccess}, Errors={Errors}", 
                result.IsSuccess, result.IsSuccess ? "none" : string.Join(", ", result.Errors ?? new List<string>()));
            
            if (result.IsSuccess)
            {
                await LoadInvestigatiiAsync();
                // Reset form
                _selectedImagisticaId = null;
                _observatiiImagistice = string.Empty;
                await OnChanged.InvokeAsync();
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Eroare la adaugarea investigatiei imagistice");
        }
    }

    private async Task AddExplorareAsync()
    {
        Logger.LogInformation("AddExplorareAsync called: ConsultatieId={ConsultatieId}, SelectedId={SelectedId}", 
            ConsultatieId, _selectedExplorareId);
            
        if (!ConsultatieId.HasValue)
        {
            Logger.LogWarning("ConsultatieId is null - cannot add explorare");
            return;
        }
        
        if (!_selectedExplorareId.HasValue)
        {
            Logger.LogWarning("SelectedExplorareId is null - please select an explorare");
            return;
        }
        
        try
        {
            var command = new AddExplorareRecomandataCommand
            {
                ConsultatieID = ConsultatieId.Value,
                ExplorareNomenclatorID = _selectedExplorareId,
                DenumireExplorare = _selectedExplorareDenumire,
                CodExplorare = _selectedExplorareCod,
                Prioritate = "Normala",
                EsteCito = false,
                IndicatiiClinice = _observatiiExplorari,
                CreatDe = CurrentUserId ?? Guid.Empty
            };
            
            Logger.LogInformation("Sending AddExplorareRecomandataCommand for {Denumire}", _selectedExplorareDenumire);
            
            var result = await Mediator.Send(command);
            
            Logger.LogInformation("Result: IsSuccess={IsSuccess}, Errors={Errors}", 
                result.IsSuccess, result.IsSuccess ? "none" : string.Join(", ", result.Errors ?? new List<string>()));
            
            if (result.IsSuccess)
            {
                await LoadInvestigatiiAsync();
                // Reset form
                _selectedExplorareId = null;
                _observatiiExplorari = string.Empty;
                await OnChanged.InvokeAsync();
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Eroare la adaugarea explorarii functionale");
        }
    }

    private async Task AddEndoscopieAsync()
    {
        Logger.LogInformation("AddEndoscopieAsync called: ConsultatieId={ConsultatieId}, SelectedId={SelectedId}", 
            ConsultatieId, _selectedEndoscopieId);
            
        if (!ConsultatieId.HasValue)
        {
            Logger.LogWarning("ConsultatieId is null - cannot add endoscopie");
            return;
        }
        
        if (!_selectedEndoscopieId.HasValue)
        {
            Logger.LogWarning("SelectedEndoscopieId is null - please select an endoscopie");
            return;
        }
        
        try
        {
            var command = new AddEndoscopieRecomandataCommand
            {
                ConsultatieID = ConsultatieId.Value,
                EndoscopieNomenclatorID = _selectedEndoscopieId,
                DenumireEndoscopie = _selectedEndoscopieDenumire,
                CodEndoscopie = _selectedEndoscopieCod,
                Prioritate = "Normala",
                EsteCito = false,
                IndicatiiClinice = _observatiiEndoscopii,
                CreatDe = CurrentUserId ?? Guid.Empty
            };
            
            Logger.LogInformation("Sending AddEndoscopieRecomandataCommand for {Denumire}", _selectedEndoscopieDenumire);
            
            var result = await Mediator.Send(command);
            
            Logger.LogInformation("Result: IsSuccess={IsSuccess}, Errors={Errors}", 
                result.IsSuccess, result.IsSuccess ? "none" : string.Join(", ", result.Errors ?? new List<string>()));
            
            if (result.IsSuccess)
            {
                await LoadInvestigatiiAsync();
                // Reset form
                _selectedEndoscopieId = null;
                _observatiiEndoscopii = string.Empty;
                await OnChanged.InvokeAsync();
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Eroare la adaugarea endoscopiei");
        }
    }

    // ==================== DELETE METHODS ====================
    private async Task DeleteImagisticaAsync(Guid id)
    {
        try
        {
            var result = await Mediator.Send(new DeleteInvestigatieImagisticaRecomandataCommand(id));
            if (result.IsSuccess)
            {
                await LoadInvestigatiiAsync();
                await OnChanged.InvokeAsync();
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Eroare la stergerea investigatiei imagistice {Id}", id);
        }
    }

    private async Task DeleteExplorareAsync(Guid id)
    {
        try
        {
            var result = await Mediator.Send(new DeleteExplorareRecomandataCommand(id));
            if (result.IsSuccess)
            {
                await LoadInvestigatiiAsync();
                await OnChanged.InvokeAsync();
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Eroare la stergerea explorarii {Id}", id);
        }
    }

    private async Task DeleteEndoscopieAsync(Guid id)
    {
        try
        {
            var result = await Mediator.Send(new DeleteEndoscopieRecomandataCommand(id));
            if (result.IsSuccess)
            {
                await LoadInvestigatiiAsync();
                await OnChanged.InvokeAsync();
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Eroare la stergerea endoscopiei {Id}", id);
        }
    }

    // ==================== FIELD CHANGE ====================
    private async Task OnFieldChanged()
    {
        await OnChanged.InvokeAsync();
        if (IsSectionCompleted)
        {
            await OnSectionCompleted.InvokeAsync();
        }
    }

    private bool IsInvestigatiiCompleted()
    {
        // Considerăm completă dacă avem cel puțin o investigație sau 2 câmpuri text completate
        if (_investigatiiImagisticeRecomandate.Any() || _explorariRecomandate.Any() || _endoscopiiRecomandate.Any())
            return true;
            
        var completedCount = 0;
        if (!string.IsNullOrWhiteSpace(Model.InvestigatiiLaborator)) completedCount++;
        if (!string.IsNullOrWhiteSpace(Model.InvestigatiiImagistice)) completedCount++;
        if (!string.IsNullOrWhiteSpace(Model.InvestigatiiEKG)) completedCount++;
        if (!string.IsNullOrWhiteSpace(Model.AlteInvestigatii)) completedCount++;
        
        return completedCount >= 2;
    }
}
