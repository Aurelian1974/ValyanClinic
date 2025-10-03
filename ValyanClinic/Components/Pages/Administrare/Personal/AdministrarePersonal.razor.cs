using Microsoft.AspNetCore.Components;
using Syncfusion.Blazor.Grids;
using Syncfusion.Blazor.Notifications;
using MediatR;
using ValyanClinic.Application.Features.PersonalManagement.Queries.GetPersonalList;
using Microsoft.Extensions.Logging;

namespace ValyanClinic.Components.Pages.Administrare.Personal;

public partial class AdministrarePersonal : ComponentBase
{
    [Inject] private IMediator Mediator { get; set; } = default!;
    [Inject] private ILogger<AdministrarePersonal> Logger { get; set; } = default!;
    [Inject] private NavigationManager NavigationManager { get; set; } = default!;

    // Toast reference
    private SfToast? ToastRef;

    // Data lists
    private List<PersonalListDto> AllPersonalList { get; set; } = new();
    private List<PersonalListDto> PagedPersonalList { get; set; } = new();
    private PersonalListDto? SelectedPersonal { get; set; }

    // State
    private bool IsLoading { get; set; } = true;
    private bool HasError { get; set; } = false;
    private string? ErrorMessage { get; set; }

    // Paging properties
    private int CurrentPageSize = 20;
    private int currentPage = 1;
    private int jumpToPageNumber = 1;

    // Page size options
    private List<PageSizeOption> PageSizeOptions = new()
    {
        new() { Text = "10", Value = 10 },
        new() { Text = "20", Value = 20 },
        new() { Text = "50", Value = 50 },
        new() { Text = "100", Value = 100 }
    };

    // Toast properties
    private string ToastTitle { get; set; } = string.Empty;
    private string ToastContent { get; set; } = string.Empty;
    private string ToastCssClass { get; set; } = string.Empty;

    public class PageSizeOption
    {
        public string Text { get; set; } = string.Empty;
        public int Value { get; set; }
    }

    protected override async Task OnInitializedAsync()
    {
        Logger.LogInformation("Initializare pagina Administrare Personal");
        await LoadData();
    }

    private async Task LoadData()
    {
        try
        {
            IsLoading = true;
            HasError = false;
            StateHasChanged();

            Logger.LogInformation("Incarcare date personal...");

            var query = new GetPersonalListQuery();
            var result = await Mediator.Send(query);

            if (result.IsSuccess)
            {
                AllPersonalList = result.Value?.ToList() ?? new List<PersonalListDto>();
                UpdatePagedData();
                Logger.LogInformation("Date incarcate cu succes: {Count} angajati", AllPersonalList.Count);
            }
            else
            {
                HasError = true;
                ErrorMessage = string.Join(", ", result.Errors ?? new List<string> { "Eroare necunoscuta" });
                Logger.LogWarning("Eroare la incarcarea datelor: {Message}", ErrorMessage);
            }
        }
        catch (Exception ex)
        {
            HasError = true;
            ErrorMessage = $"Eroare neasteptata: {ex.Message}";
            Logger.LogError(ex, "Eroare la incarcarea datelor personal");
        }
        finally
        {
            IsLoading = false;
            StateHasChanged();
        }
    }

    private void UpdatePagedData()
    {
        var startIndex = (currentPage - 1) * CurrentPageSize;
        PagedPersonalList = AllPersonalList.Skip(startIndex).Take(CurrentPageSize).ToList();
        Logger.LogInformation("Paginare: Pagina {Page}, Dimensiune {Size}, Total {Total}", 
            currentPage, CurrentPageSize, AllPersonalList.Count);
    }

    private async Task HandleRefresh()
    {
        Logger.LogInformation("Refresh date personal");
        await LoadData();
        await ShowToast("Succes", "Datele au fost reincarcate cu succes", "e-toast-success");
    }

    private void HandleAddNew()
    {
        Logger.LogInformation("Navigare catre adaugare personal");
        NavigationManager.NavigateTo("/administrare/personal/adauga");
    }

    #region Paging Methods

