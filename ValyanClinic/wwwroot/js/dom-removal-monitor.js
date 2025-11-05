// DOM Removal Monitor - Debug Tool pentru identificarea removeChild issues
// Acest script monitorizează toate removeChild operations pentru debugging
// ⚙️ OPT-IN: Activează cu window.enableDomMonitor = true în consolă

(function() {
    'use strict';
    
    // 🔧 CHECK: Doar dacă e explicit enabled
    if (!window.enableDomMonitor) {
        console.log('📊 [DOM Monitor] Disabled (set window.enableDomMonitor = true to enable)');
        
        // Expose API chiar și când e disabled pentru easy enabling
        window.domRemovalStats = {
   enable: function() {
        window.enableDomMonitor = true;
      console.log('✅ [DOM Monitor] Enabled! Reload page to activate monitoring.');
 },
        disable: function() {
     window.enableDomMonitor = false;
  console.log('❌ [DOM Monitor] Disabled! Reload page to stop monitoring.');
      }
        };
        
      return; // Exit early dacă nu e enabled
    }
    
    console.log('🔍 [DOM Monitor] Initializing removeChild monitoring...');
    
    // Track removal statistics
    const removalStats = {
        totalRemovals: 0,
      byTag: {},
        byId: {},
        byClass: {},
        lastRemoval: null,
        removalTimeline: []
    };
    
  // Helper: Safely get className as string
    function getClassName(element) {
        if (!element) return 'no-class';
        
        // Handle DOMTokenList (element.classList)
 if (element.classList && element.classList.length > 0) {
          return Array.from(element.classList).join(' ');
        }
        
   // Handle className property
        if (typeof element.className === 'string') {
   return element.className || 'no-class';
        }
        
        // Handle SVGAnimatedString (for SVG elements)
if (element.className && element.className.baseVal) {
            return element.className.baseVal || 'no-class';
        }
        
        return 'no-class';
    }
    
// Override native removeChild pentru monitoring
    const originalRemoveChild = Node.prototype.removeChild;
    
    Node.prototype.removeChild = function(child) {
        const timestamp = new Date().toISOString();
        const timeMs = performance.now();
        
        // Gather info despre elementul care e sters - SAFE extraction
        const parentClass = getClassName(this);
        const childClass = getClassName(child);
        
        const elementInfo = {
       timestamp,
            timeMs,
            parentTag: this.tagName || 'unknown',
            parentId: this.id || 'no-id',
     parentClass: parentClass,
    childTag: child.tagName || 'unknown',
   childId: child.id || 'no-id',
 childClass: childClass
        };
        
    // Update statistics
        removalStats.totalRemovals++;
        removalStats.lastRemoval = elementInfo;
        
        // Track by tag
        removalStats.byTag[elementInfo.childTag] = (removalStats.byTag[elementInfo.childTag] || 0) + 1;
   
        // Track by ID (if exists)
        if (elementInfo.childId !== 'no-id') {
    removalStats.byId[elementInfo.childId] = (removalStats.byId[elementInfo.childId] || 0) + 1;
        }

        // Track by class (first class only for simplicity) - SAFE split
        if (elementInfo.childClass !== 'no-class' && typeof elementInfo.childClass === 'string') {
            const firstClass = elementInfo.childClass.split(' ')[0];
   if (firstClass) {
     removalStats.byClass[firstClass] = (removalStats.byClass[firstClass] || 0) + 1;
       }
        }
        
     // Add to timeline (keep last 100)
  removalStats.removalTimeline.push(elementInfo);
        if (removalStats.removalTimeline.length > 100) {
      removalStats.removalTimeline.shift();
        }
   
        // 🚨 CRITICAL: Log important removals
  if (elementInfo.childId !== 'no-id' || 
            elementInfo.childTag === 'GRID' || 
          (typeof childClass === 'string' && childClass.includes('grid')) ||
            (typeof childClass === 'string' && childClass.includes('modal')) ||
     (typeof childClass === 'string' && childClass.includes('container'))) {
       
          console.warn('🚨 IMPORTANT ELEMENT REMOVED:', {
     time: timestamp,
      parent: `<${elementInfo.parentTag} id="${elementInfo.parentId}" class="${elementInfo.parentClass}">`,
     child: `<${elementInfo.childTag} id="${elementInfo.childId}" class="${elementInfo.childClass}">`,
                totalRemovals: removalStats.totalRemovals
     });
        }
        
 // 🔍 Special tracking pentru Syncfusion Grid elements
        if ((typeof childClass === 'string' && 
           (childClass.includes('e-grid') || childClass.includes('syncfusion'))) ||
          (typeof elementInfo.childId === 'string' && elementInfo.childId.includes('grid'))) {
     
         console.error('🔴 SYNCFUSION GRID ELEMENT REMOVED!', {
        time: timestamp,
                childId: elementInfo.childId,
      childClass: elementInfo.childClass,
       parentId: elementInfo.parentId,
     stackTrace: new Error().stack
            });
        }
        
        // Call original removeChild
        try {
     return originalRemoveChild.call(this, child);
    } catch (error) {
            console.error('❌ ERROR in removeChild:', {
       error: error.message,
      stack: error.stack,
        elementInfo,
    removalStats: {
           total: removalStats.totalRemovals,
    recentRemovals: removalStats.removalTimeline.slice(-5)
        }
         });
     throw error;
        }
    };
    
    // Expose stats globally pentru debugging
    window.domRemovalStats = {
      getStats: function() {
            return {
                ...removalStats,
  topTags: Object.entries(removalStats.byTag)
             .sort((a, b) => b[1] - a[1])
           .slice(0, 10),
     topIds: Object.entries(removalStats.byId)
  .sort((a, b) => b[1] - a[1])
     .slice(0, 10),
      topClasses: Object.entries(removalStats.byClass)
         .sort((a, b) => b[1] - a[1])
             .slice(0, 10)
            };
      },
        
        reset: function() {
            removalStats.totalRemovals = 0;
      removalStats.byTag = {};
   removalStats.byId = {};
            removalStats.byClass = {};
        removalStats.removalTimeline = [];
console.log('🔄 [DOM Monitor] Stats reset');
},
        
        printReport: function() {
            const stats = this.getStats();
console.log('📊 DOM REMOVAL REPORT:');
   console.log('Total removals:', stats.totalRemovals);
     console.log('\nTop 10 Tags:', stats.topTags);
         console.log('\nTop 10 IDs:', stats.topIds);
            console.log('\nTop 10 Classes:', stats.topClasses);
          console.log('\nRecent removals (last 10):', stats.removalTimeline.slice(-10));
        },
        
enable: function() {
  console.log('⚠️ [DOM Monitor] Already enabled! It was activated on page load.');
      },
        
        disable: function() {
  window.enableDomMonitor = false;
console.log('❌ [DOM Monitor] Will be disabled on next page reload.');
        }
    };
    
    // Auto-report on page unload
    window.addEventListener('beforeunload', function() {
        if (removalStats.totalRemovals > 0) {
          console.log('🏁 [DOM Monitor] Page unload - Final report:');
            window.domRemovalStats.printReport();
        }
    });
    
    // Report every 10 seconds if there's activity
    setInterval(function() {
  if (removalStats.totalRemovals > 0) {
            const recent = removalStats.removalTimeline.slice(-5);
            if (recent.length > 0) {
           console.log(`📊 [DOM Monitor] Activity report - Total: ${removalStats.totalRemovals}, Recent (last 5):`, recent);
}
        }
    }, 10000);
    
    console.log('✅ [DOM Monitor] removeChild monitoring active - Use window.domRemovalStats.printReport() for stats');
})();
