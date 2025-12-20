// Toast Notifications - Global JavaScript Functions
(function() {
    'use strict';

    // Store for active toasts to track positioning
    const activeToasts = [];

    /**
     * Display a toast notification
     * @param {string} message - The message to display
     * @param {string} type - Toast type: 'success', 'error', 'warning', 'info'
     * @param {number} duration - Duration in milliseconds (default: 3000)
     */
    window.showToast = function(message, type = 'info', duration = 3000) {
        // Create toast container if it doesn't exist
        let container = document.querySelector('.toast-container');
        if (!container) {
            container = document.createElement('div');
            container.className = 'toast-container';
            document.body.appendChild(container);
        }

        // Create toast element
        const toast = document.createElement('div');
        toast.className = `toast toast-${type} toast-enter`;
        
        // Icon based on type
        const icons = {
            success: '<i class="fas fa-check-circle"></i>',
            error: '<i class="fas fa-times-circle"></i>',
            warning: '<i class="fas fa-exclamation-triangle"></i>',
            info: '<i class="fas fa-info-circle"></i>'
        };
        
        toast.innerHTML = `
            ${icons[type] || icons.info}
            <span>${message}</span>
            <button class="toast-close" aria-label="Close">
                <i class="fas fa-times"></i>
            </button>
        `;

        // Add to container
        container.appendChild(toast);
        activeToasts.push(toast);

        // Force reflow to trigger animation
        toast.offsetHeight;
        toast.classList.add('toast-show');

        // Close button handler
        const closeBtn = toast.querySelector('.toast-close');
        closeBtn.addEventListener('click', () => closeToast(toast));

        // Auto-close after duration
        const timeoutId = setTimeout(() => closeToast(toast), duration);

        // Store timeout ID for manual close
        toast.dataset.timeoutId = timeoutId;
    };

    /**
     * Close a toast notification
     * @param {HTMLElement} toast - The toast element to close
     */
    function closeToast(toast) {
        // Clear timeout if exists
        if (toast.dataset.timeoutId) {
            clearTimeout(parseInt(toast.dataset.timeoutId));
        }

        // Remove from active toasts
        const index = activeToasts.indexOf(toast);
        if (index > -1) {
            activeToasts.splice(index, 1);
        }

        // Animate out
        toast.classList.remove('toast-show');
        toast.classList.add('toast-exit');

        // Remove from DOM after animation
        setTimeout(() => {
            if (toast.parentElement) {
                toast.parentElement.removeChild(toast);
            }
        }, 300);
    }

    /**
     * Register keyboard shortcuts for a page
     * @param {function} ctrlSHandler - Handler for Ctrl+S
     * @param {function} ctrlLeftHandler - Handler for Ctrl+Left Arrow
     * @param {function} ctrlRightHandler - Handler for Ctrl+Right Arrow
     * @returns {function} Cleanup function to unregister handlers
     */
    window.registerKeyboardShortcuts = function(ctrlSHandler, ctrlLeftHandler, ctrlRightHandler) {
        const handler = function(e) {
            if (e.ctrlKey || e.metaKey) {
                if (e.key === 's' || e.key === 'S') {
                    e.preventDefault();
                    if (ctrlSHandler) {
                        ctrlSHandler.invokeMethodAsync('InvokeAsync');
                    }
                } else if (e.key === 'ArrowLeft') {
                    e.preventDefault();
                    if (ctrlLeftHandler) {
                        ctrlLeftHandler.invokeMethodAsync('InvokeAsync');
                    }
                } else if (e.key === 'ArrowRight') {
                    e.preventDefault();
                    if (ctrlRightHandler) {
                        ctrlRightHandler.invokeMethodAsync('InvokeAsync');
                    }
                }
            }
        };

        document.addEventListener('keydown', handler);

        // Return cleanup function
        return function() {
            document.removeEventListener('keydown', handler);
        };
    };

    /**
     * Register beforeunload handler to warn about unsaved changes
     * @param {boolean} hasUnsavedChanges - Whether there are unsaved changes
     * @returns {function} Cleanup function to unregister handler
     */
    window.registerBeforeUnloadHandler = function(hasUnsavedChanges) {
        const handler = function(e) {
            if (hasUnsavedChanges) {
                e.preventDefault();
                e.returnValue = 'Aveți modificări nesalvate. Sigur doriți să părăsiți pagina?';
                return e.returnValue;
            }
        };

        window.addEventListener('beforeunload', handler);

        // Return cleanup function
        return function() {
            window.removeEventListener('beforeunload', handler);
        };
    };

    console.log('Notifications.js loaded successfully');
})();
