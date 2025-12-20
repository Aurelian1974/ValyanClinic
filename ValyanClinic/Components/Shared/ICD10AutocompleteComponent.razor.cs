using Microsoft.AspNetCore.Components;
using MediatR;
using Microsoft.Extensions.Logging;
using ValyanClinic.Application.Features.ICD10Management.Queries.SearchICD10;
using ValyanClinic.Application.Features.ICD10Management.DTOs;

namespace ValyanClinic.Components.Shared;

/// <summary>
/// Code-behind pentru componenta ICD10 Autocomplete
/// Implementeaza cautare inteligenta cu debounce ?i highlight
/// </summary>
public partial class ICD10AutocompleteComponent : ComponentBase, IDisposable
{
    // ==================== INJECTED SERVICES ====================
    [Inject] private IMediator Mediator { get; set; } = default!;
    [Inject] private ILogger<ICD10AutocompleteComponent> Logger { get; set; } = default!;

    // ==================== PARAMETERS ====================

    /// <summary>
    /// Label-ul afi?at deasupra input-ului
    /// </summary>
    [Parameter] public string Label { get; set; } = "Cod ICD-10";

    /// <summary>
    /// Placeholder text în input
    /// </summary>
    [Parameter] public string Placeholder { get; set; } = "Cauta dupa cod sau descriere...";

    /// <summary>
    /// Text de ajutor afi?at sub input
    /// </summary>
    [Parameter] public string? HelpText { get; set; }

    /// <summary>
    /// Indica daca câmpul este obligatoriu (afi?eaza *)
    /// </summary>
    [Parameter] public bool IsRequired { get; set; }

    /// <summary>
    /// Filtreaza dupa categorie specifica (ex: "Cardiovascular")
    /// </summary>
    [Parameter] public string? Category { get; set; }

    /// <summary>
    /// Afi?eaza doar coduri frecvente (IsCommon = true)
    /// </summary>
    [Parameter] public bool OnlyCommon { get; set; }

    /// <summary>
    /// Numar maxim de rezultate afi?ate în dropdown
    /// </summary>
    [Parameter] public int MaxResults { get; set; } = 10;

    /// <summary>
    /// Lungime minima text pentru a declan?a cautarea
    /// </summary>
    [Parameter] public int MinSearchLength { get; set; } = 2;

    /// <summary>
    /// Delay în milisecunde pentru debounce cautare
    /// </summary>
    [Parameter] public int SearchDebounceMs { get; set; } = 300;

    // ==================== TWO-WAY BINDING ====================

    /// <summary>
    /// Codul ICD-10 selectat (two-way binding)
    /// </summary>
    [Parameter] public string? SelectedCode { get; set; }

    /// <summary>
    /// Event callback pentru schimbarea codului selectat
    /// </summary>
    [Parameter] public EventCallback<string?> SelectedCodeChanged { get; set; }

    /// <summary>
    /// Event callback când un cod este selectat (returneaza DTO complet)
    /// </summary>
    [Parameter] public EventCallback<ICD10SearchResultDto?> OnCodeSelected { get; set; }

    // ==================== STATE ====================

    /// <summary>
    /// Text curent din input de cautare
    /// </summary>
    private string SearchText { get; set; } = string.Empty;

    /// <summary>
    /// Indica daca dropdown-ul este deschis
    /// </summary>
    private bool IsOpen { get; set; }

    /// <summary>
    /// Indica daca se efectueaza o cautare (loading state)
    /// </summary>
    private bool IsSearching { get; set; }

    /// <summary>
    /// Indica daca input-ul are focus
    /// </summary>
    private bool IsFocused { get; set; }

    // ==================== DATA ====================

    /// <summary>
    /// Lista de rezultate din cautare
    /// </summary>
    private List<ICD10SearchResultDto> Results { get; set; } = new();

    /// <summary>
    /// Rezultatul selectat curent (pentru highlight în dropdown)
    /// </summary>
    private ICD10SearchResultDto? SelectedResult { get; set; }

    // ==================== TIMERS ====================

    /// <summary>
    /// Timer pentru debounce cautare
    /// </summary>
    private System.Threading.Timer? _debounceTimer;

    /// <summary>
    /// Timer pentru delay blur (pentru a permite click pe rezultate)
    /// </summary>
    private System.Threading.Timer? _blurTimer;

    // ==================== LIFECYCLE METHODS ====================

    /// <summary>
    /// Se apeleaza când parametrii se schimba
    /// Pre-populeaza search text daca exista un cod selectat
    /// </summary>
    protected override void OnParametersSet()
    {
        if (!string.IsNullOrEmpty(SelectedCode) && string.IsNullOrEmpty(SearchText))
        {
            SearchText = SelectedCode;
        }
    }

    // ==================== EVENT HANDLERS ====================

