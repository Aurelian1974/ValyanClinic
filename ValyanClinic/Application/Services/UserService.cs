using ValyanClinic.Components.Pages.Models;
using ValyanClinic.Application.Common;
using ValyanClinic.Application.Exceptions;
using Microsoft.Extensions.Logging;

namespace ValyanClinic.Application.Services;

public interface IUserService
{
    Task<Result<List<User>>> GetAllUsersAsync();
    Task<Result<User>> GetUserByIdAsync(int id);
    Task<Result<User>> CreateUserAsync(User user, string createdBy);
    Task<Result<User>> UpdateUserAsync(User user, string modifiedBy);
    Task<Result> DeleteUserAsync(int id, string deletedBy);
    Task<Result<List<User>>> SearchUsersAsync(string searchTerm);
    Task<Result<List<User>>> GetUsersByRoleAsync(UserRole role);
    Task<Result<List<User>>> GetUsersByStatusAsync(UserStatus status);
    Task<Result<List<User>>> GetUsersByDepartmentAsync(string department);
    Task<Result<UserStatistics>> GetUserStatisticsAsync();
    Task<Result<bool>> IsUsernameAvailableAsync(string username, int? excludeUserId = null);
    Task<Result<bool>> IsEmailAvailableAsync(string email, int? excludeUserId = null);
    Task<Result> ValidateUserBusinessRulesAsync(User user, bool isUpdate = false);
}

/// <summary>
/// Rich User Service cu business logic avansat și Result Pattern
/// Nu mai este simple pass-through - conține reguli de business
/// </summary>
public class UserService : IUserService
{
    private static List<User> _users = GenerateDummyUsers();
    private readonly ILogger<UserService> _logger;
    
    // Business constants
    private const int MIN_USERNAME_LENGTH = 3;
    private const int MAX_USERNAME_LENGTH = 30;
    private const int MIN_PASSWORD_LENGTH = 8;
    private const int MAX_ADMINS_ALLOWED = 5;

    public UserService(ILogger<UserService> logger)
    {
        _logger = logger;
    }
    
    public async Task<Result<List<User>>> GetAllUsersAsync()
    {
        try
        {
            _logger.LogInformation("Getting all users - Total count: {Count}", _users.Count);
            await Task.Delay(200); // Simulate database delay
            
            // Business logic: sort by status (Active first), then by role priority, then by name
            var sortedUsers = _users
                .OrderBy(u => u.Status != UserStatus.Active ? 1 : 0)
                .ThenBy(u => GetRolePriority(u.Role))
                .ThenBy(u => u.LastName)
                .ThenBy(u => u.FirstName)
                .ToList();
            
            return Result<List<User>>.Success(sortedUsers, $"Loaded {sortedUsers.Count} users successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all users");
            return Result<List<User>>.Failure("Failed to load users");
        }
    }
    
    public async Task<Result<User>> GetUserByIdAsync(int id)
    {
        try
        {
            _logger.LogInformation("Getting user by ID: {UserId}", id);
            await Task.Delay(100);
            
            var user = _users.FirstOrDefault(u => u.Id == id);
            if (user == null)
            {
                _logger.LogWarning("User not found: {UserId}", id);
                return Result<User>.Failure($"User with ID {id} not found");
            }
            
            return Result<User>.Success(user);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user by ID: {UserId}", id);
            return Result<User>.Failure("Failed to get user");
        }
    }
    
    public async Task<Result<User>> CreateUserAsync(User user, string createdBy)
    {
        try
        {
            _logger.LogInformation("Creating user: {Username}", user.Username);
            
            // Apply business rules for creation
            var businessValidation = await ValidateUserBusinessRulesAsync(user, false);
            if (!businessValidation.IsSuccess)
                return Result<User>.Failure(businessValidation.Errors);

            // Check uniqueness constraints
            var uniquenessCheck = await ValidateUniquenessConstraintsAsync(user, null);
            if (!uniquenessCheck.IsSuccess)
                return Result<User>.Failure(uniquenessCheck.Errors);

            // Apply creation business rules
            user = ApplyCreationBusinessRules(user, createdBy);
            
            await Task.Delay(200);
            _users.Add(user);
            
            // Business logic: send welcome notification for important roles
            if (user.Role == UserRole.Doctor || user.Role == UserRole.Administrator)
            {
                await SendWelcomeNotificationAsync(user);
            }
            
            _logger.LogInformation("User created successfully: {UserId} - {Username}", user.Id, user.Username);
            return Result<User>.Success(user, $"User {user.FullName} created successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating user: {Username}", user.Username);
            return Result<User>.Failure($"Failed to create user: {ex.Message}");
        }
    }
    
