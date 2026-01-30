using Syncfusion.HtmlConverter;
using Syncfusion.Pdf;
using Syncfusion.Pdf.Graphics;

namespace ValyanClinic.Application.Services.ScrisoareMedicala;

/// <summary>
/// Generator PDF pentru Scrisoare Medicală folosind Syncfusion HTML-to-PDF
/// Convertește exact HTML+CSS în PDF, păstrând toate stilurile
/// Utilizează motorul Blink (Chromium) care este inclus în pachetul NuGet
/// </summary>
public class SyncfusionPdfGenerator
{
    /// <summary>
    /// Generează PDF din HTML și CSS
    /// </summary>
    /// <param name="htmlContent">Conținutul HTML al documentului (doar partea de document, fără overlay/toolbar)</param>
    /// <param name="cssContent">CSS-ul complet pentru stilizare</param>
    /// <returns>Byte array cu PDF-ul generat</returns>
    public byte[] GeneratePdfFromHtml(string htmlContent, string cssContent)
    {
        Console.WriteLine($"[SyncfusionPdfGenerator] Starting HTML to PDF conversion...");
        
        // Construiește HTML-ul complet cu CSS inline (fără resurse externe pentru a evita timeout)
        var fullHtml = BuildFullHtmlDocument(htmlContent, cssContent);
        Console.WriteLine($"[SyncfusionPdfGenerator] Full HTML built, length: {fullHtml.Length} chars");

        // Inițializează convertorul HTML to PDF
        var htmlConverter = new HtmlToPdfConverter();

        // Configurare BlinkConverterSettings
        var blinkConverterSettings = new BlinkConverterSettings();
        
        // Setează calea explicită către Blink binaries (Chrome)
        var blinkPath = GetBlinkBinariesPath();
        if (!string.IsNullOrEmpty(blinkPath))
        {
            blinkConverterSettings.BlinkPath = blinkPath;
            Console.WriteLine($"[SyncfusionPdfGenerator] BlinkPath set to: {blinkPath}");
        }
        
        // CRITICAL: Command line arguments pentru a rula Chrome fără sandbox
        // Acestea sunt necesare pentru a evita probleme de permisiuni
        blinkConverterSettings.CommandLineArguments.Add("--no-sandbox");
        blinkConverterSettings.CommandLineArguments.Add("--disable-setuid-sandbox");
        blinkConverterSettings.CommandLineArguments.Add("--disable-gpu");
        blinkConverterSettings.CommandLineArguments.Add("--disable-extensions");
        blinkConverterSettings.CommandLineArguments.Add("--disable-dev-shm-usage");
        blinkConverterSettings.CommandLineArguments.Add("--headless");
        
        // Setări pagină A4
        blinkConverterSettings.ViewPortSize = new Syncfusion.Drawing.Size(1280, 0); // Width fix, height auto
        
        // Marginile sunt în CSS, deci marginile PDF sunt 0
        blinkConverterSettings.Margin = new PdfMargins { All = 0 };
        
        // Alte setări recomandate
        blinkConverterSettings.EnableJavaScript = false; // Nu avem nevoie de JS, doar CSS
        blinkConverterSettings.AdditionalDelay = 500; // 500ms pentru randare
        blinkConverterSettings.MediaType = MediaType.Print;

        htmlConverter.ConverterSettings = blinkConverterSettings;
        
        Console.WriteLine($"[SyncfusionPdfGenerator] Converting HTML string to PDF with Blink engine...");
        
        // Convertește HTML STRING în PDF
        // Al doilea parametru este baseUrl pentru resurse relative (empty string = nu avem)
        using var pdfDocument = htmlConverter.Convert(fullHtml, string.Empty);
        
        Console.WriteLine($"[SyncfusionPdfGenerator] Conversion successful, saving to memory stream...");
        
        // Salvează în memory stream
        using var memoryStream = new MemoryStream();
        pdfDocument.Save(memoryStream);
        pdfDocument.Close(true);
        
        var result = memoryStream.ToArray();
        Console.WriteLine($"[SyncfusionPdfGenerator] PDF generated successfully, size: {result.Length} bytes");
        
        return result;
    }
    
