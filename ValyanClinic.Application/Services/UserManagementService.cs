using ValyanClinic.Application.Models;
using ValyanClinic.Domain.Models;
using ValyanClinic.Domain.Enums;

namespace ValyanClinic.Application.Services;

// Rich Service cu business logic, nu simple pass-through
public interface IUserManagementService
{
    Task<UserOperationResult> CreateUserAsync(CreateUserRequest request);
    Task<UserOperationResult> UpdateUserAsync(UpdateUserRequest request);
    Task<UserOperationResult> ActivateUserAsync(int userId);
    Task<UserOperationResult> DeactivateUserAsync(int userId, string reason);
    Task<UserOperationResult> SuspendUserAsync(int userId, string reason);
    Task<UserOperationResult> DeleteUserAsync(int userId);
    Task<User?> GetUserByIdAsync(int id);
    Task<User?> GetUserByUsernameAsync(string username);
    Task<User?> GetUserByEmailAsync(string email);
    Task<UserListResult> SearchUsersAsync(UserSearchRequest request);
    Task<UserStatistics> GetUserStatisticsAsync();
    Task<UserOperationResult> ValidateUserCreationAsync(CreateUserRequest request);
    Task<bool> IsUsernameAvailableAsync(string username, int? excludeUserId = null);
    Task<bool> IsEmailAvailableAsync(string email, int? excludeUserId = null);
    Task<List<User>> GetUsersByRoleAsync(UserRole role);
    Task<List<User>> GetInactiveUsersAsync(int days);
    Task<UserOperationResult> BulkStatusUpdateAsync(List<int> userIds, UserStatus newStatus);
}

public class UserManagementService : IUserManagementService
{
    private static List<User> _users = GenerateDummyUsers();
    
    public async Task<UserOperationResult> CreateUserAsync(CreateUserRequest request)
    {
        await Task.Delay(100); // Simulate async work
        
        // Business validation
        var validation = await ValidateUserCreationAsync(request);
        if (!validation.IsSuccess)
            return validation;
            
        // Business rules
        var user = new User
        {
            Id = _users.Any() ? _users.Max(u => u.Id) + 1 : 1,
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = request.Email,
            Username = request.Username,
            Phone = request.Phone,
            Role = request.Role,
            Status = UserStatus.Active, // Business rule: new users are active by default
            Department = request.Department,
            JobTitle = request.JobTitle,
            CreatedDate = DateTime.Now
        };
        
        _users.Add(user);
        
        // Business logic: send welcome email for doctors and admins
        if (user.IsDoctor || user.IsAdmin)
        {
            await SendWelcomeEmailAsync(user);
        }
        
        return UserOperationResult.Success(user, 
            $"Utilizatorul {user.FullName} a fost creat cu succes.");
    }
    
    public async Task<UserOperationResult> UpdateUserAsync(UpdateUserRequest request)
    {
        await Task.Delay(100);
        
        var existingUser = await GetUserByIdAsync(request.Id);
        if (existingUser == null)
            return UserOperationResult.Failure("Utilizatorul nu a fost gasit.");
            
        // Business validation
        if (!await IsUsernameAvailableAsync(request.Username, request.Id))
            return UserOperationResult.Failure("Numele de utilizator este deja folosit.");
            
        if (!await IsEmailAvailableAsync(request.Email, request.Id))
            return UserOperationResult.Failure("Adresa de email este deja folosita.");
            
        // Update properties
        existingUser.FirstName = request.FirstName;
        existingUser.LastName = request.LastName;
        existingUser.Email = request.Email;
        existingUser.Username = request.Username;
        existingUser.Phone = request.Phone;
        existingUser.Role = request.Role;
        existingUser.Status = request.Status;
        existingUser.Department = request.Department;
        existingUser.JobTitle = request.JobTitle;
        
        return UserOperationResult.Success(existingUser, 
            $"Utilizatorul {existingUser.FullName} a fost actualizat cu succes.");
    }
    
