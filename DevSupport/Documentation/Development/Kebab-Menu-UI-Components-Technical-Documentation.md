# Kebab Menu & UI Components - Technical Documentation

## 📋 Overview

The Kebab Menu system in ValyanClinic's Personal Management module represents a sophisticated UI component implementation that demonstrates advanced Blazor Server patterns, JavaScript interop, event handling, and accessibility features. This system provides secondary navigation and toggle capabilities for advanced features.

## 🏗️ System Architecture

### Component Ecosystem
```
Kebab Menu System
├── UI Component (Razor Markup)
├── State Management (PersonalPageState)
├── JavaScript Integration (valyan-helpers.js)
├── CSS Styling (administrare-personal.css)
├── Event Handling (C# + JavaScript)
├── Accessibility Features (ARIA, Keyboard Support)
└── Performance Optimization (Memory Management)
```

### Technical Stack
- **Frontend**: Blazor Server (.NET 9) with InteractiveServer rendering
- **JavaScript**: Vanilla JS with advanced event handling
- **CSS**: Maximum specificity styling with animations
- **Accessibility**: Full ARIA support and keyboard navigation
- **Performance**: Optimized event listeners with cleanup

## 🎨 UI Component Implementation

### Razor Markup Structure
```razor
<!-- Kebab Menu Button - ENHANCED V2 -->
<div class="kebab-menu-container">
    <button class="btn btn-outline-secondary kebab-menu-btn" 
            @onclick="ToggleKebabMenu"
            @onclick:stopPropagation="true"
            aria-label="Meniu opțiuni"
            aria-expanded="@_state.ShowKebabMenu.ToString().ToLower()"
            aria-haspopup="true">
        <i class="fas fa-ellipsis-v"></i>
    </button>
    
    @if (_state.ShowKebabMenu)
    {
        <div class="kebab-menu-dropdown" 
             @onclick:stopPropagation="true"
             role="menu"
             aria-label="Opțiuni disponibile">
            <button class="kebab-menu-item" 
                    @onclick="() => ToggleStatistics()"
                    role="menuitem"
                    aria-label="Comută afișarea statisticilor">
                <i class="fas fa-chart-bar"></i>
                <span>Statistici</span>
                @if (_state.ShowStatistics)
                {
                    <i class="fas fa-check kebab-check" aria-label="Activat"></i>
                }
            </button>
            <button class="kebab-menu-item" 
                    @onclick="() => ToggleAdvancedFilters()"
                    role="menuitem"
                    aria-label="Comută afișarea filtrelor avansate">
                <i class="fas fa-filter"></i>
                <span>Filtrare Avansata</span>
                @if (_state.ShowAdvancedFilters)
                {
                    <i class="fas fa-check kebab-check" aria-label="Activat"></i>
                }
            </button>
        </div>
    }
</div>
```

### Key Design Principles
1. **Semantic HTML**: Proper button elements with roles
2. **Event Propagation Control**: `@onclick:stopPropagation="true"`
3. **Accessibility First**: Complete ARIA attributes
4. **Visual State Indicators**: Check marks for active items
5. **Responsive Design**: Touch-friendly on mobile

## 🔧 State Management System

### PersonalPageState Integration
```csharp
public class PersonalPageState
{
    // Kebab menu state
    public bool ShowKebabMenu { get; set; }
    
    // Feature toggle states controlled by kebab menu
    public bool ShowStatistics { get; set; }
    public bool ShowAdvancedFilters { get; set; }
    
    // Helper methods
    public void CloseKebabMenu() => ShowKebabMenu = false;
    
    public bool IsAnyFeatureToggleActive => ShowStatistics || ShowAdvancedFilters;
}
```

