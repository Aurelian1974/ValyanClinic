using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Syncfusion.Blazor.Grids;
using Syncfusion.Blazor.Notifications;
using Syncfusion.Blazor.Popups;
using Syncfusion.Blazor.DropDowns;
using Syncfusion.Blazor.Inputs;
using ValyanClinic.Application.Services;
using ValyanClinic.Application.Models;
using ValyanClinic.Domain.Models;
using ValyanClinic.Domain.Enums;

namespace ValyanClinic.Components.Pages.UtilizatoriPage;

/// <summary>
/// Business Logic pentru Utilizatori.razor - ORGANIZAT iN FOLDER UtilizatoriPage
/// Separated Business Logic pentru gestionarea utilizatorilor
/// </summary>
public partial class Utilizatori : ComponentBase
{
    [Inject] private IUserManagementService UserService { get; set; } = default!;
    [Inject] private IJSRuntime JSRuntime { get; set; } = default!;

    // Component References
    private SfGrid<User>? GridRef;
    private SfToast? ToastRef;
    private SfDialog? UserDetailModal;
    private SfDialog? AddEditUserModal;

    // State Management in acelai folder
    private UtilizatoriState _state = new();
    private UtilizatoriModels _models = new();

    // Dialog Animation Settings
    private DialogAnimationSettings DialogAnimation = new()
    {
        Effect = DialogEffect.FadeZoom,
        Duration = 300
    };

    protected override async Task OnInitializedAsync()
    {
        Console.WriteLine("DEBUG: Utilizatori component initializing...");
        await LoadUsers();
        InitializeFilterOptions();
        InitializeFormOptions();
        Console.WriteLine("DEBUG: Utilizatori component initialization complete");
    }

    #region Data Loading

    private async Task LoadUsers()
    {
        try
        {
            _state.IsLoading = true;
            StateHasChanged();

            // incarc toi utilizatorii prin search cu parametri implicii
            var searchRequest = new UserSearchRequest(
                SearchTerm: "",
                Role: null,
                Status: null,
                Department: null,
                DaysInactive: null,
                PageNumber: 1,
                PageSize: 100 // Toate
            );
            
            var searchResult = await UserService.SearchUsersAsync(searchRequest);
            _models.SetUsers(searchResult.Users);
            _models.CalculateStatistics();

            Console.WriteLine($"DEBUG: Loaded {searchResult.Users.Count} users with statistics");
            _state.SetError(null);
            StateHasChanged();
        }
        catch (Exception ex)
        {
            var errorMessage = "Nu s-au putut incrca utilizatorii";
            Console.WriteLine($"Error loading users: {ex.Message}");
            _state.SetError(errorMessage);
            await ShowToast("Eroare", errorMessage, "e-toast-danger");
        }
        finally
        {
            _state.IsLoading = false;
            StateHasChanged();
        }
    }

    private async Task RefreshData()
    {
        await LoadUsers();

        if (GridRef != null)
        {
            await GridRef.Refresh();
        }

        await ShowToast("Succes", "Datele au fost actualizate", "e-toast-success");
    }

    #endregion

    #region Filter Logic

    private void InitializeFilterOptions()
    {
        _models.InitializeFilterOptions();
    }

    private void ToggleFilterPanel()
    {
        _state.ShowAdvancedFilters = !_state.ShowAdvancedFilters;
        StateHasChanged();
    }

    private async Task OnRoleFilterChanged(ChangeEventArgs<UserRole?, UtilizatoriModels.FilterOption<UserRole?>> args)
    {
        _state.SelectedRoleFilter = args.Value;
        await ApplyAdvancedFilters();
    }

    private async Task OnStatusFilterChanged(ChangeEventArgs<UserStatus?, UtilizatoriModels.FilterOption<UserStatus?>> args)
    {
        _state.SelectedStatusFilter = args.Value;
        await ApplyAdvancedFilters();
    }

    private async Task OnDepartmentFilterChanged(ChangeEventArgs<string, string> args)
    {
        _state.SelectedDepartmentFilter = args.Value ?? "";
        await ApplyAdvancedFilters();
    }

    private async Task OnGlobalSearchChanged(ChangedEventArgs args)
    {
        _state.GlobalSearchText = args.Value ?? "";
        await ApplyAdvancedFilters();
    }

    private async Task OnActivityPeriodChanged(ChangeEventArgs<string, string> args)
    {
        _state.SelectedActivityPeriod = args.Value ?? "";
        await ApplyAdvancedFilters();
    }

