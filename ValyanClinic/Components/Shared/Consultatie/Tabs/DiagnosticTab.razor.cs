using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using MediatR;
using ValyanClinic.Application.Features.ConsultatieManagement.Commands.CreateConsultatie;
using ValyanClinic.Application.Features.ICD10Management.DTOs;
using ValyanClinic.Application.Features.ICD10Management.Queries.SearchICD10;

namespace ValyanClinic.Components.Shared.Consultatie.Tabs;

/// <summary>
/// Tab pentru Diagnostic - cu căutare inline ICD-10 (fără drag & drop)
/// </summary>
public partial class DiagnosticTab : ComponentBase, IDisposable
{
    [Inject] private ILogger<DiagnosticTab> Logger { get; set; } = default!;
    [Inject] private IMediator Mediator { get; set; } = default!;

    [Parameter] public CreateConsultatieCommand Model { get; set; } = new();
    [Parameter] public EventCallback OnChanged { get; set; }
    [Parameter] public EventCallback OnSectionCompleted { get; set; }
    [Parameter] public bool ShowValidation { get; set; } = false;

    // Search state pentru Principal
    private string SearchTermPrincipal { get; set; } = string.Empty;
    private bool IsSearchingPrincipal { get; set; }
    private List<ICD10SearchResultDto> SearchResultsPrincipal { get; set; } = new();
    private System.Timers.Timer? _debounceTimerPrincipal;

    // Search state pentru Secundar
    private string SearchTermSecundar { get; set; } = string.Empty;
    private bool IsSearchingSecundar { get; set; }
    private List<ICD10SearchResultDto> SearchResultsSecundar { get; set; } = new();
    private System.Timers.Timer? _debounceTimerSecundar;

    private bool IsComplete => !string.IsNullOrWhiteSpace(Model.DiagnosticPozitiv);

    protected override void OnInitialized()
    {
        // Setup debounce timers
        _debounceTimerPrincipal = new System.Timers.Timer(300);
        _debounceTimerPrincipal.Elapsed += async (s, e) => await ExecuteSearchPrincipalAsync();
        _debounceTimerPrincipal.AutoReset = false;

        _debounceTimerSecundar = new System.Timers.Timer(300);
        _debounceTimerSecundar.Elapsed += async (s, e) => await ExecuteSearchSecundarAsync();
        _debounceTimerSecundar.AutoReset = false;
    }

    private async Task OnFieldChanged()
    {
        await OnChanged.InvokeAsync();

        if (IsComplete)
        {
            await OnSectionCompleted.InvokeAsync();
        }
    }

    // ==================== PRINCIPAL SEARCH ====================

    private void OnSearchPrincipalAsync()
    {
        _debounceTimerPrincipal?.Stop();
        
        if (string.IsNullOrWhiteSpace(SearchTermPrincipal) || SearchTermPrincipal.Length < 2)
        {
            SearchResultsPrincipal.Clear();
            return;
        }
        
        IsSearchingPrincipal = true;
        _debounceTimerPrincipal?.Start();
    }

    private async Task ExecuteSearchPrincipalAsync()
    {
        try
        {
            var query = new SearchICD10Query(SearchTermPrincipal, MaxResults: 10);
            var result = await Mediator.Send(query);
            
            if (result.IsSuccess && result.Value != null)
            {
                SearchResultsPrincipal = result.Value.ToList();
            }
            else
            {
                SearchResultsPrincipal.Clear();
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "[DiagnosticTab] Error searching ICD-10 Principal");
            SearchResultsPrincipal.Clear();
        }
        finally
        {
            IsSearchingPrincipal = false;
            await InvokeAsync(StateHasChanged);
        }
    }

    private async Task SelectICD10Principal(ICD10SearchResultDto item)
    {
        Model.CoduriICD10 = item.Code;
        SearchTermPrincipal = string.Empty;
        SearchResultsPrincipal.Clear();
        Logger.LogInformation("[DiagnosticTab] Selected Principal ICD-10: {Code}", item.Code);
        await OnFieldChanged();
    }

    private async Task ClearICD10Principal()
    {
        Model.CoduriICD10 = null;
        await OnFieldChanged();
    }

    // ==================== SECUNDAR SEARCH ====================

    private void OnSearchSecundarAsync()
    {
        _debounceTimerSecundar?.Stop();
        
        if (string.IsNullOrWhiteSpace(SearchTermSecundar) || SearchTermSecundar.Length < 2)
        {
            SearchResultsSecundar.Clear();
            return;
        }
        
        IsSearchingSecundar = true;
        _debounceTimerSecundar?.Start();
    }

    private async Task ExecuteSearchSecundarAsync()
    {
        try
        {
            var query = new SearchICD10Query(SearchTermSecundar, MaxResults: 10);
            var result = await Mediator.Send(query);
            
            if (result.IsSuccess && result.Value != null)
            {
                SearchResultsSecundar = result.Value.ToList();
            }
            else
            {
                SearchResultsSecundar.Clear();
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "[DiagnosticTab] Error searching ICD-10 Secundar");
            SearchResultsSecundar.Clear();
        }
        finally
        {
            IsSearchingSecundar = false;
            await InvokeAsync(StateHasChanged);
        }
    }

    private async Task SelectICD10Secundar(ICD10SearchResultDto item)
    {
        Model.CoduriICD10Secundare = item.Code;
        SearchTermSecundar = string.Empty;
        SearchResultsSecundar.Clear();
        Logger.LogInformation("[DiagnosticTab] Selected Secundar ICD-10: {Code}", item.Code);
        await OnFieldChanged();
    }

    private async Task ClearICD10Secundar()
    {
        Model.CoduriICD10Secundare = null;
        await OnFieldChanged();
    }

    // ==================== DISPOSE ====================

    public void Dispose()
    {
        _debounceTimerPrincipal?.Stop();
        _debounceTimerPrincipal?.Dispose();
        _debounceTimerSecundar?.Stop();
        _debounceTimerSecundar?.Dispose();
    }
}
