/**
 * JavaScript Helper Functions pentru ValyanClinic
 * Funcții de utilitare pentru interacțiunea cu componentele Blazor
 */

window.addClickEventListener = (dotnetRef) => {
    try {
        const clickHandler = () => {
            try {
                dotnetRef.invokeMethodAsync('CloseKebabMenu');
            } catch (error) {
                console.warn('Failed to invoke CloseKebabMenu:', error);
            }
        };
        
        document.addEventListener('click', clickHandler, { passive: true });
        
        // Store reference for cleanup if needed
        if (!window.valyanClinicEventHandlers) {
            window.valyanClinicEventHandlers = [];
        }
        window.valyanClinicEventHandlers.push({ type: 'click', handler: clickHandler });
        
    } catch (error) {
        console.warn('Failed to add click event listener:', error);
    }
};

// Clean up function for when components are disposed
window.removeEventListeners = () => {
    try {
        if (window.valyanClinicEventHandlers) {
            window.valyanClinicEventHandlers.forEach(handler => {
                document.removeEventListener(handler.type, handler.handler);
            });
            window.valyanClinicEventHandlers = [];
        }
    } catch (error) {
        console.warn('Failed to remove event listeners:', error);
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

console.log('ValyanClinic JavaScript helpers loaded successfully');
