// Smart Tooltip Positioning for Calendar Appointments
window.initSmartTooltips = function () {
  console.log('Initializing smart tooltips...');

    // Remove previous event listeners
    document.querySelectorAll('.appointment-card').forEach(card => {
        const clone = card.cloneNode(true);
        card.parentNode?.replaceChild(clone, card);
    });

    // Add new event listeners
    const cards = document.querySelectorAll('.appointment-card');
    console.log(`Found ${cards.length} appointment cards`);

    cards.forEach(card => {
        card.addEventListener('mouseenter', function (e) {
            const tooltip = this.querySelector('.appointment-tooltip');
            if (!tooltip) {
       console.warn('No tooltip found in card');
            return;
    }

            // Get positions
    const cardRect = this.getBoundingClientRect();
        const scrollContainer = document.querySelector('.e-content-wrap');
            const containerRect = scrollContainer?.getBoundingClientRect();

            if (!containerRect) {
     console.warn('Scroll container not found');
                return;
    }

        // Calculate available space
    const tooltipHeight = 200; // Approximate height
      const spaceBelow = containerRect.bottom - cardRect.bottom;
            const spaceAbove = cardRect.top - containerRect.top;

            console.log(`Card position: top=${cardRect.top}, bottom=${cardRect.bottom}`);
       console.log(`Container: top=${containerRect.top}, bottom=${containerRect.bottom}`);
console.log(`Space: below=${spaceBelow}, above=${spaceAbove}`);

       // Decide position
            if (spaceBelow < tooltipHeight && spaceAbove > tooltipHeight) {
             console.log('Showing tooltip ABOVE');
     tooltip.classList.add('show-above');
            } else {
                console.log('Showing tooltip BELOW');
      tooltip.classList.remove('show-above');
   }

            // Adjust horizontal position for edge cases
            const cardLeft = cardRect.left;
   const cardRight = cardRect.right;
            const viewportWidth = window.innerWidth;

            if (cardLeft < 150) {
        tooltip.style.left = '0';
    tooltip.style.transform = 'translateX(0)';
          } else if (cardRight > viewportWidth - 150) {
       tooltip.style.left = 'auto';
         tooltip.style.right = '0';
                tooltip.style.transform = 'translateX(0)';
            } else {
                tooltip.style.left = '50%';
      tooltip.style.right = 'auto';
         tooltip.style.transform = 'translateX(-50%)';
            }
        });

     card.addEventListener('mouseleave', function () {
 const tooltip = this.querySelector('.appointment-tooltip');
       if (tooltip) {
            // Reset on mouse leave
      tooltip.classList.remove('show-above');
        }
        });
    });

console.log('Smart tooltips initialized successfully');
};

// Auto-initialize when DOM is ready
if (document.readyState === 'loading') {
    document.addEventListener('DOMContentLoaded', window.initSmartTooltips);
} else {
    window.initSmartTooltips();
}

// Re-initialize on window resize
let resizeTimeout;
window.addEventListener('resize', function () {
    clearTimeout(resizeTimeout);
resizeTimeout = setTimeout(window.initSmartTooltips, 250);
});