    public async Task<Result<User>> UpdateUserAsync(User user, string modifiedBy)
    {
        try
        {
            _logger.LogInformation("Updating user: {UserId}", user.Id);
            
            var existingUser = _users.FirstOrDefault(u => u.Id == user.Id);
            if (existingUser == null)
            {
                return Result<User>.Failure($"User with ID {user.Id} not found");
            }

            // Apply business rules for update
            var businessValidation = await ValidateUserBusinessRulesAsync(user, true);
            if (!businessValidation.IsSuccess)
                return Result<User>.Failure(businessValidation.Errors);

            // Check uniqueness constraints (excluding current user)
            var uniquenessCheck = await ValidateUniquenessConstraintsAsync(user, user.Id);
            if (!uniquenessCheck.IsSuccess)
                return Result<User>.Failure(uniquenessCheck.Errors);

            // Business rule: preserve critical data
            user = ApplyUpdateBusinessRules(user, existingUser, modifiedBy);
            
            await Task.Delay(200);
            var index = _users.FindIndex(u => u.Id == user.Id);
            _users[index] = user;
            
            _logger.LogInformation("User updated successfully: {UserId} - {Username}", user.Id, user.Username);
            return Result<User>.Success(user, $"User {user.FullName} updated successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating user: {UserId}", user.Id);
            return Result<User>.Failure($"Failed to update user: {ex.Message}");
        }
    }
    
    public async Task<Result> DeleteUserAsync(int id, string deletedBy)
    {
        try
        {
            _logger.LogInformation("Deleting user: {UserId}", id);
            
            var user = _users.FirstOrDefault(u => u.Id == id);
            if (user == null)
            {
                return Result.Failure($"User with ID {id} not found");
            }

            // Business rule: cannot delete the last administrator
            if (user.Role == UserRole.Administrator)
            {
                var adminCount = _users.Count(u => u.Role == UserRole.Administrator && u.Status == UserStatus.Active);
                if (adminCount <= 1)
                {
                    return Result.Failure("Cannot delete the last administrator in the system");
                }
            }

            // Business rule: cannot delete users with recent activity (last 7 days)
            if (user.LastLoginDate.HasValue && user.LastLoginDate.Value > DateTime.Now.AddDays(-7))
            {
                return Result.Failure("Cannot delete users who have been active in the last 7 days");
            }

            await Task.Delay(150);
            _users.Remove(user);
            
            // Business logic: log critical deletion
            await LogCriticalActionAsync(deletedBy, "DELETE_USER", $"Deleted user {user.FullName} (ID: {id})");
            
            _logger.LogInformation("User deleted successfully: {UserId} - {Username}", id, user.Username);
            return Result.Success($"User {user.FullName} deleted successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting user: {UserId}", id);
            return Result.Failure($"Failed to delete user: {ex.Message}");
        }
    }

    public async Task<Result<List<User>>> SearchUsersAsync(string searchTerm)
    {
        try
        {
            _logger.LogInformation("Searching users with term: {SearchTerm}", searchTerm);
            await Task.Delay(200);
            
            if (string.IsNullOrWhiteSpace(searchTerm))
                return await GetAllUsersAsync();

            var results = _users.Where(u => 
                u.FullName.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                u.Email.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                u.Username.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                (u.Phone?.Contains(searchTerm) ?? false) ||
                (u.Department?.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ?? false) ||
                (u.JobTitle?.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ?? false)
            ).ToList();

            // Business logic: prioritize active users in search results
            results = results
                .OrderBy(u => u.Status != UserStatus.Active ? 1 : 0)
                .ThenBy(u => u.LastName)
                .ToList();

            return Result<List<User>>.Success(results, $"Found {results.Count} matching users");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching users with term: {SearchTerm}", searchTerm);
            return Result<List<User>>.Failure("Failed to search users");
        }
    }

    public async Task<Result<List<User>>> GetUsersByRoleAsync(UserRole role)
    {
        try
        {
            _logger.LogInformation("Getting users by role: {Role}", role);
            await Task.Delay(100);
            
            var users = _users.Where(u => u.Role == role).ToList();
            
            // Business logic: add role-specific sorting
            users = role switch
            {
                UserRole.Doctor => users.OrderBy(u => u.Department).ThenBy(u => u.LastName).ToList(),
                UserRole.Administrator => users.OrderBy(u => u.CreatedDate).ToList(),
                _ => users.OrderBy(u => u.LastName).ToList()
            };

            return Result<List<User>>.Success(users, $"Found {users.Count} users with role {role}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting users by role: {Role}", role);
            return Result<List<User>>.Failure("Failed to get users by role");
        }
    }