    /// <summary>
    /// Obține calea către Blink binaries din NuGet packages
    /// </summary>
    private static string GetBlinkBinariesPath()
    {
        // Încearcă să găsească binaries în NuGet packages
        var userProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        var nugetPath = Path.Combine(userProfile, ".nuget", "packages", 
            "syncfusion.htmltopdfconverter.net.windows", "28.1.33", "runtimes", "win-x64", "native");
        
        if (Directory.Exists(nugetPath) && File.Exists(Path.Combine(nugetPath, "chrome.exe")))
        {
            Console.WriteLine($"[SyncfusionPdfGenerator] Found Blink binaries at: {nugetPath}");
            return nugetPath;
        }
        
        // Încearcă alte versiuni
        var packagesPath = Path.Combine(userProfile, ".nuget", "packages", "syncfusion.htmltopdfconverter.net.windows");
        if (Directory.Exists(packagesPath))
        {
            var versions = Directory.GetDirectories(packagesPath).OrderByDescending(d => d).ToList();
            foreach (var version in versions)
            {
                var nativePath = Path.Combine(version, "runtimes", "win-x64", "native");
                if (Directory.Exists(nativePath) && File.Exists(Path.Combine(nativePath, "chrome.exe")))
                {
                    Console.WriteLine($"[SyncfusionPdfGenerator] Found Blink binaries at: {nativePath}");
                    return nativePath;
                }
            }
        }
        
        Console.WriteLine($"[SyncfusionPdfGenerator] WARNING: Blink binaries not found!");
        return string.Empty;
    }

    /// <summary>
    /// Generează PDF din HTML și CSS și salvează într-un fișier
    /// </summary>
    public void GeneratePdfFromHtmlAndSave(string htmlContent, string cssContent, string filePath)
    {
        var pdfBytes = GeneratePdfFromHtml(htmlContent, cssContent);
        File.WriteAllBytes(filePath, pdfBytes);
    }

    /// <summary>
    /// Construiește documentul HTML complet cu CSS inline
    /// IMPORTANT: Nu folosim resurse externe (Google Fonts, Font Awesome) pentru a evita timeout
    /// Toate stilurile sunt inline pentru conversie rapidă
    /// </summary>
    private string BuildFullHtmlDocument(string htmlContent, string cssContent)
    {
        return $@"<!DOCTYPE html>
<html lang=""ro"">
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>Scrisoare Medicală - Anexa 43</title>
    
    <style>
        /* Reset și setări de bază */
        * {{
            margin: 0;
            padding: 0;
            box-sizing: border-box;
        }}
        
        body {{
            font-family: Arial, Helvetica, sans-serif;
            font-size: 14px;
            line-height: 1.5;
            color: #333;
            background: #fff;
            -webkit-print-color-adjust: exact !important;
            print-color-adjust: exact !important;
            color-adjust: exact !important;
        }}
        
        /* Font substitution - folosim fonturi sistem în loc de Google Fonts */
        .clinic-name, .doc-title h1, .section-title {{
            font-family: Georgia, 'Times New Roman', serif;
        }}
        
        /* Icon placeholders - înlocuim Font Awesome cu text/simboluri */
        .fa, .fas, .far, .fab {{
            font-family: Arial, sans-serif;
            font-style: normal;
        }}
        
        /* CSS din componentă */
        {cssContent}
        
        /* Suprascrie stilurile pentru PDF (nu avem overlay/toolbar) */
        .document-container {{
            padding: 15mm 18mm;
            background: #fff;
            max-width: 210mm;
            margin: 0 auto;
        }}
    </style>
</head>
<body>
    {htmlContent}
</body>
</html>";
    }

