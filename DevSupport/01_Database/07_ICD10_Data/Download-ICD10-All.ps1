# ========================================
# Download ICD-10 Codes from Multiple Sources
# Database: ValyanMed
# Descriere: Descarc? coduri ICD-10 din surse gratuite
# ========================================

param(
    [ValidateSet("GitHub", "CMS", "All")]
    [string]$Source = "GitHub",
    [string]$OutputPath = ".\downloads",
    [switch]$ShowHelp
)

$ErrorActionPreference = "Stop"

if ($ShowHelp) {
    Write-Host @"
========================================
   ICD-10 DOWNLOAD SCRIPT
========================================

USAGE:
    .\Download-ICD10-All.ps1 [-Source <source>] [-OutputPath <path>]

PARAMETERS:
    -Source     : GitHub (default), CMS, or All
    -OutputPath : Folder for downloads (default: .\downloads)

EXAMPLES:
    .\Download-ICD10-All.ps1
    .\Download-ICD10-All.ps1 -Source CMS
    .\Download-ICD10-All.ps1 -Source All -OutputPath "D:\Data\ICD10"

SOURCES:
    GitHub  : ~14,400 codes from kamillamagna/ICD-10-CSV
    CMS     : ~72,000 codes from CMS.gov (ICD-10-CM 2024)

"@
    exit 0
}

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "   ICD-10 DATA DOWNLOAD" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Create output directory
if (-not (Test-Path $OutputPath)) {
    New-Item -ItemType Directory -Path $OutputPath -Force | Out-Null
    Write-Host "?? Created directory: $OutputPath" -ForegroundColor Green
}

$OutputPath = Resolve-Path $OutputPath

# ========================================
# GITHUB SOURCE (kamillamagna/ICD-10-CSV)
# ========================================
function Download-FromGitHub {
    Write-Host ""
    Write-Host "?? Downloading from GitHub (kamillamagna/ICD-10-CSV)..." -ForegroundColor Yellow
    
    $url = "https://raw.githubusercontent.com/kamillamagna/ICD-10-CSV/master/codes.csv"
    $outputFile = Join-Path $OutputPath "github_icd10_codes.csv"
    
    try {
        # Use .NET WebClient for better compatibility
        $webClient = New-Object System.Net.WebClient
        $webClient.Headers.Add("User-Agent", "PowerShell/ValyanClinic")
        $webClient.DownloadFile($url, $outputFile)
        
        $fileSize = (Get-Item $outputFile).Length
        $lineCount = (Get-Content $outputFile | Measure-Object).Count
        
        Write-Host "? Downloaded: $outputFile" -ForegroundColor Green
        Write-Host "   Size: $([math]::Round($fileSize / 1KB, 2)) KB" -ForegroundColor White
        Write-Host "   Lines: $lineCount" -ForegroundColor White
        
        # Preview first lines
        Write-Host ""
        Write-Host "?? Preview (first 5 lines):" -ForegroundColor Cyan
        Get-Content $outputFile -First 5 | ForEach-Object { Write-Host "   $_" -ForegroundColor Gray }
        
        return $outputFile
    }
    catch {
        Write-Host "? Error downloading from GitHub: $($_.Exception.Message)" -ForegroundColor Red
        return $null
    }
}

