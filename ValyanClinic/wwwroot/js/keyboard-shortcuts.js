window.keyboardShortcuts = (function () {
    let dotnetRef = null;
    let handler = null;

    function isTypingInInput() {
        const el = document.activeElement;
        if (!el) return false;
        const tag = el.tagName;
        if (tag === 'INPUT' || tag === 'TEXTAREA') return true;
        if (el.isContentEditable) return true;
        return false;
    }

    function onKey(e) {
        try {
            // High-priority shortcuts that should work even when typing in inputs
            if ((e.ctrlKey || e.metaKey) && e.key.toLowerCase() === 'n') {
                console.debug('keyboardShortcuts: Ctrl+N intercepted');
                e.preventDefault();
                try { e.stopImmediatePropagation(); e.stopPropagation(); } catch (ex) {}
                dotnetRef?.invokeMethodAsync('OnKeyboardShortcut', 'new');
                return;
            }

            if ((e.ctrlKey || e.metaKey) && (e.key.toLowerCase() === 'e')) {
                e.preventDefault();
                try { e.stopImmediatePropagation(); e.stopPropagation(); } catch (ex) {}
                dotnetRef?.invokeMethodAsync('OnKeyboardShortcut', 'edit');
                return;
            }

            // F2 alone should also trigger edit when not typing in an input
            if (e.key === 'F2' && !isTypingInInput()) {
                console.debug('keyboardShortcuts: F2 intercepted');
                e.preventDefault();
                try { e.stopImmediatePropagation(); e.stopPropagation(); } catch (ex) {}
                dotnetRef?.invokeMethodAsync('OnKeyboardShortcut', 'edit');
                return;
            }

            if (e.key === 'Delete') {
                // Only trigger delete when not typing to avoid accidental deletions in inputs
                if (isTypingInInput()) return;
                e.preventDefault();
                dotnetRef?.invokeMethodAsync('OnKeyboardShortcut', 'delete');
                return;
            }

            // Allow some shortcuts even when typing
            if (e.key === 'Escape') {
                e.preventDefault();
                dotnetRef?.invokeMethodAsync('OnKeyboardShortcut', 'escape');
                return;
            }

            // Ctrl/Cmd + F -> focus search (allow even when typing)
            if ((e.ctrlKey || e.metaKey) && e.key.toLowerCase() === 'f') {
                e.preventDefault();
                dotnetRef?.invokeMethodAsync('OnKeyboardShortcut', 'focusSearch');
                return;
            }

            // If typing inside input, do not intercept other shortcuts
            if (isTypingInInput()) return;

            // Global shortcuts
            if (e.key === 'F5') {
                e.preventDefault();
                dotnetRef?.invokeMethodAsync('OnKeyboardShortcut', 'reload');
                return;
            }
        }
        catch (err) {
            console.debug('keyboardShortcuts onKey error', err);
        }
    }

    return {
        register: function (dotnetObjectRef) {
            try {
                dotnetRef = dotnetObjectRef;
                handler = onKey;
                // Use capture and passive:false so we can reliably intercept browser shortcuts like Ctrl+N
                window.addEventListener('keydown', handler, { capture: true, passive: false });
                console.debug('keyboardShortcuts registered (capture:true)');
            }
            catch (err) {
                console.debug('keyboardShortcuts register error', err);
            }
        },
        unregister: function () {
            try {
                if (handler) {
                    // remove with capture:true to match registration
                    window.removeEventListener('keydown', handler, { capture: true });
                }
                handler = null;
                dotnetRef = null;
            }
            catch (err) {
                console.debug('keyboardShortcuts unregister error', err);
            }
        },
        focusSearch: function () {
            try {
                const el = document.querySelector('.search-input');
                if (el) el.focus();
            }
            catch (err) {
                console.debug('keyboardShortcuts focusSearch error', err);
            }
        }
    };
})();