    /// <summary>
    /// Obține CSS-ul pentru print din fișierul CSS
    /// Extrage doar stilurile necesare pentru PDF (fără overlay, toolbar, etc.)
    /// </summary>
    public static string GetPrintCss()
    {
        // CSS-ul este inclus direct - în producție ar fi citit din fișier
        return @"
/* ==================== Document Container ==================== */
.document-container {
    padding: 20mm 18mm;
    background: #fff;
}

/* ==================== Header ==================== */
.doc-header {
    display: flex;
    justify-content: space-between;
    align-items: flex-start;
    padding-bottom: 15px;
    border-bottom: 2px solid #3b82f6;
    margin-bottom: 20px;
}

.clinic-info {
    flex: 1;
}

.clinic-name {
    font-family: 'Crimson Pro', Georgia, serif;
    font-size: 1.6rem;
    font-weight: 700;
    color: #3b82f6;
    margin-bottom: 4px;
}

.clinic-details {
    font-size: 0.8rem;
    color: #666;
    line-height: 1.5;
}

.clinic-details span {
    display: block;
}

.header-right {
    display: flex;
    align-items: flex-start;
    gap: 15px;
}

.doc-logo {
    width: 70px;
    height: 70px;
    background: linear-gradient(135deg, #3b82f6 0%, #2563eb 100%);
    border-radius: 50%;
    display: flex;
    align-items: center;
    justify-content: center;
    color: #fff;
    font-size: 1.8rem;
}

.anexa-badge {
    background: #3b82f6;
    color: #fff;
    padding: 6px 12px;
    border-radius: 6px;
    font-size: 0.75rem;
    font-weight: 700;
    text-align: center;
}

/* ==================== Document Title ==================== */
.doc-title {
    text-align: center;
    margin: 25px 0;
}

.doc-title h1 {
    font-family: 'Crimson Pro', Georgia, serif;
    font-size: 1.5rem;
    font-weight: 700;
    color: #1a1a1a;
    text-transform: uppercase;
    letter-spacing: 0.1em;
    margin-bottom: 5px;
}

.doc-number {
    font-size: 0.8rem;
    color: #666;
    margin-top: 5px;
}

/* ==================== Patient Box ==================== */
.patient-box {
    background: #f8f9fa;
    border: 1px solid #ddd;
    border-radius: 8px;
    padding: 15px 18px;
    margin-bottom: 20px;
}

.patient-box-title {
    font-size: 0.7rem;
    font-weight: 700;
    text-transform: uppercase;
    letter-spacing: 0.05em;
    color: #3b82f6;
    margin-bottom: 10px;
}

.patient-grid {
    display: grid;
    grid-template-columns: repeat(2, 1fr);
    gap: 8px 20px;
}

.patient-field {
    display: flex;
    font-size: 0.85rem;
}

.patient-label {
    color: #666;
    min-width: 110px;
}

.patient-value {
    font-weight: 600;
    color: #1a1a1a;
}

/* ==================== Intro Text ==================== */
.intro-text {
    background: linear-gradient(135deg, #f8f9fa 0%, #e9ecef 100%);
    border-left: 4px solid #3b82f6;
    padding: 15px 18px;
    margin-bottom: 20px;
    border-radius: 0 8px 8px 0;
    font-size: 0.9rem;
    color: #333;
    line-height: 1.7;
}

/* ==================== Sections ==================== */
.section {
    margin-bottom: 18px;
    page-break-inside: avoid !important;
    break-inside: avoid !important;
}

.section-title {
    font-family: 'Crimson Pro', Georgia, serif;
    font-size: 1rem;
    font-weight: 700;
    color: #3b82f6;
    border-bottom: 1px solid #ddd;
    padding-bottom: 5px;
    margin-bottom: 10px;
    display: flex;
    align-items: center;
    gap: 8px;
    page-break-after: avoid !important;
    break-after: avoid !important;
}

.section-title i {
    font-size: 0.9rem;
    opacity: 0.7;
}

.section-content {
    font-size: 0.9rem;
    color: #333;
    text-align: justify;
}

.section-content p {
    margin-bottom: 8px;
}

.section-content p:last-child {
    margin-bottom: 0;
}

/* ==================== Subsections ==================== */
.subsection {
    margin-bottom: 12px;
}

.subsection-title {
    font-size: 0.8rem;
    font-weight: 700;
    color: #1a1a1a;
    margin-bottom: 4px;
}

.subsection-content {
    font-size: 0.85rem;
    color: #333;
    padding-left: 12px;
}

.subsection-inline {
    margin-bottom: 8px;
    display: flex;
    flex-wrap: wrap;
    align-items: baseline;
    gap: 6px;
}

.subsection-inline .subsection-title {
    font-size: 0.8rem;
    font-weight: 700;
    color: #1a1a1a;
    margin-bottom: 0;
    flex-shrink: 0;
}

.subsection-inline .subsection-content {
    font-size: 0.85rem;
    color: #333;
    padding-left: 0;
    flex: 1;
}

/* ==================== Examen Clinic Compact ==================== */
.examen-clinic-compact {
    display: flex;
    flex-wrap: wrap;
    gap: 8px 16px;
    margin-bottom: 10px;
    font-size: 0.85rem;
    line-height: 1.4;
}

.examen-clinic-compact span {
    display: inline-block;
}

.examen-clinic-compact strong {
    color: #1a1a1a;
    font-weight: 600;
}

/* ==================== Data Grid ==================== */
.data-grid {
    display: grid;
    grid-template-columns: repeat(4, 1fr);
    gap: 8px;
    margin-bottom: 10px;
}

.data-item {
    background: #f8f9fa;
    padding: 8px 10px;
    border-radius: 4px;
    text-align: center;
}

.data-label {
    font-size: 0.65rem;
    text-transform: uppercase;
    letter-spacing: 0.03em;
    color: #666;
    margin-bottom: 2px;
}

.data-value {
    font-size: 0.9rem;
    font-weight: 700;
    color: #1a1a1a;
}

.data-unit {
    font-size: 0.7rem;
    font-weight: 400;
    color: #666;
}

/* ==================== Diagnosis Box ==================== */
.diagnosis-box {
    border: 2px solid #3b82f6;
    border-radius: 8px;
    overflow: hidden;
    margin-bottom: 15px;
}

.diagnosis-header {
    background: #3b82f6;
    color: #fff;
    padding: 8px 12px;
    font-size: 0.75rem;
    font-weight: 700;
    text-transform: uppercase;
    letter-spacing: 0.05em;
}

.diagnosis-content {
    padding: 12px;
}

.diagnosis-item {
    margin-bottom: 10px;
    padding-bottom: 10px;
    border-bottom: 1px dashed #ddd;
}

.diagnosis-item:last-child {
    margin-bottom: 0;
    padding-bottom: 0;
    border-bottom: none;
}

.diagnosis-type {
    display: inline-block;
    font-size: 0.65rem;
    font-weight: 700;
    text-transform: uppercase;
    padding: 2px 8px;
    border-radius: 3px;
    margin-bottom: 4px;
}

.diagnosis-type.principal {
    background: #3b82f6;
    color: #fff;
}

.diagnosis-type.secundar {
    background: #e0e7ff;
    color: #4338ca;
}

.diagnosis-code {
    font-family: 'Courier New', monospace;
    font-size: 0.85rem;
    font-weight: 700;
    color: #3b82f6;
}

.diagnosis-name {
    font-size: 0.9rem;
    font-weight: 600;
    color: #1a1a1a;
    margin-left: 8px;
}

.diagnosis-details {
    font-size: 0.8rem;
    color: #666;
    margin-top: 4px;
    padding-left: 4px;
    font-style: italic;
}

/* ==================== Oncologic Status ==================== */
.oncologic-status {
    display: inline-flex;
    align-items: center;
    gap: 15px;
    background-color: #eff6ff !important;
    padding: 10px 15px;
    border-radius: 6px;
    border: 2px solid #3b82f6 !important;
    margin-bottom: 15px;
    font-size: 0.85rem;
    page-break-inside: avoid;
    break-inside: avoid;
    -webkit-print-color-adjust: exact !important;
    print-color-adjust: exact !important;
    color-adjust: exact !important;
}

.oncologic-status .status-label {
    font-weight: 700;
    color: #000 !important;
}

.oncologic-option {
    display: inline-flex;
    align-items: center;
    gap: 5px;
    color: #000 !important;
    font-weight: 600;
}

/* ==================== Lab Results ==================== */
.lab-results {
    display: grid;
    grid-template-columns: 1fr 1fr;
    gap: 15px;
    margin-bottom: 10px;
}

.lab-section {
    background: #f8f9fa;
    padding: 12px;
    border-radius: 6px;
}

.lab-section-title {
    font-size: 0.75rem;
    font-weight: 700;
    text-transform: uppercase;
    color: #3b82f6;
    margin-bottom: 8px;
    padding-bottom: 5px;
    border-bottom: 1px solid #ddd;
}

.lab-section.pathologic {
    background: #fef2f2;
    border: 1px solid #fecaca;
}

.lab-section.pathologic .lab-section-title {
    color: #dc2626;
}

.lab-item {
    font-size: 0.8rem;
    margin-bottom: 4px;
}

.lab-item:last-child {
    margin-bottom: 0;
}

/* ==================== Analize Efectuate ==================== */
.analize-efectuate-container {
    display: flex;
    flex-direction: column;
    gap: 15px;
}

.analize-efectuate-section {
    border-radius: 6px;
    padding: 12px;
    page-break-inside: avoid;
    break-inside: avoid;
}

.analize-efectuate-section.anormale {
    background: #fef2f2;
    border: 1px solid #fecaca;
}

.analize-efectuate-section.normale {
    background: #f0fdf4;
    border: 1px solid #bbf7d0;
}

.analize-efectuate-title {
    font-size: 0.75rem;
    font-weight: 700;
    text-transform: uppercase;
    margin-bottom: 10px;
    display: flex;
    align-items: center;
    gap: 6px;
}

.analize-efectuate-section.anormale .analize-efectuate-title {
    color: #dc2626;
}

.analize-efectuate-section.normale .analize-efectuate-title {
    color: #16a34a;
}

.analize-efectuate-table {
    width: 100%;
    border-collapse: collapse;
    font-size: 0.75rem;
}

.analize-efectuate-table th {
    background: rgba(0, 0, 0, 0.05);
    padding: 5px 8px;
    text-align: left;
    font-weight: 600;
    font-size: 0.65rem;
    text-transform: uppercase;
    border-bottom: 1px solid rgba(0, 0, 0, 0.1);
}

.analize-efectuate-table td {
    padding: 4px 8px;
    border-bottom: 1px solid rgba(0, 0, 0, 0.05);
    vertical-align: middle;
}

.analize-efectuate-table tr:last-child td {
    border-bottom: none;
}

.analize-efectuate-table .row-anormal td {
    color: #991b1b;
}

.analize-efectuate-table .row-anormal td strong {
    color: #dc2626;
}

.analize-normale-grid {
    display: grid;
    grid-template-columns: repeat(2, 1fr);
    gap: 4px 16px;
}

.analiza-normala-item {
    font-size: 0.75rem;
    display: flex;
    gap: 6px;
}

.analiza-normala-item .analiza-nume {
    color: #374151;
}

.analiza-normala-item .analiza-rezultat {
    font-weight: 600;
    color: #16a34a;
}

/* ==================== Treatment Table ==================== */
.treatment-table {
    width: 100%;
    border-collapse: collapse;
    font-size: 0.8rem;
    margin-bottom: 12px;
    page-break-inside: avoid !important;
    break-inside: avoid !important;
}

.treatment-table thead,
.treatment-table tbody {
    page-break-inside: avoid !important;
    break-inside: avoid !important;
}

.treatment-table th {
    background: #f8f9fa;
    padding: 8px 10px;
    text-align: left;
    font-weight: 700;
    color: #1a1a1a;
    border-bottom: 2px solid #ddd;
    font-size: 0.7rem;
    text-transform: uppercase;
    letter-spacing: 0.03em;
}

.treatment-table td {
    padding: 8px 10px;
    border-bottom: 1px solid #ddd;
    vertical-align: top;
}

.treatment-table tr {
    page-break-inside: avoid;
    break-inside: avoid;
}

.treatment-table tr:last-child td {
    border-bottom: none;
}

.med-name {
    font-weight: 600;
    color: #1a1a1a;
}

.med-note {
    font-size: 0.75rem;
    color: #3b82f6;
    font-style: italic;
}

/* ==================== CITO Badge & Row ==================== */
.cito-badge {
    display: inline-block;
    background: #dc2626;
    color: #fff;
    font-size: 0.6rem;
    font-weight: 700;
    padding: 2px 6px;
    border-radius: 3px;
    margin-left: 8px;
    text-transform: uppercase;
    letter-spacing: 0.05em;
}

.cito-row {
    background-color: #fef2f2;
}

.cito-row td {
    border-left: 3px solid #dc2626;
}

/* ==================== Analize Recomandate - 3 Columns Grid ==================== */
.analize-grid-3col {
    display: grid;
    grid-template-columns: repeat(3, 1fr);
    gap: 12px;
    margin-top: 10px;
}

.analize-column {
    min-width: 0;
}

.analize-compact-table {
    width: 100%;
    border-collapse: collapse;
    font-size: 0.7rem;
    page-break-inside: avoid;
    break-inside: avoid;
}

.analize-compact-table th {
    background: #f1f5f9;
    padding: 4px 6px;
    text-align: left;
    font-weight: 600;
    color: #1a1a1a;
    border-bottom: 1px solid #cbd5e1;
    font-size: 0.65rem;
    text-transform: uppercase;
    letter-spacing: 0.02em;
}

.analize-compact-table td {
    padding: 3px 6px;
    border-bottom: 1px solid #e2e8f0;
    vertical-align: middle;
    line-height: 1.3;
    word-break: break-word;
}

.analize-compact-table tr:last-child td {
    border-bottom: none;
}

.analize-compact-table .cito-row td:first-child {
    border-left: 2px solid #dc2626;
    padding-left: 4px;
}

.cito-badge-sm {
    display: inline-block;
    background: #dc2626;
    color: #fff;
    font-size: 0.55rem;
    font-weight: 700;
    width: 12px;
    height: 12px;
    line-height: 12px;
    text-align: center;
    border-radius: 50%;
    margin-left: 3px;
    vertical-align: middle;
}

/* ==================== Recommendations List ==================== */
.recommendations-list {
    padding-left: 20px;
    margin: 0;
}

.recommendations-list li {
    margin-bottom: 6px;
    font-size: 0.85rem;
}

/* ==================== Checkbox Sections ==================== */
.checkbox-section {
    background: #f8f9fa;
    border-radius: 8px;
    padding: 15px;
    margin-bottom: 15px;
    page-break-inside: avoid !important;
    break-inside: avoid !important;
}

.checkbox-section-title {
    font-size: 0.75rem;
    font-weight: 700;
    text-transform: uppercase;
    color: #3b82f6;
    margin-bottom: 10px;
    page-break-after: avoid !important;
    break-after: avoid !important;
}

.checkbox-group {
    display: flex;
    flex-direction: column;
    gap: 8px;
    page-break-inside: avoid;
    break-inside: avoid;
}

.checkbox-item {
    display: flex;
    align-items: flex-start;
    gap: 10px;
    font-size: 0.85rem;
}

.checkbox {
    width: 18px;
    height: 18px;
    border: 2px solid #3b82f6;
    border-radius: 4px;
    display: flex;
    align-items: center;
    justify-content: center;
    flex-shrink: 0;
    margin-top: 1px;
    font-size: 12px;
    color: #3b82f6;
    font-weight: bold;
    background: #fff;
}

.checkbox.checked {
    background: #3b82f6;
    color: #fff;
}

.checkbox.checkbox-small {
    width: 14px;
    height: 14px;
    font-size: 10px;
}

.checkbox-label {
    flex: 1;
    color: #333;
}

.checkbox-label .field-value {
    font-weight: 600;
    color: #3b82f6;
}

/* ==================== Notes Box ==================== */
.notes-box {
    background: #fffbeb;
    border: 1px solid #fcd34d;
    border-radius: 8px;
    padding: 15px;
    margin: 20px 0;
    page-break-inside: avoid;
    break-inside: avoid;
}

.notes-box-title {
    font-size: 0.75rem;
    font-weight: 700;
    text-transform: uppercase;
    color: #b45309;
    margin-bottom: 10px;
    display: flex;
    align-items: center;
    gap: 8px;
}

.notes-box p {
    font-size: 0.8rem;
    color: #92400e;
    margin-bottom: 8px;
    text-align: justify;
}

.notes-box p:last-child {
    margin-bottom: 0;
}

.attention-box {
    background: #fef2f2;
    border: 1px solid #fecaca;
    border-radius: 6px;
    padding: 12px;
    margin: 10px 0;
    page-break-inside: avoid;
    break-inside: avoid;
}

.attention-title {
    font-size: 0.8rem;
    font-weight: 700;
    color: #dc2626;
    margin-bottom: 6px;
}

.attention-box p {
    font-size: 0.8rem;
    color: #991b1b;
}

/* ==================== Footer ==================== */
.doc-footer {
    margin-top: 30px;
    padding-top: 15px;
    border-top: 1px solid #ddd;
    page-break-inside: avoid !important;
    break-inside: avoid !important;
}

.footer-grid {
    display: grid;
    grid-template-columns: 1fr 1fr;
    gap: 30px;
    page-break-inside: avoid !important;
    break-inside: avoid !important;
}

.footer-section h4 {
    font-size: 0.7rem;
    font-weight: 700;
    text-transform: uppercase;
    letter-spacing: 0.05em;
    color: #666;
    margin-bottom: 8px;
}

.doctor-signature {
    text-align: left;
}

.doctor-name {
    font-size: 1rem;
    font-weight: 700;
    color: #1a1a1a;
}

.doctor-specialty {
    font-size: 0.8rem;
    color: #666;
}

.doctor-stamp {
    margin-top: 10px;
    width: 80px;
    height: 80px;
    border: 2px dashed #ddd;
    border-radius: 50%;
    display: flex;
    align-items: center;
    justify-content: center;
    font-size: 0.65rem;
    color: #666;
    text-transform: uppercase;
    text-align: center;
}

.emit-date {
    font-size: 1.1rem;
    font-weight: 700;
    color: #3b82f6;
    margin-bottom: 15px;
}

.transmission-section {
    background: #f8f9fa;
    padding: 12px;
    border-radius: 6px;
}

.transmission-title {
    font-size: 0.75rem;
    font-weight: 700;
    text-transform: uppercase;
    color: #3b82f6;
    margin-bottom: 8px;
}

.transmission-options {
    display: flex;
    flex-direction: column;
    gap: 6px;
}

.transmission-option {
    display: flex;
    align-items: center;
    gap: 8px;
    font-size: 0.85rem;
}

/* ==================== Footnotes ==================== */
.footnotes {
    margin-top: 20px;
    padding-top: 15px;
    border-top: 1px dashed #ddd;
}

.footnotes p {
    font-size: 0.75rem;
    color: #666;
    margin-bottom: 6px;
    text-align: justify;
}
";
    }
}