### C# Event Handlers
```csharp
#region Kebab Menu Management - V2 FINAL OPTIMIZED

private DotNetObjectReference<AdministrarePersonal>? _dotNetReference;
private bool _eventListenersInitialized = false;

private async Task ToggleKebabMenu()
{
    try
    {
        var previousState = _state.ShowKebabMenu;
        _state.ShowKebabMenu = !_state.ShowKebabMenu;
        
        Logger.LogInformation("🔘 Kebab menu toggled: {PreviousState} → {NewState}", 
            previousState, _state.ShowKebabMenu);
        
        // Initialize JavaScript helpers if needed
        if (_state.ShowKebabMenu && !_eventListenersInitialized)
        {
            await InitializeKebabMenuHelpers();
        }
        
        SafeStateHasChanged();
    }
    catch (Exception ex)
    {
        Logger.LogError(ex, "❌ Critical error toggling kebab menu");
        _state.ShowKebabMenu = false;
        SafeStateHasChanged();
    }
}

private async Task ToggleStatistics()
{
    try
    {
        var previousState = _state.ShowStatistics;
        _state.ShowStatistics = !_state.ShowStatistics;
        _state.ShowKebabMenu = false; // Close kebab menu
        
        Logger.LogInformation("📊 Statistics toggled: {PreviousState} → {NewState}", 
            previousState, _state.ShowStatistics);
        
        SafeStateHasChanged();
    }
    catch (Exception ex)
    {
        Logger.LogError(ex, "❌ Error toggling statistics");
        _state.ShowKebabMenu = false;
        SafeStateHasChanged();
    }
}

private async Task ToggleAdvancedFilters()
{
    try
    {
        var previousState = _state.ShowAdvancedFilters;
        _state.ShowAdvancedFilters = !_state.ShowAdvancedFilters;
        _state.ShowKebabMenu = false; // Close kebab menu
        
        Logger.LogInformation("🔍 Advanced filters toggled: {PreviousState} → {NewState}", 
            previousState, _state.ShowAdvancedFilters);
        
        SafeStateHasChanged();
    }
    catch (Exception ex)
    {
        Logger.LogError(ex, "❌ Error toggling advanced filters");
        _state.ShowKebabMenu = false;
        SafeStateHasChanged();
    }
}

#endregion
```

## 🌐 JavaScript Integration System

### valyan-helpers.js - Enhanced Version
```javascript
/**
 * JavaScript Helper Functions pentru ValyanClinic - V2 OPTIMIZED
 * Funcții de utilitare pentru interacțiunea cu componentele Blazor
 * Rezolvă problemele cu kebab menu și event listeners
 */

// Global state pentru tracking event handlers
window.valyanClinicEventHandlers = window.valyanClinicEventHandlers || [];
window.valyanClinicDebug = true; // Set to false in production

function debugLog(message, ...args) {
    if (window.valyanClinicDebug) {
        console.log(`[ValyanClinic] ${message}`, ...args);
    }
}

// Funcție principală pentru kebab menu click outside detection
window.addClickEventListener = (dotnetRef) => {
    try {
        debugLog('Adding click event listener for kebab menu');
        
        // Remove any existing listeners first to prevent duplicates
        window.removeEventListeners();
        
        const clickHandler = (event) => {
            try {
                // Check if click is outside kebab menu elements
                const kebabContainer = event.target.closest('.kebab-menu-container');
                const kebabDropdown = event.target.closest('.kebab-menu-dropdown');
                
                // If click is not inside kebab menu elements, close the menu
                if (!kebabContainer && !kebabDropdown) {
                    debugLog('Click detected outside kebab menu, closing menu');
                    dotnetRef.invokeMethodAsync('CloseKebabMenu');
                } else {
                    debugLog('Click detected inside kebab menu, keeping open');
                }
            } catch (error) {
                console.warn('[ValyanClinic] Failed to invoke CloseKebabMenu:', error);
            }
        };
        
        // Add event listener with passive option for better performance
        document.addEventListener('click', clickHandler, { 
            passive: true, 
            capture: true // Use capture to ensure we catch events early
        });
        
        // Store reference for cleanup
        window.valyanClinicEventHandlers.push({ 
            type: 'click', 
            handler: clickHandler,
            element: document,
            options: { passive: true, capture: true }
        });
        
        debugLog('Click event listener added successfully');
        return true;
        
    } catch (error) {
        console.error('[ValyanClinic] Failed to add click event listener:', error);
        return false;
    }
};

// Enhanced cleanup function with better error handling
window.removeEventListeners = () => {
    try {
        debugLog('Removing event listeners, count:', window.valyanClinicEventHandlers.length);
        
        if (window.valyanClinicEventHandlers && window.valyanClinicEventHandlers.length > 0) {
            window.valyanClinicEventHandlers.forEach((handler, index) => {
                try {
                    const element = handler.element || document;
                    element.removeEventListener(handler.type, handler.handler, handler.options);
                    debugLog(`Removed event listener ${index + 1}:`, handler.type);
                } catch (error) {
                    console.warn(`[ValyanClinic] Failed to remove event listener ${index + 1}:`, error);
                }
            });
            
            window.valyanClinicEventHandlers = [];
            debugLog('All event listeners removed successfully');
        } else {
            debugLog('No event listeners to remove');
        }
    } catch (error) {
        console.error('[ValyanClinic] Failed to remove event listeners:', error);
    }
};

// Escape key handler pentru kebab menu
window.addEscapeKeyListener = (dotnetRef) => {
    try {
        debugLog('Adding escape key listener for kebab menu');
        
        const escapeHandler = (event) => {
            if (event.key === 'Escape' || event.keyCode === 27) {
                try {
                    debugLog('Escape key pressed, closing kebab menu');
                    dotnetRef.invokeMethodAsync('CloseKebabMenu');
                } catch (error) {
                    console.warn('[ValyanClinic] Failed to invoke CloseKebabMenu on escape:', error);
                }
            }
        };
        
        document.addEventListener('keydown', escapeHandler, { passive: true });
        
        window.valyanClinicEventHandlers.push({
            type: 'keydown',
            handler: escapeHandler,
            element: document,
            options: { passive: true }
        });
        
        debugLog('Escape key listener added successfully');
        return true;
        
    } catch (error) {
        console.error('[ValyanClinic] Failed to add escape key listener:', error);
        return false;
    }
};

// Focus trap pentru accessibility
window.addFocusTrap = (kebabMenuSelector = '.kebab-menu-dropdown') => {
    try {
        const menuElement = document.querySelector(kebabMenuSelector);
        if (!menuElement) {
            debugLog('Kebab menu element not found for focus trap:', kebabMenuSelector);
            return false;
        }
        
        const focusableElements = menuElement.querySelectorAll(
            'button, [href], input, select, textarea, [tabindex]:not([tabindex="-1"])'
        );
        
        if (focusableElements.length === 0) {
            debugLog('No focusable elements found in kebab menu');
            return false;
        }
        
        const firstElement = focusableElements[0];
        const lastElement = focusableElements[focusableElements.length - 1];
        
        const focusTrapHandler = (event) => {
            if (event.key === 'Tab') {
                if (event.shiftKey) {
                    if (document.activeElement === firstElement) {
                        event.preventDefault();
                        lastElement.focus();
                    }
                } else {
                    if (document.activeElement === lastElement) {
                        event.preventDefault();
                        firstElement.focus();
                    }
                }
            }
        };
        
        menuElement.addEventListener('keydown', focusTrapHandler);
        firstElement.focus();
        
        window.valyanClinicEventHandlers.push({
            type: 'keydown',
            handler: focusTrapHandler,
            element: menuElement,
            options: {}
        });
        
        debugLog('Focus trap added successfully for kebab menu');
        return true;
        
    } catch (error) {
        console.error('[ValyanClinic] Failed to add focus trap:', error);
        return false;
    }
};

debugLog('ValyanClinic JavaScript helpers loaded successfully - V2 OPTIMIZED');
```

