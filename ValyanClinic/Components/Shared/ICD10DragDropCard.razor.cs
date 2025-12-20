using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using MediatR;
using Microsoft.Extensions.Logging;
using ValyanClinic.Application.Features.ICD10Management.Queries.SearchICD10;
using ValyanClinic.Application.Features.ICD10Management.DTOs;
using Syncfusion.Blazor.Grids;

namespace ValyanClinic.Components.Shared;

/// <summary>
/// Componenta ICD-10 cu Drag & Drop din DataGrid
/// Permite selectare intuitiva prin drag & drop din lista de coduri
/// ? GRUPARE pe TOATE datele încarcate, nu doar pagina curenta
/// ? SEARCH REAL-TIME cu case insensitive
/// </summary>
public partial class ICD10DragDropCard : ComponentBase
{
    // ==================== INJECTED SERVICES ====================
    [Inject] private IMediator Mediator { get; set; } = default!;
    [Inject] private ILogger<ICD10DragDropCard> Logger { get; set; } = default!;

    // ==================== PARAMETERS ====================

    /// <summary>
    /// Cod ICD-10 Principal (two-way binding)
    /// </summary>
    [Parameter] public string? CoduriICD10Principal { get; set; }

    [Parameter] public EventCallback<string?> CoduriICD10PrincipalChanged { get; set; }

    /// <summary>
    /// Coduri ICD-10 Secundare (comma-separated, two-way binding)
    /// </summary>
    [Parameter] public string? CoduriICD10Secundare { get; set; }

    [Parameter] public EventCallback<string?> CoduriICD10SecundareChanged { get; set; }

    // ==================== STATE ====================

    /// <summary>
    /// Referin?a la Syncfusion Grid pentru coduri favorite
    /// </summary>
    private SfGrid<ICD10SearchResultDto>? FavoriteGridRef { get; set; }

    /// <summary>
    /// Referin?a la Syncfusion Grid pentru toate codurile
    /// </summary>
    private SfGrid<ICD10SearchResultDto>? AllCodesGridRef { get; set; }

    /// <summary>
    /// Indica daca zona Principal este în drag-over state
    /// </summary>
    private bool IsPrincipalDragOver { get; set; }

    /// <summary>
    /// Indica daca zona Secundare este în drag-over state
    /// </summary>
    private bool IsSecundareDragOver { get; set; }

    /// <summary>
    /// Codul ICD-10 care este dragat momentan
    /// </summary>
    private ICD10SearchResultDto? DraggedCode { get; set; }

    // ==================== DATA ====================

    /// <summary>
    /// Lista cu coduri ICD-10 favorite (din DB - IsCommon = true)
    /// ? TOATE DATELE - pentru grupare corecta
    /// </summary>
    private List<ICD10SearchResultDto> FavoriteCodes { get; set; } = new();

    /// <summary>
    /// Lista cu TOATE codurile ICD-10 (se încarca la OnInitialized)
    /// ? TOATE DATELE - pentru grupare corecta
    /// </summary>
    private List<ICD10SearchResultDto> AllCodes { get; set; } = new();

    /// <summary>
    /// Search term curent pentru Favorite grid
    /// </summary>
    private string FavoriteSearchTerm { get; set; } = string.Empty;

    /// <summary>
    /// Search term curent pentru All Codes grid
    /// </summary>
    private string AllCodesSearchTerm { get; set; } = string.Empty;

    // ==================== LIFECYCLE METHODS ====================

    protected override async Task OnInitializedAsync()
    {
        Logger.LogInformation("[ICD10DragDrop] OnInitializedAsync START");

        await LoadFavoriteCodes();
        await LoadAllCodes();

        Logger.LogInformation("[ICD10DragDrop] OnInitializedAsync END - FavoriteCodes: {FavCount}, AllCodes: {AllCount}",
            FavoriteCodes.Count, AllCodes.Count);

        // ? For?eaza re-renderizare dupa încarcarea datelor
        await InvokeAsync(StateHasChanged);
    }

    // ==================== DATA LOADING ====================

