# ========================================
# PowerShell Script: Download ICD-10 WHO (Romanian)
# Database: ValyanMed
# Descriere: Download ICD-10 OMS versiune ROMÂN?
# Source: Ministerul S?n?t??ii + WHO
# ========================================

param(
    [string]$ServerInstance = "DESKTOP-9H54BCS\SQLSERVER",
    [string]$Database = "ValyanMed"
)

$ErrorActionPreference = "Stop"

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "   ICD-10 ROMÂNIA - DOWNLOAD" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

Write-Host "???? Pentru România trebuie s? folose?ti ICD-10 OMS (NU ICD-10-CM)" -ForegroundColor Yellow
Write-Host ""

Write-Host "?? Surse oficiale pentru ICD-10 în limba român?:" -ForegroundColor Cyan
Write-Host ""

Write-Host "1??  MINISTERUL S?N?T??II ROMÂNIA (OFICIAL)" -ForegroundColor Green
Write-Host "   • Website: https://www.ms.ro" -ForegroundColor White
Write-Host "   • Clasificarea ICD-10 în român? (Ordin 1438/2009)" -ForegroundColor White
Write-Host "   • Descarc?: ICD10_RO_Official.xlsx" -ForegroundColor White
Write-Host ""

Write-Host "2??  CNAS - Casa Na?ional? de Asigur?ri de S?n?tate" -ForegroundColor Green
Write-Host "   • Website: https://www.cnas.ro" -ForegroundColor White
Write-Host "   • Nomenclatoare medicale SIUI" -ForegroundColor White
Write-Host "   • Include coduri ICD-10 validate pentru raportare" -ForegroundColor White
Write-Host ""

Write-Host "3??  WHO ICD-10 (Interna?ional, traducere român?)" -ForegroundColor Green
Write-Host "   • Website: https://icd.who.int/browse10/2019/en" -ForegroundColor White
Write-Host "   • API disponibil (necesit? înregistrare)" -ForegroundColor White
Write-Host "   • Multilingv (inclusiv român?)" -ForegroundColor White
Write-Host ""

Write-Host "4??  GitHub - ICD-10 Romanian datasets" -ForegroundColor Green
Write-Host "   • https://github.com/search?q=icd-10+romanian" -ForegroundColor White
Write-Host "   • Unele surse open-source cu traducere român?" -ForegroundColor White
Write-Host ""

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "   DOWNLOAD AUTOMAT" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Încearc? download automat din GitHub (dataset românesc)
$githubUrls = @{
    "WHO_Base" = "https://raw.githubusercontent.com/kamillamagna/ICD-10-CSV/master/codes.csv"
    # TODO: Adaug? link c?tre dataset românesc când g?sim unul
}

Write-Host "?? Downloading ICD-10 base dataset..." -ForegroundColor Cyan
$csvPath = Join-Path $PSScriptRoot "icd10_codes_base.csv"

try {
    Invoke-WebRequest -Uri $githubUrls["WHO_Base"] -OutFile $csvPath
    Write-Host "? Downloaded base dataset (English)" -ForegroundColor Green
    Write-Host "?? Location: $csvPath" -ForegroundColor White
    Write-Host ""
    
    # Cite?te CSV
    $csvData = Import-Csv $csvPath
    Write-Host "?? Found $($csvData.Count) codes" -ForegroundColor Cyan
    Write-Host ""
    
    # Sample
    Write-Host "Sample data:" -ForegroundColor Yellow
    $csvData | Select-Object -First 5 | Format-Table
    
    Write-Host ""
    Write-Host "??  IMPORTANT: Acest dataset este în ENGLEZ?" -ForegroundColor Yellow
    Write-Host "   Pentru versiunea ROMÂN? oficial?, vezi sursele de mai sus." -ForegroundColor Yellow
}
catch {
    Write-Host "? Download failed: $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "   NEXT STEPS" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

Write-Host "Pentru a ob?ine ICD-10 ROMÂNESC complet:" -ForegroundColor Yellow
Write-Host ""
Write-Host "OP?IUNEA A (RECOMANDAT): Manual din surse oficiale" -ForegroundColor Green
Write-Host "  1. Descarc? ICD-10 RO de la Ministerul S?n?t??ii" -ForegroundColor White
Write-Host "  2. Converte?te în CSV (dac? e Excel/PDF)" -ForegroundColor White
Write-Host "  3. Ruleaz?: .\Import-ICD10-FromCSV.ps1 -CsvFilePath 'ICD10_RO.csv'" -ForegroundColor White
Write-Host ""

Write-Host "OP?IUNEA B: Folose?te base dataset EN + traducere manual?" -ForegroundColor Green
Write-Host "  1. Import? dataset EN existent" -ForegroundColor White
Write-Host "  2. Adaug? traduceri române?ti în coloana 'ShortDescription'" -ForegroundColor White
Write-Host "  3. Prioritizeaz? codurile comune (IsCommon=1) pentru traducere" -ForegroundColor White
Write-Host ""

Write-Host "OP?IUNEA C: WHO API cu limba român?" -ForegroundColor Green
Write-Host "  1. Înregistrare la: https://icd.who.int/icdapi" -ForegroundColor White
Write-Host "  2. Ob?ine API credentials" -ForegroundColor White
Write-Host "  3. Folose?te API-ul pentru a ob?ine traduceri RO" -ForegroundColor White
Write-Host ""

# Ofer? op?iune de import rapid cu dataset EN
$importNow = Read-Host "Vrei s? impor?i dataset-ul EN existent acum? (Y/N)"

if ($importNow -eq 'Y') {
    Write-Host ""
    Write-Host "?? Starting import..." -ForegroundColor Cyan
    
    $importScript = Join-Path $PSScriptRoot "Import-ICD10-FromCSV.ps1"
    
    if (Test-Path $importScript) {
        & $importScript -CsvFilePath $csvPath -ServerInstance $ServerInstance -Database $Database
    }
    else {
        Write-Host "? Import script not found: $importScript" -ForegroundColor Red
    }
}

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "?? RESURSE UTILE" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "• Legisla?ie: Ordin MS 1438/2009 (ICD-10 obligatoriu în România)" -ForegroundColor White
Write-Host "• CNAS SIUI: https://www.cnas.ro" -ForegroundColor White
Write-Host "• WHO ICD-10: https://icd.who.int" -ForegroundColor White
Write-Host "• GitHub ICD-10: https://github.com/topics/icd-10" -ForegroundColor White
Write-Host ""

Write-Host "Press any key to exit..."
$null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")