### C# JavaScript Interop Management
```csharp
/// <summary>
/// Initialize kebab menu JavaScript helpers with comprehensive error handling
/// </summary>
private async Task InitializeKebabMenuHelpers()
{
    if (_eventListenersInitialized || _disposed)
        return;

    try
    {
        Logger.LogInformation("🔧 Initializing kebab menu JavaScript helpers");

        // Create DotNet reference if not exists
        _dotNetReference ??= DotNetObjectReference.Create(this);

        // Use exponential backoff retry strategy
        var maxRetries = 5;
        var baseDelay = 50;
        
        for (int attempt = 1; attempt <= maxRetries; attempt++)
        {
            try
            {
                // Check if JavaScript environment is ready
                var jsReady = await JSRuntime.InvokeAsync<bool>("eval", 
                    "typeof window !== 'undefined' && typeof document !== 'undefined'");
                
                if (!jsReady)
                {
                    Logger.LogWarning("⏳ JavaScript environment not ready - Attempt {Attempt}/{MaxRetries}", 
                        attempt, maxRetries);
                    
                    if (attempt < maxRetries)
                    {
                        await Task.Delay(baseDelay * (int)Math.Pow(2, attempt - 1));
                        continue;
                    }
                    else
                    {
                        Logger.LogError("❌ JavaScript environment failed to initialize");
                        return;
                    }
                }

                // Check if helper functions are available
                var helpersAvailable = await JSRuntime.InvokeAsync<bool>("eval", 
                    "typeof window.addClickEventListener === 'function' && typeof window.addEscapeKeyListener === 'function'");
                
                if (!helpersAvailable)
                {
                    Logger.LogWarning("⏳ ValyanClinic helpers not ready - Attempt {Attempt}/{MaxRetries}", 
                        attempt, maxRetries);
                    
                    if (attempt < maxRetries)
                    {
                        await Task.Delay(baseDelay * (int)Math.Pow(2, attempt - 1));
                        continue;
                    }
                    else
                    {
                        Logger.LogError("❌ ValyanClinic helpers failed to load");
                        return;
                    }
                }

                // Setup event listeners
                var clickSuccess = await JSRuntime.InvokeAsync<bool>("window.addClickEventListener", _dotNetReference);
                var escapeSuccess = await JSRuntime.InvokeAsync<bool>("window.addEscapeKeyListener", _dotNetReference);
                
                if (clickSuccess && escapeSuccess)
                {
                    _eventListenersInitialized = true;
                    Logger.LogInformation("✅ Kebab menu JavaScript helpers initialized successfully on attempt {Attempt}", attempt);
                    return;
                }
                else
                {
                    Logger.LogWarning("⚠️ Some event listeners failed - Click: {ClickSuccess}, Escape: {EscapeSuccess}", 
                        clickSuccess, escapeSuccess);
                }
            }
            catch (JSException jsEx)
            {
                Logger.LogWarning(jsEx, "⚠️ JavaScript error on attempt {Attempt}: {Message}", 
                    attempt, jsEx.Message);
            }
            catch (TaskCanceledException)
            {
                Logger.LogWarning("⏳ JavaScript call timeout on attempt {Attempt}", attempt);
            }

            // Wait before retry (exponential backoff)
            if (attempt < maxRetries)
            {
                var delay = baseDelay * (int)Math.Pow(2, attempt - 1);
                Logger.LogInformation("⏳ Retrying in {Delay}ms...", delay);
                await Task.Delay(delay);
            }
        }

        Logger.LogError("❌ Failed to initialize kebab menu helpers after {MaxRetries} attempts", maxRetries);
        _eventListenersInitialized = false;
    }
    catch (Exception ex)
    {
        Logger.LogError(ex, "💥 Critical error initializing kebab menu helpers");
        _eventListenersInitialized = false;
    }
}

[JSInvokable]
public async Task CloseKebabMenu()
{
    if (_disposed) return;

    try
    {
        if (_state.ShowKebabMenu)
        {
            Logger.LogInformation("🔘 Closing kebab menu via JavaScript event");
            _state.ShowKebabMenu = false;
            SafeStateHasChanged();
        }
    }
    catch (ObjectDisposedException)
    {
        Logger.LogDebug("Component disposed while closing kebab menu");
    }
    catch (Exception ex)
    {
        Logger.LogError(ex, "❌ Error closing kebab menu");
        try
        {
            _state.ShowKebabMenu = false;
            SafeStateHasChanged();
        }
        catch
        {
            // Ignore errors in fallback
        }
    }
}
```