    private async Task ApplyAdvancedFilters()
    {
        try
        {
            var filteredUsers = _models.ApplyFilters(_state);

            if (GridRef != null)
            {
                GridRef.DataSource = filteredUsers;
                await GridRef.Refresh();
            }

            await ShowToast("Filtru aplicat",
                $"Gasite {filteredUsers.Count} rezultate din {_models.Users.Count} utilizatori",
                "e-toast-info");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error applying filters: {ex.Message}");
            await ShowToast("Eroare", "Eroare la aplicarea filtrelor", "e-toast-danger");
        }
    }

    private async Task ClearAdvancedFilters()
    {
        _state.ClearFilters();

        if (GridRef != null)
        {
            GridRef.DataSource = _models.Users;
            await GridRef.Refresh();
        }

        await ShowToast("Filtre curatate", "Toate filtrele au fost eliminate", "e-toast-success");
        StateHasChanged();
    }

    private async Task ExportFilteredData()
    {
        try
        {
            await ShowToast("Export", "Functia de export va fi implementata in viitor", "e-toast-info");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Export error: {ex.Message}");
            await ShowToast("Eroare Export", "Eroare la exportul datelor", "e-toast-danger");
        }
    }

    #endregion

    #region User Detail Modal - RESTORED WITH SEPARATE COMPONENT