    public async Task<UserOperationResult> ActivateUserAsync(int userId)
    {
        await Task.Delay(50);
        
        var user = await GetUserByIdAsync(userId);
        if (user == null)
            return UserOperationResult.Failure("Utilizatorul nu a fost gasit.");
            
        if (user.Status == UserStatus.Active)
            return UserOperationResult.Failure("Utilizatorul este deja activ.");
            
        user.Status = UserStatus.Active;
        
        return UserOperationResult.Success(user, 
            $"Utilizatorul {user.FullName} a fost activat cu succes.");
    }
    
    public async Task<UserOperationResult> DeactivateUserAsync(int userId, string reason)
    {
        await Task.Delay(50);
        
        var user = await GetUserByIdAsync(userId);
        if (user == null)
            return UserOperationResult.Failure("Utilizatorul nu a fost gasit.");
            
        // Business rule: can't deactivate the last admin
        if (user.IsAdmin)
        {
            var activeAdmins = _users.Count(u => u.IsAdmin && u.IsActive && u.Id != userId);
            if (activeAdmins == 0)
                return UserOperationResult.Failure("Nu puteti dezactiva ultimul administrator din sistem.");
        }
        
        user.Status = UserStatus.Inactive;
        
        // Business logic: log deactivation reason
        await LogUserActionAsync(userId, "DEACTIVATED", reason);
        
        return UserOperationResult.Success(user, 
            $"Utilizatorul {user.FullName} a fost dezactivat cu succes.");
    }
    
    public async Task<UserOperationResult> SuspendUserAsync(int userId, string reason)
    {
        await Task.Delay(50);
        
        var user = await GetUserByIdAsync(userId);
        if (user == null)
            return UserOperationResult.Failure("Utilizatorul nu a fost gasit.");
            
        user.Status = UserStatus.Suspended;
        
        await LogUserActionAsync(userId, "SUSPENDED", reason);
        
        return UserOperationResult.Success(user, 
            $"Utilizatorul {user.FullName} a fost suspendat cu succes.");
    }
    
    public async Task<UserOperationResult> DeleteUserAsync(int userId)
    {
        await Task.Delay(50);
        
        var user = await GetUserByIdAsync(userId);
        if (user == null)
            return UserOperationResult.Failure("Utilizatorul nu a fost gasit.");
            
        // Business rule: can't delete active doctors or admins
        if (user.IsActive && (user.IsDoctor || user.IsAdmin))
            return UserOperationResult.Failure("Nu puteti sterge doctori sau administratori activi.");
            
        _users.Remove(user);
        
        return UserOperationResult.Success(user, 
            $"Utilizatorul {user.FullName} a fost sters cu succes.");
    }
    
    public async Task<User?> GetUserByIdAsync(int id)
    {
        await Task.Delay(10);
        return _users.FirstOrDefault(u => u.Id == id);
    }
    
    public async Task<User?> GetUserByUsernameAsync(string username)
    {
        await Task.Delay(10);
        return _users.FirstOrDefault(u => u.Username.Equals(username, StringComparison.OrdinalIgnoreCase));
    }
    
    public async Task<User?> GetUserByEmailAsync(string email)
    {
        await Task.Delay(10);
        return _users.FirstOrDefault(u => u.Email.Equals(email, StringComparison.OrdinalIgnoreCase));
    }
    
