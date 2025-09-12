using ValyanClinic.Components.Pages.Models;

namespace ValyanClinic.Application.Services;

public interface IUserService
{
    Task<List<User>> GetAllUsersAsync();
    Task<User?> GetUserByIdAsync(int id);
    Task<User> CreateUserAsync(User user);
    Task<User> UpdateUserAsync(User user);
    Task<bool> DeleteUserAsync(int id);
    Task<List<User>> SearchUsersAsync(string searchTerm);
    Task<List<User>> GetUsersByRoleAsync(UserRole role);
    Task<List<User>> GetUsersByStatusAsync(UserStatus status);
    Task<List<User>> GetUsersByDepartmentAsync(string department);
}

public class UserService : IUserService
{
    private static List<User> _users = GenerateDummyUsers();
    
    public async Task<List<User>> GetAllUsersAsync()
    {
        await Task.Delay(500); // Simulate async operation
        return _users.ToList();
    }
    
    public async Task<User?> GetUserByIdAsync(int id)
    {
        await Task.Delay(200);
        return _users.FirstOrDefault(u => u.Id == id);
    }
    
    public async Task<User> CreateUserAsync(User user)
    {
        await Task.Delay(300);
        user.Id = _users.Max(u => u.Id) + 1;
        user.CreatedDate = DateTime.Now;
        _users.Add(user);
        return user;
    }
    
    public async Task<User> UpdateUserAsync(User user)
    {
        await Task.Delay(300);
        var existingUser = _users.FirstOrDefault(u => u.Id == user.Id);
        if (existingUser != null)
        {
            var index = _users.IndexOf(existingUser);
            _users[index] = user;
        }
        return user;
    }
    
    public async Task<bool> DeleteUserAsync(int id)
    {
        await Task.Delay(200);
        var user = _users.FirstOrDefault(u => u.Id == id);
        if (user != null)
        {
            _users.Remove(user);
            return true;
        }
        return false;
    }
    
    public async Task<List<User>> SearchUsersAsync(string searchTerm)
    {
        await Task.Delay(300);
        if (string.IsNullOrEmpty(searchTerm))
            return _users.ToList();
            
        return _users.Where(u => 
            u.FullName.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
            u.Email.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
            u.Username.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
            (u.Phone?.Contains(searchTerm) ?? false) ||
            (u.Department?.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ?? false)
        ).ToList();
    }
    
    public async Task<List<User>> GetUsersByRoleAsync(UserRole role)
    {
        await Task.Delay(200);
        return _users.Where(u => u.Role == role).ToList();
    }
    
    public async Task<List<User>> GetUsersByStatusAsync(UserStatus status)
    {
        await Task.Delay(200);
        return _users.Where(u => u.Status == status).ToList();
    }
    
    public async Task<List<User>> GetUsersByDepartmentAsync(string department)
    {
        await Task.Delay(200);
        return _users.Where(u => u.Department?.Equals(department, StringComparison.OrdinalIgnoreCase) ?? false).ToList();
    }
    
