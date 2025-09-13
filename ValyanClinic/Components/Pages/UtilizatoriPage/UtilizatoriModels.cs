using ValyanClinic.Domain.Models;
using ValyanClinic.Domain.Enums;

namespace ValyanClinic.Components.Pages.UtilizatoriPage;

/// <summary>
/// Page-specific models pentru Utilizatori - ORGANIZAT ÎN FOLDER UtilizatoriPage
/// Optimizat pentru C# 13 & .NET 9
/// </summary>
public class UtilizatoriModels
{
    // Main Data
    public List<User> Users { get; private set; } = [];
    public List<User> FilteredUsers { get; private set; } = [];
    
    // Statistics
    public List<UserStatistic> UserStatistics { get; private set; } = [];
    
    // Page Configuration
    public string[] PageSizes { get; } = ["10", "20", "50", "100", "All"];
    
    // Filter Options
    public List<FilterOption<UserRole?>> RoleFilterOptions { get; private set; } = [];
    public List<FilterOption<UserStatus?>> StatusFilterOptions { get; private set; } = [];
    public List<string> DepartmentFilterOptions { get; private set; } = [];
    
    // Form Options
    public List<UserRole> AllRoles { get; private set; } = [];
    public List<UserStatus> AllStatuses { get; private set; } = [];
    public List<string> DepartmentOptions { get; private set; } = [];
    
    // Activity Period Options
    public List<string> ActivityPeriodOptions { get; } =
    [
        "Ultima saptamana",
        "Ultima luna", 
        "Ultimele 3 luni",
        "Ultimele 6 luni",
        "Ultimul an",
        "Niciodata conectati"
    ];

    #region Data Management

    public void SetUsers(List<User> users)
    {
        Users = users ?? [];
        FilteredUsers = Users;
    }

    public User CreateNewUser()
    {
        return new User
        {
            Status = UserStatus.Active,
            CreatedDate = DateTime.Now,
            Role = UserRole.Operator
        };
    }

    public User CloneUser(User source)
    {
        return new User
        {
            Id = source.Id,
            FirstName = source.FirstName,
            LastName = source.LastName,
            Email = source.Email,
            Username = source.Username,
            Phone = source.Phone,
            Role = source.Role,
            Status = source.Status,
            Department = source.Department,
            JobTitle = source.JobTitle,
            CreatedDate = source.CreatedDate,
            LastLoginDate = source.LastLoginDate
        };
    }

    #endregion

    #region Statistics

    public void CalculateStatistics()
    {
        UserStatistics =
        [
            new() { Label = "Total Utilizatori", Value = Users.Count },
            new() { Label = "Utilizatori Activi", Value = Users.Count(u => u.Status == UserStatus.Active) },
            new() { Label = "Doctori", Value = Users.Count(u => u.Role == UserRole.Doctor) },
            new() { Label = "Activi saptamana aceasta", Value = Users.Count(u => u.LastLoginDate >= DateTime.Now.AddDays(-7)) },
            new() { Label = "Asistente Medicale", Value = Users.Count(u => u.Role == UserRole.Nurse) },
            new() { Label = "Administratori", Value = Users.Count(u => u.Role == UserRole.Administrator) },
            new() { Label = "Manageri", Value = Users.Count(u => u.Role == UserRole.Manager) },
            new() { Label = "Suspendati", Value = Users.Count(u => u.Status == UserStatus.Suspended) }
        ];
    }

    #endregion

    #region Filter Logic

    public void InitializeFilterOptions()
    {
        // Role Filter Options
        RoleFilterOptions = [new() { Text = "Toate rolurile", Value = null }];
        RoleFilterOptions.AddRange(Enum.GetValues<UserRole>().Select(r => 
            new FilterOption<UserRole?> { Text = GetRoleDisplayName(r), Value = r }));

        // Status Filter Options
        StatusFilterOptions = [new() { Text = "Toate statusurile", Value = null }];
        StatusFilterOptions.AddRange(Enum.GetValues<UserStatus>().Select(s => 
            new FilterOption<UserStatus?> { Text = GetStatusDisplayName(s), Value = s }));

        // Department Filter Options
        DepartmentFilterOptions =
        [
            "", "Cardiologie", "Neurologie", "Pediatrie", "Chirurgie", 
            "Radiologie", "Laborator", "Administrare", "Management"
        ];
    }

    public void InitializeFormOptions()
    {
        AllRoles = [.. Enum.GetValues<UserRole>()];
        AllStatuses = [.. Enum.GetValues<UserStatus>()];
        DepartmentOptions =
        [
            "Cardiologie", "Neurologie", "Pediatrie", "Chirurgie", "Radiologie", 
            "Laborator", "Administrare", "Management", "Urgente", "Ginecologie"
        ];
    }

