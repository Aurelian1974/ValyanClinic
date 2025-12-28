window.keyboardShortcuts = (function () {
    let dotnetRef = null;
    let handler = null;
    let keyupHandler = null;
    let keypressHandler = null;
    let registered = false;
    let lastCtrlAt = 0;
    let lastMetaAt = 0;

    function ensureDebugElement() {
        try {
            let el = document.getElementById('ks-debug-badge');
            if (!el) {
                el = document.createElement('div');
                el.id = 'ks-debug-badge';
                el.style.position = 'fixed';
                el.style.left = '20px';
                el.style.bottom = '20px';
                el.style.background = 'rgba(0,0,0,0.7)';
                el.style.color = '#fff';
                el.style.padding = '6px 8px';
                el.style.borderRadius = '6px';
                el.style.fontSize = '12px';
                el.style.zIndex = '2000';
                el.style.pointerEvents = 'none';
                el.textContent = 'KS: initializing...';
                document.body.appendChild(el);
            }
            return el;
        }
        catch (e) {
            // ignore
            return null;
        }
    }

    function updateDebug(msg) {
        try {
            const el = document.getElementById('ks-debug-badge');
            if (el) el.textContent = 'KS: ' + msg;
            console.debug('keyboardShortcuts debug:', msg);
        }
        catch (e) { }
    }

    function isTypingInInput() {
        const el = document.activeElement;
        if (!el) return false;
        const tag = el.tagName;
        if (tag === 'INPUT' || tag === 'TEXTAREA' || tag === 'SELECT') return true;
        if (el.isContentEditable) return true;
        return false;
    }

    function handleKey(e) {
        try {
            updateDebug((e.ctrlKey ? 'Ctrl+' : '') + (e.metaKey ? 'Meta+' : '') + e.key + " (code=" + e.code + ", keyCode=" + e.keyCode + ")");
        
        // Track Control/Meta timestamps to handle certain browser quirks where control keydown is delivered separately
        if (e.key === 'Control') lastCtrlAt = Date.now();
        if (e.key === 'Meta') lastMetaAt = Date.now();

            // High-priority shortcuts that should work even when typing in inputs
            // Detect Ctrl+N with multiple fallbacks (modifiers set OR short delay after Control press)
            if (((e.ctrlKey || e.metaKey) && (e.key.toLowerCase() === 'n' || e.code === 'KeyN'))
                || ((e.key.toLowerCase() === 'n' || e.code === 'KeyN') && (Date.now() - lastCtrlAt) < 600)
                || ((e.key.toLowerCase() === 'n' || e.code === 'KeyN') && (Date.now() - lastMetaAt) < 600)) {
                try { e.preventDefault(); e.stopImmediatePropagation(); e.stopPropagation(); } catch (ex) { }
                updateDebug('Ctrl+N -> new (fallback)');
                dotnetRef?.invokeMethodAsync('OnKeyboardShortcut', 'new');
                return;
            }

            if ((e.ctrlKey || e.metaKey) && (e.key.toLowerCase() === 'e')) {
                try { e.preventDefault(); e.stopImmediatePropagation(); e.stopPropagation(); } catch (ex) { }
                updateDebug('Ctrl+E -> edit');
                dotnetRef?.invokeMethodAsync('OnKeyboardShortcut', 'edit');
                return;
            }

            // F2 alone should also trigger edit when not typing in an input (check code variant too)
            if ((e.key === 'F2' || e.code === 'F2') && !isTypingInInput()) {
                try { e.preventDefault(); e.stopImmediatePropagation(); e.stopPropagation(); } catch (ex) { }
                updateDebug('F2 -> edit');
                dotnetRef?.invokeMethodAsync('OnKeyboardShortcut', 'edit');
                return;
            }

            if (e.key === 'Delete') {
                // Only trigger delete when not typing to avoid accidental deletions in inputs
                if (isTypingInInput()) return;
                try { e.preventDefault(); } catch { }
                updateDebug('Delete -> delete');
                dotnetRef?.invokeMethodAsync('OnKeyboardShortcut', 'delete');
                return;
            }

            // Allow some shortcuts even when typing
            if (e.key === 'Escape') {
                try { e.preventDefault(); } catch { }
                updateDebug('Escape -> escape');
                dotnetRef?.invokeMethodAsync('OnKeyboardShortcut', 'escape');
                return;
            }

            // Ctrl/Cmd + F -> focus search (allow even when typing)
            if ((e.ctrlKey || e.metaKey) && e.key.toLowerCase() === 'f') {
                try { e.preventDefault(); } catch { }
                updateDebug('Ctrl+F -> focusSearch');
                dotnetRef?.invokeMethodAsync('OnKeyboardShortcut', 'focusSearch');
                return;
            }

            // If typing inside input, do not intercept other shortcuts
            if (isTypingInInput()) return;

            // Global shortcuts
            if (e.key === 'F5') {
                try { e.preventDefault(); } catch { }
                updateDebug('F5 -> reload');
                dotnetRef?.invokeMethodAsync('OnKeyboardShortcut', 'reload');
                return;
            }
        }
        catch (err) {
            console.debug('keyboardShortcuts handleKey error', err);
        }
    }

    function handleKeyUp(e) {
        try {
            // As fallback, log keyup (some browsers swallow keydown for certain combos)
            updateDebug('keyup:' + e.key);
        }
        catch (ex) { }
    }

    return {
        register: function (dotnetObjectRef) {
            try {
                dotnetRef = dotnetObjectRef;
                handler = handleKey;
                keyupHandler = handleKeyUp;

                ensureDebugElement();
                updateDebug('registered');

                // Multiple listeners as fallback: document/window, capture and bubble
                document.addEventListener('keydown', handler, { capture: true, passive: false });
                window.addEventListener('keydown', handler, { capture: true, passive: false });
                document.addEventListener('keydown', handler, { capture: false, passive: false });
                document.addEventListener('keyup', keyupHandler, { capture: false, passive: true });
                // Add keypress as fallback for some browser/IME scenarios
                keypressHandler = function (e) { updateDebug('keypress:' + e.key + ' code=' + e.code); };
                document.addEventListener('keypress', keypressHandler, { capture: false, passive: true });

                registered = true;
                console.debug('keyboardShortcuts registered (multiple listeners)');
            }
            catch (err) {
                console.debug('keyboardShortcuts register error', err);
                updateDebug('register error');
            }
        },
        unregister: function () {
            try {
                if (handler) {
                    document.removeEventListener('keydown', handler, { capture: true });
                    window.removeEventListener('keydown', handler, { capture: true });
                    document.removeEventListener('keydown', handler, { capture: false });
                }

                if (keyupHandler) {
                    document.removeEventListener('keyup', keyupHandler, { capture: false });
                }

                if (keypressHandler) {
                    document.removeEventListener('keypress', keypressHandler, { capture: false });
                }

                const el = document.getElementById('ks-debug-badge');
                if (el && el.parentNode) el.parentNode.removeChild(el);

                handler = null;
                keyupHandler = null;
                keypressHandler = null;
                dotnetRef = null;
                registered = false;

                console.debug('keyboardShortcuts unregistered');
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
        },
        _debug_update: function (msg) { updateDebug(msg); }
    };
})();