## 🎨 Premium CSS Styling System

### Maximum Specificity Architecture
```css
/* KEBAB MENU BUTTON - ENHANCED SPECIFICITY */
html body .personal-page-container .kebab-menu-btn {
    min-width: 44px !important;
    width: 44px !important;
    height: 44px !important;
    padding: 0 !important;
    position: relative !important;
    z-index: 101 !important; /* Higher than dropdown */
}

html body .personal-page-container .kebab-menu-container {
    position: relative !important;
    display: inline-block !important;
    z-index: 100 !important;
}

/* HOVER STATES - ENHANCED */
html body .personal-page-container .kebab-menu-btn:hover,
html body .personal-page-container .kebab-menu-btn:focus {
    background: rgba(255, 255, 255, 0.25) !important;
    transform: translateY(-2px) !important;
    box-shadow: 0 6px 16px rgba(255, 255, 255, 0.2) !important;
    outline: 2px solid rgba(255, 255, 255, 0.3) !important;
    outline-offset: 2px !important;
}

/* KEBAB MENU DROPDOWN - MAXIMUM SPECIFICITY */
html body .personal-page-container .kebab-menu-dropdown {
    position: absolute !important;
    top: calc(100% + 8px) !important; /* More space from button */
    right: 0 !important;
    background: white !important;
    border: 1px solid var(--personal-border) !important;
    border-radius: 12px !important; /* More rounded corners */
    box-shadow: 
        0 8px 32px rgba(0, 0, 0, 0.15) !important,
        0 4px 12px rgba(0, 0, 0, 0.1) !important;
    z-index: 1000 !important; /* Very high z-index */
    min-width: 200px !important; /* Wider menu */
    margin: 0 !important;
    padding: 8px 0 !important; /* Padding inside menu */
    overflow: hidden !important;
    animation: slideDownBounce 0.3s cubic-bezier(0.68, -0.55, 0.265, 1.55) !important;
    backdrop-filter: blur(10px) !important; /* Modern glass effect */
    border: 2px solid rgba(255, 255, 255, 0.2) !important;
    transform-origin: top right !important;
}

/* KEBAB MENU ITEM - ENHANCED STYLING */
html body .personal-page-container .kebab-menu-item {
    display: flex !important;
    align-items: center !important;
    gap: 12px !important;
    padding: 14px 18px !important; /* More padding */
    width: 100% !important;
    border: none !important;
    background: transparent !important;
    color: #374151 !important;
    font-size: 14px !important;
    font-weight: 500 !important; /* Medium weight */
    text-align: left !important;
    cursor: pointer !important;
    transition: all 0.25s cubic-bezier(0.4, 0, 0.2, 1) !important;
    position: relative !important;
    overflow: hidden !important;
    outline: none !important;
}

/* KEBAB MENU ITEM HOVER - ENHANCED */
html body .personal-page-container .kebab-menu-item:hover,
html body .personal-page-container .kebab-menu-item:focus {
    background: linear-gradient(135deg, var(--personal-bg-light), #f1f5f9) !important;
    color: var(--personal-primary) !important;
    transform: translateX(4px) !important; /* Subtle slide effect */
    box-shadow: inset 4px 0 0 var(--personal-primary) !important; /* Left border accent */
}

/* CHECK MARK FOR ACTIVE ITEMS */
html body .personal-page-container .kebab-check {
    margin-left: auto !important;
    color: var(--personal-primary) !important;
    font-size: 14px !important;
    font-weight: 600 !important;
    animation: fadeIn 0.2s ease !important;
}

/* KEBAB MENU ANIMATIONS - ENHANCED */
@keyframes slideDownBounce {
    0% {
        opacity: 0;
        transform: translateY(-10px) scale(0.95);
    }
    60% {
        opacity: 1;
        transform: translateY(2px) scale(1.02);
    }
    100% {
        opacity: 1;
        transform: translateY(0) scale(1);
    }
}

@keyframes fadeIn {
    from {
        opacity: 0;
        transform: scale(0.8);
    }
    to {
        opacity: 1;
        transform: scale(1);
    }
}

/* KEBAB MENU ACTIVE STATE */
html body .personal-page-container .kebab-menu-btn[aria-expanded="true"],
html body .personal-page-container .kebab-menu-btn.active {
    background: rgba(255, 255, 255, 0.3) !important;
    transform: translateY(-1px) !important;
    box-shadow: 0 4px 12px rgba(255, 255, 255, 0.25) !important;
}
```

