using Microsoft.AspNetCore.Components;
using ValyanClinic.Application.Interfaces;
using Microsoft.Extensions.Logging;
using Syncfusion.Blazor.DropDowns;
using ValyanClinic.Domain.Models;

namespace ValyanClinic.Components.Shared;

/// <summary>
/// Code-behind pentru LocationDependentGridDropdowns
/// Varianta optimizata pentru integrarea in grid-ulformularului
/// </summary>
public partial class LocationDependentGridDropdowns : ComponentBase, IDisposable
{
    #region Parameters si Dependency Injection

    // Proprietati publice pentru binding
    [Parameter] public int? SelectedJudetId { get; set; }
    [Parameter] public EventCallback<int?> SelectedJudetIdChanged { get; set; }
    
    [Parameter] public int? SelectedLocalitateId { get; set; }
    [Parameter] public EventCallback<int?> SelectedLocalitateIdChanged { get; set; }
    
    // Labels si placeholders
    [Parameter] public string JudetLabel { get; set; } = "Judet";
    [Parameter] public string JudetPlaceholder { get; set; } = "-- Selecteaza judetul --";
    [Parameter] public string LocalitateLabel { get; set; } = "Localitate";
    [Parameter] public string LocalitatePlaceholder { get; set; } = "-- Selecteaza localitatea --";
    
    // Event callbacks pentru valori text
    [Parameter] public EventCallback<string?> OnJudetNameChanged { get; set; }
    [Parameter] public EventCallback<string?> OnLocalitateNameChanged { get; set; }
    
    // Servicii
    [Inject] private ILocationService LocationService { get; set; } = default!;
    [Inject] private ILogger<LocationDependentGridDropdowns> Logger { get; set; } = default!;
    [Inject] private IServiceProvider ServiceProvider { get; set; } = default!;

    #endregion

    #region Private Fields

    private LocationDependentState? _state;
    private bool _disposed = false;

    #endregion

    #region Public Properties pentru accesul din markup

    /// <summary>
    /// Judetele disponibile pentru dropdown
    /// </summary>
    public List<Judet> Judete => _state?.Judete ?? new List<Judet>();

    /// <summary>
    /// Localitatile disponibile pentru dropdown
    /// </summary>
    public List<Localitate> Localitati => _state?.Localitati ?? new List<Localitate>();

    /// <summary>
    /// Indica daca se incarca judetele
    /// </summary>
    public bool IsLoadingJudete => _state?.IsLoadingJudete ?? false;

    /// <summary>
    /// Indica daca se incarca localitatile
    /// </summary>
    public bool IsLoadingLocalitati => _state?.IsLoadingLocalitati ?? false;

    /// <summary>
    /// Mesajul de eroare curent
    /// </summary>
    public string? ErrorMessage => _state?.ErrorMessage;

    /// <summary>
    /// Indica daca dropdown-ul de localitati este activat
    /// </summary>
    public bool IsLocalitateEnabled => SelectedJudetId.HasValue && SelectedJudetId > 0;

    #endregion

    #region Lifecycle Methods