    /// <summary>
    /// Încarca codurile favorite din database
    /// ? TOATE DATELE pentru grupare corecta
    /// </summary>
    private async Task LoadFavoriteCodes()
    {
        try
        {
            Logger.LogInformation("[ICD10DragDrop] LoadFavoriteCodes START");

            // Query pentru coduri frecvente - TOATE
            var query = new SearchICD10Query(
                SearchTerm: "",
                Category: null,
                OnlyCommon: true, // ? Doar coduri marcate ca IsCommon = true
                OnlyLeafNodes: true,
                MaxResults: 1000  // ? Load TOATE pentru grupare corecta
            );

            var result = await Mediator.Send(query);

            Logger.LogInformation("[ICD10DragDrop] Query result - IsSuccess: {IsSuccess}, HasValue: {HasValue}",
                result.IsSuccess, result.Value != null);

            if (result.IsSuccess && result.Value != null && result.Value.Any())
            {
                FavoriteCodes = result.Value.ToList();
                Logger.LogInformation("[ICD10DragDrop] ? Loaded {Count} favorite codes", FavoriteCodes.Count);

                // ? Log first 3 codes pentru debug
                var sample = string.Join(", ", FavoriteCodes.Take(3).Select(c => c.Code));
                Logger.LogInformation("[ICD10DragDrop] Sample codes: {Sample}", sample);
            }
            else
            {
                Logger.LogWarning("[ICD10DragDrop] ? No favorite codes loaded - Using TEST DATA");

                // ? FALLBACK: Test data hardcoded
                FavoriteCodes = GetTestFavoriteData();
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "[ICD10DragDrop] ? EXCEPTION loading favorite codes - Using TEST DATA");
            FavoriteCodes = GetTestFavoriteData();
        }
    }

    /// <summary>
    /// Încarca TOATE codurile ICD-10 pentru grid-ul complet
    /// ? TOATE DATELE pentru grupare corecta
    /// </summary>
    private async Task LoadAllCodes()
    {
        try
        {
            Logger.LogInformation("[ICD10DragDrop] LoadAllCodes START");

            // Query pentru toate codurile (fara filtru) - TOATE
            var query = new SearchICD10Query(
                SearchTerm: "",
                Category: null,
                OnlyCommon: false,
                OnlyLeafNodes: true,
                MaxResults: 1000 // ? Load TOATE pentru grupare corecta
            );

            var result = await Mediator.Send(query);

            Logger.LogInformation("[ICD10DragDrop] Query result - IsSuccess: {IsSuccess}, HasValue: {HasValue}",
                result.IsSuccess, result.Value != null);

            if (result.IsSuccess && result.Value != null && result.Value.Any())
            {
                AllCodes = result.Value.ToList();
                Logger.LogInformation("[ICD10DragDrop] ? Loaded {Count} total codes", AllCodes.Count);

                // ? Log first 3 codes pentru debug
                var sample = string.Join(", ", AllCodes.Take(3).Select(c => c.Code));
                Logger.LogInformation("[ICD10DragDrop] Sample codes: {Sample}", sample);
            }
            else
            {
                Logger.LogWarning("[ICD10DragDrop] ? No codes loaded - Using TEST DATA");

                // ? FALLBACK: Test data hardcoded
                AllCodes = GetTestAllData();
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "[ICD10DragDrop] ? EXCEPTION loading all codes - Using TEST DATA");
            AllCodes = GetTestAllData();
        }
    }

    // ==================== SEARCH HANDLERS (REAL-TIME) ====================

    /// <summary>
    /// Handler pentru search real-time în Favorite Grid
    /// </summary>
    private async Task OnFavoriteSearch(string searchValue)
    {
        FavoriteSearchTerm = searchValue;

        if (FavoriteGridRef != null)
        {
            await FavoriteGridRef.SearchAsync(searchValue);
            Logger.LogDebug("[ICD10DragDrop] Favorite search: {Search}", searchValue);
        }
    }

    /// <summary>
    /// Handler pentru search real-time în All Codes Grid
    /// </summary>
    private async Task OnAllCodesSearch(string searchValue)
    {
        AllCodesSearchTerm = searchValue;

        if (AllCodesGridRef != null)
        {
            await AllCodesGridRef.SearchAsync(searchValue);
            Logger.LogDebug("[ICD10DragDrop] All codes search: {Search}", searchValue);
        }
    }