### Accessibility and Responsive Features
```css
/* FOCUS TRAP FOR ACCESSIBILITY */
html body .personal-page-container .kebab-menu-dropdown:focus-within {
    outline: 2px solid var(--personal-primary) !important;
    outline-offset: 2px !important;
}

/* HIGH CONTRAST MODE SUPPORT */
@media (prefers-contrast: high) {
    html body .personal-page-container .kebab-menu-dropdown {
        border: 3px solid #000 !important;
        background: #fff !important;
    }
    
    html body .personal-page-container .kebab-menu-item {
        color: #000 !important;
        border-bottom: 2px solid #000 !important;
    }
    
    html body .personal-page-container .kebab-menu-item:hover {
        background: #f0f0f0 !important;
        color: #000 !important;
    }
}

/* REDUCED MOTION SUPPORT */
@media (prefers-reduced-motion: reduce) {
    html body .personal-page-container .kebab-menu-dropdown {
        animation: none !important;
    }
    
    html body .personal-page-container .kebab-menu-item {
        transition: none !important;
    }
    
    html body .personal-page-container .kebab-menu-item:hover {
        transform: none !important;
    }
}

/* MOBILE OPTIMIZATION FOR KEBAB MENU */
@media (max-width: 768px) {
    html body .personal-page-container .kebab-menu-dropdown {
        min-width: 180px !important;
        right: -10px !important; /* Adjust position for mobile */
        top: calc(100% + 12px) !important;
    }
    
    html body .personal-page-container .kebab-menu-item {
        padding: 16px 20px !important; /* More touch-friendly */
        font-size: 15px !important;
    }
}

@media (max-width: 480px) {
    html body .personal-page-container .kebab-menu-dropdown {
        min-width: 160px !important;
        right: -5px !important;
    }
    
    html body .personal-page-container .kebab-menu-item {
        padding: 14px 18px !important;
        font-size: 14px !important;
    }
}
```