    public async Task<Result<List<User>>> GetUsersByStatusAsync(UserStatus status)
    {
        try
        {
            _logger.LogInformation("Getting users by status: {Status}", status);
            await Task.Delay(100);
            
            var users = _users.Where(u => u.Status == status)
                              .OrderBy(u => u.LastName)
                              .ToList();

            return Result<List<User>>.Success(users, $"Found {users.Count} users with status {status}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting users by status: {Status}", status);
            return Result<List<User>>.Failure("Failed to get users by status");
        }
    }

    public async Task<Result<List<User>>> GetUsersByDepartmentAsync(string department)
    {
        try
        {
            _logger.LogInformation("Getting users by department: {Department}", department);
            await Task.Delay(100);
            
            var users = _users.Where(u => u.Department?.Equals(department, StringComparison.OrdinalIgnoreCase) ?? false)
                              .OrderBy(u => GetRolePriority(u.Role))
                              .ThenBy(u => u.LastName)
                              .ToList();

            return Result<List<User>>.Success(users, $"Found {users.Count} users in department {department}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting users by department: {Department}", department);
            return Result<List<User>>.Failure("Failed to get users by department");
        }
    }

    public async Task<Result<UserStatistics>> GetUserStatisticsAsync()
    {
        try
        {
            _logger.LogInformation("Calculating user statistics");
            await Task.Delay(150);

            var stats = new UserStatistics
            {
                TotalUsers = _users.Count,
                ActiveUsers = _users.Count(u => u.Status == UserStatus.Active),
                InactiveUsers = _users.Count(u => u.Status == UserStatus.Inactive),
                SuspendedUsers = _users.Count(u => u.Status == UserStatus.Suspended),
                DoctorsCount = _users.Count(u => u.Role == UserRole.Doctor),
                NursesCount = _users.Count(u => u.Role == UserRole.Nurse),
                AdminsCount = _users.Count(u => u.Role == UserRole.Administrator),
                ReceptionistsCount = _users.Count(u => u.Role == UserRole.Receptionist),
                ManagersCount = _users.Count(u => u.Role == UserRole.Manager),
                OperatorsCount = _users.Count(u => u.Role == UserRole.Operator),
                
                // Business metrics
                RecentlyActiveUsers = _users.Count(u => u.LastLoginDate.HasValue && u.LastLoginDate.Value > DateTime.Now.AddDays(-7)),
                NeverLoggedInUsers = _users.Count(u => !u.LastLoginDate.HasValue),
                
                // Department statistics
                DepartmentDistribution = _users
                    .Where(u => !string.IsNullOrEmpty(u.Department))
                    .GroupBy(u => u.Department!)
                    .ToDictionary(g => g.Key, g => g.Count()),
                    
                // Role distribution with business logic
                RoleDistribution = _users
                    .GroupBy(u => u.Role)
                    .ToDictionary(g => g.Key, g => g.Count())
            };

            return Result<UserStatistics>.Success(stats, "User statistics calculated successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating user statistics");
            return Result<UserStatistics>.Failure("Failed to calculate statistics");
        }
    }

    #region Business Logic Validation Methods

    public async Task<Result<bool>> IsUsernameAvailableAsync(string username, int? excludeUserId = null)
    {
        try
        {
            await Task.Delay(50);
            
            var isAvailable = !_users.Any(u => 
                u.Username.Equals(username, StringComparison.OrdinalIgnoreCase) && 
                u.Id != excludeUserId);

            return Result<bool>.Success(isAvailable);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking username availability: {Username}", username);
            return Result<bool>.Failure("Failed to check username availability");
        }
    }

    public async Task<Result<bool>> IsEmailAvailableAsync(string email, int? excludeUserId = null)
    {
        try
        {
            await Task.Delay(50);
            
            var isAvailable = !_users.Any(u => 
                u.Email.Equals(email, StringComparison.OrdinalIgnoreCase) && 
                u.Id != excludeUserId);

            return Result<bool>.Success(isAvailable);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking email availability: {Email}", email);
            return Result<bool>.Failure("Failed to check email availability");
        }
    }

