# ========================================
# Script pentru Verificarea Locatiei Fiserelor
# ValyanClinic - Location Check
# ========================================

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "VERIFICARE LOCATIE FISIERE" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan

$correctPath = "..\..\Database"
$wrongPath = "..\Database"

Write-Host "`nVerific locatia corecta: $correctPath" -ForegroundColor Yellow

if (Test-Path $correctPath) {
    $tableFiles = Get-ChildItem "$correctPath\TableStructure" -Filter "*.sql" -ErrorAction SilentlyContinue | Measure-Object
    $spFiles = Get-ChildItem "$correctPath\StoredProcedures" -Filter "*.sql" -ErrorAction SilentlyContinue | Measure-Object
    
    Write-Host "? LOCATIE CORECTA: DevSupport\Database\" -ForegroundColor Green
    Write-Host "  TableStructure: $($tableFiles.Count) fisiere" -ForegroundColor Green
    Write-Host "  StoredProcedures: $($spFiles.Count) fisiere" -ForegroundColor Green
} else {
    Write-Host "? ATENTIE: Locatia corecta nu exista!" -ForegroundColor Red
}

Write-Host "`nVerific locatia gresita: $wrongPath" -ForegroundColor Yellow

if (Test-Path $wrongPath) {
    $wrongTableFiles = Get-ChildItem "$wrongPath\TableStructure" -Filter "*.sql" -ErrorAction SilentlyContinue | Measure-Object
    $wrongSpFiles = Get-ChildItem "$wrongPath\StoredProcedures" -Filter "*.sql" -ErrorAction SilentlyContinue | Measure-Object
    
    Write-Host "? ATENTIE: Fisiere in locatia gresita!" -ForegroundColor Yellow
    Write-Host "  TableStructure: $($wrongTableFiles.Count) fisiere" -ForegroundColor Yellow
    Write-Host "  StoredProcedures: $($wrongSpFiles.Count) fisiere" -ForegroundColor Yellow
    Write-Host "  Trebuie mutate in DevSupport\Database\" -ForegroundColor Yellow
} else {
    Write-Host "? OK: Nu exista fisiere in locatia gresita" -ForegroundColor Green
}

Write-Host "`n========================================" -ForegroundColor Cyan
Write-Host "VERIFICARE COMPLETA" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan

Write-Host "`nLocatia corecta pentru fisiere:" -ForegroundColor Cyan
Write-Host "DevSupport\Database\" -ForegroundColor White
Write-Host "??? TableStructure\" -ForegroundColor White
Write-Host "??? StoredProcedures\" -ForegroundColor White
Write-Host "??? Functions\" -ForegroundColor White
Write-Host "??? Views\" -ForegroundColor White
Write-Host "??? README.md" -ForegroundColor White