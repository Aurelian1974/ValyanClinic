using ValyanClinic.Domain.Models;
using ValyanClinic.Domain.Enums;
using PersonalModel = ValyanClinic.Domain.Models.Personal;

namespace ValyanClinic.Components.Pages.Administrare.Personal;

/// <summary>
/// State management class pentru pagina Personal
/// Centralizare state logic conform best practices
/// SIMPLIFIED VERSION - Removed kebab menu and advanced filtering
/// </summary>
public class PersonalPageState
{
    // Loading and error state
    public bool IsLoading { get; private set; }
    public bool HasError => !string.IsNullOrEmpty(ErrorMessage);
    public string? ErrorMessage { get; private set; }

    // Data
    public PersonalPagedResult? PagedResult { get; set; }
    public ValyanClinic.Application.Services.PersonalDropdownOptions? DropdownOptions { get; set; }

    // Modal state
    public bool IsModalVisible { get; set; }
    public PersonalModel? SelectedPersonal { get; set; }
    public bool IsAddEditModalVisible { get; set; }
    public PersonalModel? EditingPersonal { get; set; }
    public PersonalModel? SelectedPersonalForEdit { get; set; }
    public bool IsEditMode { get; set; }

    // Pagination
    public int CurrentPage { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public int[] PageSizes { get; } = { 10, 20, 50, 100 };

    // Basic filtering only
    public string? SearchText { get; set; }
    public string? SelectedDepartment { get; set; }
    public string? SelectedStatus { get; set; }
    public Departament? SelectedDepartmentFilter { get; set; }
    public StatusAngajat? SelectedStatusFilter { get; set; }

    // Status options
    public List<StatusItem> StatusOptions { get; } = new()
    {
        new("", "Toate statusurile"),
        new("Activ", "Activ"),
        new("Inactiv", "Inactiv")
    };

    // Computed properties - SIMPLIFIED
    public bool IsAnyFilterActive => 
        !string.IsNullOrEmpty(SearchText) || 
        !string.IsNullOrEmpty(SelectedDepartment) || 
        !string.IsNullOrEmpty(SelectedStatus);

    // State management methods
    public void SetLoading(bool loading)
    {
        IsLoading = loading;
        if (loading) ErrorMessage = null;
    }

    public void SetError(string error)
    {
        ErrorMessage = error;
        IsLoading = false;
    }

    public void ClearError()
    {
        ErrorMessage = null;
    }

    public void ClearFilters()
    {
        SearchText = null;
        SelectedDepartment = null;
        SelectedStatus = null;
        SelectedDepartmentFilter = null;
        SelectedStatusFilter = null;
        CurrentPage = 1;
    }

    // Modal management
    public string GetModalTitle() => IsEditMode ? "Editeaza Personal" : "Adauga Personal Nou";
    public string GetModalSubtitle() => IsEditMode 
        ? $"Modifica informatiile pentru {EditingPersonal?.NumeComplet}"
        : "Completeaza formularul pentru a adauga personal nou";
}

/// <summary>
/// Helper classes pentru dropdown-uri - COMPATIBILITATE
/// </summary>
public class StatusItem
{
    public StatusItem(string value, string text)
    {
        Value = value;
        Text = text;
    }

    public string Value { get; set; }
    public string Text { get; set; }
}

public class DropdownItem
{
    public DropdownItem(string value, string text)
    {
        Value = value;
        Text = text;
    }

    public string Value { get; set; }
    public string Text { get; set; }
}

/// <summary>
/// Models pentru rezultate paginare - COMPATIBILITATE
/// </summary>
public class PersonalPagedResult
{
    public IEnumerable<PersonalModel> Data { get; set; } = new List<PersonalModel>();
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
}
