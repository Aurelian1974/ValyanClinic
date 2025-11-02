// Helper pentru refresh automat la prima încărcare + Cleanup Syncfusion
window.pageRefreshHelper = {
    // Verifică dacă pagina a fost deja refresh-uită
    checkAndRefresh: function(pageKey) {
        const storageKey = `page_refreshed_${pageKey}`;
        const wasRefreshed = sessionStorage.getItem(storageKey);
        
        if (!wasRefreshed) {
            console.log(`[PageRefresh] Prima încărcare detectată pentru ${pageKey} - se forțează reload`);
            
            // Marchează că vom face refresh
            sessionStorage.setItem(storageKey, 'true');
            
            // Force reload
            setTimeout(() => {
                window.location.reload(true);
            }, 50);
            
            return true;
        }
        
        console.log(`[PageRefresh] Pagina ${pageKey} deja refresh-uită - continuă normal`);
        return false;
    },
    
    // Curăță flag-ul când plecăm de pe pagină
    clearRefreshFlag: function(pageKey) {
        const storageKey = `page_refreshed_${pageKey}`;
        sessionStorage.removeItem(storageKey);
        console.log(`[PageRefresh] Cleanup flag pentru ${pageKey}`);
    },
    
    // Cleanup Syncfusion înainte de navigare
    cleanupSyncfusion: function() {
        if (!window.sfBlazor || !window.sfBlazor.instances) {
            return;
        }
        
        try {
            const instances = Object.keys(window.sfBlazor.instances);
            if (instances.length === 0) return;
            
            console.log(`[PageRefresh] Cleaning ${instances.length} Syncfusion instances`);
            
            instances.forEach(key => {
                try {
                    const instance = window.sfBlazor.instances[key];
                    if (instance && typeof instance.destroy === 'function') {
                        instance.destroy();
                    }
                } catch (e) {
                    // Silent fail
                }
            });
            
            // Clear instances
            window.sfBlazor.instances = {};
            
            console.log('[PageRefresh] Syncfusion cleanup complete');
        } catch (e) {
            console.warn('[PageRefresh] Syncfusion cleanup error:', e);
        }
    }
};

// Cleanup Syncfusion la unload
window.addEventListener('beforeunload', function() {
    console.log('[PageRefresh] Page unload - cleaning Syncfusion');
    window.pageRefreshHelper.cleanupSyncfusion();
});
