window.personalMedicalEffects = (function() {
    return {
        highlightRow: function(personalId, durationMs) {
            try {
                var anchor = document.getElementById('row-anchor-' + personalId);
                if (!anchor) return false;
                var row = anchor.closest('.e-row');
                if (!row) return false;
                row.classList.add('row-highlight');
                setTimeout(function() { row.classList.remove('row-highlight'); }, durationMs || 2000);
                return true;
            } catch (e) {
                console.debug('personalMedicalEffects.highlightRow error', e);
                return false;
            }
        }
    };
})();