    public List<User> ApplyFilters(UtilizatoriState state)
    {
        var query = Users.AsQueryable();

        // Apply role filter
        if (state.SelectedRoleFilter.HasValue)
        {
            query = query.Where(u => u.Role == state.SelectedRoleFilter.Value);
        }

        // Apply status filter
        if (state.SelectedStatusFilter.HasValue)
        {
            query = query.Where(u => u.Status == state.SelectedStatusFilter.Value);
        }

        // Apply department filter
        if (!string.IsNullOrEmpty(state.SelectedDepartmentFilter))
        {
            query = query.Where(u => u.Department == state.SelectedDepartmentFilter);
        }

        // Apply global search
        if (!string.IsNullOrEmpty(state.GlobalSearchText))
        {
            var searchTerm = state.GlobalSearchText;
            query = query.Where(u => 
                u.FirstName.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                u.LastName.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                u.Email.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                u.Username.Contains(searchTerm, StringComparison.OrdinalIgnoreCase));
        }

        // Apply activity period filter
        if (!string.IsNullOrEmpty(state.SelectedActivityPeriod))
        {
            query = query.Where(u => FilterByActivityPeriod(u, state.SelectedActivityPeriod));
        }

        FilteredUsers = [.. query.Distinct()];
        return FilteredUsers;
    }

    private static bool FilterByActivityPeriod(User user, string period)
    {
        if (user.LastLoginDate == null) 
            return period == "Niciodata conectati";

        var now = DateTime.Now;
        return period switch
        {
            "Ultima saptamana" => user.LastLoginDate >= now.AddDays(-7),
            "Ultima luna" => user.LastLoginDate >= now.AddMonths(-1),
            "Ultimele 3 luni" => user.LastLoginDate >= now.AddMonths(-3),
            "Ultimele 6 luni" => user.LastLoginDate >= now.AddMonths(-6),
            "Ultimul an" => user.LastLoginDate >= now.AddYears(-1),
            "Niciodata conectati" => false,
            _ => true
        };
    }

    #endregion

    #region Helper Methods

    private static string GetRoleDisplayName(UserRole role) => role switch
    {
        UserRole.Administrator => "Administrator",
        UserRole.Doctor => "Doctor",
        UserRole.Nurse => "Asistent medical",
        UserRole.Receptionist => "Recep?ioner",
        UserRole.Operator => "Operator",
        UserRole.Manager => "Manager",
        _ => "Necunoscut"
    };

    private static string GetStatusDisplayName(UserStatus status) => status switch
    {
        UserStatus.Active => "Activ",
        UserStatus.Inactive => "Inactiv",
        UserStatus.Suspended => "Suspendat",
        UserStatus.Locked => "Blocat",
        _ => "Necunoscut"
    };

    #endregion

    #region Supporting Classes

    public class FilterOption<T>
    {
        public required string Text { get; init; }
        public required T Value { get; init; }
    }

    public class UserStatistic
    {
        public required string Label { get; init; }
        public required int Value { get; init; }
        
        public string DisplayText => $"{Label}: {Value}";
        public bool IsHighValue => Value > 10;
        public string CssClass => IsHighValue ? "stat-high" : "stat-normal";
    }

    #endregion
}

// Extension methods pentru enhanced functionality
public static class UtilizatoriModelsExtensions
{
    public static IEnumerable<User> GetActiveUsers(this UtilizatoriModels models) =>
        models.Users.Where(u => u.Status == UserStatus.Active);
    
    public static IEnumerable<User> GetUsersByRole(this UtilizatoriModels models, UserRole role) =>
        models.Users.Where(u => u.Role == role);
    
    public static IEnumerable<User> GetRecentlyActiveUsers(this UtilizatoriModels models, int days = 7) =>
        models.Users.Where(u => u.LastLoginDate >= DateTime.Now.AddDays(-days));
    
    public static string GetStatisticsSummary(this UtilizatoriModels models)
    {
        var activeCount = models.Users.Count(u => u.Status == UserStatus.Active);
        var totalCount = models.Users.Count;
        var percentage = totalCount > 0 ? (activeCount * 100.0 / totalCount) : 0;
        
        return $"{activeCount} activi din {totalCount} ({percentage:F0}%)";
    }

    public static UtilizatoriModels.UserStatistic? GetStatisticByLabel(this UtilizatoriModels models, string label) =>
        models.UserStatistics.FirstOrDefault(s => s.Label.Equals(label, StringComparison.OrdinalIgnoreCase));
}