    /// <summary>
    /// Handler pentru input change cu debounce
    /// </summary>
    private async Task HandleSearchInput(ChangeEventArgs e)
    {
        SearchText = e.Value?.ToString() ?? string.Empty;

        // Dispose timer anterior daca exista
        _debounceTimer?.Dispose();

        // Verifica lungime minima
        if (string.IsNullOrWhiteSpace(SearchText) || SearchText.Length < MinSearchLength)
        {
            Results.Clear();
            IsOpen = false;
            StateHasChanged();
            return;
        }

        // Creeaza timer nou pentru debounce
        _debounceTimer = new System.Threading.Timer(async _ =>
        {
            await InvokeAsync(async () =>
            {
                await PerformSearch();
            });
        }, null, SearchDebounceMs, Timeout.Infinite);
    }

    /// <summary>
    /// Efectueaza cautarea efectiva prin MediatR
    /// </summary>
    private async Task PerformSearch()
    {
        try
        {
            IsSearching = true;
            StateHasChanged();

            Logger.LogInformation("[ICD10Autocomplete] Searching for: {SearchTerm}", SearchText);

            var query = new SearchICD10Query(
                SearchText,
                Category,
                OnlyCommon,
                OnlyLeafNodes: true,
                MaxResults: 50 // Get more results, apoi le filtram
            );

            var result = await Mediator.Send(query);

            if (result.IsSuccess && result.Value != null)
            {
                Results = result.Value;
                IsOpen = Results.Any();

                Logger.LogInformation("[ICD10Autocomplete] Found {Count} results", Results.Count);
            }
            else
            {
                Results.Clear();
                IsOpen = false;
                Logger.LogWarning("[ICD10Autocomplete] Search failed: {Errors}", string.Join(", ", result.Errors ?? new List<string>()));
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "[ICD10Autocomplete] Error searching ICD-10");
            Results.Clear();
            IsOpen = false;
        }
        finally
        {
            IsSearching = false;
            StateHasChanged();
        }
    }

    /// <summary>
    /// Handler pentru selectarea unui rezultat din dropdown
    /// </summary>
    private async Task SelectResult(ICD10SearchResultDto result)
    {
        SelectedResult = result;
        SelectedCode = result.Code;
        IsOpen = false;

        // Notify parent component
        await SelectedCodeChanged.InvokeAsync(result.Code);
        await OnCodeSelected.InvokeAsync(result);

        Logger.LogInformation("[ICD10Autocomplete] Selected code: {Code}", result.Code);

        // ? RESETEAZA SearchText pentru a permite adaugarea urmatorului cod
        // For?eaza UI update prin InvokeAsync + StateHasChanged
        await InvokeAsync(() =>
        {
            SearchText = string.Empty;
            Results.Clear();
            SelectedResult = null;
            StateHasChanged();
        });
    }

    /// <summary>
    /// Handler pentru focus pe input
    /// </summary>
    private void HandleFocus()
    {
        IsFocused = true;
        _blurTimer?.Dispose();

        // Redeschide dropdown daca exista rezultate
        if (!string.IsNullOrEmpty(SearchText) && Results.Any())
        {
            IsOpen = true;
            StateHasChanged();
        }
    }

    /// <summary>
    /// Handler pentru blur pe input (cu delay pentru click pe rezultate)
    /// </summary>
    private void HandleBlur()
    {
        _blurTimer = new System.Threading.Timer(_ =>
        {
            InvokeAsync(() =>
            {
                IsFocused = false;
                IsOpen = false;
                StateHasChanged();
            });
        }, null, 200, Timeout.Infinite); // 200ms delay pentru a permite click pe rezultate
    }

    /// <summary>
    /// ?terge cautarea ?i reseteaza starea
    /// </summary>
    private void ClearSearch()
    {
        SearchText = string.Empty;
        SelectedCode = null;
        SelectedResult = null;
        Results.Clear();
        IsOpen = false;

        SelectedCodeChanged.InvokeAsync(null);
        OnCodeSelected.InvokeAsync(null);

        StateHasChanged();
    }

    // ==================== HELPER METHODS ====================

    /// <summary>
    /// Converte?te severitatea din engleza în româna
    /// </summary>
    private string GetSeverityText(string? severity)
    {
        return severity switch
        {
            "Mild" => "U?oara",
            "Moderate" => "Moderata",
            "Severe" => "Severa",
            "Critical" => "Critica",
            _ => severity ?? ""
        };
    }

    /// <summary>
    /// Eviden?iaza termenul de cautare în text cu tag mark
    /// </summary>
    private MarkupString HighlightSearchTerm(string text, string searchTerm)
    {
        if (string.IsNullOrEmpty(searchTerm) || string.IsNullOrEmpty(text))
            return (MarkupString)text;

        var index = text.IndexOf(searchTerm, StringComparison.OrdinalIgnoreCase);
        if (index < 0)
            return (MarkupString)text;

        var before = text.Substring(0, index);
        var match = text.Substring(index, searchTerm.Length);
        var after = text.Substring(index + searchTerm.Length);

        return (MarkupString)$"{before}<mark>{match}</mark>{after}";
    }

    // ==================== DISPOSE ====================

    /// <summary>
    /// Cleanup resources
    /// </summary>
    public void Dispose()
    {
        _debounceTimer?.Dispose();
        _blurTimer?.Dispose();
    }
}
