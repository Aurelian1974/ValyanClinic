using ValyanClinic.Domain.Models;
using ValyanClinic.Domain.Enums;

namespace ValyanClinic.Components.Pages.UtilizatoriPage;

/// <summary>
/// State Management pentru Utilizatori page - ORGANIZAT ÎN FOLDER UtilizatoriPage
/// </summary>
public class UtilizatoriState
{
    // Modal State
    public bool IsModalVisible { get; set; } = false;
    public bool IsAddEditModalVisible { get; set; } = false;
    public bool IsEditMode { get; set; } = false;
    public User? SelectedUser { get; set; } = null;
    public User EditingUser { get; set; } = new();

    // Loading State
    public bool IsLoading { get; set; } = true;
    public string? ErrorMessage { get; set; }

    // Filter State
    public bool ShowAdvancedFilters { get; set; } = false;
    public UserRole? SelectedRoleFilter { get; set; } = null;
    public UserStatus? SelectedStatusFilter { get; set; } = null;
    public string SelectedDepartmentFilter { get; set; } = "";
    public string GlobalSearchText { get; set; } = "";
    public string SelectedActivityPeriod { get; set; } = "";

    // Computed Properties
    public bool HasError => !string.IsNullOrEmpty(ErrorMessage);
    public bool IsAnyFilterActive => 
        SelectedRoleFilter.HasValue ||
        SelectedStatusFilter.HasValue ||
        !string.IsNullOrEmpty(SelectedDepartmentFilter) ||
        !string.IsNullOrEmpty(GlobalSearchText) ||
        !string.IsNullOrEmpty(SelectedActivityPeriod);

    // State Management Methods
    public void SetError(string? error)
    {
        ErrorMessage = error;
    }

    public void ClearError()
    {
        ErrorMessage = null;
    }

    public void ClearFilters()
    {
        SelectedRoleFilter = null;
        SelectedStatusFilter = null;
        SelectedDepartmentFilter = "";
        GlobalSearchText = "";
        SelectedActivityPeriod = "";
    }

    public void ResetModalState()
    {
        IsModalVisible = false;
        IsAddEditModalVisible = false;
        IsEditMode = false;
        SelectedUser = null;
        EditingUser = new();
    }

    // Business Logic Methods
    public bool CanDeleteUser(User user)
    {
        // Business rule: Can't delete the last active admin
        return !(user.Role == UserRole.Administrator && user.Status == UserStatus.Active);
    }

    public bool CanEditUser(User user)
    {
        // Business rule: Can edit any user (for now)
        return true;
    }

    public string GetModalTitle()
    {
        if (!IsAddEditModalVisible) return "";
        
        return IsEditMode 
            ? $"Editeaza Utilizatorul - {EditingUser?.FullName}"
            : "Adauga Utilizator Nou";
    }

    public string GetModalSubtitle()
    {
        if (!IsAddEditModalVisible) return "";
        
        return IsEditMode 
            ? "Modifica informatiile utilizatorului existent"
            : "Completeaza informatiile pentru noul utilizator";
    }
}