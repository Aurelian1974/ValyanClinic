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

        // Aplicăm stiluri inline pentru a forța page-break corect
        const sectionsToProtect = element.querySelectorAll('.section, .checkbox-section, .treatment-table, .checkbox-group, .doc-footer, .footer-section');
        sectionsToProtect.forEach(section => {
            section.style.pageBreakInside = 'avoid';
            section.style.breakInside = 'avoid';
        });

        // Protejăm titlurile de a fi separate de conținut
        const titles = element.querySelectorAll('.section-title, .checkbox-section-title, h2, h3, h4');
        titles.forEach(title => {
            title.style.pageBreakAfter = 'avoid';
            title.style.breakAfter = 'avoid';
        });

        // Protejăm rândurile de tabel
        const tableRows = element.querySelectorAll('tr');
        tableRows.forEach(row => {
            row.style.pageBreakInside = 'avoid';
            row.style.breakInside = 'avoid';
        });

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
                windowHeight: element.scrollHeight,
                // Îmbunătățire pentru paginare
                height: element.scrollHeight,
                windowWidth: element.scrollWidth
            },
            jsPDF: {
                unit: 'mm',
                format: 'a4',
                orientation: 'portrait'
            },
            pagebreak: { 
                mode: ['avoid-all', 'css', 'legacy'],
                before: '.page-break-before',
                after: '.page-break-after',
                avoid: [
                    '.section',
                    '.checkbox-section', 
                    '.treatment-table',
                    '.checkbox-group',
                    '.checkbox-item',
                    '.notes-box',
                    '.attention-box',
                    '.subsection',
                    '.footer-section',
                    '.doc-footer',
                    '.section-title',
                    '.checkbox-section-title',
                    'table',
                    'thead',
                    'tbody',
                    'tr'
                ]
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