    public async Task<UserListResult> SearchUsersAsync(UserSearchRequest request)
    {
        await Task.Delay(100);
        
        var query = _users.AsQueryable();
        
        // Apply filters
        if (!string.IsNullOrEmpty(request.SearchTerm))
        {
            query = query.Where(u => 
                u.FirstName.Contains(request.SearchTerm, StringComparison.OrdinalIgnoreCase) ||
                u.LastName.Contains(request.SearchTerm, StringComparison.OrdinalIgnoreCase) ||
                u.Email.Contains(request.SearchTerm, StringComparison.OrdinalIgnoreCase) ||
                u.Username.Contains(request.SearchTerm, StringComparison.OrdinalIgnoreCase) ||
                (u.Department != null && u.Department.Contains(request.SearchTerm, StringComparison.OrdinalIgnoreCase))
            );
        }
        
        if (request.Role.HasValue)
            query = query.Where(u => u.Role == request.Role.Value);
            
        if (request.Status.HasValue)
            query = query.Where(u => u.Status == request.Status.Value);
            
        if (!string.IsNullOrEmpty(request.Department))
            query = query.Where(u => u.Department == request.Department);
            
        if (request.DaysInactive.HasValue)
        {
            var cutoffDate = DateTime.Now.AddDays(-request.DaysInactive.Value);
            query = query.Where(u => !u.LastLoginDate.HasValue || u.LastLoginDate < cutoffDate);
        }
        
        var totalCount = query.Count();
        
        // Apply pagination
        var users = query
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .OrderBy(u => u.LastName)
            .ThenBy(u => u.FirstName)
            .ToList();
            
        return new UserListResult
        {
            Users = users,
            TotalCount = totalCount,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize
        };
    }
    
    public async Task<UserStatistics> GetUserStatisticsAsync()
    {
        await Task.Delay(100);
        
        var now = DateTime.Now;
        var weekAgo = now.AddDays(-7);
        
        return new UserStatistics
        {
            TotalUsers = _users.Count,
            ActiveUsers = _users.Count(u => u.IsActive),
            DoctorsCount = _users.Count(u => u.IsDoctor),
            NursesCount = _users.Count(u => u.Role == UserRole.Nurse),
            AdminsCount = _users.Count(u => u.IsAdmin),
            RecentlyActiveUsers = _users.Count(u => u.HasRecentActivity()),
            InactiveUsers = _users.Count(u => !u.IsActive),
            UsersByRole = _users.GroupBy(u => u.Role).ToDictionary(g => g.Key, g => g.Count()),
            UsersByStatus = _users.GroupBy(u => u.Status).ToDictionary(g => g.Key, g => g.Count()),
            UsersByDepartment = _users
                .Where(u => !string.IsNullOrEmpty(u.Department))
                .GroupBy(u => u.Department!)
                .ToDictionary(g => g.Key, g => g.Count())
        };
    }
    
    public async Task<UserOperationResult> ValidateUserCreationAsync(CreateUserRequest request)
    {
        await Task.Delay(10);
        
        var errors = new List<string>();
        
        if (string.IsNullOrWhiteSpace(request.FirstName))
            errors.Add("Numele este obligatoriu.");
            
        if (string.IsNullOrWhiteSpace(request.LastName))
            errors.Add("Prenumele este obligatoriu.");
            
        if (string.IsNullOrWhiteSpace(request.Email))
            errors.Add("Email-ul este obligatoriu.");
        else if (!IsValidEmail(request.Email))
            errors.Add("Format email invalid.");
            
        if (string.IsNullOrWhiteSpace(request.Username))
            errors.Add("Numele de utilizator este obligatoriu.");
        else if (request.Username.Length < 3)
            errors.Add("Numele de utilizator trebuie sa aiba minim 3 caractere.");
            
        if (!await IsUsernameAvailableAsync(request.Username))
            errors.Add("Numele de utilizator este deja folosit.");
            
        if (!await IsEmailAvailableAsync(request.Email))
            errors.Add("Adresa de email este deja folosita.");
            
        if (errors.Any())
            return UserOperationResult.Failure(errors.ToArray());
            
        return UserOperationResult.Success(new User());
    }
    
    public async Task<bool> IsUsernameAvailableAsync(string username, int? excludeUserId = null)
    {
        await Task.Delay(10);
        return !_users.Any(u => u.Username.Equals(username, StringComparison.OrdinalIgnoreCase) && 
                               u.Id != excludeUserId);
    }
    
    public async Task<bool> IsEmailAvailableAsync(string email, int? excludeUserId = null)
    {
        await Task.Delay(10);
        return !_users.Any(u => u.Email.Equals(email, StringComparison.OrdinalIgnoreCase) && 
                               u.Id != excludeUserId);
    }
    
    public async Task<List<User>> GetUsersByRoleAsync(UserRole role)
    {
        await Task.Delay(50);
        return _users.Where(u => u.Role == role).ToList();
    }
    