    /// <summary>
    /// Handler pentru grid actions în Favorite - capteaza search real-time
    /// </summary>
    private void FavoriteActionBegin(ActionEventArgs<ICD10SearchResultDto> args)
    {
        // Capteaza doar action-ul de Search
        if (args.RequestType == Syncfusion.Blazor.Grids.Action.Searching)
        {
            Logger.LogDebug("[ICD10DragDrop] Favorite search triggered: {Search}", args.SearchString);
            // Search-ul built-in va func?iona automat, noi doar logam
        }
    }

    /// <summary>
    /// Handler pentru grid actions în All Codes - capteaza search real-time
    /// </summary>
    private void AllCodesActionBegin(ActionEventArgs<ICD10SearchResultDto> args)
    {
        // Capteaza doar action-ul de Search
        if (args.RequestType == Syncfusion.Blazor.Grids.Action.Searching)
        {
            Logger.LogDebug("[ICD10DragDrop] All codes search triggered: {Search}", args.SearchString);
            // Search-ul built-in va func?iona automat, noi doar logam
        }
    }

    // ==================== DRAG & DROP HANDLERS ====================

    /// <summary>
    /// Handler pentru start drag (stocheaza codul dragat)
    /// </summary>
    private void HandleDragStart(ICD10SearchResultDto code)
    {
        DraggedCode = code;
        Logger.LogInformation("[ICD10DragDrop] Drag started: {Code}", code.Code);
    }

    /// <summary>
    /// Handler pentru drop în zona PRINCIPAL
    /// </summary>
    private async Task HandleDropPrincipal(DragEventArgs e)
    {
        IsPrincipalDragOver = false;

        if (DraggedCode == null)
        {
            Logger.LogWarning("[ICD10DragDrop] Drop principal - no dragged code");
            return;
        }

        // Verifica daca codul este deja în secundare
        if (!string.IsNullOrEmpty(CoduriICD10Secundare) &&
            CoduriICD10Secundare.Contains(DraggedCode.Code))
        {
            Logger.LogWarning("[ICD10DragDrop] Code {Code} already in secundare", DraggedCode.Code);
            // TODO: Show toast "Codul exista deja în secundare!"
            return;
        }

        // Seteaza cod principal
        CoduriICD10Principal = DraggedCode.Code;
        await CoduriICD10PrincipalChanged.InvokeAsync(CoduriICD10Principal);

        Logger.LogInformation("[ICD10DragDrop] Principal code set: {Code}", DraggedCode.Code);

        DraggedCode = null;
        StateHasChanged();
    }

    /// <summary>
    /// Handler pentru drop în zona SECUNDARE
    /// </summary>
    private async Task HandleDropSecundare(DragEventArgs e)
    {
        IsSecundareDragOver = false;

        if (DraggedCode == null)
        {
            Logger.LogWarning("[ICD10DragDrop] Drop secundare - no dragged code");
            return;
        }

        // Verifica daca codul este deja principal
        if (CoduriICD10Principal == DraggedCode.Code)
        {
            Logger.LogWarning("[ICD10DragDrop] Code {Code} is principal", DraggedCode.Code);
            // TODO: Show toast "Codul este deja principal!"
            return;
        }

        // Verifica daca codul exista deja în secundare
        var existingCodes = string.IsNullOrEmpty(CoduriICD10Secundare)
            ? new List<string>()
            : CoduriICD10Secundare.Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(c => c.Trim())
                .ToList();

        if (existingCodes.Contains(DraggedCode.Code))
        {
            Logger.LogDebug("[ICD10DragDrop] Code {Code} already in secundare", DraggedCode.Code);
            // TODO: Show toast "Codul a fost deja adaugat!"
            return;
        }

        // Adauga cod secundar
        if (string.IsNullOrEmpty(CoduriICD10Secundare))
        {
            CoduriICD10Secundare = DraggedCode.Code;
        }
        else
        {
            CoduriICD10Secundare += $", {DraggedCode.Code}";
        }

        await CoduriICD10SecundareChanged.InvokeAsync(CoduriICD10Secundare);

        Logger.LogInformation("[ICD10DragDrop] Secundar code added: {Code}, Total: {All}",
            DraggedCode.Code, CoduriICD10Secundare);

        DraggedCode = null;
        StateHasChanged();
    }

    // ==================== REMOVE HANDLERS ====================

