/**
 * JavaScript Helper Functions pentru ValyanClinic - V2 ENHANCED
 * Funcții de utilitare pentru interacțiunea cu componentele Blazor
 * Reparat kebab menu click outside detection și event handling
 */

// Global state pentru tracking event handlers
window.valyanClinicEventHandlers = window.valyanClinicEventHandlers || [];
window.valyanClinicDebug = true; // Set to false in production

function debugLog(message, ...args) {
    if (window.valyanClinicDebug) {
        console.log(`[ValyanClinic] ${message}`, ...args);
    }
}

// Funcție principală pentru kebab menu click outside detection - ENHANCED
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

// Enhanced escape key handler pentru kebab menu
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

// Focus trap pentru accessibility - NEW FEATURE
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

// Form submission helpers (safer alternatives to document.getElementById().requestSubmit())
window.submitFormSafely = (formId) => {
    try {
        const form = document.getElementById(formId);
        if (form) {
            // Use form.submit() instead of requestSubmit() for better compatibility
            form.submit();
            return true;
        } else {
            console.warn(`Form with ID '${formId}' not found`);
            return false;
        }
    } catch (error) {
        console.error('Error submitting form:', error);
        return false;
    }
};

// Alternative form validation and submission
window.validateAndSubmitForm = (formId) => {
    try {
        const form = document.getElementById(formId);
        if (!form) {
            console.warn(`Form with ID '${formId}' not found`);
            return false;
        }
        
        // Check if form has HTML5 validation
        if (form.checkValidity && !form.checkValidity()) {
            form.reportValidity();
            return false;
        }
        
        // If validation passes, submit the form
        form.submit();
        return true;
        
    } catch (error) {
        console.error('Error validating and submitting form:', error);
        return false;
    }
};

debugLog('ValyanClinic JavaScript helpers loaded successfully - V2 ENHANCED');