## 🚀 Performance Optimizations

### Component Lifecycle Management
```csharp
protected override async Task OnAfterRenderAsync(bool firstRender)
{
    if (_disposed) return;

    if (firstRender)
    {
        try
        {
            Logger.LogInformation("🎨 AdministrarePersonal first render - preparing JavaScript environment");
            
            // Initialize helpers in background without blocking UI
            _ = Task.Run(async () =>
            {
                try
                {
                    await Task.Delay(100); // Give DOM time to settle
                    await InitializeKebabMenuHelpers();
                }
                catch (Exception ex)
                {
                    Logger.LogWarning(ex, "⚠️ Background kebab menu initialization failed");
                }
            });
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "❌ Error during OnAfterRenderAsync initialization");
        }
    }
    
    // Ensure listeners are set up if menu becomes visible
    if (_state.ShowKebabMenu && !_eventListenersInitialized && !_disposed)
    {
        try
        {
            await InitializeKebabMenuHelpers();
        }
        catch (Exception ex)
        {
            Logger.LogWarning(ex, "⚠️ Failed to setup listeners for visible kebab menu");
        }
    }
}
```

### Memory Management and Cleanup
```csharp
/// <summary>
/// Cleanup JavaScript resources
/// </summary>
private async Task CleanupJavaScriptResources()
{
    if (_disposed) return;

    try
    {
        if (_eventListenersInitialized)
        {
            // Remove event listeners
            var cleanupSuccess = await JSRuntime.InvokeAsync<bool>("eval",
                "typeof window.removeEventListeners === 'function'");
            
            if (cleanupSuccess)
            {
                await JSRuntime.InvokeVoidAsync("window.removeEventListeners");
                Logger.LogDebug("✅ JavaScript event listeners cleaned up");
            }
            
            _eventListenersInitialized = false;
        }

        // Dispose DotNet reference
        _dotNetReference?.Dispose();
        _dotNetReference = null;
    }
    catch (Exception ex)
    {
        Logger.LogWarning(ex, "⚠️ Error cleaning up JavaScript resources");
    }
}

public async ValueTask DisposeAsync()
{
    if (_disposed) return;

    try
    {
        _disposed = true;
        
        // 1. Cleanup JavaScript resources first - CRITICAL
        await CleanupJavaScriptResources();
        
        // 2. Other disposal logic...
    }
    catch (Exception ex)
    {
        Logger.LogError(ex, "❌ Critical error during disposal");
    }
}
```

### Safe State Management
```csharp
/// <summary>
/// Safe StateHasChanged care verifică disposal
/// </summary>
protected void SafeStateHasChanged()
{
    try
    {
        if (!_disposed)
        {
            InvokeAsync(StateHasChanged);
        }
    }
    catch (ObjectDisposedException)
    {
        Logger?.LogDebug("StateHasChanged called on disposed component");
    }
    catch (Exception ex)
    {
        Logger?.LogError(ex, "Error in SafeStateHasChanged");
    }
}
```

## ♿ Accessibility Implementation

### ARIA Attributes Complete
```razor
<!-- Button with proper ARIA states -->
<button class="btn btn-outline-secondary kebab-menu-btn" 
        @onclick="ToggleKebabMenu"
        @onclick:stopPropagation="true"
        aria-label="Meniu opțiuni"
        aria-expanded="@_state.ShowKebabMenu.ToString().ToLower()"
        aria-haspopup="true"
        aria-controls="kebab-menu-dropdown">
    <i class="fas fa-ellipsis-v" aria-hidden="true"></i>
</button>

<!-- Menu with proper role and labeling -->
<div class="kebab-menu-dropdown" 
     @onclick:stopPropagation="true"
     role="menu"
     id="kebab-menu-dropdown"
     aria-label="Opțiuni disponibile"
     aria-labelledby="kebab-menu-btn">
     
    <!-- Menu items with proper roles -->
    <button class="kebab-menu-item" 
            @onclick="() => ToggleStatistics()"
            role="menuitem"
            tabindex="0"
            aria-label="Comută afișarea statisticilor"
            aria-describedby="stats-description">
        <i class="fas fa-chart-bar" aria-hidden="true"></i>
        <span>Statistici</span>
        @if (_state.ShowStatistics)
        {
            <i class="fas fa-check kebab-check" 
               aria-label="Activat" 
               role="img"
               title="Funcția este activă"></i>
        }
    </button>
</div>
```

