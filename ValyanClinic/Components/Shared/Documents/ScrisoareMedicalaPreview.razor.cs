using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using ValyanClinic.Application.Features.ConsultatieManagement.DTOs;
using ValyanClinic.Application.Services.ScrisoareMedicala;

namespace ValyanClinic.Components.Shared.Documents;

/// <summary>
/// Code-behind pentru componenta Scrisoare Medicală Preview
/// Gestionează afișarea, printarea și exportul PDF al scrisorii medicale
/// </summary>
public partial class ScrisoareMedicalaPreview : ComponentBase
{
    #region Injected Services

    [Inject] private IJSRuntime JSRuntime { get; set; } = default!;
    [Inject] private IScrisoareMedicalaService ScrisoareService { get; set; } = default!;

    #endregion

    #region Parameters

    /// <summary>
    /// Controlează vizibilitatea componentei
    /// </summary>
    [Parameter] public bool IsVisible { get; set; }

    /// <summary>
    /// Callback pentru închiderea componentei
    /// </summary>
    [Parameter] public EventCallback OnClose { get; set; }

    /// <summary>
    /// ID-ul consultației pentru care se generează scrisoarea (opțional)
    /// </summary>
    [Parameter] public Guid? ConsultatieId { get; set; }

    /// <summary>
    /// DTO-ul consultației pentru preview (opțional, pentru draft)
    /// </summary>
    [Parameter] public ConsulatieDetailDto? ConsultatieData { get; set; }

    /// <summary>
    /// Dacă true, folosește date mock pentru preview/demo
    /// </summary>
    [Parameter] public bool UseMockData { get; set; } = false;

    #endregion

    #region State

    private ScrisoareMedicalaDto Model { get; set; } = new();
    private bool IsLoading { get; set; } = false;

    #endregion

    #region Lifecycle

    protected override async Task OnParametersSetAsync()
    {
        if (IsVisible)
        {
            await LoadDataAsync();
        }
    }

    #endregion

    #region Data Loading

    private async Task LoadDataAsync()
    {
        IsLoading = true;
        StateHasChanged();

        try
        {
            if (UseMockData)
            {
                // Folosește date mock pentru demo
                Model = ScrisoareService.GenerateMockData();
            }
            else if (ConsultatieData != null)
            {
                // Generează din DTO-ul consultației (preview draft)
                var result = await ScrisoareService.GenerateFromDraftAsync(ConsultatieData);
                if (result.IsSuccess)
                {
                    Model = result.Value!;
                }
                else
                {
                    // Fallback la date mock în caz de eroare
                    Model = ScrisoareService.GenerateMockData();
                }
            }
            else if (ConsultatieId.HasValue)
            {
                // Încarcă din baza de date
                var result = await ScrisoareService.GenerateFromConsultatieAsync(ConsultatieId.Value);
                if (result.IsSuccess)
                {
                    Model = result.Value!;
                }
                else
                {
                    // Fallback la date mock în caz de eroare
                    Model = ScrisoareService.GenerateMockData();
                }
            }
            else
            {
                // Default: date mock
                Model = ScrisoareService.GenerateMockData();
            }
        }
        finally
        {
            IsLoading = false;
            StateHasChanged();
        }
    }

    #endregion

    #region Event Handlers

    private async Task Close()
    {
        await OnClose.InvokeAsync();
    }

    private void HandleOverlayClick()
    {
        // Nu închidem la click pe overlay - documentul trebuie citit
    }

    private async Task HandlePrint()
    {
        // Adaugă clasa pe body pentru a ascunde restul paginii la print
        await JSRuntime.InvokeVoidAsync("eval", "document.body.classList.add('printing-scrisoare')");
        
        // Așteaptă puțin pentru aplicarea stilurilor
        await Task.Delay(100);
        
        // Printează
        await JSRuntime.InvokeVoidAsync("window.print");
        
        // Scoate clasa după print
        await JSRuntime.InvokeVoidAsync("eval", "document.body.classList.remove('printing-scrisoare')");
    }

    private async Task HandleSavePdf()
    {
        // Generează nume fișier cu data și numele pacientului
        var fileName = $"ScrisoareMedicala_{Model.PacientNumeComplet?.Replace(" ", "_")}_{DateTime.Now:yyyyMMdd}.pdf";
        
        // Apelează funcția JavaScript pentru generare PDF
        await JSRuntime.InvokeVoidAsync("generateScrisoarePdf", "scrisoare-print-area", fileName);
    }

    #endregion
}
