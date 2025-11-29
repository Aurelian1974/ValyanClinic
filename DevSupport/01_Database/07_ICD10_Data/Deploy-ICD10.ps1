# ========================================
# PowerShell Script: Deploy ICD-10 Database
# Database: ValyanMed
# Descriere: Ruleaza automat toate scripturile ICD-10
# ========================================

param(
    [string]$ServerInstance = "DESKTOP-9H54BCS\SQLSERVER",
    [string]$Database = "ValyanMed",
    [switch]$Verbose
)

$ErrorActionPreference = "Stop"

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "   ICD-10 DATABASE DEPLOYMENT" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

$scriptPath = Split-Path -Parent $MyInvocation.MyCommand.Path
$sqlFiles = @(
    "01_Create_ICD10_Table.sql",
    "02_Insert_ICD10_Cardiovascular.sql",
    "03_Insert_ICD10_Endocrin.sql",
    "04_Insert_ICD10_Respirator.sql",
    "05_Create_SP_SearchICD10.sql",
    "06_Update_Common_Codes_RO.sql",
    "07_Create_SP_GetICD10ById.sql",
    "08_Create_Additional_SPs.sql"
)

Write-Host "?? Script Path: $scriptPath" -ForegroundColor Yellow
Write-Host "???  Server: $ServerInstance" -ForegroundColor Yellow
Write-Host "?? Database: $Database" -ForegroundColor Yellow
Write-Host ""

$successCount = 0
$errorCount = 0

foreach ($sqlFile in $sqlFiles) {
    $fullPath = Join-Path $scriptPath $sqlFile
    
    if (Test-Path $fullPath) {
        Write-Host "??  Executing: $sqlFile" -ForegroundColor Cyan
        
        try {
            Invoke-Sqlcmd -ServerInstance $ServerInstance `
                          -Database $Database `
                          -InputFile $fullPath `
                          -Verbose:$Verbose `
                          -ErrorAction Stop
            
            Write-Host "   ? SUCCESS: $sqlFile" -ForegroundColor Green
            $successCount++
        }
        catch {
            Write-Host "   ? ERROR: $sqlFile" -ForegroundColor Red
            Write-Host "   Message: $($_.Exception.Message)" -ForegroundColor Red
            $errorCount++
        }
        
        Write-Host ""
    }
    else {
        Write-Host "   ??  FILE NOT FOUND: $sqlFile" -ForegroundColor Yellow
        Write-Host ""
    }
}

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "   DEPLOYMENT SUMMARY" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "? Success: $successCount" -ForegroundColor Green
Write-Host "? Errors: $errorCount" -ForegroundColor Red
Write-Host ""

if ($errorCount -eq 0) {
    Write-Host "?? Deployment completed successfully!" -ForegroundColor Green
    
    # Test query
    Write-Host ""
    Write-Host "?? Testing ICD-10 database..." -ForegroundColor Cyan
    
    try {
        # Test 1: Count codes
        $countQuery = "SELECT COUNT(*) as TotalCodes FROM ICD10_Codes"
        $countResult = Invoke-Sqlcmd -ServerInstance $ServerInstance `
                                     -Database $Database `
                                     -Query $countQuery
        
        Write-Host "? Total ICD-10 codes: $($countResult.TotalCodes)" -ForegroundColor Green
        
        # Test 2: Search test
        $testQuery = @"
EXEC sp_SearchICD10 
    @SearchTerm = 'diabet', 
    @MaxResults = 5
"@
        
        $searchResults = Invoke-Sqlcmd -ServerInstance $ServerInstance `
                                       -Database $Database `
                                       -Query $testQuery
        
        Write-Host "? Search test results for 'diabet':" -ForegroundColor Green
        $searchResults | Format-Table Code, ShortDescription, Category -AutoSize
        
        # Test 3: Check stored procedures
        $spQuery = @"
SELECT name FROM sys.objects 
WHERE type = 'P' AND name LIKE 'sp_%ICD10%'
ORDER BY name
"@
        
        $spResults = Invoke-Sqlcmd -ServerInstance $ServerInstance `
                                   -Database $Database `
                                   -Query $spQuery
        
        Write-Host ""
        Write-Host "? Stored Procedures created:" -ForegroundColor Green
        $spResults | ForEach-Object { Write-Host "   - $($_.name)" -ForegroundColor White }
    }
    catch {
        Write-Host "??  Test queries failed: $($_.Exception.Message)" -ForegroundColor Yellow
    }
}
else {
    Write-Host "??  Deployment completed with errors. Please review." -ForegroundColor Yellow
}

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "?? NEXT STEPS" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "1. Verify ICD-10 codes in database" -ForegroundColor White
Write-Host "2. Test ICD-10 autocomplete in Blazor app" -ForegroundColor White
Write-Host "3. Add more codes using Import-ICD10-FromCSV.ps1" -ForegroundColor White
Write-Host ""
Write-Host "Press any key to exit..."
$null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")