    public async Task<List<User>> GetInactiveUsersAsync(int days)
    {
        await Task.Delay(50);
        var cutoffDate = DateTime.Now.AddDays(-days);
        return _users
            .Where(u => !u.LastLoginDate.HasValue || u.LastLoginDate < cutoffDate)
            .ToList();
    }
    
    public async Task<UserOperationResult> BulkStatusUpdateAsync(List<int> userIds, UserStatus newStatus)
    {
        await Task.Delay(100);
        
        var users = _users.Where(u => userIds.Contains(u.Id)).ToList();
        if (!users.Any())
            return UserOperationResult.Failure("Nu au fost gasiti utilizatori pentru actualizare.");
            
        foreach (var user in users)
        {
            user.Status = newStatus;
        }
        
        return UserOperationResult.Success(
            users.First(), 
            $"{users.Count} utilizatori au fost actualizati cu statusul {newStatus}."
        );
    }
    
    // Private helper methods
    private static bool IsValidEmail(string email)
    {
        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }
    
    private async Task SendWelcomeEmailAsync(User user)
    {
        await Task.Delay(10);
        // TODO: Implement email sending
        Console.WriteLine($"Welcome email sent to {user.Email}");
    }
    
    private async Task LogUserActionAsync(int userId, string action, string reason)
    {
        await Task.Delay(10);
        // TODO: Implement audit logging
        Console.WriteLine($"User {userId}: {action} - {reason}");
    }
    
