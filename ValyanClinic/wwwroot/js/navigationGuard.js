// navigationGuard.js - Browser navigation protection
// Prevents data loss by warning user before leaving page

let isEnabled = false;
let customMessage = "Aveți modificări nesalvate. Sigur doriți să părăsiți pagina?";

// Enable beforeunload event
export function enableBeforeUnload(message) {
    customMessage = message || customMessage;
    isEnabled = true;

    window.addEventListener('beforeunload', handleBeforeUnload);
}

// Disable beforeunload event
export function disableBeforeUnload() {
    isEnabled = false;
    window.removeEventListener('beforeunload', handleBeforeUnload);
}

// Event handler
function handleBeforeUnload(e) {
    if (!isEnabled) return;

    // Modern browsers ignore custom message and show generic warning
    // But we still need to set returnValue to trigger the warning
    e.preventDefault();
    e.returnValue = customMessage; // Chrome requires returnValue to be set
    return customMessage; // Older browsers
}

// Cleanup on page unload (optional)
window.addEventListener('unload', () => {
    disableBeforeUnload();
});