# ========================================
# CMS.GOV SOURCE (ICD-10-CM 2024)
# ========================================
function Download-FromCMS {
    Write-Host ""
    Write-Host "?? Downloading from CMS.gov (ICD-10-CM 2024)..." -ForegroundColor Yellow
    
    $zipUrl = "https://www.cms.gov/files/zip/2024-code-descriptions-tabular-order-updated-01112024.zip"
    $zipFile = Join-Path $OutputPath "cms_icd10cm_2024.zip"
    $extractPath = Join-Path $OutputPath "cms_icd10cm_2024"
    
    try {
        # Download ZIP
        Write-Host "   Downloading ZIP file (may take a moment)..." -ForegroundColor White
        
        $webClient = New-Object System.Net.WebClient
        $webClient.Headers.Add("User-Agent", "PowerShell/ValyanClinic")
        $webClient.DownloadFile($zipUrl, $zipFile)
        
        $zipSize = (Get-Item $zipFile).Length
        Write-Host "   ZIP downloaded: $([math]::Round($zipSize / 1MB, 2)) MB" -ForegroundColor White
        
        # Extract
        Write-Host "   Extracting..." -ForegroundColor White
        
        if (Test-Path $extractPath) {
            Remove-Item $extractPath -Recurse -Force
        }
        
        Expand-Archive -Path $zipFile -DestinationPath $extractPath -Force
        
        # Find the codes file
        $codesFile = Get-ChildItem $extractPath -Recurse -Filter "*.txt" | 
                     Where-Object { $_.Name -match "code" -or $_.Name -match "icd10" } |
                     Select-Object -First 1
        
        if ($codesFile) {
            Write-Host "? Extracted: $($codesFile.FullName)" -ForegroundColor Green
            
            $lineCount = (Get-Content $codesFile.FullName | Measure-Object).Count
            Write-Host "   Lines: $lineCount" -ForegroundColor White
            
            # Preview
            Write-Host ""
            Write-Host "?? Preview (first 5 lines):" -ForegroundColor Cyan
            Get-Content $codesFile.FullName -First 5 | ForEach-Object { Write-Host "   $_" -ForegroundColor Gray }
            
            return $codesFile.FullName
        }
        else {
            Write-Host "   Files in archive:" -ForegroundColor White
            Get-ChildItem $extractPath -Recurse | ForEach-Object { Write-Host "   - $($_.Name)" -ForegroundColor Gray }
            return $extractPath
        }
    }
    catch {
        Write-Host "? Error downloading from CMS: $($_.Exception.Message)" -ForegroundColor Red
        return $null
    }
}

# ========================================
# ALTERNATIVE: datasets/icd-10
# ========================================
function Download-FromDatasets {
    Write-Host ""
    Write-Host "?? Downloading from datasets/icd-10..." -ForegroundColor Yellow
    
    $url = "https://raw.githubusercontent.com/datasets/icd-10/master/icd-10.csv"
    $outputFile = Join-Path $OutputPath "datasets_icd10.csv"
    
    try {
        $webClient = New-Object System.Net.WebClient
        $webClient.Headers.Add("User-Agent", "PowerShell/ValyanClinic")
        $webClient.DownloadFile($url, $outputFile)
        
        $fileSize = (Get-Item $outputFile).Length
        Write-Host "? Downloaded: $outputFile ($([math]::Round($fileSize / 1KB, 2)) KB)" -ForegroundColor Green
        
        return $outputFile
    }
    catch {
        Write-Host "?? Could not download from datasets/icd-10: $($_.Exception.Message)" -ForegroundColor Yellow
        return $null
    }
}

# ========================================
# MAIN EXECUTION
# ========================================

$downloadedFiles = @()

switch ($Source) {
    "GitHub" {
        $file = Download-FromGitHub
        if ($file) { $downloadedFiles += $file }
    }
    "CMS" {
        $file = Download-FromCMS
        if ($file) { $downloadedFiles += $file }
    }
    "All" {
        $file1 = Download-FromGitHub
        if ($file1) { $downloadedFiles += $file1 }
        
        $file2 = Download-FromCMS
        if ($file2) { $downloadedFiles += $file2 }
        
        $file3 = Download-FromDatasets
        if ($file3) { $downloadedFiles += $file3 }
    }
}

# ========================================
# SUMMARY
# ========================================
Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "   DOWNLOAD SUMMARY" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

if ($downloadedFiles.Count -gt 0) {
    Write-Host "? Downloaded files:" -ForegroundColor Green
    $downloadedFiles | ForEach-Object { 
        Write-Host "   ?? $_" -ForegroundColor White 
    }
    
    Write-Host ""
    Write-Host "?? NEXT STEPS:" -ForegroundColor Yellow
    Write-Host "   1. Review the downloaded files" -ForegroundColor White
    Write-Host "   2. Run Import-ICD10-FromCSV.ps1 to import into database:" -ForegroundColor White
    Write-Host ""
    Write-Host "      .\Import-ICD10-FromCSV.ps1 -CsvFilePath `"$($downloadedFiles[0])`"" -ForegroundColor Cyan
    Write-Host ""
}
else {
    Write-Host "? No files were downloaded." -ForegroundColor Red
    Write-Host ""
    Write-Host "Troubleshooting:" -ForegroundColor Yellow
    Write-Host "   - Check internet connection" -ForegroundColor White
    Write-Host "   - Try running as Administrator" -ForegroundColor White
    Write-Host "   - Check if antivirus is blocking downloads" -ForegroundColor White
}

Write-Host ""
Write-Host "Press any key to exit..."
$null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")
