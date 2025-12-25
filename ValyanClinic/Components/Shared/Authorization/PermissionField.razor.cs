using Microsoft.AspNetCore.Components;
using ValyanClinic.Application.Interfaces;

namespace ValyanClinic.Components.Shared.Authorization;

/// <summary>
/// PermissionField - Wrapper generic pentru orice input cu verificare permisiuni.
/// Permite să înfășurați orice conținut și să aplicați logica de permisiuni.
/// </summary>
public partial class PermissionField : ComponentBase
{
    [Inject]
    private IFieldPermissionService PermissionService { get; set; } = default!;
    
    /// <summary>
    /// Entitatea pentru verificare permisiuni
    /// </summary>
    [Parameter] 
    public string Entity { get; set; } = string.Empty;
    
    /// <summary>
    /// Câmpul pentru verificare permisiuni
    /// </summary>
    [Parameter] 
    public string Field { get; set; } = string.Empty;
    
    /// <summary>
    /// Este în mod editare
    /// </summary>
    [Parameter] 
    public bool IsEditMode { get; set; }
    
    /// <summary>
    /// Conținutul interior (RenderFragment cu context)
    /// </summary>
    [Parameter] 
    public RenderFragment<FieldContext>? ChildContent { get; set; }
    
    /// <summary>
    /// Label-ul câmpului
    /// </summary>
    [Parameter] 
    public string? Label { get; set; }
    
    /// <summary>
    /// Text de ajutor
    /// </summary>
    [Parameter] 
    public string? HelpText { get; set; }
    
    /// <summary>
    /// Câmp obligatoriu
    /// </summary>
    [Parameter] 
    public bool Required { get; set; }
    
    /// <summary>
    /// CSS class suplimentară
    /// </summary>
    [Parameter] 
    public string? CssClass { get; set; }
    
    /// <summary>
    /// Arată icon lock pentru readonly
    /// </summary>
    [Parameter] 
    public bool ShowLockIcon { get; set; } = true;
    
    /// <summary>
    /// Arată mesaj când e readonly
    /// </summary>
    [Parameter] 
    public bool ShowReadOnlyMessage { get; set; }
    
    /// <summary>
    /// Mesaj personalizat pentru readonly
    /// </summary>
    [Parameter] 
    public string? ReadOnlyMessage { get; set; } = "Lipsă permisiune de editare";
    
    /// <summary>
    /// Arată placeholder când câmpul e ascuns (pentru debug)
    /// </summary>
    [Parameter] 
    public bool ShowHiddenPlaceholder { get; set; }
    
    /// <summary>
    /// Starea curentă a câmpului
    /// </summary>
    public FieldState State { get; private set; } = FieldState.Editable;
    
    protected override void OnParametersSet()
    {
        base.OnParametersSet();
        
        if (!string.IsNullOrEmpty(Entity) && !string.IsNullOrEmpty(Field))
        {
            State = PermissionService.GetFieldState(Entity, Field, IsEditMode);
        }
        else
        {
            State = IsEditMode ? FieldState.Editable : FieldState.ReadOnly;
        }
    }
}
