using Microsoft.AspNetCore.Components;
using ValyanClinic.Application.Interfaces;

namespace ValyanClinic.Components.Shared.Authorization;

/// <summary>
/// DropDownList cu verificare automată de permisiuni la nivel de câmp.
/// </summary>
public partial class PermissionDropDown<TValue, TItem> : ComponentBase
{
    [Inject]
    private IFieldPermissionService PermissionService { get; set; } = default!;
    
    private FieldState _fieldState = FieldState.Editable;
    
    #region Parameters
    
    [Parameter] public string Entity { get; set; } = string.Empty;
    [Parameter] public string Field { get; set; } = string.Empty;
    [Parameter] public TValue? Value { get; set; }
    [Parameter] public EventCallback<TValue?> ValueChanged { get; set; }
    [Parameter] public IEnumerable<TItem>? DataSource { get; set; }
    [Parameter] public string? TextField { get; set; }
    [Parameter] public string? ValueField { get; set; }
    [Parameter] public bool IsEditMode { get; set; }
    [Parameter] public string Id { get; set; } = string.Empty;
    [Parameter] public string? Label { get; set; }
    [Parameter] public string? Placeholder { get; set; }
    [Parameter] public string? HelpText { get; set; }
    [Parameter] public bool Required { get; set; }
    [Parameter] public bool AllowFiltering { get; set; }
    [Parameter] public bool ShowClearButton { get; set; }
    [Parameter] public string? CssClass { get; set; }
    [Parameter] public bool ForceReadOnly { get; set; }
    [Parameter] public bool ShowReadOnlyMessage { get; set; } = true;
    
    #endregion
    
    private bool IsReadOnly => ForceReadOnly || _fieldState == FieldState.ReadOnly;
    private bool IsEnabled => _fieldState != FieldState.Hidden;
    
    protected override void OnParametersSet()
    {
        base.OnParametersSet();
        
        if (!string.IsNullOrEmpty(Entity) && !string.IsNullOrEmpty(Field))
        {
            _fieldState = PermissionService.GetFieldState(Entity, Field, IsEditMode);
        }
        else
        {
            _fieldState = IsEditMode ? FieldState.Editable : FieldState.ReadOnly;
        }
        
        if (string.IsNullOrEmpty(Id))
        {
            Id = $"{Entity?.ToLower()}-{Field?.ToLower()}";
        }
    }
    
    private string GetInputCssClass()
    {
        var classes = new List<string> { "form-control" };
        if (_fieldState == FieldState.ReadOnly) classes.Add("readonly-field");
        return string.Join(" ", classes);
    }
}