    private static List<User> GenerateDummyUsers()
    {
        var random = new Random();
        var departments = new[] { "Cardiologie", "Neurologie", "Pediatrie", "Chirurgie", "Radiologie", "Laborator", "Administrare" };
        var jobTitles = new[] { "Medic Primar", "Medic Rezident", "Asistent ?ef", "Asistent", "Tehnician", "Secretar Medical", "Manager", "Administrator" };
        
        return new List<User>
        {
            new User { Id = 1, FirstName = "Elena", LastName = "Popescu", Email = "elena.popescu@valyanmed.ro", Username = "elena.popescu", Phone = "0721234567", Role = UserRole.Doctor, Status = UserStatus.Active, Department = "Cardiologie", JobTitle = "Medic Primar", CreatedDate = DateTime.Now.AddMonths(-6), LastLoginDate = DateTime.Now.AddHours(-2) },
            new User { Id = 2, FirstName = "Mihai", LastName = "Ionescu", Email = "mihai.ionescu@valyanmed.ro", Username = "mihai.ionescu", Phone = "0722345678", Role = UserRole.Doctor, Status = UserStatus.Active, Department = "Neurologie", JobTitle = "Medic Primar", CreatedDate = DateTime.Now.AddMonths(-8), LastLoginDate = DateTime.Now.AddDays(-1) },
            new User { Id = 3, FirstName = "Ana", LastName = "Maria", Email = "ana.maria@valyanmed.ro", Username = "ana.maria", Phone = "0723456789", Role = UserRole.Nurse, Status = UserStatus.Active, Department = "Pediatrie", JobTitle = "Asistent ?ef", CreatedDate = DateTime.Now.AddMonths(-4), LastLoginDate = DateTime.Now.AddHours(-4) },
            new User { Id = 4, FirstName = "Alexandru", LastName = "Dumitrescu", Email = "alexandru.dumitrescu@valyanmed.ro", Username = "alex.dumitrescu", Phone = "0724567890", Role = UserRole.Administrator, Status = UserStatus.Active, Department = "Administrare", JobTitle = "Administrator Sistem", CreatedDate = DateTime.Now.AddYears(-1), LastLoginDate = DateTime.Now.AddMinutes(-30) },
            new User { Id = 5, FirstName = "Carmen", LastName = "Georgescu", Email = "carmen.georgescu@valyanmed.ro", Username = "carmen.georgescu", Phone = "0725678901", Role = UserRole.Receptionist, Status = UserStatus.Active, Department = "Administrare", JobTitle = "Receptioner", CreatedDate = DateTime.Now.AddMonths(-3), LastLoginDate = DateTime.Now.AddHours(-1) },
            new User { Id = 6, FirstName = "Radu", LastName = "Stanescu", Email = "radu.stanescu@valyanmed.ro", Username = "radu.stanescu", Phone = "0726789012", Role = UserRole.Doctor, Status = UserStatus.Inactive, Department = "Chirurgie", JobTitle = "Medic Rezident", CreatedDate = DateTime.Now.AddMonths(-2), LastLoginDate = DateTime.Now.AddDays(-14) },
            new User { Id = 7, FirstName = "Ioana", LastName = "Petrescu", Email = "ioana.petrescu@valyanmed.ro", Username = "ioana.petrescu", Phone = "0727890123", Role = UserRole.Nurse, Status = UserStatus.Active, Department = "Radiologie", JobTitle = "Tehnician", CreatedDate = DateTime.Now.AddMonths(-5), LastLoginDate = DateTime.Now.AddDays(-3) },
            new User { Id = 8, FirstName = "Cristian", LastName = "Marinescu", Email = "cristian.marinescu@valyanmed.ro", Username = "cristian.marinescu", Phone = "0728901234", Role = UserRole.Manager, Status = UserStatus.Active, Department = "Cardiologie", JobTitle = "Manager Departament", CreatedDate = DateTime.Now.AddMonths(-10), LastLoginDate = DateTime.Now.AddHours(-6) },
            new User { Id = 9, FirstName = "Maria", LastName = "Constantinescu", Email = "maria.constantinescu@valyanmed.ro", Username = "maria.constantinescu", Phone = "0729012345", Role = UserRole.Nurse, Status = UserStatus.Suspended, Department = "Laborator", JobTitle = "Asistent", CreatedDate = DateTime.Now.AddMonths(-1), LastLoginDate = DateTime.Now.AddDays(-7) },
            new User { Id = 10, FirstName = "Bogdan", LastName = "Munteanu", Email = "bogdan.munteanu@valyanmed.ro", Username = "bogdan.munteanu", Phone = "0720123456", Role = UserRole.Operator, Status = UserStatus.Pending, Department = "Administrare", JobTitle = "Operator", CreatedDate = DateTime.Now.AddDays(-7), LastLoginDate = null },
            new User { Id = 11, FirstName = "Andreea", LastName = "Vasile", Email = "andreea.vasile@valyanmed.ro", Username = "andreea.vasile", Phone = "0731234567", Role = UserRole.Doctor, Status = UserStatus.Active, Department = "Pediatrie", JobTitle = "Medic Primar", CreatedDate = DateTime.Now.AddMonths(-7), LastLoginDate = DateTime.Now.AddHours(-3) },
            new User { Id = 12, FirstName = "Stefan", LastName = "Nicolae", Email = "stefan.nicolae@valyanmed.ro", Username = "stefan.nicolae", Phone = "0732345678", Role = UserRole.Nurse, Status = UserStatus.Active, Department = "Chirurgie", JobTitle = "Asistent", CreatedDate = DateTime.Now.AddMonths(-3), LastLoginDate = DateTime.Now.AddDays(-2) },
            new User { Id = 13, FirstName = "Gabriela", LastName = "Radu", Email = "gabriela.radu@valyanmed.ro", Username = "gabriela.radu", Phone = "0733456789", Role = UserRole.Receptionist, Status = UserStatus.Active, Department = "Administrare", JobTitle = "Secretar Medical", CreatedDate = DateTime.Now.AddMonths(-5), LastLoginDate = DateTime.Now.AddHours(-5) },
            new User { Id = 14, FirstName = "Daniel", LastName = "Florea", Email = "daniel.florea@valyanmed.ro", Username = "daniel.florea", Phone = "0734567890", Role = UserRole.Doctor, Status = UserStatus.Active, Department = "Neurologie", JobTitle = "Medic Rezident", CreatedDate = DateTime.Now.AddMonths(-4), LastLoginDate = DateTime.Now.AddDays(-1) },
            new User { Id = 15, FirstName = "Laura", LastName = "Diaconu", Email = "laura.diaconu@valyanmed.ro", Username = "laura.diaconu", Phone = "0735678901", Role = UserRole.Manager, Status = UserStatus.Active, Department = "Radiologie", JobTitle = "Manager Departament", CreatedDate = DateTime.Now.AddMonths(-9), LastLoginDate = DateTime.Now.AddHours(-8) }
        };
    }
}