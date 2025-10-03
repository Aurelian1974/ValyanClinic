// Sidebar Management - Persistent CSS Variable Handler with MutationObserver
(function() {
    'use strict';

    let currentWidth = null;

    // Initialize sidebar width from localStorage
    function initializeSidebarWidth() {
        const isCollapsed = localStorage.getItem('sidebar-collapsed') === 'true';
        const sidebarWidth = isCollapsed ? '70px' : '280px';
        
        if (currentWidth !== sidebarWidth) {
            console.log('JS: Setting sidebar width to:', sidebarWidth);
            document.documentElement.style.setProperty('--sidebar-width', sidebarWidth);
            currentWidth = sidebarWidth;
        }
    }

    // Force update continuously until Blazor is ready
    function forceCSSVariable() {
        initializeSidebarWidth();
        
        // Check again after small delays to catch Blazor interference
        requestAnimationFrame(initializeSidebarWidth);
        setTimeout(initializeSidebarWidth, 10);
        setTimeout(initializeSidebarWidth, 50);
        setTimeout(initializeSidebarWidth, 100);
        setTimeout(initializeSidebarWidth, 200);
    }

    // Watch for changes in localStorage (for multi-tab sync)
    window.addEventListener('storage', function(e) {
        if (e.key === 'sidebar-collapsed') {
            forceCSSVariable();
        }
    });

    // Expose global function for Blazor to call
    window.updateSidebarWidth = function(isCollapsed) {
        const sidebarWidth = isCollapsed ? '70px' : '280px';
        console.log('JS: updateSidebarWidth called with isCollapsed:', isCollapsed, '=> width:', sidebarWidth);
        
        // Save to localStorage FIRST
        localStorage.setItem('sidebar-collapsed', isCollapsed.toString());
        
        // Then apply CSS
        document.documentElement.style.setProperty('--sidebar-width', sidebarWidth);
        currentWidth = sidebarWidth;
        
        // Force multiple times to overcome Blazor re-renders
        requestAnimationFrame(function() {
            document.documentElement.style.setProperty('--sidebar-width', sidebarWidth);
        });
    };

    // Watch for Blazor navigation events
    const originalPushState = history.pushState;
    const originalReplaceState = history.replaceState;

    history.pushState = function() {
        originalPushState.apply(history, arguments);
        console.log('JS: Navigation detected (pushState)');
        forceCSSVariable();
    };

    history.replaceState = function() {
        originalReplaceState.apply(history, arguments);
        console.log('JS: Navigation detected (replaceState)');
        forceCSSVariable();
    };

    window.addEventListener('popstate', function() {
        console.log('JS: Navigation detected (popstate)');
        forceCSSVariable();
    });

    // Watch for DOM changes (Blazor renders)
    const observer = new MutationObserver(function(mutations) {
        // Only check if we have a saved state
        if (localStorage.getItem('sidebar-collapsed')) {
            initializeSidebarWidth();
        }
    });

    // Start observing when body is ready
    function startObserving() {
        if (document.body) {
            observer.observe(document.body, {
                childList: true,
                subtree: false
            });
            console.log('JS: MutationObserver started');
        } else {
            setTimeout(startObserving, 10);
        }
    }

    // Run immediately and repeatedly
    forceCSSVariable();

    // Run when DOM is ready
    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', function() {
            console.log('JS: DOMContentLoaded event');
            forceCSSVariable();
            startObserving();
        });
    } else {
        startObserving();
    }

    // Also run on load
    window.addEventListener('load', function() {
        console.log('JS: Window load event');
        forceCSSVariable();
    });

    console.log('JS: Sidebar management script loaded');
})();
