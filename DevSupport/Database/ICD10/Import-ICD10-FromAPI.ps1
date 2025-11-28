# ========================================
# PowerShell Script: Import ICD-10 from WHO API
# Database: ValyanMed
# Descriere: Importa coduri ICD-10 din WHO ICD API
# Source: https://icd.who.int/icdapi
# ========================================

param(
    [string]$ServerInstance = "DESKTOP-9H54BCS\SQLSERVER",
    [string]$Database = "ValyanMed",
    [string]$Language = "ro",  # ro, en
    [int]$MaxCodes = 5000,
    [switch]$Verbose
)

$ErrorActionPreference = "Stop"

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "   WHO ICD-10 API IMPORT" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# WHO ICD API endpoint
$apiBase = "https://id.who.int/icd/release/10/2019"
$clientId = "YOUR_CLIENT_ID_HERE"  # Register at https://icd.who.int/icdapi
$clientSecret = "YOUR_CLIENT_SECRET_HERE"

Write-Host "??  IMPORTANT: You need to register for WHO ICD API access" -ForegroundColor Yellow
Write-Host "   1. Visit: https://icd.who.int/icdapi" -ForegroundColor Yellow
Write-Host "   2. Create account and get API credentials" -ForegroundColor Yellow
Write-Host "   3. Update clientId and clientSecret in this script" -ForegroundColor Yellow
Write-Host ""

# Alternative: GitHub datasets
Write-Host "?? Alternative: Using ICD-10 CSV from GitHub..." -ForegroundColor Cyan
Write-Host ""

$githubUrls = @{
    "EN" = "https://raw.githubusercontent.com/kamillamagna/ICD-10-CSV/master/codes.csv"
    "Comprehensive" = "https://github.com/cedis-unb/icd10-data"
}

Write-Host "Available GitHub sources:" -ForegroundColor Green
Write-Host "  1. kamillamagna/ICD-10-CSV (English, ~14k codes)" -ForegroundColor White
Write-Host "  2. cedis-unb/icd10-data (JSON format, multilingual)" -ForegroundColor White
Write-Host "  3. Downlod ICD-10-CM 2025 from CMS.gov" -ForegroundColor White
Write-Host ""

Write-Host "Recommended approach:" -ForegroundColor Yellow
Write-Host "  1. Download ICD-10-CM-2025.xlsx from:" -ForegroundColor White
Write-Host "     https://www.cms.gov/files/zip/2025-code-descriptions-tabular-order.zip" -ForegroundColor Cyan
Write-Host "  2. Convert to CSV using Excel" -ForegroundColor White
Write-Host "  3. Use Import-ICD10-FromCSV.ps1 script" -ForegroundColor White
Write-Host ""

# Function to download CSV
function Download-ICD10CSV {
    param($Url, $OutputPath)
    
    Write-Host "??  Downloading from: $Url" -ForegroundColor Cyan
    
    try {
        Invoke-WebRequest -Uri $Url -OutFile $OutputPath
        Write-Host "? Downloaded to: $OutputPath" -ForegroundColor Green
        return $true
    }
    catch {
        Write-Host "? Download failed: $($_.Exception.Message)" -ForegroundColor Red
        return $false
    }
}

# Download basic dataset
$csvPath = Join-Path $PSScriptRoot "icd10_codes.csv"

if (Download-ICD10CSV -Url $githubUrls["EN"] -OutputPath $csvPath) {
    Write-Host ""
    Write-Host "?? Parsing CSV..." -ForegroundColor Cyan
    
    $csvData = Import-Csv $csvPath
    Write-Host "? Found $($csvData.Count) codes in CSV" -ForegroundColor Green
    Write-Host ""
    
    # Sample first 5
    Write-Host "Sample data:" -ForegroundColor Yellow
    $csvData | Select-Object -First 5 | Format-Table
    
    Write-Host ""
    Write-Host "Next steps:" -ForegroundColor Cyan
    Write-Host "  1. Review CSV structure" -ForegroundColor White
    Write-Host "  2. Run Import-ICD10-FromCSV.ps1 to insert into database" -ForegroundColor White
    Write-Host "  3. Add Romanian translations manually or from another source" -ForegroundColor White
}

Write-Host ""
Write-Host "Press any key to exit..."
$null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")
