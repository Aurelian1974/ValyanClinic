// 🔍 Login Performance Monitor
// Plasează acest cod în browser console (F12) pentru a diagnostica "dubla încărcare"

window.loginMonitor = {
    events: [],
    startTime: null,
    
    log: function(event, details = '') {
        const time = this.startTime ? (performance.now() - this.startTime).toFixed(0) : 0;
        const entry = {
            time: `+${time}ms`,
            event: event,
            details: details,
            timestamp: new Date().toISOString()
        };
        this.events.push(entry);
        console.log(`[+${time}ms] ${event}`, details);
    },
    
    start: function() {
        this.events = [];
        this.startTime = performance.now();
        this.log('🎬 LOGIN MONITORING START');
        
        // Monitor form submission
        const form = document.querySelector('form');
        if (form) {
            form.addEventListener('submit', () => {
                this.log('📝 Form submitted');
            });
        }
        
        // Monitor fetch calls
        const originalFetch = window.fetch;
        window.fetch = async (...args) => {
            this.log('🌐 Fetch START', args[0]);
            const response = await originalFetch(...args);
            this.log('✅ Fetch END', `Status: ${response.status}`);
            return response;
        };
        
        // Monitor navigation
        const originalNavigateTo = window.Blazor?.navigateTo || (() => {});
        if (window.Blazor) {
            window.Blazor.navigateTo = (url, forceLoad) => {
                this.log('🔄 Navigation START', `URL: ${url}, forceLoad: ${forceLoad}`);
                return originalNavigateTo.call(window.Blazor, url, forceLoad);
            };
        }
        
        // Monitor page visibility changes
        document.addEventListener('visibilitychange', () => {
            this.log('👁️ Visibility changed', `Hidden: ${document.hidden}`);
        });
        
        // Monitor Blazor events
        if (window.Blazor) {
            this.log('✅ Blazor detected');
        }
        
        console.log('📊 Login Monitor Ready! Click "Autentificare" to track events.');
    },
    
    report: function() {
        console.log('\n📊 ===== LOGIN PERFORMANCE REPORT =====\n');
        console.table(this.events);
        
        const totalTime = this.events.length > 0 
            ? parseInt(this.events[this.events.length - 1].time.replace('+', '').replace('ms', ''))
            : 0;
        
        console.log(`\n⏱️ Total Time: ${totalTime}ms`);
        console.log(`📍 Events Count: ${this.events.length}`);
        
        // Identify potential issues
        const issues = [];
        
        // Check for multiple navigations
        const navEvents = this.events.filter(e => e.event.includes('Navigation'));
        if (navEvents.length > 1) {
            issues.push(`⚠️ Multiple navigations detected (${navEvents.length})`);
        }
        
        // Check for slow fetch
        const fetchEvents = this.events.filter(e => e.event.includes('Fetch'));
        if (fetchEvents.length >= 2) {
            const fetchTime = parseInt(fetchEvents[1].time.replace('+', '').replace('ms', '')) -
                            parseInt(fetchEvents[0].time.replace('+', '').replace('ms', ''));
            if (fetchTime > 500) {
                issues.push(`⚠️ Slow API call: ${fetchTime}ms`);
            }
        }
        
        if (issues.length > 0) {
            console.log('\n🐛 Potential Issues:');
            issues.forEach(issue => console.log(issue));
        } else {
            console.log('\n✅ No obvious issues detected');
        }
        
        console.log('\n=======================================\n');
    },
    
    clear: function() {
        this.events = [];
        this.startTime = null;
        console.clear();
        console.log('🧹 Monitor cleared');
    }
};

// Auto-start monitoring
console.log('🔍 Login Performance Monitor loaded!');
console.log('Commands:');
console.log('  loginMonitor.start()  - Start monitoring');
console.log('  loginMonitor.report() - Show detailed report');
console.log('  loginMonitor.clear()  - Clear and restart');
console.log('\n📝 Starting automatic monitoring...');
loginMonitor.start();
