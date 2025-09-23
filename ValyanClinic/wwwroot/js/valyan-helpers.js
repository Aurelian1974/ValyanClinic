/**
 * JavaScript Helper Functions pentru ValyanClinic - V6 CLEAN
 * Simplified utility functions - removed kebab menu functionality
 */

// Global debug flag
window.valyanClinicDebug = true; // Set to false in production

function debugLog(message, ...args) {
    if (window.valyanClinicDebug) {
        console.log(`[ValyanClinic] ${message}`, ...args);
    }
}

// Enhanced DOM readiness check
function isDOMReady() {
    return typeof window !== 'undefined' && 
           typeof document !== 'undefined' && 
           document.readyState === 'complete' &&
           typeof document.addEventListener === 'function';
}

// Wait for DOM to be ready with timeout
function waitForDOMReady(timeout = 5000) {
    return new Promise((resolve) => {
        if (isDOMReady()) {
            resolve(true);
            return;
        }

        const startTime = Date.now();
        const checkInterval = setInterval(() => {
            if (isDOMReady()) {
                clearInterval(checkInterval);
                resolve(true);
            } else if (Date.now() - startTime > timeout) {
                clearInterval(checkInterval);
                resolve(false);
            }
        }, 50);
    });
}

// ========================================
// FORM UTILITIES
// ========================================

/**
 * Form submission helpers - Safe alternatives to requestSubmit()
 */
window.submitFormSafely = async (formId) => {
    try {
        const domReady = await waitForDOMReady();
        if (!domReady) {
            console.warn('[ValyanClinic] DOM not ready, cannot submit form');
            return false;
        }

        const form = document.getElementById(formId);
        if (form) {
            form.submit();
            debugLog('Form submitted successfully:', formId);
            return true;
        } else {
            console.warn(`[ValyanClinic] Form with ID '${formId}' not found`);
            return false;
        }
    } catch (error) {
        console.error('[ValyanClinic] Error submitting form:', error);
        return false;
    }
};

/**
 * Form validation and submission with HTML5 validation
 */
window.validateAndSubmitForm = async (formId) => {
    try {
        const domReady = await waitForDOMReady();
        if (!domReady) {
            console.warn('[ValyanClinic] DOM not ready, cannot validate form');
            return false;
        }

        const form = document.getElementById(formId);
        if (!form) {
            console.warn(`[ValyanClinic] Form with ID '${formId}' not found`);
            return false;
        }
        
        // Check if form has HTML5 validation
        if (form.checkValidity && !form.checkValidity()) {
            form.reportValidity();
            debugLog('Form validation failed:', formId);
            return false;
        }
        
        // If validation passes, submit the form
        form.submit();
        debugLog('Form validated and submitted successfully:', formId);
        return true;
        
    } catch (error) {
        console.error('[ValyanClinic] Error validating and submitting form:', error);
        return false;
    }
};

// ========================================
// PERFORMANCE MONITORING
// ========================================

/**
 * Simple performance measurement for any operation
 */
window.measurePerformance = (operationName, operation) => {
    const startTime = performance.now();
    
    try {
        const result = operation();
        const endTime = performance.now();
        const duration = endTime - startTime;
        
        debugLog(`Performance: ${operationName} completed in ${duration.toFixed(2)}ms`);
        
        return { result, duration };
    } catch (error) {
        const endTime = performance.now();
        const duration = endTime - startTime;
        
        console.error(`[ValyanClinic] Error in ${operationName} after ${duration.toFixed(2)}ms:`, error);
        throw error;
    }
};

// ========================================
// SCROLL UTILITIES
// ========================================

/**
 * Smooth scroll to element
 */
window.scrollToElement = (selector) => {
    try {
        const element = document.querySelector(selector);
        if (element) {
            element.scrollIntoView({ 
                behavior: 'smooth', 
                block: 'center' 
            });
            return true;
        }
        return false;
    } catch (error) {
        console.error('[ValyanClinic] Error scrolling to element:', error);
        return false;
    }
};

// ========================================
// FOCUS MANAGEMENT
// ========================================

/**
 * Focus management for accessibility
 */
window.focusElement = (selector) => {
    try {
        const element = document.querySelector(selector);
        if (element && element.focus) {
            element.focus();
            return true;
        }
        return false;
    } catch (error) {
        console.error('[ValyanClinic] Error focusing element:', error);
        return false;
    }
};

// ========================================
// INITIALIZATION
// ========================================

// Initialize when DOM is loaded
(async function initializeValyanClinicHelpers() {
    debugLog('Initializing ValyanClinic JavaScript helpers - V6 CLEAN');
    
    const domReady = await waitForDOMReady(10000); // 10 second timeout
    if (domReady) {
        debugLog('✅ ValyanClinic JavaScript helpers loaded successfully - DOM ready');
        debugLog('📋 Available features:');
        debugLog('  - Form submission utilities');
        debugLog('  - Performance monitoring tools');
        debugLog('  - Scroll and focus utilities');
    } else {
        console.warn('⚠️ ValyanClinic JavaScript helpers loaded but DOM not ready within timeout');
        console.warn('Some features may not be available');
    }
})();

debugLog('ValyanClinic JavaScript helpers loaded - V6 CLEAN - Kebab menu functionality removed');
