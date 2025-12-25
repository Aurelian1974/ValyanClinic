using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using ValyanClinic.Application.Authorization.Requirements;
using ValyanClinic.Domain.Interfaces.Repositories;

namespace ValyanClinic.Application.Authorization.Handlers;

/// <summary>
/// Handler pentru PermissionRequirement.
/// Verifică dacă utilizatorul are permisiunea specificată bazat pe permisiunile din DB.
/// </summary>
public class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
{
    private readonly ILogger<PermissionAuthorizationHandler> _logger;
    private readonly IRolRepository _rolRepository;

    public PermissionAuthorizationHandler(
        ILogger<PermissionAuthorizationHandler> logger,
        IRolRepository rolRepository)
    {
        _logger = logger;
        _rolRepository = rolRepository;
    }

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        PermissionRequirement requirement)
    {
        // Obține rolul utilizatorului din claims
        var roleClaim = context.User.FindFirst(ClaimTypes.Role);
        if (roleClaim == null)
        {
            _logger.LogWarning("Authorization failed: No role claim found for user {User}", 
                context.User.Identity?.Name ?? "Unknown");
            return;
        }

        var role = roleClaim.Value;
        
        // ✅ Citește permisiunile din baza de date
        var permissions = await _rolRepository.GetPermisiuniForRolByDenumireAsync(role);
        var hasPermission = permissions.Contains(requirement.Permission);

        if (hasPermission)
        {
            _logger.LogDebug("Authorization succeeded: User {User} with role {Role} has permission {Permission}",
                context.User.Identity?.Name, role, requirement.Permission);
            context.Succeed(requirement);
        }
        else
        {
            _logger.LogWarning("Authorization failed: User {User} with role {Role} lacks permission {Permission}",
                context.User.Identity?.Name, role, requirement.Permission);
        }
    }
}

/// <summary>
/// Handler pentru AnyPermissionRequirement.
/// Verifică dacă utilizatorul are cel puțin una din permisiunile specificate din DB.
/// </summary>
public class AnyPermissionAuthorizationHandler : AuthorizationHandler<AnyPermissionRequirement>
{
    private readonly ILogger<AnyPermissionAuthorizationHandler> _logger;
    private readonly IRolRepository _rolRepository;

    public AnyPermissionAuthorizationHandler(
        ILogger<AnyPermissionAuthorizationHandler> logger,
        IRolRepository rolRepository)
    {
        _logger = logger;
        _rolRepository = rolRepository;
    }

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        AnyPermissionRequirement requirement)
    {
        var roleClaim = context.User.FindFirst(ClaimTypes.Role);
        if (roleClaim == null)
        {
            return;
        }

        var role = roleClaim.Value;
        
        // ✅ Citește permisiunile din baza de date
        var userPermissions = await _rolRepository.GetPermisiuniForRolByDenumireAsync(role);
        var permissionsList = userPermissions.ToList();
        
        foreach (var permission in requirement.Permissions)
        {
            if (permissionsList.Contains(permission))
            {
                _logger.LogDebug("Authorization succeeded: User {User} has permission {Permission}",
                    context.User.Identity?.Name, permission);
                context.Succeed(requirement);
                return;
            }
        }

        _logger.LogWarning("Authorization failed: User {User} lacks all required permissions",
            context.User.Identity?.Name);
    }
}

/// <summary>
/// Handler pentru MedicalStaffRequirement.
/// Verifică dacă utilizatorul are permisiuni medicale în DB (Consultatie.*).
/// </summary>
public class MedicalStaffAuthorizationHandler : AuthorizationHandler<MedicalStaffRequirement>
{
    private const string CLAIM_PERSONAL_MEDICAL_ID = "PersonalMedicalID";
    private readonly ILogger<MedicalStaffAuthorizationHandler> _logger;
    private readonly IRolRepository _rolRepository;

    public MedicalStaffAuthorizationHandler(
        ILogger<MedicalStaffAuthorizationHandler> logger,
        IRolRepository rolRepository)
    {
        _logger = logger;
        _rolRepository = rolRepository;
    }

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        MedicalStaffRequirement requirement)
    {
        // Obține rolul utilizatorului din claims
        var roleClaim = context.User.FindFirst(ClaimTypes.Role);
        if (roleClaim == null)
        {
            _logger.LogWarning("Authorization failed: No role claim found for user {User}",
                context.User.Identity?.Name);
            return;
        }

        var role = roleClaim.Value;
        
        // ✅ Verifică dacă rolul are permisiuni medicale (Consultatie.*)
        var permissions = await _rolRepository.GetPermisiuniForRolByDenumireAsync(role);
        var permissionsList = permissions.ToList();
        var hasMedicalPermissions = permissionsList.Any(p => 
            p.StartsWith("Consultatie.", StringComparison.OrdinalIgnoreCase) ||
            p.StartsWith("Pacient.", StringComparison.OrdinalIgnoreCase));

        if (!hasMedicalPermissions)
        {
            _logger.LogWarning("Authorization failed: User {User} with role {Role} has no medical permissions",
                context.User.Identity?.Name, role);
            return;
        }

        // Verifică PersonalMedicalID dacă este necesar
        if (requirement.RequirePersonalMedicalId)
        {
            var personalMedicalId = context.User.FindFirst(CLAIM_PERSONAL_MEDICAL_ID)?.Value;
            
            if (string.IsNullOrEmpty(personalMedicalId) || 
                !Guid.TryParse(personalMedicalId, out var guid) || 
                guid == Guid.Empty)
            {
                _logger.LogWarning("Authorization failed: User {User} lacks valid PersonalMedicalID",
                    context.User.Identity?.Name);
                return;
            }
        }

        _logger.LogDebug("Authorization succeeded: User {User} is valid medical staff",
            context.User.Identity?.Name);
        context.Succeed(requirement);
    }
}