    private static List<User> GenerateDummyUsers()
    {
        return new List<User>
        {
            new() { Id = 1, FirstName = "Elena", LastName = "Popescu", Email = "elena.popescu@valyanmed.ro", Username = "elena.popescu", Phone = "0721234567", Role = UserRole.Doctor, Status = UserStatus.Active, Department = "Cardiologie", JobTitle = "Medic Primar", CreatedDate = DateTime.Now.AddMonths(-6), LastLoginDate = DateTime.Now.AddHours(-2) },
            new() { Id = 2, FirstName = "Mihai", LastName = "Ionescu", Email = "mihai.ionescu@valyanmed.ro", Username = "mihai.ionescu", Phone = "0722345678", Role = UserRole.Doctor, Status = UserStatus.Active, Department = "Neurologie", JobTitle = "Medic Primar", CreatedDate = DateTime.Now.AddMonths(-8), LastLoginDate = DateTime.Now.AddDays(-1) },
            new() { Id = 3, FirstName = "Ana", LastName = "Maria", Email = "ana.maria@valyanmed.ro", Username = "ana.maria", Phone = "0723456789", Role = UserRole.Nurse, Status = UserStatus.Active, Department = "Pediatrie", JobTitle = "Asistent Sef", CreatedDate = DateTime.Now.AddMonths(-4), LastLoginDate = DateTime.Now.AddHours(-4) },
            new() { Id = 4, FirstName = "Alexandru", LastName = "Dumitrescu", Email = "alexandru.dumitrescu@valyanmed.ro", Username = "alex.dumitrescu", Phone = "0724567890", Role = UserRole.Administrator, Status = UserStatus.Active, Department = "Administrare", JobTitle = "Administrator Sistem", CreatedDate = DateTime.Now.AddYears(-1), LastLoginDate = DateTime.Now.AddMinutes(-30) },
            new() { Id = 5, FirstName = "Carmen", LastName = "Georgescu", Email = "carmen.georgescu@valyanmed.ro", Username = "carmen.georgescu", Phone = "0725678901", Role = UserRole.Receptionist, Status = UserStatus.Active, Department = "Administrare", JobTitle = "Receptioner", CreatedDate = DateTime.Now.AddMonths(-3), LastLoginDate = DateTime.Now.AddHours(-1) },
            new() { Id = 6, FirstName = "Radu", LastName = "Stanescu", Email = "radu.stanescu@valyanmed.ro", Username = "radu.stanescu", Phone = "0726789012", Role = UserRole.Doctor, Status = UserStatus.Inactive, Department = "Chirurgie", JobTitle = "Medic Rezident", CreatedDate = DateTime.Now.AddMonths(-2), LastLoginDate = DateTime.Now.AddDays(-14) },
            new() { Id = 7, FirstName = "Ioana", LastName = "Petrescu", Email = "ioana.petrescu@valyanmed.ro", Username = "ioana.petrescu", Phone = "0727890123", Role = UserRole.Nurse, Status = UserStatus.Active, Department = "Radiologie", JobTitle = "Tehnician", CreatedDate = DateTime.Now.AddMonths(-5), LastLoginDate = DateTime.Now.AddDays(-3) },
            new() { Id = 8, FirstName = "Cristian", LastName = "Marinescu", Email = "cristian.marinescu@valyanmed.ro", Username = "cristian.marinescu", Phone = "0728901234", Role = UserRole.Manager, Status = UserStatus.Active, Department = "Cardiologie", JobTitle = "Manager Departament", CreatedDate = DateTime.Now.AddMonths(-10), LastLoginDate = DateTime.Now.AddHours(-6) },
            new() { Id = 9, FirstName = "Maria", LastName = "Constantinescu", Email = "maria.constantinescu@valyanmed.ro", Username = "maria.constantinescu", Phone = "0729012345", Role = UserRole.Nurse, Status = UserStatus.Suspended, Department = "Laborator", JobTitle = "Asistent", CreatedDate = DateTime.Now.AddMonths(-1), LastLoginDate = DateTime.Now.AddDays(-7) },
            new() { Id = 10, FirstName = "Bogdan", LastName = "Munteanu", Email = "bogdan.munteanu@valyanmed.ro", Username = "bogdan.munteanu", Phone = "0720123456", Role = UserRole.Operator, Status = UserStatus.Locked, Department = "Administrare", JobTitle = "Operator", CreatedDate = DateTime.Now.AddDays(-7), LastLoginDate = null },
            new() { Id = 11, FirstName = "Andreea", LastName = "Vasile", Email = "andreea.vasile@valyanmed.ro", Username = "andreea.vasile", Phone = "0731234567", Role = UserRole.Doctor, Status = UserStatus.Active, Department = "Pediatrie", JobTitle = "Medic Primar", CreatedDate = DateTime.Now.AddMonths(-7), LastLoginDate = DateTime.Now.AddHours(-3) },
            new() { Id = 12, FirstName = "Stefan", LastName = "Nicolae", Email = "stefan.nicolae@valyanmed.ro", Username = "stefan.nicolae", Phone = "0732345678", Role = UserRole.Nurse, Status = UserStatus.Active, Department = "Chirurgie", JobTitle = "Asistent", CreatedDate = DateTime.Now.AddMonths(-3), LastLoginDate = DateTime.Now.AddDays(-2) },
            new() { Id = 13, FirstName = "Gabriela", LastName = "Radu", Email = "gabriela.radu@valyanmed.ro", Username = "gabriela.radu", Phone = "0733456789", Role = UserRole.Receptionist, Status = UserStatus.Active, Department = "Administrare", JobTitle = "Secretar Medical", CreatedDate = DateTime.Now.AddMonths(-5), LastLoginDate = DateTime.Now.AddHours(-5) },
            new() { Id = 14, FirstName = "Daniel", LastName = "Florea", Email = "daniel.florea@valyanmed.ro", Username = "daniel.florea", Phone = "0734567890", Role = UserRole.Doctor, Status = UserStatus.Active, Department = "Neurologie", JobTitle = "Medic Rezident", CreatedDate = DateTime.Now.AddMonths(-4), LastLoginDate = DateTime.Now.AddDays(-1) },
            new() { Id = 15, FirstName = "Laura", LastName = "Diaconu", Email = "laura.diaconu@valyanmed.ro", Username = "laura.diaconu", Phone = "0735678901", Role = UserRole.Manager, Status = UserStatus.Active, Department = "Radiologie", JobTitle = "Manager Departament", CreatedDate = DateTime.Now.AddMonths(-9), LastLoginDate = DateTime.Now.AddHours(-8) }
        };
    }
}