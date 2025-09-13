using ValyanClinic.Domain.Models;
using ValyanClinic.Domain.Enums;

namespace ValyanClinic.Application.Models;

// Request/Response models pentru operations
public record CreateUserRequest(
    string FirstName,
    string LastName,
    string Email,
    string Username,
    string? Phone,
    UserRole Role,
    string? Department,
    string? JobTitle
);

public record UpdateUserRequest(
    int Id,
    string FirstName,
    string LastName,
    string Email,
    string Username,
    string? Phone,
    UserRole Role,
    UserStatus Status,
    string? Department,
    string? JobTitle
);

public record UserSearchRequest(
    string? SearchTerm,
    UserRole? Role,
    UserStatus? Status,
    string? Department,
    int? DaysInactive,
    int PageNumber = 1,
    int PageSize = 20
);

// Result models
public class UserOperationResult
{
    public bool IsSuccess { get; private set; }
    public string? SuccessMessage { get; private set; }
    public List<string> Errors { get; private set; } = new();
    public User? User { get; private set; }
    
    public static UserOperationResult Success(User user, string? message = null)
        => new() { IsSuccess = true, User = user, SuccessMessage = message };
        
    public static UserOperationResult Failure(params string[] errors)
        => new() { IsSuccess = false, Errors = errors.ToList() };
}

public class UserListResult  
{
    public List<User> Users { get; set; } = new();
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public bool HasNextPage => PageNumber * PageSize < TotalCount;
    public bool HasPreviousPage => PageNumber > 1;
}

// Statistics models
public class UserStatistics
{
    public int TotalUsers { get; set; }
    public int ActiveUsers { get; set; }
    public int DoctorsCount { get; set; }
    public int NursesCount { get; set; }
    public int AdminsCount { get; set; }
    public int RecentlyActiveUsers { get; set; }
    public int InactiveUsers { get; set; }
    public Dictionary<UserRole, int> UsersByRole { get; set; } = new();
    public Dictionary<UserStatus, int> UsersByStatus { get; set; } = new();
    public Dictionary<string, int> UsersByDepartment { get; set; } = new();
}