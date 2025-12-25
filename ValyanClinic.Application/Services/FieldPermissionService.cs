using Microsoft.Extensions.Logging;
using ValyanClinic.Application.Authorization;
using ValyanClinic.Application.Interfaces;
using ValyanClinic.Domain.Interfaces.Repositories;

namespace ValyanClinic.Application.Services;

/// <summary>
/// Implementare a serviciului de permisiuni la nivel de câmp.
/// Citește permisiunile din baza de date și oferă verificări granulare.
/// </summary>
public class FieldPermissionService : IFieldPermissionService
{
    private readonly IRolRepository _rolRepository;
    private readonly ILogger<FieldPermissionService> _logger;
    private HashSet<string> _permissions = new(StringComparer.OrdinalIgnoreCase);
    private bool _isLoaded;
    private string _currentRole = string.Empty;

    public FieldPermissionService(
        IRolRepository rolRepository,
        ILogger<FieldPermissionService> logger)
    {
        _rolRepository = rolRepository;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task LoadPermissionsAsync(string roleDenumire)
    {
        if (string.IsNullOrWhiteSpace(roleDenumire))
        {
            _logger.LogWarning("Attempted to load permissions with empty role");
            return;
        }

        // Evită reîncărcarea pentru același rol
        if (_isLoaded && _currentRole.Equals(roleDenumire, StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        try
        {
            var permissions = await _rolRepository.GetPermisiuniForRolByDenumireAsync(roleDenumire);
            _permissions = new HashSet<string>(permissions, StringComparer.OrdinalIgnoreCase);
            _currentRole = roleDenumire;
            _isLoaded = true;

            _logger.LogDebug("Loaded {Count} permissions for role {Role}", 
                _permissions.Count, roleDenumire);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load permissions for role {Role}", roleDenumire);
            _permissions = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        }
    }

    /// <inheritdoc />
    public bool HasPermission(string permission)
    {
        if (string.IsNullOrWhiteSpace(permission))
            return false;

        // Special.FullAccess permite orice
        if (_permissions.Contains(Permissions.Special.FullAccess))
            return true;

        return _permissions.Contains(permission);
    }

    /// <inheritdoc />
    public bool CanViewField(string entity, string fieldName)
    {
        // 1. Verifică permisiunea specifică de câmp (ex: Pacient.View.CNP)
        var fieldPermission = $"{entity}.View.{fieldName}";
        if (_permissions.Contains(fieldPermission))
            return true;

        // 2. Verifică permisiunea de date sensibile pentru câmpuri sensibile
        if (IsSensitiveField(entity, fieldName))
        {
            var sensitivePermission = $"{entity}.ViewSensitiveData";
            if (_permissions.Contains(sensitivePermission))
                return true;
        }

        // 3. Fallback: verifică permisiunea generală de vizualizare
        var generalPermission = $"{entity}.View";
        return _permissions.Contains(generalPermission);
    }

    /// <inheritdoc />
    public bool CanEditField(string entity, string fieldName)
    {
        // 1. Verifică permisiunea specifică de editare câmp (ex: Pacient.Edit.CNP)
        var fieldPermission = $"{entity}.Edit.{fieldName}";
        if (_permissions.Contains(fieldPermission))
            return true;

        // 2. Verifică dacă există permisiunea specifică dar e negată explicit
        // (dacă avem Pacient.Edit dar NU avem Pacient.Edit.CNP explicit definit,
        // verificăm dacă câmpul e sensibil - dacă da, nu permitem fără permisiunea specifică)
        if (IsSensitiveField(entity, fieldName))
        {
            // Pentru câmpuri sensibile, trebuie permisiune explicită
            var sensitivePermission = $"{entity}.ViewSensitiveData";
            if (!_permissions.Contains(sensitivePermission) && !_permissions.Contains(fieldPermission))
                return false;
        }

        // 3. Fallback: verifică permisiunea generală de editare
        var generalPermission = $"{entity}.Edit";
        return _permissions.Contains(generalPermission);
    }

    /// <inheritdoc />
    public bool HasAnyPermission(params string[] permissions)
    {
        if (permissions == null || permissions.Length == 0)
            return false;

        // Special.FullAccess permite orice
        if (_permissions.Contains(Permissions.Special.FullAccess))
            return true;

        return permissions.Any(p => _permissions.Contains(p));
    }

    /// <inheritdoc />
    public bool HasAllPermissions(params string[] permissions)
    {
        if (permissions == null || permissions.Length == 0)
            return true;

        // Special.FullAccess permite orice
        if (_permissions.Contains(Permissions.Special.FullAccess))
            return true;

        return permissions.All(p => _permissions.Contains(p));
    }

    /// <inheritdoc />
    public IReadOnlyList<string> GetCurrentPermissions()
    {
        return _permissions.ToList().AsReadOnly();
    }

    /// <inheritdoc />
    public FieldState GetFieldState(string entity, string fieldName, bool isEditMode)
    {
        var canView = CanViewField(entity, fieldName);
        
        if (!canView)
            return FieldState.Hidden;

        if (!isEditMode)
            return FieldState.ReadOnly;

        var canEdit = CanEditField(entity, fieldName);
        return canEdit ? FieldState.Editable : FieldState.ReadOnly;
    }

    /// <summary>
    /// Determină dacă un câmp este considerat sensibil și necesită permisiuni speciale.
    /// </summary>
    private static bool IsSensitiveField(string entity, string fieldName)
    {
        // Lista câmpurilor sensibile per entitate
        var sensitiveFields = new Dictionary<string, HashSet<string>>(StringComparer.OrdinalIgnoreCase)
        {
            ["Pacient"] = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "CNP",
                "Alergii",
                "BoliCronice",
                "GrupaSanguina",
                "Observatii"
            },
            ["Personal"] = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "CNP",
                "Salariu"
            },
            ["Consultatie"] = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "Diagnostic",
                "Tratament",
                "Reteta"
            }
        };

        return sensitiveFields.TryGetValue(entity, out var fields) && 
               fields.Contains(fieldName);
    }
}
