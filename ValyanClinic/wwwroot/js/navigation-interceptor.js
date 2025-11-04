// Navigation Interceptor - DISABLED
// This script was causing 47k+ removeChild operations during navigation
// Blazor and Syncfusion can handle their own lifecycle

window.navigationInterceptor = {
    // Minimal cleanup - DOAR log, fără destroy agresiv
    initialize: function() {
        console.log('[NavInterceptor] Minimal interceptor initialized (cleanup disabled)');
        
        // Doar log pentru debug
        window.addEventListener('beforeunload', () => {
            console.log('[NavInterceptor] Page unload detected');
        });
    }
};

// Initialize minimal
if (document.readyState === 'loading') {
    document.addEventListener('DOMContentLoaded', () => {
        window.navigationInterceptor.initialize();
    });
} else {
    window.navigationInterceptor.initialize();
}