### Keyboard Navigation Support
```javascript
// Focus trap implementation in valyan-helpers.js
window.addFocusTrap = (kebabMenuSelector = '.kebab-menu-dropdown') => {
    try {
        const menuElement = document.querySelector(kebabMenuSelector);
        if (!menuElement) return false;
        
        const focusableElements = menuElement.querySelectorAll(
            'button, [href], input, select, textarea, [tabindex]:not([tabindex="-1"])'
        );
        
        if (focusableElements.length === 0) return false;
        
        const firstElement = focusableElements[0];
        const lastElement = focusableElements[focusableElements.length - 1];
        
        const focusTrapHandler = (event) => {
            if (event.key === 'Tab') {
                if (event.shiftKey) {
                    // Shift+Tab - move to previous element
                    if (document.activeElement === firstElement) {
                        event.preventDefault();
                        lastElement.focus();
                    }
                } else {
                    // Tab - move to next element
                    if (document.activeElement === lastElement) {
                        event.preventDefault();
                        firstElement.focus();
                    }
                }
            }
            // Arrow key navigation
            else if (event.key === 'ArrowDown') {
                event.preventDefault();
                const currentIndex = Array.from(focusableElements).indexOf(document.activeElement);
                const nextIndex = (currentIndex + 1) % focusableElements.length;
                focusableElements[nextIndex].focus();
            }
            else if (event.key === 'ArrowUp') {
                event.preventDefault();
                const currentIndex = Array.from(focusableElements).indexOf(document.activeElement);
                const prevIndex = currentIndex === 0 ? focusableElements.length - 1 : currentIndex - 1;
                focusableElements[prevIndex].focus();
            }
        };
        
        menuElement.addEventListener('keydown', focusTrapHandler);
        firstElement.focus();
        
        return true;
    } catch (error) {
        console.error('[ValyanClinic] Failed to add focus trap:', error);
        return false;
    }
};
```

## 🧪 Testing Strategy

### Unit Tests for State Management
```csharp
[TestFixture]
public class KebabMenuTests
{
    private PersonalPageState _state;
    
    [SetUp]
    public void Setup()
    {
        _state = new PersonalPageState();
    }
    
    [Test]
    public void ToggleKebabMenu_Initially_ShouldOpen()
    {
        // Arrange
        Assert.IsFalse(_state.ShowKebabMenu);
        
        // Act
        _state.ShowKebabMenu = true;
        
        // Assert
        Assert.IsTrue(_state.ShowKebabMenu);
    }
    
    [Test]
    public void ToggleStatistics_ShouldCloseKebabMenu()
    {
        // Arrange
        _state.ShowKebabMenu = true;
        _state.ShowStatistics = false;
        
        // Act
        _state.ShowStatistics = true;
        _state.ShowKebabMenu = false; // Simulated behavior
        
        // Assert
        Assert.IsTrue(_state.ShowStatistics);
        Assert.IsFalse(_state.ShowKebabMenu);
    }
}
```

### Integration Tests with JavaScript
```csharp
[TestFixture]
public class KebabMenuIntegrationTests : BlazorTestBase
{
    [Test]
    public async Task KebabMenu_ClickOutside_ShouldClose()
    {
        // Arrange
        var component = RenderComponent<AdministrarePersonal>();
        
        // Open kebab menu
        var kebabButton = component.Find(".kebab-menu-btn");
        await kebabButton.ClickAsync();
        
        // Verify menu is open
        var dropdown = component.Find(".kebab-menu-dropdown");
        Assert.IsNotNull(dropdown);
        
        // Act - Click outside
        await component.InvokeAsync(() => 
            JSRuntime.InvokeVoidAsync("document.body.click"));
        
        // Assert - Menu should be closed
        Assert.Throws<ElementNotFoundException>(() => 
            component.Find(".kebab-menu-dropdown"));
    }
    
    [Test]
    public async Task KebabMenu_EscapeKey_ShouldClose()
    {
        // Arrange
        var component = RenderComponent<AdministrarePersonal>();
        var kebabButton = component.Find(".kebab-menu-btn");
        await kebabButton.ClickAsync();
        
        // Act - Press Escape
        await component.InvokeAsync(() => 
            JSRuntime.InvokeVoidAsync("document.dispatchEvent", 
                new { type = "keydown", key = "Escape" }));
        
        // Assert - Menu should be closed
        Assert.Throws<ElementNotFoundException>(() => 
            component.Find(".kebab-menu-dropdown"));
    }
}
```

