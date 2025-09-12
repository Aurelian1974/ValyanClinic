using System.ComponentModel.DataAnnotations;

namespace ValyanClinic.Components.Pages.Models;

public class PatientListModel
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public DateTime LastVisit { get; set; }
    public PatientStatus Status { get; set; }
    public int Age { get; set; }
    public string CNP { get; set; } = string.Empty;
}

public class PatientSearchRequest
{
    public string SearchTerm { get; set; } = string.Empty;
    public FilterOptions StatusFilter { get; set; } = FilterOptions.All;
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}

public class PatientFilterState
{
    public string SearchTerm { get; set; } = string.Empty;
    public string SelectedStatus { get; set; } = string.Empty;
    public bool HasActiveFilters => !string.IsNullOrEmpty(SearchTerm) || !string.IsNullOrEmpty(SelectedStatus);
}