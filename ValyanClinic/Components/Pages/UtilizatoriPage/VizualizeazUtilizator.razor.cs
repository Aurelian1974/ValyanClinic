using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Syncfusion.Blazor.Notifications;
using ValyanClinic.Application.Services;
using ValyanClinic.Domain.Models;
using ValyanClinic.Domain.Enums;

namespace ValyanClinic.Components.Pages.UtilizatoriPage;

public partial class VizualizeazUtilizator : ComponentBase
{
    [Parameter] public int UserId { get; set; }
    [Inject] private IUserManagementService UserService { get; set; } = default!;
    [Inject] private NavigationManager Navigation { get; set; } = default!;
    [Inject] private IJSRuntime JSRuntime { get; set; } = default!;

    private User? User { get; set; }
    private bool IsLoading { get; set; } = true;
    private bool HasError { get; set; }
    private string ErrorMessage { get; set; } = string.Empty;

    private SfToast? ToastRef;

    protected override async Task OnInitializedAsync()
    {
        try
        {
            // For now, create a dummy user for viewing
            User = new User
            {
                Id = UserId,
                FirstName = "Maria",
                LastName = "Constantinescu",
                Email = "maria.constantinescu@valyanmed.ro",
                Username = "maria.constantinescu",
                Phone = "0729012345",
                Role = UserRole.Doctor,
                Status = UserStatus.Active,
                Department = "Cardiologie",
                JobTitle = "Medic Specialist Cardiolog",
                CreatedDate = DateTime.Now.AddMonths(-12),
                LastLoginDate = DateTime.Now.AddHours(-2)
            };
        }
        catch (Exception ex)
        {
            HasError = true;
            ErrorMessage = $"Eroare la înc?rcarea datelor: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task EditUser()
    {
        if (User != null)
        {
            var editUrl = $"/utilizatori/editeaza/{User.Id}";
            await JSRuntime.InvokeVoidAsync("window.open", editUrl, "_blank", "width=900,height=700,scrollbars=yes,resizable=yes");
        }
    }

    private async Task CloseWindow()
    {
        await JSRuntime.InvokeVoidAsync("window.close");
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

    private string GetActivityStatus(DateTime? lastLogin)
    {
        if (!lastLogin.HasValue) return "Niciodat? conectat";

        var daysSince = (DateTime.Now - lastLogin.Value).Days;
        return daysSince switch
        {
            0 => "Activ ast?zi",
            1 => "Activ ieri",
            <= 7 => $"Activ acum {daysSince} zile",
            <= 30 => $"Activ acum {daysSince} zile",
            _ => "Inactiv de mult timp"
        };
    }

    private string GetSystemAge(DateTime createdDate)
    {
        var age = DateTime.Now - createdDate;
        if (age.Days < 30)
            return $"{age.Days} zile";
        else if (age.Days < 365)
            return $"{age.Days / 30} luni";
        else
            return $"{age.Days / 365} ani";
    }
}