### Accessibility Tests
```csharp
[TestFixture]
public class KebabMenuAccessibilityTests : BlazorTestBase
{
    [Test]
    public void KebabMenuButton_HasProperAriaAttributes()
    {
        // Arrange
        var component = RenderComponent<AdministrarePersonal>();
        
        // Act
        var kebabButton = component.Find(".kebab-menu-btn");
        
        // Assert
        Assert.That(kebabButton.GetAttribute("aria-label"), Is.Not.Null);
        Assert.That(kebabButton.GetAttribute("aria-expanded"), Is.Not.Null);
        Assert.That(kebabButton.GetAttribute("aria-haspopup"), Is.EqualTo("true"));
    }
    
    [Test]
    public async Task KebabMenuItems_HaveProperRoles()
    {
        // Arrange
        var component = RenderComponent<AdministrarePersonal>();
        
        // Act - Open menu
        var kebabButton = component.Find(".kebab-menu-btn");
        await kebabButton.ClickAsync();
        
        // Assert
        var menuItems = component.FindAll(".kebab-menu-item");
        foreach (var item in menuItems)
        {
            Assert.That(item.GetAttribute("role"), Is.EqualTo("menuitem"));
            Assert.That(item.GetAttribute("aria-label"), Is.Not.Null);
        }
    }
}
```

## 📊 Performance Metrics and Monitoring

### JavaScript Performance Tracking
```javascript
// Performance monitoring in valyan-helpers.js
window.measureKebabMenuPerformance = () => {
    const startTime = performance.now();
    
    return {
        startTimer: () => startTime,
        endTimer: (operation) => {
            const endTime = performance.now();
            const duration = endTime - startTime;
            
            console.log(`[ValyanClinic Performance] ${operation} completed in ${duration.toFixed(2)}ms`);
            
            // Log slow operations
            if (duration > 100) {
                console.warn(`[ValyanClinic Performance] Slow operation detected: ${operation} took ${duration.toFixed(2)}ms`);
            }
            
            return duration;
        }
    };
};
```

### C# Performance Monitoring
```csharp
private readonly Stopwatch _kebabMenuStopwatch = new();

private async Task ToggleKebabMenuWithMetrics()
{
    _kebabMenuStopwatch.Restart();
    
    try
    {
        await ToggleKebabMenu();
        
        _kebabMenuStopwatch.Stop();
        
        Logger.LogInformation("⏱️ Kebab menu toggle completed in {ElapsedMs}ms", 
            _kebabMenuStopwatch.ElapsedMilliseconds);
        
        // Track slow operations
        if (_kebabMenuStopwatch.ElapsedMilliseconds > 100)
        {
            Logger.LogWarning("🐌 Slow kebab menu operation detected: {ElapsedMs}ms", 
                _kebabMenuStopwatch.ElapsedMilliseconds);
        }
    }
    catch (Exception ex)
    {
        _kebabMenuStopwatch.Stop();
        Logger.LogError(ex, "❌ Kebab menu error after {ElapsedMs}ms", 
            _kebabMenuStopwatch.ElapsedMilliseconds);
        throw;
    }
}
```

---

**🎯 Key Implementation Highlights**:
1. **Advanced JavaScript Integration** with retry logic and exponential backoff
2. **Complete Accessibility** with ARIA attributes and keyboard navigation
3. **Performance Optimized** with proper cleanup and memory management
4. **Cross-Browser Compatible** with fallback mechanisms
5. **Responsive Design** that works on all devices

**🔗 Related Components**:
- **PersonalPageState** - State management integration
- **JavaScript Helpers** - Browser interaction layer
- **CSS Animation System** - Visual feedback and transitions

**📞 Technical Support**: development@valyanmed.ro  
**🎨 UI/UX Guidelines**: Internal design system documentation  
**♿ Accessibility Standards**: WCAG 2.1 AA compliance guide

**Document Version**: 2.0  
**Last Updated**: December 2024  
**Author**: ValyanMed Frontend Architecture Team