    public async Task<Result> ValidateUserBusinessRulesAsync(User user, bool isUpdate = false)
    {
        var errors = new List<string>();
        
        // Business rule: Username length and format
        if (string.IsNullOrWhiteSpace(user.Username) || 
            user.Username.Length < MIN_USERNAME_LENGTH || 
            user.Username.Length > MAX_USERNAME_LENGTH)
        {
            errors.Add($"Username must be between {MIN_USERNAME_LENGTH} and {MAX_USERNAME_LENGTH} characters");
        }

        // Business rule: Username format (only letters, numbers, dots, underscores)
        if (!System.Text.RegularExpressions.Regex.IsMatch(user.Username, @"^[a-zA-Z0-9._]+$"))
        {
            errors.Add("Username can only contain letters, numbers, dots, and underscores");
        }

        // Business rule: Email must be from approved domains for certain roles
        if (user.Role == UserRole.Administrator || user.Role == UserRole.Doctor)
        {
            if (!user.Email.EndsWith("@valyanmed.ro", StringComparison.OrdinalIgnoreCase))
            {
                errors.Add("Administrators and Doctors must use @valyanmed.ro email addresses");
            }
        }

        // Business rule: Limit number of administrators
        if (user.Role == UserRole.Administrator && !isUpdate)
        {
            var adminCount = _users.Count(u => u.Role == UserRole.Administrator && u.Status == UserStatus.Active);
            if (adminCount >= MAX_ADMINS_ALLOWED)
            {
                errors.Add($"Cannot create more than {MAX_ADMINS_ALLOWED} administrators");
            }
        }

        // Business rule: Department required for medical roles
        if ((user.Role == UserRole.Doctor || user.Role == UserRole.Nurse) && 
            string.IsNullOrWhiteSpace(user.Department))
        {
            errors.Add("Department is required for medical staff");
        }

        await Task.Delay(10); // Simulate async validation

        return errors.Any() 
            ? Result.Failure(errors) 
            : Result.Success("Business rules validation passed");
    }

    #endregion

    #region Private Business Logic Methods

    private async Task<Result> ValidateUniquenessConstraintsAsync(User user, int? excludeUserId)
    {
        var errors = new List<string>();

        var usernameCheck = await IsUsernameAvailableAsync(user.Username, excludeUserId);
        if (usernameCheck.IsSuccess && !usernameCheck.Value)
        {
            errors.Add("Username is already taken");
        }

        var emailCheck = await IsEmailAvailableAsync(user.Email, excludeUserId);
        if (emailCheck.IsSuccess && !emailCheck.Value)
        {
            errors.Add("Email address is already in use");
        }

        return errors.Any() 
            ? Result.Failure(errors) 
            : Result.Success();
    }

    private User ApplyCreationBusinessRules(User user, string createdBy)
    {
        // Generate new ID
        user.Id = _users.Any() ? _users.Max(u => u.Id) + 1 : 1;
        
        // Set audit fields with local time for consistency
        user.CreatedDate = DateTime.Now;
        
        // Business rule: new users are active by default unless specified otherwise
        if (user.Status == default)
            user.Status = UserStatus.Active;

        // Business rule: normalize data
        user = NormalizeUserData(user);

        return user;
    }

    private User ApplyUpdateBusinessRules(User user, User existingUser, string modifiedBy)
    {
        // Business rule: preserve creation data
        user.CreatedDate = existingUser.CreatedDate;
        
        // Business rule: preserve last login if not provided
        if (user.LastLoginDate == default && existingUser.LastLoginDate.HasValue)
            user.LastLoginDate = existingUser.LastLoginDate;

        // Business rule: normalize data
        user = NormalizeUserData(user);

        return user;
    }

    private User NormalizeUserData(User user)
    {
        // Normalize names
        user.FirstName = NormalizeName(user.FirstName);
        user.LastName = NormalizeName(user.LastName);
        
        // Normalize email
        user.Email = user.Email.Trim().ToLowerInvariant();
        
        // Normalize username  
        user.Username = user.Username.Trim().ToLowerInvariant();

        // Normalize phone
        if (!string.IsNullOrEmpty(user.Phone))
            user.Phone = NormalizePhoneNumber(user.Phone);

        return user;
    }

    private static string NormalizeName(string name)
    {
        if (string.IsNullOrWhiteSpace(name)) return name ?? string.Empty;

        name = name.Trim();
        if (string.IsNullOrEmpty(name)) return name;

        // First letter uppercase, rest lowercase for each word
        return string.Join(" ", name.Split(' ', StringSplitOptions.RemoveEmptyEntries)
            .Select(word => char.ToUpper(word[0]) + word[1..].ToLower()));
    }