    private void OnPageSizeChanged(int newPageSize)
    {
        Logger.LogInformation("Schimbare dimensiune pagina: {OldSize} -> {NewSize}", CurrentPageSize, newPageSize);
        CurrentPageSize = newPageSize;
        currentPage = 1;
        jumpToPageNumber = 1;
        UpdatePagedData();
        StateHasChanged();
    }

    private int GetCurrentPage() => currentPage;

    private int GetTotalPages()
    {
        if (AllPersonalList.Count == 0 || CurrentPageSize == 0) return 1;
        return (int)Math.Ceiling((double)AllPersonalList.Count / CurrentPageSize);
    }

    private int GetDisplayedRecordsStart()
    {
        if (AllPersonalList.Count == 0) return 0;
        return (currentPage - 1) * CurrentPageSize + 1;
    }

    private int GetDisplayedRecordsEnd()
    {
        var end = currentPage * CurrentPageSize;
        return Math.Min(end, AllPersonalList.Count);
    }

    private int GetPagerStart()
    {
        var totalPages = GetTotalPages();
        var start = Math.Max(1, currentPage - 2);
        if (totalPages <= 5) return 1;
        if (currentPage >= totalPages - 2) return Math.Max(1, totalPages - 4);
        return start;
    }

    private int GetPagerEnd()
    {
        var totalPages = GetTotalPages();
        if (totalPages <= 5) return totalPages;
        if (currentPage <= 3) return Math.Min(5, totalPages);
        return Math.Min(totalPages, currentPage + 2);
    }

    private void GoToPage(int pageNumber)
    {
        if (pageNumber >= 1 && pageNumber <= GetTotalPages() && pageNumber != currentPage)
        {
            Logger.LogInformation("Navigare la pagina {Page}", pageNumber);
            currentPage = pageNumber;
            jumpToPageNumber = pageNumber;
            UpdatePagedData();
            StateHasChanged();
        }
    }

    private void JumpToPage()
    {
        if (jumpToPageNumber >= 1 && jumpToPageNumber <= GetTotalPages())
        {
            GoToPage(jumpToPageNumber);
        }
    }

    private void OnJumpToPageKeyPress(Microsoft.AspNetCore.Components.Web.KeyboardEventArgs e)
    {
        if (e.Key == "Enter")
        {
            JumpToPage();
        }
    }

    #endregion

    private void OnRowSelected(RowSelectEventArgs<PersonalListDto> args)
    {
        SelectedPersonal = args.Data;
        Logger.LogInformation("Personal selectat: {PersonalId}", SelectedPersonal?.Id_Personal);
    }

    private async Task OnCommandClicked(CommandClickEventArgs<PersonalListDto> args)
    {
        var personal = args.RowData;
        var commandName = args.CommandColumn?.ButtonOption?.Content ?? "";

        Logger.LogInformation("Comanda executata: {Command} pentru {PersonalId}", 
            commandName, personal.Id_Personal);

        switch (commandName)
        {
            case "Vizualizeaza":
                await HandleView(personal);
                break;
            case "Editeaza":
                await HandleEdit(personal);
                break;
            case "Sterge":
                await HandleDelete(personal);
                break;
        }
    }

    private async Task HandleView(PersonalListDto personal)
    {
        Logger.LogInformation("Vizualizare personal: {PersonalId}", personal.Id_Personal);
        NavigationManager.NavigateTo($"/administrare/personal/vizualizeaza/{personal.Id_Personal}");
        await Task.CompletedTask;
    }

    private async Task HandleEdit(PersonalListDto personal)
    {
        Logger.LogInformation("Editare personal: {PersonalId}", personal.Id_Personal);
        NavigationManager.NavigateTo($"/administrare/personal/editeaza/{personal.Id_Personal}");
        await Task.CompletedTask;
    }

    private async Task HandleDelete(PersonalListDto personal)
    {
        Logger.LogInformation("Stergere personal: {PersonalId}", personal.Id_Personal);
        
        await ShowToast("Atentie", 
            $"Functionalitatea de stergere pentru {personal.NumeComplet} va fi implementata", 
            "e-toast-warning");
    }

    private async Task ShowToast(string title, string content, string cssClass)
    {
        ToastTitle = title;
        ToastContent = content;
        ToastCssClass = cssClass;

        if (ToastRef != null)
        {
            await ToastRef.ShowAsync();
        }
    }
}