    private async Task ShowUserDetailModal(User user)
    {
        try
        {
            Console.WriteLine($"DEBUG: Opening modal for user {user.FullName}");
            _state.SelectedUser = user;
            _state.IsModalVisible = true;
            StateHasChanged();

            await ShowToast("Detalii", $"Afiare detalii pentru {user.FullName}", "e-toast-info");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"ERROR: Error showing user detail modal: {ex.Message}");
            await ShowToast("Eroare", "Eroare la afiarea detaliilor", "e-toast-danger");
        }
    }

    private async Task CloseUserDetailModal()
    {
        try
        {
            _state.IsModalVisible = false;
            _state.SelectedUser = null;
            StateHasChanged();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error closing modal: {ex.Message}");
            _state.IsModalVisible = false;
            _state.SelectedUser = null;
            StateHasChanged();
        }
    }

    private async Task EditUserFromModal()
    {
        if (_state.SelectedUser != null)
        {
            var userToEdit = _state.SelectedUser;
            await CloseUserDetailModal();
            await Task.Delay(200);
            await ShowEditUserModal(userToEdit);
        }
    }

    private void OnModalClosed()
    {
        _state.IsModalVisible = false;
        _state.SelectedUser = null;
        StateHasChanged();
    }

    #endregion

    #region Add/Edit User Modal - RESTORED WITH SEPARATE COMPONENT

    private void InitializeFormOptions()
    {
        _models.InitializeFormOptions();
    }

    private async Task ShowAddUserModal()
    {
        try
        {
            Console.WriteLine("DEBUG: Opening Add User Modal");
            _state.IsEditMode = false;
            _state.EditingUser = _models.CreateNewUser();
            _state.IsAddEditModalVisible = true;
            StateHasChanged();

            await ShowToast("Nou utilizator", "Completeaz? formularul pentru a aduga un utilizator nou", "e-toast-info");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"ERROR: Error showing add user modal: {ex.Message}");
            await ShowToast("Eroare", "Eroare la deschiderea formularului de adugare", "e-toast-danger");
        }
    }

    private async Task ShowEditUserModal(User user)
    {
        try
        {
            _state.IsEditMode = true;
            _state.EditingUser = _models.CloneUser(user);
            _state.SelectedUserForEdit = user; // Pstrm utilizatorul original pentru referin??
            _state.IsAddEditModalVisible = true;
            StateHasChanged();

            await ShowToast("Editare utilizator", $"Modificai informaiile pentru {user.FullName}", "e-toast-info");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error showing edit user modal: {ex.Message}");
            await ShowToast("Eroare", "Eroare la deschiderea formularului de editare", "e-toast-danger");
        }
    }

    private async Task CloseAddEditModal()
    {
        try
        {
            _state.IsAddEditModalVisible = false;
            _state.EditingUser = new();
            _state.IsEditMode = false;
            StateHasChanged();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error closing add/edit modal: {ex.Message}");
            _state.IsAddEditModalVisible = false;
            _state.EditingUser = new();
            _state.IsEditMode = false;
            StateHasChanged();
        }
    }

    private void OnAddEditModalClosed()
    {
        _state.IsAddEditModalVisible = false;
        _state.EditingUser = new();
        _state.IsEditMode = false;
        StateHasChanged();
    }

    private async Task SaveUser()
    {
        try
        {
            _state.IsLoading = true;
            StateHasChanged();

            if (_state.IsEditMode)
            {
                // TODO: Implement actual update logic
                await ShowToast("Actualizare", $"Utilizatorul {_state.EditingUser.FullName} a fost actualizat cu succes", "e-toast-success");
            }
            else
            {
                // TODO: Implement actual create logic  
                await ShowToast("Creare", $"Utilizatorul {_state.EditingUser.FullName} a fost creat cu succes", "e-toast-success");
            }

            await Task.Delay(1000); // Simulate processing time
            await CloseAddEditModal();
            await LoadUsers(); // Refresh the grid
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error saving user: {ex.Message}");
            await ShowToast("Eroare", $"Eroare la salvarea utilizatorului: {ex.Message}", "e-toast-danger");
        }
        finally
        {
            _state.IsLoading = false;
            StateHasChanged();
        }
    }

    // Metod? pentru a fi apelat? de butonul submit din footer
    private async Task OnFormSubmit()
    {
        // Trigger form validation and submit from the child component
        await JSRuntime.InvokeVoidAsync("document.getElementById('addEditUserForm').requestSubmit");
    }

    #endregion

    #region User Actions

    private async Task EditUser(User user)
    {
        try
        {
            await ShowEditUserModal(user);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error editing user: {ex.Message}");
            await ShowToast("Eroare", "Eroare la editarea utilizatorului", "e-toast-danger");
        }
    }

    private async Task DeleteUser(User user)
    {
        try
        {
            var confirmDelete = await JSRuntime.InvokeAsync<bool>("confirm",
                $"Sigur doriti sa stergeti utilizatorul {user.FullName}?");

            if (confirmDelete)
            {
                await ShowToast("Stergere", $"Utilizatorul {user.FullName} va fi sters", "e-toast-info");
                // TODO: Implement actual delete logic
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error deleting user: {ex.Message}");
            await ShowToast("Eroare", "Eroare la stergerea utilizatorului", "e-toast-danger");
        }
    }

    #endregion

    #region Grid Events

    public void RowSelected(RowSelectEventArgs<User> args) { }
    public void RowDeselected(RowDeselectEventArgs<User> args) { }

    #endregion

    #region Display Helper Methods

    private string GetRoleDisplayName(UserRole role) => role switch
    {
        UserRole.Administrator => "Administrator",
        UserRole.Doctor => "Doctor",
        UserRole.Nurse => "Asistent medical",
        UserRole.Receptionist => "Recepioner",
        UserRole.Operator => "Operator",
        UserRole.Manager => "Manager",
        _ => "Necunoscut"
    };

    private string GetStatusDisplayName(UserStatus status) => status switch
    {
        UserStatus.Active => "Activ",
        UserStatus.Inactive => "Inactiv",
        UserStatus.Suspended => "Suspendat",
        UserStatus.Locked => "Blocat",
        _ => "Necunoscut"
    };

    private string GetActivityStatus(DateTime? lastLogin)
    {
        if (lastLogin == null) return "Niciodata conectat";

        var daysSinceLogin = (DateTime.Now - lastLogin.Value).Days;
        return daysSinceLogin switch
        {
            0 => "Activ astazi",
            1 => "Activ ieri",
            <= 7 => $"Activ acum {daysSinceLogin} zile",
            <= 30 => $"Activ acum {daysSinceLogin / 7} saptamani",
            <= 365 => $"Activ acum {daysSinceLogin / 30} luni",
            _ => $"Inactiv de peste {daysSinceLogin / 365} ani"
        };
    }

    private string GetSystemAge(DateTime createdDate)
    {
        var age = DateTime.Now - createdDate;
        if (age.TotalDays < 1) return "Sub 24 ore";
        if (age.TotalDays < 30) return $"{age.Days} zile";
        if (age.TotalDays < 365) return $"{age.Days / 30} luni";
        return $"{age.Days / 365} ani";
    }

    #endregion

    #region Toast Notifications

    private async Task ShowToast(string title, string content, string cssClass)
    {
        if (ToastRef != null)
        {
            var toastModel = new ToastModel()
            {
                Title = title,
                Content = content,
                CssClass = cssClass,
                ShowCloseButton = true,
                Timeout = 3000
            };
            await ToastRef.ShowAsync(toastModel);
        }
    }

    #endregion
}