    protected override async Task OnInitializedAsync()
    {
        try
        {
            Logger.LogInformation("🚀 LocationDependentGridDropdowns initializing...");
            
            // Creeaza state management instance
            var logger = ServiceProvider.GetRequiredService<ILogger<LocationDependentState>>();
            _state = new LocationDependentState(LocationService, logger);
            
            Logger.LogInformation("✅ State management instance created");
            
            // Aboneaza-te la evenimente
            _state.StateChanged += OnStateChanged;
            _state.JudetNameChanged += OnJudetNameChangedInternal;
            _state.LocalitateNameChanged += OnLocalitateNameChangedInternal;

            Logger.LogInformation("✅ Event handlers subscribed");

            // Seteaza valorile initiale in state
            _state.SelectedJudetId = SelectedJudetId;
            _state.SelectedLocalitateId = SelectedLocalitateId;

            Logger.LogInformation("📊 Initial values set - JudetId: {JudetId}, LocalitateId: {LocalitateId}", 
                SelectedJudetId, SelectedLocalitateId);

            // Initializeaza state-ul
            Logger.LogInformation("🔄 Starting state initialization...");
            await _state.InitializeAsync();
            
            Logger.LogInformation("🎉 LocationDependentGridDropdowns initialized successfully! Judete count: {JudeteCount}", 
                Judete.Count);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "💥 CRITICAL ERROR initializing LocationDependentGridDropdowns");
            throw; // Re-throw pentru a vedea eroarea in UI
        }
    }

    protected override async Task OnParametersSetAsync()
    {
        Logger.LogInformation("🔥 OnParametersSetAsync called - Parameter values: JudetId={JudetId}, LocalitateId={LocalitateId}", 
            SelectedJudetId, SelectedLocalitateId);

        // Sincronizeaza parametrii cu state-ul DOAR daca sunt diferite
        // pentru a evita reset-urile accidentale
        if (_state != null)
        {
            Logger.LogInformation("🔥 Current state values: State.JudetId={StateJudetId}, State.LocalitateId={StateLocalitateId}", 
                _state.SelectedJudetId, _state.SelectedLocalitateId);

            // Verifica daca valorile au fost schimbate din exterior
            bool judetChanged = _state.SelectedJudetId != SelectedJudetId;
            bool localitateChanged = _state.SelectedLocalitateId != SelectedLocalitateId;
            
            Logger.LogInformation("🔥 Change detection: judetChanged={JudetChanged}, localitateChanged={LocalitateChanged}", 
                judetChanged, localitateChanged);
            
            if (judetChanged)
            {
                Logger.LogInformation("🔄 External judet change detected: {OldValue} → {NewValue}", 
                    _state.SelectedJudetId, SelectedJudetId);
                _state.SelectedJudetId = SelectedJudetId;
                
                // Daca judetul s-a schimbat din exterior, incarca localitatile
                if (SelectedJudetId.HasValue && SelectedJudetId > 0)
                {
                    await _state.LoadLocalitatiAsync(SelectedJudetId.Value);
                }
            }
            
            if (localitateChanged && !judetChanged) // Nu reseteaza localitatea daca judetul s-a schimbat
            {
                Logger.LogWarning("🔥 ⚠️ ALERT: External localitate change detected: {OldValue} → {NewValue} - This might be the RESET!", 
                    _state.SelectedLocalitateId, SelectedLocalitateId);
                _state.SelectedLocalitateId = SelectedLocalitateId;
            }
        }

        await base.OnParametersSetAsync();
    }

    #endregion

    #region Event Handlers pentru Syncfusion Components

    /// <summary>
    /// Handler pentru schimbarea judetului in dropdown
    /// </summary>
    public async Task OnJudetChangedAsync(ChangeEventArgs<int?, Judet> args)
    {
        try
        {
            if (_state == null) return;

            var judetId = args.Value;
            var judetName = args.ItemData?.Nume;

            Logger.LogInformation("🔥 JUDET STEP 1: OnJudetChangedAsync called - JudetId: {JudetId}, Name: {Name}", 
                judetId, judetName);

            // PROTECtIE iMPOTRIVA EVENIMENTELOR SPURIOASE NULL
            // Daca primim null imediat dupa o valoare valida, ignora
            if (judetId == null && _state.SelectedJudetId.HasValue)
            {
                Logger.LogWarning("🚫 IGNORING SPURIOUS JUDET NULL EVENT - State has valid value: {ValidValue}", 
                    _state.SelectedJudetId);
                return;
            }

            // Actualizeaza state-ul
            await _state.ChangeJudetAsync(judetId, judetName);

            // Notifica parintele despre schimbare
            await SelectedJudetIdChanged.InvokeAsync(judetId);
            await SelectedLocalitateIdChanged.InvokeAsync(null); // Reset localitate

            Logger.LogInformation("Judet grid dropdown changed to: {JudetId} - {JudetName}", judetId, judetName);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error handling judet change in grid dropdown");
        }
    }

    /// <summary>
    /// Handler pentru schimbarea localitatii in dropdown
    /// </summary>
    public async Task OnLocalitateChangedAsync(ChangeEventArgs<int?, Localitate> args)
    {
        try
        {
            if (_state == null) return;

            var localitateId = args.Value;
            var localitateName = args.ItemData?.Nume;

            Logger.LogInformation("🔥 STEP 1: OnLocalitateChangedAsync called - LocalitateId: {LocalitateId}, Name: {Name}", 
                localitateId, localitateName);

            // PROTECtIE iMPOTRIVA EVENIMENTELOR SPURIOASE NULL
            // Daca primim null imediat dupa o valoare valida, ignora
            if (localitateId == null && _state.SelectedLocalitateId.HasValue)
            {
                Logger.LogWarning("🚫 IGNORING SPURIOUS NULL EVENT - State has valid value: {ValidValue}", 
                    _state.SelectedLocalitateId);
                return;
            }

            // Actualizeaza state-ul
            _state.ChangeLocalitate(localitateId, localitateName);
            
            Logger.LogInformation("🔥 STEP 2: State updated - State.SelectedLocalitateId: {StateId}", 
                _state.SelectedLocalitateId);

            // Notifica parintele despre schimbare
            Logger.LogInformation("🔥 STEP 3: About to notify parent - Parameter SelectedLocalitateId: {ParamId}", 
                SelectedLocalitateId);
            
            await SelectedLocalitateIdChanged.InvokeAsync(localitateId);
            
            Logger.LogInformation("🔥 STEP 4: Parent notified - New Parameter value should be: {NewValue}", 
                localitateId);

            Logger.LogInformation("Localitate grid dropdown changed to: {LocalitateId} - {LocalitateName}", 
                localitateId, localitateName);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "💥 Error handling localitate change in grid dropdown");
        }
    }

    #endregion

    #region Internal Event Handlers

    /// <summary>
    /// Handler intern pentru schimbarile de state
    /// </summary>
    private void OnStateChanged()
    {
        InvokeAsync(StateHasChanged);
    }

    /// <summary>
    /// Handler intern pentru schimbarea numelui de judet
    /// </summary>
    private void OnJudetNameChangedInternal(string? judetName)
    {
        // Nu apela StateHasChanged aici pentru ca parintele ar trebui sa se actualizeze singur
        // dupa ce primeste callback-ul
        InvokeAsync(async () => 
        {
            Logger.LogDebug("🏛️ Notifying parent about judet name change: {Name}", judetName);
            await OnJudetNameChanged.InvokeAsync(judetName);
        });
    }

    /// <summary>
    /// Handler intern pentru schimbarea numelui de localitate
    /// </summary>
    private void OnLocalitateNameChangedInternal(string? localitateName)
    {
        // Nu apela StateHasChanged aici pentru ca parintele ar trebui sa se actualizeze singur
        // dupa ce primeste callback-ul
        InvokeAsync(async () => 
        {
            Logger.LogDebug("🏠 Notifying parent about localitate name change: {Name}", localitateName);
            await OnLocalitateNameChanged.InvokeAsync(localitateName);
        });
    }

    #endregion

    #region Public Methods pentru control din exterior

    /// <summary>
    /// Seteaza judetul pe baza numelui (pentru editare)
    /// </summary>
    public async Task SetJudetByNameAsync(string? judetName)
    {
        if (_state != null)
        {
            await _state.SetJudetByNameAsync(judetName);
        }
    }

    /// <summary>
    /// Seteaza localitatea pe baza numelui (pentru editare)
    /// </summary>
    public void SetLocalitateByName(string? localitateName)
    {
        _state?.SetLocalitateByName(localitateName);
    }

    /// <summary>
    /// Reseteaza componenta la starea initiala
    /// </summary>
    public void Reset()
    {
        _state?.Reset();
    }

    #endregion

    #region IDisposable Implementation

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed && disposing)
        {
            if (_state != null)
            {
                // Dezaboneaza-te de la evenimente
                _state.StateChanged -= OnStateChanged;
                _state.JudetNameChanged -= OnJudetNameChangedInternal;
                _state.LocalitateNameChanged -= OnLocalitateNameChangedInternal;
            }
            _disposed = true;
        }
    }

    #endregion
}
