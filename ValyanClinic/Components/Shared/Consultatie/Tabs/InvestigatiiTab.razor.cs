using MediatR;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using Syncfusion.Blazor.DropDowns;
using Syncfusion.Blazor.RichTextEditor;
using ValyanClinic.Application.Common.Results;
using ValyanClinic.Application.Features.ConsultatieManagement.Commands.CreateConsultatie;
using ValyanClinic.Application.DTOs.Investigatii;
using ValyanClinic.Application.Features.Investigatii.Queries;
using ValyanClinic.Application.Features.Investigatii.Commands;

namespace ValyanClinic.Components.Shared.Consultatie.Tabs;

/// <summary>
/// Tab Investigații pentru consultație - Design simplificat cu 3 carduri
/// Include: Imagistice, Explorări Funcționale, Endoscopii + Legacy text fields (RTE)
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

    // ==================== TOOLBAR RTE MINIMAL ====================
    /// <summary>
    /// Toolbar items minimal pentru Rich Text Editor în carduri și observații
    /// </summary>
    private List<ToolbarItemModel> MinimalToolbarItems { get; } = new List<ToolbarItemModel>
    {
        new ToolbarItemModel() { Command = ToolbarCommand.Bold },
        new ToolbarItemModel() { Command = ToolbarCommand.Italic },
        new ToolbarItemModel() { Command = ToolbarCommand.Underline },
        new ToolbarItemModel() { Command = ToolbarCommand.Separator },
        new ToolbarItemModel() { Command = ToolbarCommand.OrderedList },
        new ToolbarItemModel() { Command = ToolbarCommand.UnorderedList },
        new ToolbarItemModel() { Command = ToolbarCommand.Separator },
        new ToolbarItemModel() { Command = ToolbarCommand.Undo },
        new ToolbarItemModel() { Command = ToolbarCommand.Redo }
    };

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
    
    // ==================== STATE PENTRU INVESTIGAȚII EFECTUATE ====================
    // Liste filtrate pentru efectuate
    private List<NomenclatorInvestigatieImagisticaDto> _filteredImagisticeEfect = new();
    private List<NomenclatorExplorareFuncDto> _filteredExplorariEfect = new();
    private List<NomenclatorEndoscopieDto> _filteredEndoscopiiEfect = new();
    
    // Investigații efectuate (adăugate)
    private List<InvestigatieImagisticaEfectuataDto> _investigatiiImagisticeEfectuate = new();
    private List<ExplorareEfectuataDto> _explorariEfectuate = new();
    private List<EndoscopieEfectuataDto> _endoscopiiEfectuate = new();
    
    // Categorii selectate pentru efectuate
    private string? _selectedCategorieImagisticeEfect;
    private string? _selectedCategorieExplorariEfect;
    private string? _selectedCategorieEndoscopiiEfect;
    
    // Selected IDs pentru dropdown-uri efectuate
    private Guid? _selectedImagisticaEfectId;
    private Guid? _selectedExplorareEfectId;
    private Guid? _selectedEndoscopieEfectId;
    
    // Observații RTE pentru efectuate
    private string _observatiiImagisticeEfect = string.Empty;
    private string _observatiiExplorariEfect = string.Empty;
    private string _observatiiEndoscopiiEfect = string.Empty;
    
    // Denumiri selectate pentru efectuate
    private string _selectedImagisticaEfectDenumire = string.Empty;
    private string? _selectedImagisticaEfectCod;
    private string _selectedExplorareEfectDenumire = string.Empty;
    private string? _selectedExplorareEfectCod;
    private string _selectedEndoscopieEfectDenumire = string.Empty;
    private string? _selectedEndoscopieEfectCod;
    
    // Guard pentru a preveni încărcări repetate
    private Guid? _lastLoadedConsultatieId;
    private bool _nomenclatoareLoaded = false;
    
    // ==================== EDITARE INVESTIGAȚII EFECTUATE ====================
    private bool _isEditModalVisible = false;
    private string _editModalTitle = string.Empty;
    private string _editModalType = string.Empty; // "imagistica", "explorare", "endoscopie"
    private Guid _editInvestigatieId;
    private string _editRezultat = string.Empty;
    
    // ==================== SUMARE INVESTIGAȚII EFECTUATE ====================
    /// <summary>
    /// Generează HTML cu bullet points pentru sumarul investigațiilor imagistice efectuate
    /// </summary>
    private string SumarImagisticeEfectuate => GenerateSumarHtml(
        _investigatiiImagisticeEfectuate
            .Where(i => !string.IsNullOrWhiteSpace(i.Rezultat))
            .Select(i => $"<strong>{i.DenumireInvestigatie}</strong>: {i.Rezultat}"));
    
    /// <summary>
    /// Generează HTML cu bullet points pentru sumarul explorărilor efectuate
    /// </summary>
    private string SumarExplorariEfectuate => GenerateSumarHtml(
        _explorariEfectuate
            .Where(e => !string.IsNullOrWhiteSpace(e.Rezultat))
            .Select(e => $"<strong>{e.DenumireExplorare}</strong>: {e.Rezultat}"));
    
    /// <summary>
    /// Generează HTML cu bullet points pentru sumarul endoscopiilor efectuate
    /// </summary>
    private string SumarEndoscopiiEfectuate => GenerateSumarHtml(
        _endoscopiiEfectuate
            .Where(e => !string.IsNullOrWhiteSpace(e.Rezultat))
            .Select(e => $"<strong>{e.DenumireEndoscopie}</strong>: {e.Rezultat}"));
    
    /// <summary>
    /// Generează HTML cu sumarul complet al tuturor investigațiilor efectuate (Imagistice + Explorări + Endoscopii)
    /// </summary>
    private string SumarCompletInvestigatiiEfectuate => GenerateSumarCompletHtml();
    
    /// <summary>
    /// Verifică dacă există cel puțin o investigație efectuată cu rezultat
    /// </summary>
    private bool AreInvestigatiiEfectuateCuRezultat =>
        _investigatiiImagisticeEfectuate.Any(i => !string.IsNullOrWhiteSpace(i.Rezultat)) ||
        _explorariEfectuate.Any(e => !string.IsNullOrWhiteSpace(e.Rezultat)) ||
        _endoscopiiEfectuate.Any(e => !string.IsNullOrWhiteSpace(e.Rezultat));
    
    /// <summary>
    /// Helper pentru generarea HTML-ului cu bullet points
    /// </summary>
    private static string GenerateSumarHtml(IEnumerable<string> items)
    {
        var list = items.ToList();
        if (!list.Any()) return string.Empty;
        if (list.Count == 1) return list.First();
        return "<ul>" + string.Join("", list.Select(i => $"<li>{i}</li>")) + "</ul>";
    }
    
    /// <summary>
    /// Generează HTML-ul complet pentru toate investigațiile efectuate, grupate pe categorii
    /// </summary>
    private string GenerateSumarCompletHtml()
    {
        var sections = new List<string>();
        
        // Imagistice
        var imagistice = _investigatiiImagisticeEfectuate
            .Where(i => !string.IsNullOrWhiteSpace(i.Rezultat))
            .Select(i => $"<li><strong>{i.DenumireInvestigatie}</strong>: {i.Rezultat}</li>")
            .ToList();
        if (imagistice.Any())
            sections.Add($"<p><strong><i class='fas fa-x-ray'></i> Investigații Imagistice:</strong></p><ul>{string.Join("", imagistice)}</ul>");
        
        // Explorări
        var explorari = _explorariEfectuate
            .Where(e => !string.IsNullOrWhiteSpace(e.Rezultat))
            .Select(e => $"<li><strong>{e.DenumireExplorare}</strong>: {e.Rezultat}</li>")
            .ToList();
        if (explorari.Any())
            sections.Add($"<p><strong><i class='fas fa-heartbeat'></i> Explorări Funcționale:</strong></p><ul>{string.Join("", explorari)}</ul>");
        
        // Endoscopii
        var endoscopii = _endoscopiiEfectuate
            .Where(e => !string.IsNullOrWhiteSpace(e.Rezultat))
            .Select(e => $"<li><strong>{e.DenumireEndoscopie}</strong>: {e.Rezultat}</li>")
            .ToList();
        if (endoscopii.Any())
            sections.Add($"<p><strong><i class='fas fa-microscope'></i> Endoscopii:</strong></p><ul>{string.Join("", endoscopii)}</ul>");
        
        return string.Join("<hr style='margin: 0.5rem 0; border-color: #e5e7eb;'/>", sections);
    }

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
            // Încarcă investigații recomandate
            var imagisticeTask = Mediator.Send(new GetInvestigatiiImagisticeRecomandateByConsultatieQuery(ConsultatieId.Value));
            var explorariTask = Mediator.Send(new GetExplorariRecomandateByConsultatieQuery(ConsultatieId.Value));
            var endoscopiiTask = Mediator.Send(new GetEndoscopiiRecomandateByConsultatieQuery(ConsultatieId.Value));
            
            // Încarcă investigații efectuate
            var imagisticeEfectTask = Mediator.Send(new GetInvestigatiiImagisticeEfectuateByConsultatieQuery(ConsultatieId.Value));
            var explorariEfectTask = Mediator.Send(new GetExplorariEfectuateByConsultatieQuery(ConsultatieId.Value));
            var endoscopiiEfectTask = Mediator.Send(new GetEndoscopiiEfectuateByConsultatieQuery(ConsultatieId.Value));
            
            await Task.WhenAll(
                imagisticeTask, explorariTask, endoscopiiTask,
                imagisticeEfectTask, explorariEfectTask, endoscopiiEfectTask);
            
            // Recomandate
            if (imagisticeTask.Result.IsSuccess)
                _investigatiiImagisticeRecomandate = imagisticeTask.Result.Value.ToList();
            if (explorariTask.Result.IsSuccess)
                _explorariRecomandate = explorariTask.Result.Value.ToList();
            if (endoscopiiTask.Result.IsSuccess)
                _endoscopiiRecomandate = endoscopiiTask.Result.Value.ToList();
            
            // Efectuate
            if (imagisticeEfectTask.Result.IsSuccess)
                _investigatiiImagisticeEfectuate = imagisticeEfectTask.Result.Value.ToList();
            if (explorariEfectTask.Result.IsSuccess)
                _explorariEfectuate = explorariEfectTask.Result.Value.ToList();
            if (endoscopiiEfectTask.Result.IsSuccess)
                _endoscopiiEfectuate = endoscopiiEfectTask.Result.Value.ToList();
                
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

    // ==================== CATEGORY CHANGE EVENTS EFECTUATE ====================
    private void OnCategorieImagisticeEfectChanged(ChangeEventArgs<string, string> args)
    {
        _selectedCategorieImagisticeEfect = args.Value;
        _selectedImagisticaEfectId = null;
        
        if (!string.IsNullOrEmpty(_selectedCategorieImagisticeEfect))
        {
            _filteredImagisticeEfect = _nomenclatorImagistice
                .Where(x => x.Categorie == _selectedCategorieImagisticeEfect)
                .OrderBy(x => x.Ordine)
                .ToList();
        }
        else
        {
            _filteredImagisticeEfect.Clear();
        }
    }

    private void OnCategorieExplorariEfectChanged(ChangeEventArgs<string, string> args)
    {
        _selectedCategorieExplorariEfect = args.Value;
        _selectedExplorareEfectId = null;
        
        if (!string.IsNullOrEmpty(_selectedCategorieExplorariEfect))
        {
            _filteredExplorariEfect = _nomenclatorExplorari
                .Where(x => x.Categorie == _selectedCategorieExplorariEfect)
                .OrderBy(x => x.Ordine)
                .ToList();
        }
        else
        {
            _filteredExplorariEfect.Clear();
        }
    }

    private void OnCategorieEndoscopiiEfectChanged(ChangeEventArgs<string, string> args)
    {
        _selectedCategorieEndoscopiiEfect = args.Value;
        _selectedEndoscopieEfectId = null;
        
        if (!string.IsNullOrEmpty(_selectedCategorieEndoscopiiEfect))
        {
            _filteredEndoscopiiEfect = _nomenclatorEndoscopii
                .Where(x => x.Categorie == _selectedCategorieEndoscopiiEfect)
                .OrderBy(x => x.Ordine)
                .ToList();
        }
        else
        {
            _filteredEndoscopiiEfect.Clear();
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

    // ==================== SELECTION EVENTS EFECTUATE ====================
    private void OnImagisticaEfectSelected(ChangeEventArgs<Guid?, NomenclatorInvestigatieImagisticaDto> args)
    {
        _selectedImagisticaEfectId = args.Value;
        if (args.ItemData != null)
        {
            _selectedImagisticaEfectDenumire = args.ItemData.Denumire;
            _selectedImagisticaEfectCod = args.ItemData.Cod;
        }
    }

    private void OnExplorareEfectSelected(ChangeEventArgs<Guid?, NomenclatorExplorareFuncDto> args)
    {
        _selectedExplorareEfectId = args.Value;
        if (args.ItemData != null)
        {
            _selectedExplorareEfectDenumire = args.ItemData.Denumire;
            _selectedExplorareEfectCod = args.ItemData.Cod;
        }
    }

    private void OnEndoscopieEfectSelected(ChangeEventArgs<Guid?, NomenclatorEndoscopieDto> args)
    {
        _selectedEndoscopieEfectId = args.Value;
        if (args.ItemData != null)
        {
            _selectedEndoscopieEfectDenumire = args.ItemData.Denumire;
            _selectedEndoscopieEfectCod = args.ItemData.Cod;
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
                RegiuneAnatomica = null,
                Prioritate = "Normala",
                EsteCito = false,
                IndicatiiClinice = null,
                ObservatiiMedic = _observatiiImagistice,
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
                IndicatiiClinice = null,
                ObservatiiMedic = _observatiiExplorari,
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
                IndicatiiClinice = null,
                ObservatiiMedic = _observatiiEndoscopii,
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

    // ==================== ADD METHODS EFECTUATE ====================
    private async Task AddImagisticaEfectuataAsync()
    {
        if (!ConsultatieId.HasValue || !PacientId.HasValue || !_selectedImagisticaEfectId.HasValue)
        {
            Logger.LogWarning("Missing required IDs for AddImagisticaEfectuataAsync");
            return;
        }
        
        try
        {
            var command = new AddInvestigatieImagisticaEfectuataCommand
            {
                ConsultatieID = ConsultatieId.Value,
                PacientID = PacientId.Value,
                InvestigatieNomenclatorID = _selectedImagisticaEfectId,
                DenumireInvestigatie = _selectedImagisticaEfectDenumire,
                CodInvestigatie = _selectedImagisticaEfectCod,
                DataEfectuare = DateTime.Now,
                Rezultat = _observatiiImagisticeEfect,
                CreatDe = CurrentUserId ?? Guid.Empty
            };
            
            var result = await Mediator.Send(command);
            
            if (result.IsSuccess)
            {
                await LoadInvestigatiiAsync();
                _selectedImagisticaEfectId = null;
                _observatiiImagisticeEfect = string.Empty;
                await OnChanged.InvokeAsync();
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Eroare la adaugarea investigatiei imagistice efectuate");
        }
    }

    private async Task AddExplorareEfectuataAsync()
    {
        if (!ConsultatieId.HasValue || !PacientId.HasValue || !_selectedExplorareEfectId.HasValue)
        {
            Logger.LogWarning("Missing required IDs for AddExplorareEfectuataAsync");
            return;
        }
        
        try
        {
            var command = new AddExplorareEfectuataCommand
            {
                ConsultatieID = ConsultatieId.Value,
                PacientID = PacientId.Value,
                ExplorareNomenclatorID = _selectedExplorareEfectId,
                DenumireExplorare = _selectedExplorareEfectDenumire,
                CodExplorare = _selectedExplorareEfectCod,
                DataEfectuare = DateTime.Now,
                Rezultat = _observatiiExplorariEfect,
                CreatDe = CurrentUserId ?? Guid.Empty
            };
            
            var result = await Mediator.Send(command);
            
            if (result.IsSuccess)
            {
                await LoadInvestigatiiAsync();
                _selectedExplorareEfectId = null;
                _observatiiExplorariEfect = string.Empty;
                await OnChanged.InvokeAsync();
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Eroare la adaugarea explorarii efectuate");
        }
    }

    private async Task AddEndoscopieEfectuataAsync()
    {
        if (!ConsultatieId.HasValue || !PacientId.HasValue || !_selectedEndoscopieEfectId.HasValue)
        {
            Logger.LogWarning("Missing required IDs for AddEndoscopieEfectuataAsync");
            return;
        }
        
        try
        {
            var command = new AddEndoscopieEfectuataCommand
            {
                ConsultatieID = ConsultatieId.Value,
                PacientID = PacientId.Value,
                EndoscopieNomenclatorID = _selectedEndoscopieEfectId,
                DenumireEndoscopie = _selectedEndoscopieEfectDenumire,
                CodEndoscopie = _selectedEndoscopieEfectCod,
                DataEfectuare = DateTime.Now,
                Rezultat = _observatiiEndoscopiiEfect,
                CreatDe = CurrentUserId ?? Guid.Empty
            };
            
            var result = await Mediator.Send(command);
            
            if (result.IsSuccess)
            {
                await LoadInvestigatiiAsync();
                _selectedEndoscopieEfectId = null;
                _observatiiEndoscopiiEfect = string.Empty;
                await OnChanged.InvokeAsync();
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Eroare la adaugarea endoscopiei efectuate");
        }
    }

    // ==================== DELETE METHODS EFECTUATE ====================
    private async Task DeleteImagisticaEfectuataAsync(Guid id)
    {
        try
        {
            var result = await Mediator.Send(new DeleteInvestigatieImagisticaEfectuataCommand(id));
            if (result.IsSuccess)
            {
                await LoadInvestigatiiAsync();
                await OnChanged.InvokeAsync();
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Eroare la stergerea investigatiei efectuate {Id}", id);
        }
    }

    private async Task DeleteExplorareEfectuataAsync(Guid id)
    {
        try
        {
            var result = await Mediator.Send(new DeleteExplorareEfectuataCommand(id));
            if (result.IsSuccess)
            {
                await LoadInvestigatiiAsync();
                await OnChanged.InvokeAsync();
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Eroare la stergerea explorarii efectuate {Id}", id);
        }
    }

    private async Task DeleteEndoscopieEfectuataAsync(Guid id)
    {
        try
        {
            var result = await Mediator.Send(new DeleteEndoscopieEfectuataCommand(id));
            if (result.IsSuccess)
            {
                await LoadInvestigatiiAsync();
                await OnChanged.InvokeAsync();
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Eroare la stergerea endoscopiei efectuate {Id}", id);
        }
    }

    // ==================== EDIT METHODS EFECTUATE ====================
    private void OpenEditImagisticaModal(InvestigatieImagisticaEfectuataDto inv)
    {
        _editModalType = "imagistica";
        _editModalTitle = $"Editare: {inv.DenumireInvestigatie}";
        _editInvestigatieId = inv.Id;
        _editRezultat = inv.Rezultat ?? string.Empty;
        _isEditModalVisible = true;
    }

    private void OpenEditExplorareModal(ExplorareEfectuataDto exp)
    {
        _editModalType = "explorare";
        _editModalTitle = $"Editare: {exp.DenumireExplorare}";
        _editInvestigatieId = exp.Id;
        _editRezultat = exp.Rezultat ?? string.Empty;
        _isEditModalVisible = true;
    }

    private void OpenEditEndoscopieModal(EndoscopieEfectuataDto endo)
    {
        _editModalType = "endoscopie";
        _editModalTitle = $"Editare: {endo.DenumireEndoscopie}";
        _editInvestigatieId = endo.Id;
        _editRezultat = endo.Rezultat ?? string.Empty;
        _isEditModalVisible = true;
    }

    private void CloseEditModal()
    {
        _isEditModalVisible = false;
        _editRezultat = string.Empty;
    }

    private async Task SaveEditAsync()
    {
        try
        {
            var userId = CurrentUserId ?? Guid.Empty;
            
            Result<bool> result = _editModalType switch
            {
                "imagistica" => await Mediator.Send(new UpdateInvestigatieImagisticaEfectuataCommand
                {
                    Id = _editInvestigatieId,
                    Rezultat = _editRezultat,
                    ModificatDe = userId
                }),
                "explorare" => await Mediator.Send(new UpdateExplorareEfectuataCommand
                {
                    Id = _editInvestigatieId,
                    Rezultat = _editRezultat,
                    ModificatDe = userId
                }),
                "endoscopie" => await Mediator.Send(new UpdateEndoscopieEfectuataCommand
                {
                    Id = _editInvestigatieId,
                    Rezultat = _editRezultat,
                    ModificatDe = userId
                }),
                _ => Result<bool>.Failure("Tip necunoscut")
            };

            if (result.IsSuccess)
            {
                await LoadInvestigatiiAsync();
                CloseEditModal();
                await OnChanged.InvokeAsync();
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Eroare la salvarea editării investigației {Id}", _editInvestigatieId);
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

    /// <summary>
    /// Handler pentru RichTextEditor ValueChange
    /// </summary>
    private async Task OnRteValueChanged(Syncfusion.Blazor.RichTextEditor.ChangeEventArgs args)
    {
        await OnFieldChanged();
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