    private static string NormalizePhoneNumber(string phone)
    {
        // Remove all non-digits and format Romanian phone numbers
        var digits = new string(phone.Where(char.IsDigit).ToArray());
        
        if (digits.Length == 10 && digits.StartsWith("07"))
        {
            return $"0{digits[1..4]} {digits[4..7]} {digits[7..]}";
        }
        
        return phone; // Return original if not standard Romanian mobile
    }

    private int GetRolePriority(UserRole role)
    {
        return role switch
        {
            UserRole.Administrator => 0,
            UserRole.Doctor => 1,
            UserRole.Manager => 2,
            UserRole.Nurse => 3,
            UserRole.Receptionist => 4,
            UserRole.Operator => 5,
            _ => 99
        };
    }

    private async Task SendWelcomeNotificationAsync(User user)
    {
        await Task.Delay(10);
        // TODO: Implement actual notification system
        _logger.LogInformation("Welcome notification sent to {Email}", user.Email);
    }

    private async Task LogCriticalActionAsync(string performedBy, string action, string details)
    {
        await Task.Delay(10);
        // TODO: Implement actual audit logging
        _logger.LogWarning("CRITICAL ACTION: {Action} performed by {User} - {Details}", 
            action, performedBy, details);
    }

    #endregion
    
    private static List<User> GenerateDummyUsers()
    {
        return new List<User>
        {
            new User { Id = 1, FirstName = "Elena", LastName = "Popescu", Email = "elena.popescu@valyanmed.ro", Username = "elena.popescu", Phone = "0721 234 567", Role = UserRole.Doctor, Status = UserStatus.Active, Department = "Cardiologie", JobTitle = "Medic Primar", CreatedDate = DateTime.Now.AddMonths(-6), LastLoginDate = DateTime.Now.AddHours(-2) },
            new User { Id = 2, FirstName = "Mihai", LastName = "Ionescu", Email = "mihai.ionescu@valyanmed.ro", Username = "mihai.ionescu", Phone = "0722 345 678", Role = UserRole.Doctor, Status = UserStatus.Active, Department = "Neurologie", JobTitle = "Medic Primar", CreatedDate = DateTime.Now.AddMonths(-8), LastLoginDate = DateTime.Now.AddDays(-1) },
            new User { Id = 3, FirstName = "Ana", LastName = "Maria", Email = "ana.maria@valyanmed.ro", Username = "ana.maria", Phone = "0723 456 789", Role = UserRole.Nurse, Status = UserStatus.Active, Department = "Pediatrie", JobTitle = "Asistent Sef", CreatedDate = DateTime.Now.AddMonths(-4), LastLoginDate = DateTime.Now.AddHours(-4) },
            new User { Id = 4, FirstName = "Alexandru", LastName = "Dumitrescu", Email = "alexandru.dumitrescu@valyanmed.ro", Username = "alex.dumitrescu", Phone = "0724 567 890", Role = UserRole.Administrator, Status = UserStatus.Active, Department = "Administrare", JobTitle = "Administrator Sistem", CreatedDate = DateTime.Now.AddYears(-1), LastLoginDate = DateTime.Now.AddMinutes(-30) },
            new User { Id = 5, FirstName = "Carmen", LastName = "Georgescu", Email = "carmen.georgescu@valyanmed.ro", Username = "carmen.georgescu", Phone = "0725 678 901", Role = UserRole.Receptionist, Status = UserStatus.Active, Department = "Administrare", JobTitle = "Receptioner", CreatedDate = DateTime.Now.AddMonths(-3), LastLoginDate = DateTime.Now.AddHours(-1) },
            new User { Id = 6, FirstName = "Radu", LastName = "Stanescu", Email = "radu.stanescu@valyanmed.ro", Username = "radu.stanescu", Phone = "0726 789 012", Role = UserRole.Doctor, Status = UserStatus.Inactive, Department = "Chirurgie", JobTitle = "Medic Rezident", CreatedDate = DateTime.Now.AddMonths(-2), LastLoginDate = DateTime.Now.AddDays(-14) },
            new User { Id = 7, FirstName = "Ioana", LastName = "Petrescu", Email = "ioana.petrescu@valyanmed.ro", Username = "ioana.petrescu", Phone = "0727 890 123", Role = UserRole.Nurse, Status = UserStatus.Active, Department = "Radiologie", JobTitle = "Tehnician", CreatedDate = DateTime.Now.AddMonths(-5), LastLoginDate = DateTime.Now.AddDays(-3) },
            new User { Id = 8, FirstName = "Cristian", LastName = "Marinescu", Email = "cristian.marinescu@valyanmed.ro", Username = "cristian.marinescu", Phone = "0728 901 234", Role = UserRole.Manager, Status = UserStatus.Active, Department = "Cardiologie", JobTitle = "Manager Departament", CreatedDate = DateTime.Now.AddMonths(-10), LastLoginDate = DateTime.Now.AddHours(-6) },
            new User { Id = 9, FirstName = "Maria", LastName = "Constantinescu", Email = "maria.constantinescu@valyanmed.ro", Username = "maria.constantinescu", Phone = "0729 012 345", Role = UserRole.Nurse, Status = UserStatus.Suspended, Department = "Laborator", JobTitle = "Asistent", CreatedDate = DateTime.Now.AddMonths(-1), LastLoginDate = DateTime.Now.AddDays(-7) },
            new User { Id = 10, FirstName = "Bogdan", LastName = "Munteanu", Email = "bogdan.munteanu@valyanmed.ro", Username = "bogdan.munteanu", Phone = "0720 123 456", Role = UserRole.Operator, Status = UserStatus.Suspended, Department = "Administrare", JobTitle = "Operator", CreatedDate = DateTime.Now.AddDays(-7), LastLoginDate = null },
            new User { Id = 11, FirstName = "Andreea", LastName = "Vasile", Email = "andreea.vasile@valyanmed.ro", Username = "andreea.vasile", Phone = "0731 234 567", Role = UserRole.Doctor, Status = UserStatus.Active, Department = "Pediatrie", JobTitle = "Medic Primar", CreatedDate = DateTime.Now.AddMonths(-7), LastLoginDate = DateTime.Now.AddHours(-3) },
            new User { Id = 12, FirstName = "Stefan", LastName = "Nicolae", Email = "stefan.nicolae@valyanmed.ro", Username = "stefan.nicolae", Phone = "0732 345 678", Role = UserRole.Nurse, Status = UserStatus.Active, Department = "Chirurgie", JobTitle = "Asistent", CreatedDate = DateTime.Now.AddMonths(-3), LastLoginDate = DateTime.Now.AddDays(-2) },
            new User { Id = 13, FirstName = "Gabriela", LastName = "Radu", Email = "gabriela.radu@valyanmed.ro", Username = "gabriela.radu", Phone = "0733 456 789", Role = UserRole.Receptionist, Status = UserStatus.Active, Department = "Administrare", JobTitle = "Secretar Medical", CreatedDate = DateTime.Now.AddMonths(-5), LastLoginDate = DateTime.Now.AddHours(-5) },
            new User { Id = 14, FirstName = "Daniel", LastName = "Florea", Email = "daniel.florea@valyanmed.ro", Username = "daniel.florea", Phone = "0734 567 890", Role = UserRole.Doctor, Status = UserStatus.Active, Department = "Neurologie", JobTitle = "Medic Rezident", CreatedDate = DateTime.Now.AddMonths(-4), LastLoginDate = DateTime.Now.AddDays(-1) },
            new User { Id = 15, FirstName = "Laura", LastName = "Diaconu", Email = "laura.diaconu@valyanmed.ro", Username = "laura.diaconu", Phone = "0735 678 901", Role = UserRole.Manager, Status = UserStatus.Active, Department = "Radiologie", JobTitle = "Manager Departament", CreatedDate = DateTime.Now.AddMonths(-9), LastLoginDate = DateTime.Now.AddHours(-8) }
        };
    }
}

// Model pentru statistici utilizatori
public class UserStatistics
{
    public int TotalUsers { get; set; }
    public int ActiveUsers { get; set; }
    public int InactiveUsers { get; set; }
    public int SuspendedUsers { get; set; }
    public int DoctorsCount { get; set; }
    public int NursesCount { get; set; }
    public int AdminsCount { get; set; }
    public int ReceptionistsCount { get; set; }
    public int ManagersCount { get; set; }
    public int OperatorsCount { get; set; }
    public int RecentlyActiveUsers { get; set; }
    public int NeverLoggedInUsers { get; set; }
    
    public Dictionary<string, int> DepartmentDistribution { get; set; } = new();
    public Dictionary<UserRole, int> RoleDistribution { get; set; } = new();
}