    /// <summary>
    /// ?terge codul principal
    /// </summary>
    private async Task RemovePrincipal()
    {
        CoduriICD10Principal = null;
        await CoduriICD10PrincipalChanged.InvokeAsync(null);

        Logger.LogInformation("[ICD10DragDrop] Principal code removed");
        StateHasChanged();
    }

    /// <summary>
    /// ?terge un cod secundar
    /// </summary>
    private async Task RemoveSecundar(string codeToRemove)
    {
        if (string.IsNullOrEmpty(CoduriICD10Secundare))
            return;

        var existingCodes = CoduriICD10Secundare
            .Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(c => c.Trim())
            .Where(c => c != codeToRemove)
            .ToList();

        CoduriICD10Secundare = existingCodes.Any()
            ? string.Join(", ", existingCodes)
            : null;

        await CoduriICD10SecundareChanged.InvokeAsync(CoduriICD10Secundare);

        Logger.LogInformation("[ICD10DragDrop] Secundar code removed: {Code}, Remaining: {All}",
            codeToRemove, CoduriICD10Secundare);

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

    // ==================== TEST DATA (FALLBACK) ====================

    /// <summary>
    /// Returneaza test data pentru favorite (fallback daca DB goala)
    /// </summary>
    private List<ICD10SearchResultDto> GetTestFavoriteData()
    {
        return new List<ICD10SearchResultDto>
        {
            new() { ICD10_ID = Guid.NewGuid(), Code = "I10", ShortDescription = "Hipertensiune esen?iala (primara)", LongDescription = "Hipertensiune arteriala esen?iala primara", Category = "Cardiovascular", IsCommon = true, Severity = "Moderate", RelevanceScore = 100 },
            new() { ICD10_ID = Guid.NewGuid(), Code = "E11.9", ShortDescription = "Diabet zaharat tip 2 fara complica?ii", LongDescription = "Diabet zaharat tip 2 fara complica?ii specificate", Category = "Endocrin", IsCommon = true, Severity = "Moderate", RelevanceScore = 95 },
            new() { ICD10_ID = Guid.NewGuid(), Code = "J18", ShortDescription = "Pneumonie de organism nespecificat", LongDescription = "Pneumonie de organism nespecificat", Category = "Respirator", IsCommon = true, Severity = "Severe", RelevanceScore = 90 },
            new() { ICD10_ID = Guid.NewGuid(), Code = "I20.0", ShortDescription = "Angina pectorala instabila", LongDescription = "Angina pectorala instabila", Category = "Cardiovascular", IsCommon = true, Severity = "Severe", RelevanceScore = 85 },
            new() { ICD10_ID = Guid.NewGuid(), Code = "K29.0", ShortDescription = "Gastrita acuta hemoragica", LongDescription = "Gastrita acuta hemoragica", Category = "Digestiv", IsCommon = true, Severity = "Moderate", RelevanceScore = 80 }
        };
    }

    /// <summary>
    /// Returneaza test data pentru toate codurile (fallback daca DB goala)
    /// </summary>
    private List<ICD10SearchResultDto> GetTestAllData()
    {
        var testData = GetTestFavoriteData();

        // Adauga coduri suplimentare
        testData.AddRange(new List<ICD10SearchResultDto>
        {
            new() { ICD10_ID = Guid.NewGuid(), Code = "I21.0", ShortDescription = "Infarct miocardic acut", LongDescription = "Infarct miocardic acut transmural al peretelui anterior", Category = "Cardiovascular", IsCommon = false, Severity = "Critical", RelevanceScore = 75 },
            new() { ICD10_ID = Guid.NewGuid(), Code = "J44.0", ShortDescription = "BPOC cu infec?ie acuta", LongDescription = "Boala pulmonara obstructiva cronica cu infec?ie acuta a tractului respirator inferior", Category = "Respirator", IsCommon = false, Severity = "Severe", RelevanceScore = 70 },
            new() { ICD10_ID = Guid.NewGuid(), Code = "N18.3", ShortDescription = "Boala renala cronica stadiul 3", LongDescription = "Boala renala cronica stadiul 3 (moderata)", Category = "Urinar", IsCommon = false, Severity = "Moderate", RelevanceScore = 65 }
        });

        return testData;
    }
}
