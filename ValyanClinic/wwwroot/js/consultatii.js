/**
 * Consultații Page - Keyboard Navigation & Enhancements
 * Provides keyboard shortcuts and UX improvements for the consultation form
 */

window.ConsultatiiKeyboard = {
    dotNetReference: null,
    isInitialized: false,

    /**
     * Initialize keyboard event listeners
     * @param {object} dotNetRef - .NET object reference for callbacks
     */
    initialize: function (dotNetRef) {
        if (this.isInitialized) {
            this.dispose();
        }

        this.dotNetReference = dotNetRef;
        this.isInitialized = true;

        // Add keyboard event listener
        document.addEventListener('keydown', this.handleKeyDown.bind(this));

        console.log('[ConsultatiiKeyboard] Initialized');
    },

    /**
     * Handle keydown events
     * @param {KeyboardEvent} event
     */
    handleKeyDown: function (event) {
        if (!this.dotNetReference) return;

        // Ignore if user is typing in a text input (except for specific shortcuts)
        const isTextInput = event.target.matches('input, textarea, select');

        // Ctrl+S - Save Draft
        if (event.ctrlKey && event.key === 's') {
            event.preventDefault();
            this.dotNetReference.invokeMethodAsync('OnKeyboardSaveDraft');
            return;
        }

        // Ctrl+Enter - Finalize Consultation
        if (event.ctrlKey && event.key === 'Enter') {
            event.preventDefault();
            this.dotNetReference.invokeMethodAsync('OnKeyboardFinalize');
            return;
        }

        // Ctrl+Left Arrow - Previous Tab
        if (event.ctrlKey && event.key === 'ArrowLeft') {
            event.preventDefault();
            this.dotNetReference.invokeMethodAsync('OnKeyboardPreviousTab');
            return;
        }

        // Ctrl+Right Arrow - Next Tab
        if (event.ctrlKey && event.key === 'ArrowRight') {
            event.preventDefault();
            this.dotNetReference.invokeMethodAsync('OnKeyboardNextTab');
            return;
        }

        // Alt+1 through Alt+4 - Jump to specific tab
        if (event.altKey && !event.ctrlKey && !event.shiftKey) {
            const tabNumber = parseInt(event.key);
            if (tabNumber >= 1 && tabNumber <= 4) {
                event.preventDefault();
                this.dotNetReference.invokeMethodAsync('OnKeyboardSwitchTab', tabNumber);
                return;
            }
        }

        // Ctrl+P - Pause/Resume Timer
        if (event.ctrlKey && event.key === 'p') {
            event.preventDefault();
            this.dotNetReference.invokeMethodAsync('OnKeyboardToggleTimer');
            return;
        }
    },

    /**
     * Show keyboard shortcuts help modal
     */
    showShortcutsHelp: function () {
        const shortcuts = [
            { key: 'Ctrl + S', action: 'Salvează Draft' },
            { key: 'Ctrl + Enter', action: 'Finalizează Consultația' },
            { key: 'Ctrl + ←', action: 'Tab Anterior' },
            { key: 'Ctrl + →', action: 'Tab Următor' },
            { key: 'Alt + 1-4', action: 'Salt la Tab' },
            { key: 'Ctrl + P', action: 'Pauză/Continuă Timer' }
        ];

        console.log('Keyboard Shortcuts:', shortcuts);
    },

    /**
     * Focus first input in the active tab
     */
    focusFirstInput: function () {
        const activeTab = document.querySelector('.tab-panel.active');
        if (activeTab) {
            const firstInput = activeTab.querySelector('input, textarea, select');
            if (firstInput) {
                firstInput.focus();
            }
        }
    },

    /**
     * Dispose of event listeners
     */
    dispose: function () {
        document.removeEventListener('keydown', this.handleKeyDown.bind(this));
        this.dotNetReference = null;
        this.isInitialized = false;
        console.log('[ConsultatiiKeyboard] Disposed');
    }
};

/**
 * Auto-Save functionality
 */
