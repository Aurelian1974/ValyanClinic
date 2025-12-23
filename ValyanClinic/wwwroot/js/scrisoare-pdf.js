/**
 * Scrisoare Medicală PDF Generator
 * Folosește html2pdf.js pentru generarea PDF-ului din HTML
 */

// Funcție pentru generare PDF din elementul document
window.generateScrisoarePdf = async function (elementId, fileName) {
    const element = document.getElementById(elementId);
    if (!element) {
        console.error('Element not found:', elementId);
        return false;
    }

    // Verificăm dacă html2pdf este încărcat
    if (typeof html2pdf === 'undefined') {
        console.error('html2pdf library not loaded');
        // Fallback la print
        window.print();
        return false;
    }

    try {
        // Ascundem toolbar-ul temporar
        const toolbar = element.querySelector('.toolbar');
        if (toolbar) {
            toolbar.style.display = 'none';
        }

        // Configurare pentru PDF A4 cu page-break corect
        const opt = {
            margin: [10, 10, 15, 10], // top, left, bottom, right în mm (mai mult spațiu jos)
            filename: fileName || 'ScrisoareMedicala.pdf',
            image: { type: 'jpeg', quality: 0.98 },
            html2canvas: {
                scale: 2,
                useCORS: true,
                letterRendering: true,
                logging: false,
                scrollY: 0,
                windowHeight: element.scrollHeight
            },
            jsPDF: {
                unit: 'mm',
                format: 'a4',
                orientation: 'portrait'
            },
            pagebreak: { 
                mode: ['css', 'legacy'],
                before: '.page-break-before',
                after: '.page-break-after',
                avoid: '.section, .checkbox-section, .treatment-table, .checkbox-group, .notes-box, .attention-box, .subsection, .footer-section, tr'
            }
        };

        // Generare și descărcare PDF
        await html2pdf().set(opt).from(element).save();
        
        // Restaurăm toolbar-ul
        if (toolbar) {
            toolbar.style.display = '';
        }
        
        return true;
    } catch (error) {
        console.error('Error generating PDF:', error);
        // Fallback la print în caz de eroare
        window.print();
        return false;
    }
};

// Funcție alternativă care folosește print-to-PDF nativ
window.printScrisoareToPdf = function () {
    window.print();
};
