using Microsoft.AspNetCore.Components;
using Syncfusion.Blazor.Inputs;
using ValyanClinic.Application.Interfaces;

namespace ValyanClinic.Components.Shared.Authorization;

/// <summary>
/// TextBox cu verificare automată de permisiuni la nivel de câmp.
/// </summary>
public partial class PermissionTextBox : ComponentBase
{
    [Inject]
    private IFieldPermissionService PermissionService { get; set; } = default!;
    
    private SfTextBox? _textBox;
    private FieldState _fieldState = FieldState.Editable;
    
    #region Parameters
    
    /// <summary>
    /// Entitatea pentru care se verifică permisiunea (ex: "Pacient", "Consultatie")
    /// </summary>
    [Parameter]
    public string Entity { get; set; } = string.Empty;
    
    /// <summary>
    /// Numele câmpului pentru verificare permisiune (ex: "CNP", "Nume")
    /// </summary>
    [Parameter]
    public string Field { get; set; } = string.Empty;
    
    /// <summary>
    /// Valoarea curentă a câmpului
    /// </summary>
    [Parameter]
    public string? Value { get; set; }
    
    /// <summary>
    /// Callback când valoarea se schimbă
    /// </summary>
    [Parameter]
    public EventCallback<string?> ValueChanged { get; set; }
    
    /// <summary>
    /// True dacă componenta este în mod editare
    /// </summary>
    [Parameter]
    public bool IsEditMode { get; set; }
    
    /// <summary>
    /// ID-ul elementului HTML
    /// </summary>
    [Parameter]
    public string Id { get; set; } = string.Empty;
    
    /// <summary>
    /// Label-ul câmpului
    /// </summary>
    [Parameter]
    public string? Label { get; set; }
    
    /// <summary>
    /// Placeholder pentru input
    /// </summary>
    [Parameter]
    public string? Placeholder { get; set; }
    
    /// <summary>
    /// Text de ajutor afișat sub input
    /// </summary>
    [Parameter]
    public string? HelpText { get; set; }
    
    /// <summary>
    /// Câmpul este obligatoriu
    /// </summary>
    [Parameter]
    public bool Required { get; set; }
    
    /// <summary>
    /// Lungimea maximă
    /// </summary>
    [Parameter]
    public int MaxLength { get; set; }
    
    /// <summary>
    /// Input multiline (textarea)
    /// </summary>
    [Parameter]
    public bool Multiline { get; set; }
    
    /// <summary>
    /// Număr de rânduri pentru multiline
    /// </summary>
    [Parameter]
    public int RowCount { get; set; } = 3;
    
    /// <summary>
    /// Clasă CSS suplimentară
    /// </summary>
    [Parameter]
    public string? CssClass { get; set; }
    
    /// <summary>
    /// Forțează câmpul readonly indiferent de permisiuni
    /// </summary>
    [Parameter]
    public bool ForceReadOnly { get; set; }
    
    /// <summary>
    /// Arată mesajul când câmpul este readonly din cauza lipsei permisiunii
    /// </summary>
    [Parameter]
    public bool ShowReadOnlyMessage { get; set; } = true;
    
    #endregion
    
    #region Computed Properties
    
    private bool IsReadOnly => ForceReadOnly || _fieldState == FieldState.ReadOnly;
    private bool IsEnabled => _fieldState != FieldState.Hidden;
    
    #endregion
    
    protected override void OnParametersSet()
    {
        base.OnParametersSet();
        
        // Calculează starea câmpului bazat pe permisiuni
        if (!string.IsNullOrEmpty(Entity) && !string.IsNullOrEmpty(Field))
        {
            _fieldState = PermissionService.GetFieldState(Entity, Field, IsEditMode);
        }
        else
        {
            // Fără entitate/câmp specificat, comportament default
            _fieldState = IsEditMode ? FieldState.Editable : FieldState.ReadOnly;
        }
        
        // Auto-generate ID if not provided
        if (string.IsNullOrEmpty(Id))
        {
            Id = $"{Entity?.ToLower()}-{Field?.ToLower()}";
        }
    }
    
    private string GetInputCssClass()
    {
        var classes = new List<string> { "form-control" };
        
        if (_fieldState == FieldState.ReadOnly)
        {
            classes.Add("readonly-field");
        }
        
        return string.Join(" ", classes);
    }
}