window.ConsultatiiAutoSave = {
    dotNetReference: null,
    autoSaveTimer: null,
    debounceTimer: null,
    autoSaveIntervalMs: 30000, // 30 seconds
    debounceMs: 2000, // 2 seconds debounce after last change

    /**
     * Initialize auto-save
     * @param {object} dotNetRef - .NET object reference
     * @param {number} intervalMs - Auto-save interval in milliseconds
     */
    initialize: function (dotNetRef, intervalMs) {
        this.dotNetReference = dotNetRef;
        this.autoSaveIntervalMs = intervalMs || 30000;

        // Start periodic auto-save
        this.startAutoSave();

        console.log('[ConsultatiiAutoSave] Initialized with interval:', this.autoSaveIntervalMs);
    },

    /**
     * Start the auto-save timer
     */
    startAutoSave: function () {
        if (this.autoSaveTimer) {
            clearInterval(this.autoSaveTimer);
        }

        this.autoSaveTimer = setInterval(() => {
            if (this.dotNetReference) {
                this.dotNetReference.invokeMethodAsync('OnAutoSaveTick');
            }
        }, this.autoSaveIntervalMs);
    },

    /**
     * Trigger debounced save (called when user makes changes)
     */
    triggerDebouncedSave: function () {
        if (this.debounceTimer) {
            clearTimeout(this.debounceTimer);
        }

        this.debounceTimer = setTimeout(() => {
            if (this.dotNetReference) {
                this.dotNetReference.invokeMethodAsync('OnDebouncedSave');
            }
        }, this.debounceMs);
    },

    /**
     * Pause auto-save (e.g., when timer is paused)
     */
    pause: function () {
        if (this.autoSaveTimer) {
            clearInterval(this.autoSaveTimer);
            this.autoSaveTimer = null;
        }
        console.log('[ConsultatiiAutoSave] Paused');
    },

    /**
     * Resume auto-save
     */
    resume: function () {
        this.startAutoSave();
        console.log('[ConsultatiiAutoSave] Resumed');
    },

    /**
     * Dispose of timers
     */
    dispose: function () {
        if (this.autoSaveTimer) {
            clearInterval(this.autoSaveTimer);
        }
        if (this.debounceTimer) {
            clearTimeout(this.debounceTimer);
        }
        this.dotNetReference = null;
        console.log('[ConsultatiiAutoSave] Disposed');
    }
};

/**
 * Focus management utilities
 */
window.ConsultatiiFocus = {
    /**
     * Highlight the current section being edited
     * @param {string} sectionId - The section element ID
     */
    highlightSection: function (sectionId) {
        // Remove previous highlights
        document.querySelectorAll('.form-section.editing').forEach(el => {
            el.classList.remove('editing');
        });

        // Add highlight to current section
        const section = document.getElementById(sectionId);
        if (section) {
            section.classList.add('editing');
        }
    },

    /**
     * Scroll to a specific element smoothly
     * @param {string} elementId - Element ID to scroll to
     */
    scrollToElement: function (elementId) {
        const element = document.getElementById(elementId);
        if (element) {
            element.scrollIntoView({ behavior: 'smooth', block: 'center' });
        }
    },

    /**
     * Show validation error on a field
     * @param {string} fieldName - Field name/ID
     * @param {string} errorMessage - Error message to display
     */
    showFieldError: function (fieldName, errorMessage) {
        const field = document.querySelector(`[name="${fieldName}"], #${fieldName}`);
        if (field) {
            field.classList.add('is-invalid');
            
            // Create or update error tooltip
            let errorEl = field.parentElement.querySelector('.field-error');
            if (!errorEl) {
                errorEl = document.createElement('span');
                errorEl.className = 'field-error text-danger';
                field.parentElement.appendChild(errorEl);
            }
            errorEl.textContent = errorMessage;
        }
    },

    /**
     * Clear validation error from a field
     * @param {string} fieldName - Field name/ID
     */
    clearFieldError: function (fieldName) {
        const field = document.querySelector(`[name="${fieldName}"], #${fieldName}`);
        if (field) {
            field.classList.remove('is-invalid');
            const errorEl = field.parentElement.querySelector('.field-error');
            if (errorEl) {
                errorEl.remove();
            }
        }
    }
};
