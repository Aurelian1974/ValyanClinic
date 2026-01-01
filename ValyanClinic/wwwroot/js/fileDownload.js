// JavaScript helpers for file downloads

// ✅ CRITICAL: Track if navigation is happening to prevent DOM errors
window._blazorNavigating = false;

window.downloadFileFromBytes = function (filename, contentType, data) {
    // Create blob from byte array
    const blob = new Blob([new Uint8Array(data)], { type: contentType });
    
    // Create download link
    const url = window.URL.createObjectURL(blob);
    const link = document.createElement('a');
    link.href = url;
    link.download = filename;
    
    // Trigger download
    document.body.appendChild(link);
    link.click();
    
    // ✅ FIXED: Async cleanup to prevent race conditions with Blazor component disposal
    setTimeout(() => {
        try {
            if (link && link.parentNode) {
                document.body.removeChild(link);
            }
            window.URL.revokeObjectURL(url);
        } catch (err) {
            // Ignore errors if DOM was already cleaned up by Blazor
            console.debug('Download cleanup completed (link already removed)');
        }
    }, 100); // Small delay to ensure download started
};

window.downloadFileFromBase64 = function (base64, filename, contentType) {
    try {
        // Convert base64 to byte array
        const byteCharacters = atob(base64);
        const byteNumbers = new Array(byteCharacters.length);
        for (let i = 0; i < byteCharacters.length; i++) {
            byteNumbers[i] = byteCharacters.charCodeAt(i);
        }
        const byteArray = new Uint8Array(byteNumbers);
        
        // Create blob and download
        const blob = new Blob([byteArray], { type: contentType });
        const url = window.URL.createObjectURL(blob);
        const link = document.createElement('a');
        link.href = url;
        link.download = filename;
        
        // Trigger download
        document.body.appendChild(link);
        link.click();
        
        // ✅ FIXED: Async cleanup to prevent race conditions with Blazor component disposal
        setTimeout(() => {
            try {
                if (link && link.parentNode) {
                    document.body.removeChild(link);
                }
                window.URL.revokeObjectURL(url);
            } catch (err) {
                // Ignore errors if DOM was already cleaned up by Blazor
                console.debug('Download cleanup completed (link already removed)');
            }
        }, 100); // Small delay to ensure download started
    } catch (err) {
        console.error('Error in downloadFileFromBase64:', err);
        throw err;
    }
};

// ✅ CRITICAL: Cleanup Syncfusion components before navigation
window.cleanupSyncfusionBeforeNavigation = function() {
    window._blazorNavigating = true;
    
    try {
        // Stop all Syncfusion animations
        if (window.sfBlazor) {
            // Cancel any pending animations
            if (typeof cancelAnimationFrame === 'function') {
                // Try to stop any animation frames
                for (let i = 1; i < 10000; i++) {
                    cancelAnimationFrame(i);
                }
            }
            
            // Destroy Syncfusion instances if possible
            if (window.sfBlazor.instances) {
                Object.keys(window.sfBlazor.instances).forEach(key => {
                    try {
                        const instance = window.sfBlazor.instances[key];
                        if (instance && typeof instance.destroy === 'function') {
                            instance.destroy();
                        }
                    } catch (e) {
                        // Ignore - instance might already be destroyed
                    }
                });
            }
        }
        
        console.debug('[Cleanup] Syncfusion components cleaned up');
    } catch (err) {
        console.debug('[Cleanup] Error cleaning Syncfusion:', err.message);
    }
    
    // Reset flag after short delay
    setTimeout(() => {
        window._blazorNavigating = false;
    }, 500);
};

// ✅ CRITICAL: Override removeChild to prevent errors during navigation
(function() {
    const originalRemoveChild = Node.prototype.removeChild;
    
    Node.prototype.removeChild = function(child) {
        // If navigating, silently skip problematic removals
        if (window._blazorNavigating) {
            try {
                if (child && child.parentNode === this) {
                    return originalRemoveChild.call(this, child);
                }
            } catch (e) {
                console.debug('[Navigation] Skipped removeChild during navigation');
                return child;
            }
            return child;
        }
        
        // Normal operation - with safety check
        try {
            if (child && child.parentNode === this) {
                return originalRemoveChild.call(this, child);
            }
            return child;
        } catch (err) {
            console.debug('[DOM] removeChild failed safely:', err.message);
            return child;
        }
    };
})();
