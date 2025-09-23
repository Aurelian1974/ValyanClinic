using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Syncfusion.Blazor.Notifications;
using ValyanClinic.Application.Services;
using ValyanClinic.Domain.Models;
using ValyanClinic.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace ValyanClinic.Components.Pages.UtilizatoriPage;

public partial class AdaugaEditezUtilizator : ComponentBase
{
    [Parameter] public int? UserId { get; set; }
    [Parameter] public User? EditingUser { get; set; }
    [Parameter] public EventCallback OnSave { get; set; }
    [Parameter] public EventCallback OnCancel { get; set; }
    
    [Inject] private IUserManagementService UserService { get; set; } = default!;
    [Inject] private NavigationManager Navigation { get; set; } = default!;
    [Inject] private IJSRuntime JSRuntime { get; set; } = default!;

    private User CurrentUser { get; set; } = new();
    private bool IsLoading { get; set; } = true;
    private bool HasError { get; set; }
    private string ErrorMessage { get; set; } = string.Empty;
    private bool IsEditMode => UserId.HasValue && EditingUser != null;

    // Options for dropdowns
    private List<UserRole> AllRoles => Enum.GetValues<UserRole>().ToList();
    private List<UserStatus> AllStatuses => Enum.GetValues<UserStatus>().ToList();
    private List<string> DepartmentOptions => new()
    {
        "Administrare", "Cardiologie", "Chirurgie", "Laborator",
        "Neurologie", "Pediatrie", "Radiologie"
    };

    protected override async Task OnInitializedAsync()
    {
        await LoadUserData();
    }

    protected override async Task OnParametersSetAsync()
    {
        await LoadUserData();
    }

    private async Task LoadUserData()
    {
        try
        {
            IsLoading = true;
            HasError = false;

            if (IsEditMode && EditingUser != null)
            {
                // Folosim datele reale ale utilizatorului pentru editare
                CurrentUser = new User
                {
                    Id = EditingUser.Id,
                    FirstName = EditingUser.FirstName,
                    LastName = EditingUser.LastName,
                    Email = EditingUser.Email,
                    Username = EditingUser.Username,
                    Phone = EditingUser.Phone,
                    Role = EditingUser.Role,
                    Status = EditingUser.Status,
                    Department = EditingUser.Department,
                    JobTitle = EditingUser.JobTitle,
                    CreatedDate = EditingUser.CreatedDate,
                    LastLoginDate = EditingUser.LastLoginDate
                };
            }
            else
            {
                // Nou utilizator - valori implicite
                CurrentUser = new User
                {
                    Status = UserStatus.Active,
                    Role = UserRole.Receptionist,
                    CreatedDate = DateTime.Now
                };
            }
        }
        catch (Exception ex)
        {
            HasError = true;
            ErrorMessage = $"Eroare la înc?rcarea datelor: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
            StateHasChanged();
        }
    }

    private async Task SaveUser()
    {
        try
        {
            IsLoading = true;

            if (IsEditMode)
            {
                // TODO: Implement actual update logic with CurrentUser data
                Console.WriteLine($"Updating user: {CurrentUser.FullName}");
            }
            else
            {
                // TODO: Implement actual create logic with CurrentUser data
                Console.WriteLine($"Creating user: {CurrentUser.FullName}");
            }

            // Simulate API call
            await Task.Delay(500);

            // Notify parent component
            await OnSave.InvokeAsync();
        }
        catch (Exception ex)
        {
            HasError = true;
            ErrorMessage = $"Eroare la salvarea utilizatorului: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
            StateHasChanged();
        }
    }

    private async Task CancelEdit()
    {
        await OnCancel.InvokeAsync();
    }

    private string GetRoleDisplayName(UserRole role)
    {
        return role switch
        {
            UserRole.Administrator => "Administrator",
            UserRole.Doctor => "Doctor",
            UserRole.Nurse => "Asistent Medical",
            UserRole.Receptionist => "Recep?ioner",
            UserRole.Manager => "Manager",
            UserRole.Operator => "Operator",
            _ => role.ToString()
        };
    }

    private string GetStatusDisplayName(UserStatus status)
    {
        return status switch
        {
            UserStatus.Active => "Activ",
            UserStatus.Inactive => "Inactiv",
            UserStatus.Suspended => "Suspendat",
            _ => status.ToString()
        };
    }
}